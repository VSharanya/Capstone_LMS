using LoanManagementSystem.Api.Models;

namespace LoanManagementSystem.Api.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task AddDocumentAsync(LoanDocument document);
        Task<IEnumerable<LoanDocument>> GetDocumentsByLoanIdAsync(int loanId);
        Task<LoanDocument?> GetDocumentByIdAsync(int documentId);
    }
}
