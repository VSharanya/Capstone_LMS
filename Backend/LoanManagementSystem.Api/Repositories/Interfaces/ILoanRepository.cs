using LoanManagementSystem.Api.Models;

namespace LoanManagementSystem.Api.Repositories.Interfaces
{
    public interface ILoanRepository
    {
        Task<IEnumerable<LoanApplication>> GetLoansByStatusAsync(string status);
        Task<LoanApplication?> GetLoanByIdAsync(int loanId);
        Task<IEnumerable<LoanApplication>> GetLoansByCustomerIdAsync(int customerId);
        Task<IEnumerable<LoanApplication>> GetAllLoansAsync();
        Task AddLoanAsync(LoanApplication loan);
        Task UpdateLoanAsync(LoanApplication loan);
        Task<LoanType?> GetLoanTypeByIdAsync(int loanTypeId);
        
        // Support for dynamic LINQ queries
        IQueryable<LoanApplication> GetAllQueryable();

        Task<bool> AnyActiveLoansForLoanTypeAsync(int loanTypeId);
        Task<bool> HasActiveLoanOfSameTypeAsync(int customerId, int loanTypeId);
        Task<IEnumerable<LoanApplication>> GetLoansHandledByOfficerAsync(int officerId);
        Task<IEnumerable<LoanManagementSystem.Api.DTOs.Reports.CustomerLoanSummaryDto>> GetCustomerLoanSummariesAsync();
        Task SaveAsync();
    }
}
