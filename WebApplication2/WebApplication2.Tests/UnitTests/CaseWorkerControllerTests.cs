using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Routing;

public class CaseworkerControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly CaseworkerController _controller;
    private readonly ApplicationDbContext _context;
    public CaseworkerControllerTests()
    {
        // Setup in-memory database options
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Mock UserManager
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
        _controller = new CaseworkerController(new ApplicationDbContext(_dbContextOptions), _mockUserManager.Object);
        _context = new ApplicationDbContext(_dbContextOptions);
    }

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
        [Fact]
        public void ReportDetails_ExistingReport_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Prepare test data
            var testGeoChange = new GeoChange
            {
                Id = 1,
                Description = "Test Description",
                GeoJson = "{}",
                UserId = "test-user-id",
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County",
                MapVariant = "MapVariant1"
            };

            // Setup context and add test data
            using (var context = new ApplicationDbContext(options))
            {
                context.GeoChanges.Add(testGeoChange);
                context.SaveChanges();
            }

            // Use a fresh context for the test
            using (var context = new ApplicationDbContext(options))
            {
                // Setup UserManager mock
                var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
                var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                    userStoreMock.Object, null, null, null, null, null, null, null, null);

                // Create controller
                var controller = new CaseworkerController(context, mockUserManager.Object);

                // Act
                var result = controller.ReportDetails(1);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<GeoChange>(viewResult.Model);

                Assert.Equal(1, model.Id);
                Assert.Equal("Test Description", model.Description);
                Assert.Equal("Test Municipality", model.MunicipalityName);
                Assert.Equal("001", model.MunicipalityNumber);
                Assert.Equal("Test County", model.CountyName);
            }
        }

        [Fact]
        public void ReportDetails_NonExistentReport_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                // Setup UserManager mock
                var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
                var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                    userStoreMock.Object, null, null, null, null, null, null, null, null);

                // Create controller
                var controller = new CaseworkerController(context, mockUserManager.Object);

                // Act
                var result = controller.ReportDetails(999); // Non-existent ID

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public void ReportDetails_EmptyDatabase_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                // Setup UserManager mock
                var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
                var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                    userStoreMock.Object, null, null, null, null, null, null, null, null);

                // Create controller
                var controller = new CaseworkerController(context, mockUserManager.Object);

                // Act
                var result = controller.ReportDetails(1);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

    [Fact]
    public async Task EditReport_ExistingGeoChange_ReturnsRedirectToAction()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "EditReportDatabase")
            .Options;

        var geoChange = new GeoChange
        {
            Id = 1,
            Description = "Old Description",
            GeoJson = "{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 1\",\"municipalityNumber\":\"001\",\"countyName\":\"County 1\"}}",
            UserId = "test-user-id",
            MunicipalityName = "Municipality 1",
            MunicipalityNumber = "001",
            CountyName = "County 1",
            MapVariant = "MapVariant1"
        };

        using (var context = new ApplicationDbContext(options))
        {
            context.GeoChanges.Add(geoChange);
            context.SaveChanges();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var controller = new CaseworkerController(context, null);

            // Act
            var result = await controller.EditReport(1, new GeoChange
            {
                Description = "New Description",
                GeoJson = geoChange.GeoJson, // Use existing GeoJson
                UserId = geoChange.UserId, // Use existing UserId
                MunicipalityName = geoChange.MunicipalityName, // Use existing MunicipalityName
                MunicipalityNumber = geoChange.MunicipalityNumber, // Use existing MunicipalityNumber
                CountyName = geoChange.CountyName, // Use existing CountyName
                MapVariant = geoChange.MapVariant // Use existing MapVariant
            });

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CaseworkerOverview", redirectResult.ActionName);
            Assert.Equal("Caseworker", redirectResult.ControllerName);

            // Verify that the description was updated
            var updatedGeoChange = await context.GeoChanges.FindAsync(1);
            Assert.Equal("New Description", updatedGeoChange.Description);
        }
    }

    // Test for trying to edit a non-existent report
    [Fact]
    public async Task EditReport_NonExistentGeoChange_ReturnsNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "NonExistentDatabase")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var controller = new CaseworkerController(context, null);

            // Act
            var result = await controller.EditReport(999, new GeoChange { Description = "New Description" });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

    [Fact]
    public async Task EditReport_ExceptionThrown_ReturnsViewWithModel()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "ExceptionDatabase")
            .Options;

        var geoChange = new GeoChange
        {
            Id = 1,
            Description = "Old Description",
            GeoJson = "{\"type\":\"Feature\",\"properties\":{\"municipalityName\":\"Municipality 1\",\"municipalityNumber\":\"001\",\"countyName\":\"County 1\"}}",
            UserId = "test-user-id",
            MunicipalityName = "Municipality 1",
            MunicipalityNumber = "001",
            CountyName = "County 1",
            MapVariant = "MapVariant1"
        };

        using (var context = new ApplicationDbContext(options))
        {
            context.GeoChanges.Add(geoChange);
            context.SaveChanges();
        }

        // Create a mock for the DbContext
        var mockContext = new Mock<ApplicationDbContext>(options);
        mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new DbUpdateException("Simulated exception"));

        var controller = new CaseworkerController(mockContext.Object, null);

        // Act
        var result = await controller.EditReport(1, new GeoChange { Description = "New Description" });

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("New Description", ((GeoChange)viewResult.Model).Description);
    }

    [Fact]
    public async Task DeleteUser_UserExists_ReturnsViewWithModel()
    {
        // Arrange
        var userId = "123";
        var user = new ApplicationUser { Id = userId, Email = "test@example.com" };
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DeleteUserViewModel>(viewResult.Model);
        Assert.Equal(userId, model.Id);
        Assert.Equal(user.Email, model.Email);
    }

    [Fact]
    public async Task DeleteUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = "123";
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_ValidUser_DeletesUserAndRedirects()
    {
        // Arrange
        var testCaseworker = new ApplicationUser
        {
            Id = "TestUser",
            Email = "TestUser@example.com"
        };

        var serviceProviderMock = new Mock<IServiceProvider>();
        var httpContextMock = new Mock<HttpContext>();

        var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        var urlHelperMock = new Mock<IUrlHelper>();

        urlHelperFactoryMock
            .Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelperMock.Object);

        var authenticationServiceMock = new Mock<IAuthenticationService>();

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IUrlHelperFactory)))
            .Returns(urlHelperFactoryMock.Object);

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(authenticationServiceMock.Object);

        // Create HttpContext with mock service provider
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity()),
            RequestServices = serviceProviderMock.Object
        };

        // Setup controller context with the mocked HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Setup UserManager mocks
        _mockUserManager.Setup(um => um.FindByIdAsync(testCaseworker.Id))
            .ReturnsAsync(testCaseworker);

        _mockUserManager.Setup(um => um.DeleteAsync(testCaseworker))
            .ReturnsAsync(IdentityResult.Success);

        // Setup Authentication Service mock
        authenticationServiceMock
            .Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteConfirmed(testCaseworker.Id);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Userlist", redirectResult.ActionName); // Ensure it redirects to Userlist

        _mockUserManager.Verify(um => um.DeleteAsync(testCaseworker), Times.Once);
        authenticationServiceMock.Verify(
            x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()),
            Times.Once
        );
    }


    [Fact]
    public async Task DeleteConfirmed_UserNotFound_ReturnsViewWithModel()
    {
        // Arrange
        var userId = "123";
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _controller.DeleteConfirmed(userId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DeleteUserViewModel>(viewResult.Model);
        Assert.Equal(userId, model.Id);
        Assert.Null(model.Email);
    }

    [Fact]
    public async Task DeleteConfirmed_DeleteFails_ReturnsViewWithModel()
    {
        // Arrange
        var userId = "123";
        var user = new ApplicationUser { Id = userId, Email = "test@example.com" };
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
        var errors = new[] { new IdentityError { Description = "Error deleting user." } };
        _mockUserManager.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.DeleteConfirmed(userId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DeleteUserViewModel>(viewResult.Model);
        Assert.Equal(userId, model.Id);
        Assert.Equal(user.Email, model.Email);
        Assert.True(_controller.ModelState.Count > 0); // Ensure there is a model error
    }

    [Fact]
    public async Task DeleteReport_ValidId_ReturnsViewWithGeoChange()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Prepare test data
        var testGeoChange = new GeoChange
        {
            Id = 1,
            Description = "Test Description",
            GeoJson = "{}",
            UserId = "userId",
            MunicipalityName = "Test Municipality",
            MunicipalityNumber = "001",
            CountyName = "Test County",
            MapVariant = "MapVariant1"
        };

        // Setup context and add test data
        using (var context = new ApplicationDbContext(options))
        {
            context.GeoChanges.Add(testGeoChange);
            await context.SaveChangesAsync();
        }

        // Use a fresh context for the test
        using (var context = new ApplicationDbContext(options))
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Create controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = await controller.DeleteReport(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GeoChange>(viewResult.Model);

            Assert.Equal(1, model.Id);
            Assert.Equal("Test Description", model.Description);
            Assert.Equal("Test Municipality", model.MunicipalityName);
        }
    }

    [Fact]
    public async Task DeleteReport_NullId_ReturnsNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Create controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = await controller.DeleteReport(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

    [Fact]
    public async Task DeleteReport_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Create controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = await controller.DeleteReport(999); // Non-existent ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

    [Fact]
    public async Task DeleteReportConfirmed_ExistingGeoChange_RemovesAndRedirects()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Create and populate the context
        using (var context = new ApplicationDbContext(options))
        {
            var geoChange = new GeoChange
            {
                Id = 1,
                Description = "Test Description",
                GeoJson = "{}",
                UserId = "test-user-id",
                MunicipalityName = "Test Municipality",
                MunicipalityNumber = "001",
                CountyName = "Test County",
                MapVariant = "MapVariant1"
            };
            context.GeoChanges.Add(geoChange);
            await context.SaveChangesAsync();
        }

        // Use a new context to perform the deletion
        using (var context = new ApplicationDbContext(options))
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Create the controller with the new context
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = await controller.DeleteReportConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CaseworkerOverview", redirectResult.ActionName);
            Assert.Equal("Caseworker", redirectResult.ControllerName);
        }

        // Verify deletion with another fresh context
        using (var context = new ApplicationDbContext(options))
        {
            // Verify that the GeoChange was removed
            var deletedGeoChange = await context.GeoChanges.FindAsync(1);
            Assert.Null(deletedGeoChange);
        }
    }

    [Fact]
    public async Task Details_ExistingId_ReturnsViewWithGeoChange()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var testUser = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com"
        };

        var testGeoChange = new GeoChange
        {
            Id = 1,
            Description = "Test Change",
            GeoJson = "{\"type\":\"Feature\"}",
            MunicipalityName = "Test Municipality",
            MunicipalityNumber = "001",
            CountyName = "Test County",
            MapVariant = "TestVariant",
            UserId = testUser.Id,
            User = testUser
        };

        // Setup the context and add the test data
        using (var context = new ApplicationDbContext(options))
        {
            context.Users.Add(testUser);
            context.GeoChanges.Add(testGeoChange);
            await context.SaveChangesAsync();
        }

        // Use a fresh context for the test
        using (var context = new ApplicationDbContext(options))
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Create the controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GeoChange>(viewResult.Model);

            Assert.Equal(1, model.Id);
            Assert.Equal("Test Change", model.Description);
            Assert.Equal("Test Municipality", model.MunicipalityName);
            Assert.Equal("TestVariant", model.MapVariant);
            Assert.Equal(testUser.Id, model.UserId);
        }
    }

    [Fact]
    public async Task Details_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Setup UserManager mock (if required)
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            // Create the controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = await controller.Details(999); // Non-existent ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

    [Fact]
    public async Task Details_NullId_ReturnsNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Setup UserManager mock (if required)
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            // Create the controller
            var controller = new CaseworkerController(context, mockUserManager.Object);

            // Act
            var result = await controller.Details(0); // Invalid ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
    [Fact]
    public async Task PromoteToCaseworker_ValidInput_PromotesUserToCaseworker()
    {
        // Arrange
        var userId = "user-id-123";
        var user = new ApplicationUser { Id = userId, UserName = "username" };
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _controller.PromoteToCaseworker(userId) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UserList", result.ActionName);
        _mockUserManager.Verify(um => um.RemoveFromRoleAsync(user, "User"), Times.Once);
        _mockUserManager.Verify(um => um.AddToRoleAsync(user, "Caseworker"), Times.Once);
    }

    [Fact]
    public async Task DemoteToUser_ValidInput_DemotesACaseworkerToUser()
    {
        // Arrange
        var userId = "Caseworker1";
        var user = new ApplicationUser { Id = userId, UserName = "Caseworker1" };
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _controller.DemoteToUser(userId) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UserList", result.ActionName);
        _mockUserManager.Verify(um => um.RemoveFromRoleAsync(user, "Caseworker"), Times.Once);
        _mockUserManager.Verify(um => um.AddToRoleAsync(user, "User"), Times.Once);
    }
}