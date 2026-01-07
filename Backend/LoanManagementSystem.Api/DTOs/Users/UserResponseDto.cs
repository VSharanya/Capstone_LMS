namespace LoanManagementSystem.Api.DTOs.Users
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal AnnualIncome { get; set; }
        public bool IsActive { get; set; }
    }
}
