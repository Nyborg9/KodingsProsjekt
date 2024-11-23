using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
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
    public void CaseworkerOverview_WithReports_ReturnsViewWithReports()
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

            // Create test changes with minimal required properties
            var changes = new List<GeoChange>
        {
            new GeoChange
            {
                Id = 1,
                Description = "Change 1",
                GeoJson = "{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 1\",\"municipalityNumber\":\"001\",\"countyName\":\"County 1\"}}",
                UserId = testUser.Id,
                User = testUser,
                MunicipalityName = ExtractMunicipalityName("{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 1\",\"municipalityNumber\":\"001\",\"countyName\":\"County 1\"}}"),
                MunicipalityNumber = ExtractMunicipalityNumber("{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 1\",\"municipalityNumber\":\"001\",\"countyName\":\"County 1\"}}"),
                CountyName = ExtractCountyName("{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 1\",\"municipalityNumber\":\"001\",\"countyName\":\"County 1\"}}"),
                MapVariant = "MapVariant1"
            },
            new GeoChange
            {
                Id = 2,
                Description = "Change 2",
                GeoJson = "{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 2\",\"municipalityNumber\":\"002\",\"countyName\":\"County 2\"}}",
                UserId = testUser.Id,
                User = testUser,
                MunicipalityName = ExtractMunicipalityName("{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 2\",\"municipalityNumber\":\"002\",\"countyName\":\"County 2\"}}"),
                MunicipalityNumber = ExtractMunicipalityNumber("{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 2\",\"municipalityNumber\":\"002\",\"countyName\":\"County 2\"}}"),
                CountyName = ExtractCountyName("{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 2\",\"municipalityNumber\":\"002\",\"countyName\":\"County 2\"}}"),
                MapVariant = "MapVariant1"
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

    // Helper methods to extract data from GeoJSON
    private string ExtractMunicipalityName(string geoJson)
    {
        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(geoJson);
            return jsonDoc.RootElement
                .GetProperty("properties")
                .GetProperty("municipalityName")
                .GetString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractMunicipalityNumber(string geoJson)
    {
        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(geoJson);
            return jsonDoc.RootElement
                .GetProperty("properties")
                .GetProperty("municipalityNumber")
                .GetString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractCountyName(string geoJson)
    {
        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(geoJson);
            return jsonDoc.RootElement
                .GetProperty("properties")
                .GetProperty("countyName")
                .GetString();
        }
        catch
        {
            return string.Empty;
        }
    }

    [Fact]
    public void CaseworkerOverview_NoReports_ReturnsEmptyView()
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

            // Prepare a GeoJSON with all required properties
            string geoJson = JsonSerializer.Serialize(new
            {
                type = "Feature",
                properties = new
                {
                    municipalityName = "Test Municipality",
                    municipalityNumber = "123",
                    countyName = "Test County"
                },
                geometry = new
                {
                    type = "Point",
                    coordinates = new[] { 0, 0 }
                }
            });

            // Add some test data to the database
            var geoChange = new GeoChange
            {
                Id = 1,
                Description = "Test Change",
                GeoJson = geoJson,
                UserId = adminUser.Id,

                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "123",
                CountyName = "Test County",
                MapVariant = "MapVariant1"
            };

            context.GeoChanges.Add(geoChange);
            context.SaveChanges();

            // Act
            var result = controller.CaseworkerOverview();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<GeoChange>>(viewResult.Model);
            Assert.NotEmpty(model); // Ensure the model is not empty
            Assert.Single(model); // Ensure only one item was added

            // Additional assertions to verify the properties
            var savedChange = model.First();
            Assert.Equal("Test Municipality", savedChange.MunicipalityName);
            Assert.Equal("123", savedChange.MunicipalityNumber);
            Assert.Equal("Test County", savedChange.CountyName);
            Assert.Equal("MapVariant1", savedChange.MapVariant);
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
    
}