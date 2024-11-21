using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using WebApplication2.Controllers;
using WebApplication2.Data;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WebApplication2.Services;
namespace WebApplication2.Tests.UnitTests
{
    public class GeoChangesControllerTests : IDisposable
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly ApplicationDbContext _context;
        private readonly GeoChangesController _controller;

        public GeoChangesControllerTests()
        {
            // Setting up a unique in-memory database for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique database name
                .Options;

            _context = new ApplicationDbContext(options);

            // Mock UserManager
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            var mockLogger = new Mock<ILogger<GeoChangesController>>();
            // Create the controller
    
            _controller = new GeoChangesController(_context, _mockUserManager.Object, mockLogger.Object);
        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted(); // Clean up the in-memory database
            _context.Dispose();
        }

        [Fact]
        public async Task Index_UserFound_ReturnsViewWithGeoChanges()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id-123" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var geoChange1 = new GeoChange
            {
                Id = 1,
                GeoJson = "{}",
                Description = "Change 1",
                UserId = user.Id,
                // Add required properties
                MunicipalityName = "Test Municipality 1",
                MunicipalityNumber = "001",
                CountyName = "Test County 1"
            };
            var geoChange2 = new GeoChange
            {
                Id = 2,
                GeoJson = "{}",
                Description = "Change 2",
                UserId = user.Id,
                // Add required properties
                MunicipalityName = "Test Municipality 2",
                MunicipalityNumber = "002",
                CountyName = "Test County 2"
            };

            _context.GeoChanges.AddRange(geoChange1, geoChange2);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<GeoChange>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Index_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id-123")
            };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ValidInput_RedirectsToIndex()
        {
            // Arrange
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id-123")
        };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Setup mock municipality finder service
            var mockMunicipalityService = new Mock<IMunicipalityFinderService>(); // Use the actual interface from your project
            mockMunicipalityService
                .Setup(s => s.FindMunicipalityFromGeoJsonAsync(It.IsAny<string>()))
                .ReturnsAsync(("001", "Test Municipality", "Test County"));

            // Modify to use GeoChangesController instead of GeoChangeController
            var controller = new GeoChangesController(
                _context,
                _mockUserManager.Object,
                Mock.Of<ILogger<GeoChangesController>>()
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            // Act
            var result = await controller.Create("{}", "Test description");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify database state
            var geoChanges = await _context.GeoChanges.ToListAsync();
            Assert.Single(geoChanges);

            var geoChange = geoChanges[0];
            Assert.Equal("{}", geoChange.GeoJson);
            Assert.Equal("Test description", geoChange.Description);
            Assert.Equal("user-id-123", geoChange.UserId);
            Assert.Equal("001", geoChange.MunicipalityNumber);
            Assert.Equal("Test Municipality", geoChange.MunicipalityName);
            Assert.Equal("Test County", geoChange.CountyName);
        }

        [Fact]
        public async Task Create_InvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id-123")
            };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var result = await _controller.Create("", "Test description") as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GeoJson and description must be provided", result.Value);
        }

        [Fact]
        public async Task Edit_ValidInput_RedirectsToReturnUrl()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id-123" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var geoChange = new GeoChange { Id = 1, GeoJson = "{}", Description = "Original description", UserId = user.Id };
            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Set up UrlHelper
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var urlHelper = new Mock<IUrlHelper>();
            urlHelperFactory.Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);
            _controller.Url = urlHelper.Object;

            // Act
            var returnUrl = "/GeoChanges/Index"; // Provide a valid return URL
            var result = await _controller.Edit(geoChange.Id, new GeoChange { Id = geoChange.Id, Description = "Updated description" }, returnUrl);

            // Assert
            Assert.NotNull(result); // Check if result is not null

            // Check if the result is a RedirectResult
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(returnUrl, redirectResult.Url); // Ensure it redirects to the returnUrl

            // Verify that the GeoChange was updated in the context
            var updatedGeoChange = await _context.GeoChanges.FindAsync(geoChange.Id);
            Assert.NotNull(updatedGeoChange);
            Assert.Equal("Updated description", updatedGeoChange.Description);
        }


        [Fact]
        public async Task Edit_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id-123" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var result = await _controller.Edit(999, new GeoChange { Id = 999, Description = "Updated description", GeoJson = "{}" }, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_NullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Delete(null, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistentGeoChange_ReturnsNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _controller.Delete(nonExistentId, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ExistingGeoChange_ReturnsView()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id-123" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var geoChange = new GeoChange
            {
                Id = 1,
                GeoJson = "{}",
                Description = "Test Description",
                UserId = user.Id,
                // Add required properties
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County"
            };

            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(geoChange.Id, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(geoChange, viewResult.Model);
        }

        [Fact]
        public async Task DeleteConfirmed_ExistingGeoChange_RemovesAndRedirects()
        {
            // Arrange
            var geoChange = new GeoChange
            {
                Id = 1,
                Description = "Test Description",
                GeoJson = "{}",
                UserId = "test-user-id",
                // Add required properties
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County"
            };
            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            // Act
            var returnUrl = "/GeoChanges/Index";
            var result = await _controller.DeleteConfirmed(geoChange.Id, returnUrl);

            // Assert
            Assert.IsType<RedirectResult>(result);
            var redirectResult = result as RedirectResult;
            Assert.Equal(returnUrl, redirectResult.Url);

            // Verify that the GeoChange was removed
            var deletedGeoChange = await _context.GeoChanges.FindAsync(geoChange.Id);
            Assert.Null(deletedGeoChange);
        }
        [Fact]
        public async Task DeleteConfirmed_ExistingGeoChange_NoReturnUrl_RedirectsToIndex()
        {
            // Arrange
            var geoChange = new GeoChange
            {
                Id = 1,
                Description = "Test Description",
                GeoJson = "{}", // Provide a valid GeoJson string
                UserId = "test-user-id", // Provide a valid UserId

                // Add required properties
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County"
            };
            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            // Mock the UrlHelper
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/GeoChanges/Index");
            urlHelperFactory.Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);
            _controller.Url = urlHelper.Object;

            // Act
            var result = await _controller.DeleteConfirmed(geoChange.Id, null);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.Equal("Index", redirectResult.ActionName);
        }

    }
}
