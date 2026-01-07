using System;
using System.Threading.Tasks;
using LoanManagementSystem.Api.DTOs.Auth;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Security;
using LoanManagementSystem.Api.Services.Implementations;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LoanManagementSystem.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly AuthService _authService;
        private readonly JwtTokenGenerator _jwtGenerator;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();

            var jwtSettings = new JwtSettings
            {
                Key = "ThisIsASecretKeyForTestingPurposeOnly12345",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryMinutes = 60
            };
            var options = Options.Create(jwtSettings);
            _jwtGenerator = new JwtTokenGenerator(options);

            _authService = new AuthService(_mockUserRepo.Object, _jwtGenerator);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnToken_WhenRegistrationSuccessful()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Email = "test@test.com",
                Password = "Password@123",
                FullName = "Test User",
                AnnualIncome = 50000
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(dto.Email))
                         .ReturnsAsync((User?)null);

            // Act
            var token = await _authService.RegisterAsync(dto);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            _mockUserRepo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenEmailExists()
        {
            // Arrange
            var dto = new RegisterDto { Email = "test@test.com" };
            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(dto.Email))
                         .ReturnsAsync(new User());

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
        {
            // Arrange
            var email = "test@test.com";
            var password = "Password@123";
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            var user = new User
            {
                UserId = 1,
                Email = email,
                FullName = "Test User", 
                Role = "Customer",
                IsActive = true
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(email))
                         .ReturnsAsync(user);

            var dto = new LoginDto { Email = email, Password = password };

            // Act
            var token = await _authService.LoginAsync(dto);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            var dto = new LoginDto { Email = "wrong@test.com", Password = "pwd" };
            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(dto.Email))
                         .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _authService.LoginAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenPasswordInvalid()
        {
            // Arrange
            var email = "test@test.com";
            var user = new User
            {
                Email = email,
                IsActive = true,
                PasswordHash = "somehash"
            };
            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(email))
                         .ReturnsAsync(user);

            var dto = new LoginDto { Email = email, Password = "wrongpassword" };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _authService.LoginAsync(dto));
        }
    }
}
