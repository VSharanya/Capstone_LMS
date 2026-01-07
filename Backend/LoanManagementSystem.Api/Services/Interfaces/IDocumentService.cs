using LoanManagementSystem.Api.DTOs.Documents;
using LoanManagementSystem.Api.Models;

namespace LoanManagementSystem.Api.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<LoanDocument> UploadDocumentAsync(DocumentUploadDto dto, int? requestingUserId = null);
        Task<IEnumerable<LoanDocument>> GetDocumentsByLoanIdAsync(int loanId, int? requestingUserId = null);
        Task<LoanDocument?> GetDocumentByIdAsync(int documentId);
        Task<(Stream FileStream, string ContentType, string FileName)?> GetDocumentFileAsync(int documentId, int? requestingUserId = null);
    }
}
