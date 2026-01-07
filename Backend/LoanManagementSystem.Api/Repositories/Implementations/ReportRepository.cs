using LoanManagementSystem.Api.Data;
using LoanManagementSystem.Api.DTOs.Reports;
using LoanManagementSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // LINQ: LOANS BY STATUS
     
        public async Task<IEnumerable<LoansByStatusDto>> GetLoansByStatusAsync()
        {
            return await _context.LoanApplications
                .GroupBy(l => l.Status)
                .Select(g => new LoansByStatusDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
        }

    
        // LINQ: TOTAL OUTSTANDING (MORATORIUM-AWARE)
        public async Task<OutstandingAmountDto> GetOutstandingAmountAsync(int loanId)
        {
            var loan = await _context.LoanApplications
                .Include(l => l.EMIs)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null)
                throw new ApplicationException("Loan not found.");

            var outstanding = loan.EMIs
                .Where(e =>
                    !e.IsPaid &&
                    e.DueDate >= loan.EmiStartDate   // MORATORIUM HANDLED
                )
                .Sum(e => e.EMIAmount);

            return new OutstandingAmountDto
            {
                LoanId = loanId,
                OutstandingAmount = outstanding
            };
        }

        // LINQ: MONTHLY EMI REPORT (MORATORIUM-AWARE)
        public async Task<IEnumerable<MonthlyEmiReportDto>> GetMonthlyEmiReportAsync(int month, int year)
        {
            return await _context.EMIs
                .Include(e => e.LoanApplication)
                .Where(e =>
                    e.DueDate.Month == month &&
                    e.DueDate.Year == year &&
                    e.DueDate >= e.LoanApplication.EmiStartDate   
                )
                .Select(e => new MonthlyEmiReportDto
                {
                    LoanId = e.LoanId,
                    DueDate = e.DueDate,
                    EmiAmount = e.EMIAmount,
                    IsPaid = e.IsPaid
                })
                .ToListAsync();
        }

        //EMI-Overdue
        public async Task<IEnumerable<EmiOverdueReportDto>> GetEmiOverdueReportAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return await _context.EMIs
                .Where(e => !e.IsPaid && e.DueDate < today)
                .Select(e => new EmiOverdueReportDto
                {
                    LoanId = e.LoanId,
                    EmiId = e.EMIId,
                    DueDate = e.DueDate,
                    EmiAmount = e.EMIAmount,
                    DaysOverdue = today.DayNumber - e.DueDate.DayNumber
                })
                .ToListAsync();
        }
    }
}
