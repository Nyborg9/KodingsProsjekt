using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Models;
using Xunit;

namespace WebApplication2.Tests
{
    public class GeoChangesControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly ApplicationDbContext _context;
        private readonly GeoChangesController _controller;

        public GeoChangesControllerTests()
        {
            // Setting up in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ApplicationDbContext(options);

            // Mock UserManager
            var userStore = new Mock<IUserStore <ApplicationUser>> ();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            // Create the controller
            _controller = new GeoChangesController(_context, _mockUserManager.Object);
        }

        [Fact]
        public async Task Index_UserFound_ReturnsViewWithGeoChanges()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id-123" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var geoChange1 = new GeoChange { Id = 1, GeoJson = "{}", Description = "Change 1", UserId = user.Id };
            var geoChange2 = new GeoChange { Id = 2, GeoJson = "{}", Description = "Change 2", UserId = user.Id };

            _context.GeoChanges.AddRange(geoChange1, geoChange2);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result); // Check that the result is a ViewResult
            var model = Assert.IsAssignableFrom<List<GeoChange>>(result.Model);
            Assert.Equal(2, model.Count); // Ensure the correct number of geo changes
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
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            // Act
            var result = await _controller.Create("{}", "Test description", "Test map variant") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("GeoChanges", result.ControllerName);

            // Verify that the GeoChange was added to the context
            var geoChanges = await _context.GeoChanges.ToListAsync();
            Assert.Single(geoChanges);
            Assert.Equal("{}", geoChanges[0].GeoJson);
            Assert.Equal("Test description", geoChanges[0].Description);
            Assert.Equal("user-id-123", geoChanges[0].UserId);
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
            var result = await _controller.Create("", "Test description", "Test map variant") as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GeoJson and description must be provided", result.Value);
        }
    }
}