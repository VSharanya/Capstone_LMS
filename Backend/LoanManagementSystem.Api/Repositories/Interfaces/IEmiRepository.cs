using LoanManagementSystem.Api.Models;

namespace LoanManagementSystem.Api.Repositories.Interfaces
{
    public interface IEmiRepository
    {
        Task<IEnumerable<EMI>> GetEmisByLoanIdAsync(int loanId);

        Task<decimal> GetTotalOutstandingAsync(int loanId);

        Task<IEnumerable<EMI>> GetMonthlyEmiReportAsync(int month, int year);

        Task AddEmiAsync(EMI emi);

        Task AddRangeAsync(IEnumerable<EMI> emis);

        Task<EMI?> GetEmiByIdAsync(int emiId);

        Task UpdateEmiAsync(EMI emi);
        
        Task RemoveRangeAsync(IEnumerable<EMI> emis);

        Task SaveAsync();
    }
}
