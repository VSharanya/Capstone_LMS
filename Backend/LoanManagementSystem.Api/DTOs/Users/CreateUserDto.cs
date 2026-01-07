using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Api.DTOs.Users
{
    public class CreateUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role {  get; set; } = string.Empty;
    }
}
