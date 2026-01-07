using LoanManagementSystem.Api.Data;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Repositories.Implementations
{
    public class EmiRepository : IEmiRepository
    {
        private readonly ApplicationDbContext _context;

        public EmiRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EMI>> GetEmisByLoanIdAsync(int loanId)
        {
            return await _context.EMIs
                .Where(e => e.LoanId == loanId)
                .OrderBy(e => e.InstallmentNumber)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalOutstandingAsync(int loanId)
        {
            return await _context.EMIs
                .Where(e => e.LoanId == loanId && !e.IsPaid)
                .SumAsync(e => e.EMIAmount);
        }

        public async Task<IEnumerable<EMI>> GetMonthlyEmiReportAsync(int month, int year)
        {
            return await _context.EMIs
                .Include(e=>e.LoanApplication)
                .Include(e=>e.LoanApplication.Customer)
                .Include(e=>e.LoanApplication.LoanType)
                .Where(e => e.DueDate.Month == month && e.DueDate.Year == year)
                .OrderBy(e=>e.DueDate)
                .ToListAsync();
        }
        
        public async Task AddEmiAsync(EMI emi)
        {
            await _context.EMIs.AddAsync(emi);
        }

        public async Task AddRangeAsync(IEnumerable<EMI> emis)
        {
            await _context.EMIs.AddRangeAsync(emis);
        }

        public async Task<EMI?> GetEmiByIdAsync(int emiId)
        {
            return await _context.EMIs
                .FirstOrDefaultAsync(e => e.EMIId == emiId);
        }

        public async Task UpdateEmiAsync(EMI emi)
        {
            _context.EMIs.Update(emi);
            await SaveAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<EMI> emis)
        {
            _context.EMIs.RemoveRange(emis);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
