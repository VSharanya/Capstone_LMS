using LoanManagementSystem.Api.Data;
using LoanManagementSystem.Api.Helpers;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Repositories.Implementations
{
    public class LoanRepository : ILoanRepository
    {
        private readonly ApplicationDbContext _context;

        public LoanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        //  LINQ: Get loans by status
        public async Task<IEnumerable<LoanApplication>> GetLoansByStatusAsync(string status)
        {
            return await _context.LoanApplications
                .Include(l => l.Customer)
                .Include(l => l.LoanType)
                .Where(l => l.Status == status)
                .ToListAsync();
        }

        // Get loans by customer
        public async Task<IEnumerable<LoanApplication>> GetLoansByCustomerIdAsync(int customerId)
        {
            return await _context.LoanApplications
                .Include(l => l.LoanType)
                .Include(l=>l.Customer)
                .Include(l => l.EMIs)
                .Include(l => l.LoanDocuments)
                .Where(l => l.CustomerId == customerId)
                .ToListAsync();
        }

        // Get all loans
        public async Task<IEnumerable<LoanApplication>> GetAllLoansAsync()
        {
            return await _context.LoanApplications
                .Include(l => l.LoanType)
                .Include(l => l.Customer)
                .ToListAsync();
        }

        public IQueryable<LoanApplication> GetAllQueryable()
        {
            return _context.LoanApplications
                .Include(l => l.Customer)
                .Include(l => l.LoanType)
                .AsQueryable();
        }

        public async Task<bool> HasActiveLoanOfSameTypeAsync(int customerId, int loanTypeId)
        {
            return await _context.LoanApplications.AnyAsync(l =>
                l.CustomerId == customerId &&
                l.LoanTypeId == loanTypeId &&
                l.Status == "Active");
        }

        public async Task<LoanApplication?> GetLoanByIdAsync(int loanId)
        {
            return await _context.LoanApplications
                .Include(l => l.Customer)
                .Include(l => l.LoanType)
                .Include(l => l.EMIs)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);
        }

        public async Task AddLoanAsync(LoanApplication loan)
        {
            await _context.LoanApplications.AddAsync(loan);
        }

        public async Task UpdateLoanAsync(LoanApplication loan)
        {
            _context.LoanApplications.Update(loan);
            await SaveAsync();
        }

        public async Task<IEnumerable<LoanApplication>> GetLoansHandledByOfficerAsync(int officerId)
        {
            return await _context.LoanApplications
                .Include(l => l.Customer) // Ensure related data is included if needed for display
                .Include(l => l.LoanType)
                .Where(l =>
                    l.ApprovedBy == officerId ||
                    l.VerifiedBy == officerId ||
                    l.Status == LoanStatusConstants.Applied ||
                    l.Status == LoanStatusConstants.UnderReview)
                .ToListAsync();
        }

        public async Task<LoanType?> GetLoanTypeByIdAsync(int loanTypeId)
        {
            return await _context.LoanTypes
                .FirstOrDefaultAsync(l => l.LoanTypeId == loanTypeId && l.IsActive);
        }
        public async Task<bool> AnyActiveLoansForLoanTypeAsync(int loanTypeId)
        {
            return await _context.LoanApplications
                .AnyAsync(l => l.LoanTypeId == loanTypeId && l.Status == "Active" || l.Status == "Approved");
        }

        public async Task<IEnumerable<DTOs.Reports.CustomerLoanSummaryDto>> GetCustomerLoanSummariesAsync()
        {
            // LINQ Join and GroupBy
            var summaries = await _context.LoanApplications
                .Include(l => l.Customer)
                .GroupBy(l => new { l.CustomerId, l.Customer.FullName, l.Customer.Email }) 
                .Select(g => new DTOs.Reports.CustomerLoanSummaryDto
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.FullName,
                    Email = g.Key.Email,
                    TotalLoans = g.Count(),
                    ActiveLoans = g.Count(l => l.Status == LoanStatusConstants.Active),
                    TotalLoanAmount = g.Sum(l => l.LoanAmount),
                 
                     TotalOutstanding = g.Where(l => l.Status == LoanStatusConstants.Active).Sum(l => l.LoanAmount)
                })
                .ToListAsync();

            return summaries;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
