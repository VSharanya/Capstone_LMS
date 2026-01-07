using LoanManagementSystem.Api.Data;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Repositories.Implementations
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddDocumentAsync(LoanDocument document)
        {
            _context.LoanDocuments.Add(document);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LoanDocument>> GetDocumentsByLoanIdAsync(int loanId)
        {
            return await _context.LoanDocuments
                .Where(d => d.LoanApplicationId == loanId)
                .ToListAsync();
        }

        public async Task<LoanDocument?> GetDocumentByIdAsync(int documentId)
        {
            return await _context.LoanDocuments
                .Include(d => d.LoanApplication)
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);
        }
    }
}
