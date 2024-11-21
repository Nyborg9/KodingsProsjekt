using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Models;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;
using Mysqlx.Crud;
using Org.BouncyCastle.Utilities.Collections;

public class CaseworkerControllerTests
{
    [Fact]
    public void CaseworkerOverview_WithChanges_ReturnsViewWithChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        // Create a test user
        var testUser = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com"
        };

        // Create the context with in-memory database
        using (var context = new ApplicationDbContext(options))
        {
            // Add test user to the context
            context.Users.Add(testUser);
            context.SaveChanges();

            // Create test changes with required properties
            var changes = new List<GeoChange>
            {
                new GeoChange
                {
                    Id = 1,
                    Description = "Change 1",
                    GeoJson = "{}", // Provide a minimal GeoJson
                    UserId = testUser.Id,
                    User = testUser
                },
                new GeoChange
                {
                    Id = 2,
                    Description = "Change 2",
                    GeoJson = "{}", // Provide a minimal GeoJson
                    UserId = testUser.Id,
                    User = testUser
                }
            };

            // Add test data
            context.GeoChanges.AddRange(changes);
            context.SaveChanges();

            // Setup UserManager mock
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            // Create controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = controller.CaseworkerOverview() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as List<GeoChange>;
            Assert.NotNull(model);
            Assert.Equal(2, model.Count);
        }
    }

    [Fact]
    public void CaseworkerOverview_NoChanges_ReturnsEmptyView()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "EmptyTestDatabase")
            .Options;

        // Create the context with in-memory database (empty)
        using (var context = new ApplicationDbContext(options))
        {
            // Setup UserManager mock
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            // Create controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = controller.CaseworkerOverview() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as List<GeoChange>;
            Assert.NotNull(model);
            Assert.Empty(model);
        }
    }
    [Fact]
    public void CaseworkerOverview_AdminUser_ReturnsViewWithData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "AdminAccessDatabase")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Create a test admin user
            var adminUser = new ApplicationUser
            {
                Id = "admin-user",
                Email = "admin@example.com"
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            // Setup UserManager mock
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(adminUser.Id);

            // Setup a mock ClaimsPrincipal with admin role
            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(x => x.IsInRole("Admin")).Returns(true);

            // Create controller
            var controller = new CaseworkerController(context, mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = mockPrincipal.Object
                    }
                }
            };

            // Add some test data to the database
            context.GeoChanges.Add(new GeoChange
            {
                Id = 1, // Explicitly set an ID
                Description = "Test Change", // Required property
                GeoJson = "{\"type\":\"Point\",\"coordinates\":[0,0]}", // Required property
                UserId = adminUser.Id, // Required property
            });
            context.SaveChanges();

            // Act
            var result = controller.CaseworkerOverview();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<GeoChange>>(viewResult.Model);
            Assert.NotEmpty(model); // Ensure the model is not empty
            Assert.Single(model); // Ensure only one item was added
        }
    }
    [Fact]
    public void CaseworkerOverview_AdminUser_NoReports_ReturnsEmptyView()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "NoRecordsDatabase")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Create a test admin user
            var adminUser = new ApplicationUser
            {
                Id = "admin-user",
                Email = "admin@example.com"
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            // Setup UserManager mock
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(adminUser.Id);

            // Setup a mock ClaimsPrincipal with admin role
            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(x => x.IsInRole("Admin")).Returns(true);

            // Create controller
            var controller = new CaseworkerController(context, mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = mockPrincipal.Object
                    }
                }
            };

            // Act
            var result = controller.CaseworkerOverview();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<GeoChange>>(viewResult.Model);
            Assert.Empty(model); // Ensure the model is empty
        }
    }
    [Fact]
    public async Task UserList_ReturnsViewWithUsers_WhenUsersExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "User ListDatabase")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Create test users
            var users = new List<ApplicationUser>
            {
                new ApplicationUser  { Id = "user1", Email = "user1@example.com" },
                new ApplicationUser  { Id = "user2", Email = "user2@example.com" }
            };

            // Add users to the context
            context.Users.AddRange(users);
            context.SaveChanges();

            // Setup UserManager mock
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of < IUserStore < ApplicationUser >> (),
                null, null, null, null, null, null, null, null);

            mockUserManager.Setup(x => x.Users)
                .Returns(context.Users);

            // Create controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = await controller.UserList() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as List<UserListViewModel>;
            Assert.NotNull(model);
            Assert.Equal(2, model.Count);
            Assert.Contains(model, u => u.Email == "user1@example.com");
            Assert.Contains(model, u => u.Email == "user2@example.com");
        }
    }

}