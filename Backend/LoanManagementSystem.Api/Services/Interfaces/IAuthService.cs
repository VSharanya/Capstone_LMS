using LoanManagementSystem.Api.DTOs.Auth;

namespace LoanManagementSystem.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
    }
}
