using LoanManagementSystem.Api.DTOs.Reports;

namespace LoanManagementSystem.Api.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<LoansByStatusDto>> GetLoansByStatusAsync();
        Task<OutstandingAmountDto> GetOutstandingAmountAsync(int loanId);
        Task<IEnumerable<MonthlyEmiReportDto>> GetMonthlyEmiReportAsync(int month,int year);
        Task<IEnumerable<EmiOverdueReportDto>> GetEmiOverdueReportAsync();
        Task<IEnumerable<CustomerLoanSummaryDto>> GetCustomerLoanSummariesAsync();
    }
}
