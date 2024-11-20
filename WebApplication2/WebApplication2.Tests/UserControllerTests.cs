using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication2.Controllers;
using WebApplication2.Models;
using Xunit;

namespace WebApplication2.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null, null, null, null);

            _controller = new UserController(_userManagerMock.Object, _signInManagerMock.Object);

            // Setup default HttpContext with a user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CheckRole_AdminUser_RedirectsToAdminPage()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com"
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            _userManagerMock.Setup(um => um.GetRolesAsync(testUser))
                .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _controller.CheckRole();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AdminPage", redirectResult.ActionName);
        }

        [Fact]
        public async Task CheckRole_UserRole_RedirectsToUserPage()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com"
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            _userManagerMock.Setup(um => um.GetRolesAsync(testUser))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.CheckRole();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserPage", redirectResult.ActionName);
        }

        [Fact]
        public async Task CheckRole_NoUser_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.CheckRole();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Register_ValidModel_CreatesUserAndSignsIn()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Change this line to return a Task<IdentityResult>
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _signInManagerMock.Setup(x => x.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserPage", redirectResult.ActionName);

            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
            _signInManagerMock.Verify(x => x.SignInAsync(It.IsAny<ApplicationUser>(), false, null), Times.Once);
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var identityErrors = new List<IdentityError>
        {
            new IdentityError { Description = "Password is too weak" }
        };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Password is too weak", _controller.ModelState[string.Empty].Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Login_ValidCredentials_RedirectsToUserPage()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            var user = new ApplicationUser { Email = model.Email };

            _signInManagerMock.Setup(x => x.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserPage", redirectResult.ActionName);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            _signInManagerMock.Setup(x => x.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Contains("Invalid login attempt.", _controller.ModelState[string.Empty].Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Logout_SignsOutUser()
        {
            // Arrange
            _signInManagerMock.Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ValidId_ReturnsViewWithViewModel()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com"
            };

            _userManagerMock.Setup(um => um.FindByIdAsync(testUser.Id))
                .ReturnsAsync(testUser);

            // Act
            var result = await _controller.DeleteUser(testUser.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DeleteUserViewModel>(viewResult.Model);
            Assert.Equal(testUser.Id, model.Id);
            Assert.Equal(testUser.Email, model.Email);
        }

        [Fact]
        public async Task DeleteUser_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.DeleteUser("non-existent-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidUser_DeletesUserAndRedirects()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com"
            };

            // Create a comprehensive mock service provider
            var serviceProviderMock = new Mock<IServiceProvider>();
            var httpContextMock = new Mock<HttpContext>();

            // Mock URL Helper Factory
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            var urlHelperMock = new Mock<IUrlHelper>();

            urlHelperFactoryMock
                .Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);

            // Mock Authentication Service
            var authenticationServiceMock = new Mock<IAuthenticationService>();

            // Setup service provider to return mocked services
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
            _userManagerMock.Setup(um => um.FindByIdAsync(testUser.Id))
                .ReturnsAsync(testUser);

            _userManagerMock.Setup(um => um.DeleteAsync(testUser))
                .ReturnsAsync(IdentityResult.Success);

            // Setup Authentication Service mock
            authenticationServiceMock
                .Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(testUser.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            // Verify that delete and sign out were called
            _userManagerMock.Verify(um => um.DeleteAsync(testUser), Times.Once);
            authenticationServiceMock.Verify(
                x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteConfirmed_DeleteFails_ReturnsViewWithErrors()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com"
            };

            var failureResult = IdentityResult.Failed(
                new IdentityError { Description = "Delete failed" }
            );

            _userManagerMock.Setup(um => um.FindByIdAsync(testUser.Id))
                .ReturnsAsync(testUser);

            _userManagerMock.Setup(um => um.DeleteAsync(testUser))
                .ReturnsAsync(failureResult);

            // Act
            var result = await _controller.DeleteConfirmed(testUser.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DeleteUserViewModel>(viewResult.Model);
            Assert.Equal(testUser.Id, model.Id);
            Assert.Equal(testUser.Email, model.Email);
        }

        [Fact]
        public async Task UserPage_AuthenticatedUser_ReturnsView()
        {
            // Arrange
            var user = new ApplicationUser { Email = "test@example.com" };
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Email)
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claims }
            };

            _userManagerMock.Setup(x => x.GetUserAsync(claims))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.UserPage();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, viewResult.Model);
        }
    }
}