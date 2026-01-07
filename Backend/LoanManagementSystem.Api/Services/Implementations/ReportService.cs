using LoanManagementSystem.Api.DTOs.Reports;
using LoanManagementSystem.Api.Repositories.Implementations;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Interfaces;

namespace LoanManagementSystem.Api.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IEmiRepository _emiRepository;
        private readonly ILoanRepository _loanRepository;

        public ReportService(IReportRepository reportRepository, IEmiRepository emiRepository, ILoanRepository loanRepository)
        {
            _reportRepository = reportRepository;
            _emiRepository = emiRepository;
            _loanRepository = loanRepository;
        }

        // Retrieves aggregated loan statistics grouped by their current status.
        public async Task<IEnumerable<LoansByStatusDto>> GetLoansByStatusAsync()
        {
            return await _reportRepository.GetLoansByStatusAsync();
        }

        // Calculates the total outstanding balance for a specific loan, accounting for moratoriums.
        public async Task<OutstandingAmountDto> GetOutstandingAmountAsync(int loanId)
        {
            return await _reportRepository.GetOutstandingAmountAsync(loanId);
        }

        // Generates a report of EMIs due for a specific month and year.
        public async Task<IEnumerable<MonthlyEmiReportDto>> GetMonthlyEmiReportAsync(int month,int year)
        {
            var emis = await _emiRepository.GetMonthlyEmiReportAsync(month, year);

            return emis.Select(e => new MonthlyEmiReportDto
            {
                LoanId = e.LoanId,
                CustomerName = e.LoanApplication.Customer.FullName,
                LoanType = e.LoanApplication.LoanType.LoanTypeName,
                DueDate = e.DueDate,
                EmiAmount = e.EMIAmount,
                IsPaid = e.IsPaid
            }).ToList();
        }

        //EMI-Overdue Report
        public async Task<IEnumerable<EmiOverdueReportDto>> GetEmiOverdueReportAsync()
        {
            return await _reportRepository.GetEmiOverdueReportAsync();
        }

        // Customer Summary Report
        public async Task<IEnumerable<CustomerLoanSummaryDto>> GetCustomerLoanSummariesAsync()
        {
            return await _loanRepository.GetCustomerLoanSummariesAsync();
        }
    }
}
