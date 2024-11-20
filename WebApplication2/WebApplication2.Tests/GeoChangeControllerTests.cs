using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Org.BouncyCastle.Utilities.Collections;
using System.Security.Claims;
using WebApplication2.Controllers;
using WebApplication2.Data;
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
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using System.Linq.Expressions;

namespace WebApplication2.Tests
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

            // Create the controller
            _controller = new GeoChangesController(_context, _mockUserManager.Object);
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
                GeoJson = "{}", // Ensure GeoJson is set
                Description = "Test Description",
                UserId = user.Id // Ensure UserId is set
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
                GeoJson = "{}", // Provide a valid GeoJson string
                UserId = "test-user-id" // Provide a valid UserId
            };
            _context.GeoChanges.Add(geoChange);
            await _context.SaveChangesAsync();

            // Act
            var returnUrl = "/GeoChanges/Index"; // Provide a valid return URL
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
                UserId = "test-user-id" // Provide a valid UserId
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
