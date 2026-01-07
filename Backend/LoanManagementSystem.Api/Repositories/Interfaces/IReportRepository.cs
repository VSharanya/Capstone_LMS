using LoanManagementSystem.Api.DTOs.Reports;

namespace LoanManagementSystem.Api.Repositories.Interfaces
{
    public interface IReportRepository
    {
        // LINQ: Loans grouped by status
        Task<IEnumerable<LoansByStatusDto>> GetLoansByStatusAsync();

        // LINQ: Outstanding per loan

        Task<OutstandingAmountDto> GetOutstandingAmountAsync(int loanId);

        // LINQ: Monthly EMI report

        Task<IEnumerable<MonthlyEmiReportDto>> GetMonthlyEmiReportAsync(int month, int year);

        //EMI-Overdue report
        Task<IEnumerable<EmiOverdueReportDto>> GetEmiOverdueReportAsync();

    }
}
