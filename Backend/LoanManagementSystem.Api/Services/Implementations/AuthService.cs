using LoanManagementSystem.Api.DTOs.Auth;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Security;
using LoanManagementSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LoanManagementSystem.Api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            JwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = new PasswordHasher<User>();
        }

       
        // Registers a new user, hashes their password, and generates a JWT token.
        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new ApplicationException("User with this email already exists.");
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                AnnualIncome = registerDto.AnnualIncome,
                Role = "Customer",
                IsActive = true,
                
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);

            await _userRepository.AddUserAsync(user);

            return _jwtTokenGenerator.GenerateToken(user);
        }


        // Authenticates a user by validating credentials and returning a JWT token.
        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

            if (user == null || !user.IsActive)
            {
                throw new ApplicationException("Invalid credentials or user is inactive.");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginDto.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                throw new ApplicationException("Invalid credentials or user is inactive.");
            }

            return _jwtTokenGenerator.GenerateToken(user);
        }
    }
}
