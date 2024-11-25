using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using WebApplication2.Controllers;
using WebApplication2.Data;
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
            var testUser = new ApplicationUser { Id = "testUserId" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);

            var geoChange1 = new GeoChange
            {
                Id = 1,
                GeoJson = "{}",
                Description = "Change 1",
                UserId = testUser.Id,
                MunicipalityName = "Test Municipality 1",
                MunicipalityNumber = "001",
                CountyName = "Test County 1",
                MapVariant = "MapVariant1"
            };
            var geoChange2 = new GeoChange
            {
                Id = 2,
                GeoJson = "{}",
                Description = "Change 2",
                UserId = testUser.Id,
                MunicipalityName = "Test Municipality 2",
                MunicipalityNumber = "002",
                CountyName = "Test County 2",
                MapVariant = "MapVariant2"
            };

            _context.GeoChanges.AddRange(geoChange1, geoChange2);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, testUser.Id)
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
                new Claim(ClaimTypes.NameIdentifier, "testUserId")
            };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() };

            // Act
            var result = await _controller.Create("{}", "Test description", "MapVariant1");

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
                new Claim(ClaimTypes.NameIdentifier, "testUserId")
            };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var result = await _controller.Create("", "Test description", "MapVariant1") as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GeoJson and description must be provided", result.Value);
        }

        [Fact]
        public async Task Edit_ValidInput_RedirectsToIndex()
        {
            // Arrange
            var testUser = new ApplicationUser { Id = "testUserId" };

            var geoChange = new GeoChange
            {
                Id = 1,
                GeoJson = "{}",
                Description = "Original description",
                UserId = testUser.Id,
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County",
                MapVariant = "MapVariant1"
            };

            // Add the initial GeoChange to the context
            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            // Prepare the updated GeoChange
            var updatedGeoChange = new GeoChange
            {
                Description = "Updated description"
            };

            // Act
            var result = await _controller.Edit(geoChange.Id, updatedGeoChange);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("GeoChanges", redirectResult.ControllerName);

            // Verify that the GeoChange was updated in the context
            var updatedEntityInDb = await _context.GeoChanges.FindAsync(geoChange.Id);
            Assert.NotNull(updatedEntityInDb);
            Assert.Equal("Updated description", updatedEntityInDb.Description);
        }

        [Fact]
        public async Task Edit_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var updatedGeoChange = new GeoChange
            {
                Description = "Updated description"
            };

            // Act
            var result = await _controller.Edit(999, updatedGeoChange);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistentGeoChange_ReturnsNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _controller.Delete(nonExistentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ExistingGeoChange_ReturnsView()
        {
            // Arrange
            var testUser = new ApplicationUser { Id = "testUserId" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);

            var geoChange = new GeoChange
            {
                Id = 1,
                GeoJson = "{}",
                Description = "Test Description",
                UserId = testUser.Id,
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County",
                MapVariant = "MapVariant1"
            };

            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(geoChange.Id);

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
                UserId = "testUserId",
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County",
                MapVariant = "MapVariant1"
            };
            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(geoChange.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("GeoChanges", redirectResult.ControllerName);

            // Verify that the GeoChange was removed
            var deletedGeoChange = await _context.GeoChanges.FindAsync(geoChange.Id);
            Assert.Null(deletedGeoChange);
        }
    }
}
