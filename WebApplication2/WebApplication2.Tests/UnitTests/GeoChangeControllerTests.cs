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
        public async Task Create_ValidInput_CreatesGeoChangeSuccessfully()
        {
            // Arrange
            // Mock the MunicipalityFinderService
            var municipalityFinderServiceMock = new Mock<MunicipalityFinderService>();
            municipalityFinderServiceMock
                .Setup(s => s.FindMunicipalityFromGeoJsonAsync(It.IsAny<string>()))
                .ReturnsAsync(("001", "Test Municipality", "Test County"));

            // Replace the service in the controller
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(MunicipalityFinderService)))
                .Returns(municipalityFinderServiceMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider.Object
            };

            var userId = "user-id-123";
            var claims = new List<Claim>
            {
              new Claim(ClaimTypes.NameIdentifier, userId)
            };
             var identity = new ClaimsIdentity(claims);

            // Setup the controller context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            httpContext.User = new ClaimsPrincipal(identity);

            var geoJson = "{\"type\":\"Point\",\"coordinates\":[10.0, 59.0]}";
            var description = "Test Change";

            // Act
            var result = await _controller.Create(geoJson, description);

            // Assert
            // Check the actual type of the result
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status302Found, actionResult.StatusCode); // Redirect status code

            // Verify the GeoChange was created
            var createdChange = await _context.GeoChanges.FirstOrDefaultAsync(g => g.UserId == userId);
            Assert.NotNull(createdChange);
            Assert.Equal(geoJson, createdChange.GeoJson);
            Assert.Equal(description, createdChange.Description);
            Assert.Equal("001", createdChange.MunicipalityNumber);
            Assert.Equal("Test Municipality", createdChange.MunicipalityName);
            Assert.Equal("Test County", createdChange.CountyName);
        }

        [Fact]
        public async Task Create_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() };

            // Act
            var result = await _controller.Create("{}", "Test description");

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not found", unauthorizedResult.Value);
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

            var geoChange = new GeoChange
            {
                Id = 1,
                GeoJson = "{}",
                Description = "Original description",
                UserId = user.Id,
                // Add required properties
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County"
            };
            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id)
    };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var returnUrl = "/GeoChanges/Index"; // Provide a valid return URL
            var result = await _controller.Edit(
                geoChange.Id,
                new GeoChange { Id = geoChange.Id, Description = "Updated description" },
                returnUrl
            );

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(returnUrl, redirectResult.Url);

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
