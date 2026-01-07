using AutoMapper;
using System.Security.Claims;
using LoanManagementSystem.Api.DTOs.Users;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly PasswordHasher<User> _passwordHasher;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<User>();
        }

        // Retrieves a list of all registered users (Admin only).
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(_mapper.Map<IEnumerable<UserResponseDto>>(users));
        }

        // Creates a new user (Admin) with default password.
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("User with this email already exists.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                IsActive = true
            };

            // Default password (can be changed later)
            user.PasswordHash = _passwordHasher.HashPassword(user, "Welcome@123");

            await _userRepository.AddUserAsync(user);

            return Ok(new{message = "User created successfully with default password."});
        }

        // Activates or deactivates a user account.
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            // Get currently logged-in admin user id from JWT
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized(new { message = "Invalid token claims." });

            var loggedInUserId = int.Parse(userIdClaim);

            // Prevent admin from deactivating themselves
            if (id == loggedInUserId && !dto.IsActive)
            {
                return BadRequest(new { message = "Admin cannot deactivate own account." });
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                return NotFound("User not found.");

            user.IsActive = dto.IsActive;
            await _userRepository.UpdateUserAsync(user);

            return Ok(new { message = "User status updated." });
        }
    }
}
