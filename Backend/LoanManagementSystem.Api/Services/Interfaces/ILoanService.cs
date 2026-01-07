using LoanManagementSystem.Api.DTOs.Loans;

namespace LoanManagementSystem.Api.Services.Interfaces
{
    public interface ILoanService
    {
        Task<int> ApplyLoanAsync(int customerId, LoanApplyDto loanApplyDto);

        Task<IEnumerable<LoanResponseDto>> GetLoansByStatusAsync(string? status);

        Task<IEnumerable<LoanResponseDto>> GetLoansByCustomerIdAsync(int customerId);

        Task<IEnumerable<LoanResponseDto>> GetAllLoansAsync();

        Task<LoanResponseDto?> GetLoanByIdAsync(int loanId);

        Task ApproveOrRejectLoanAsync(int loanId, int officerId, LoanStatusUpdateDto statusDto);
        Task<IEnumerable<LoanResponseDto>> GetLoansByOfficerAsync(int officerId);
    }
}
