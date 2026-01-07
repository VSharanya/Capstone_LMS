using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using LoanManagementSystem.Api.DTOs.Documents;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Interfaces;

namespace LoanManagementSystem.Api.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;
        private readonly ILoanRepository _loanRepository;

        public DocumentService(
            IDocumentRepository repository, 
            IWebHostEnvironment environment,
            INotificationService notificationService,
            ILoanRepository loanRepository)
        {
            _repository = repository;
            _environment = environment;
            _notificationService = notificationService;
            _loanRepository = loanRepository;
        }

        // Uploads a document file to the server and creates a database record.
        public async Task<LoanDocument> UploadDocumentAsync(DocumentUploadDto dto, int? requestingUserId = null)
        {
            if (requestingUserId.HasValue)
            {
                var loanEntity = await _loanRepository.GetLoanByIdAsync(dto.LoanApplicationId);
                if (loanEntity == null) throw new KeyNotFoundException("Loan application not found.");
                if (loanEntity.CustomerId != requestingUserId.Value)
                    throw new UnauthorizedAccessException("You are not authorized to upload documents for this loan.");
            }

            if (dto.File == null || dto.File.Length == 0)
                throw new ArgumentException("No file uploaded.");

            // VALIDATION: Check file extension
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt", ".csv" };
            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");
            }

            // Create uploads folder if not exists
            string uploadsFolder = Path.Combine(_environment.WebRootPath ?? Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.File.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(fileStream);
            }

            var document = new LoanDocument
            {
                LoanApplicationId = dto.LoanApplicationId,
                DocumentType = dto.DocumentType,
                OriginalFileName = dto.File.FileName,
                FilePath = filePath,
                UploadedDate = DateTime.UtcNow
            };

            await _repository.AddDocumentAsync(document);

            // Get Loan Details to know who to notify
            var loan = await _loanRepository.GetLoanByIdAsync(dto.LoanApplicationId);
            if (loan != null)
            {
                var message = $"New Document Uploaded: {dto.DocumentType} for loan {loan.LoanType.LoanTypeName}.";
                
                // If loan has an assigned officer, notify them
                // Otherwise notify the whole role
                // For simplicity, we can notify the LoanOfficer role or specific officer if assigned.
                // Assuming this upload is by Customer:
                await _notificationService.NotifyRoleAsync("LoanOfficer", message, "Info");
            }

            return document;
        }

        // Retrieves all document records associated with a specific loan ID.
        public async Task<IEnumerable<LoanDocument>> GetDocumentsByLoanIdAsync(int loanId, int? requestingUserId = null)
        {
            if (requestingUserId.HasValue)
            {
                var loan = await _loanRepository.GetLoanByIdAsync(loanId);
                if (loan == null) throw new KeyNotFoundException("Loan application not found.");
                if (loan.CustomerId != requestingUserId.Value)
                    throw new UnauthorizedAccessException("You are not authorized to view documents for this loan.");
            }

            return await _repository.GetDocumentsByLoanIdAsync(loanId);
        }

        public async Task<LoanDocument?> GetDocumentByIdAsync(int documentId)
        {
            return await _repository.GetDocumentByIdAsync(documentId);
        }

        // Retrieves the file stream, content type, and filename for downloading.
        public async Task<(Stream FileStream, string ContentType, string FileName)?> GetDocumentFileAsync(int documentId, int? requestingUserId = null)
        {
            var document = await _repository.GetDocumentByIdAsync(documentId);
            if (document == null) return null;

            if (requestingUserId.HasValue)
            {
                var loan = await _loanRepository.GetLoanByIdAsync(document.LoanApplicationId);
                if (loan != null && loan.CustomerId != requestingUserId.Value)
                     throw new UnauthorizedAccessException("You are not authorized to download this document.");
            }

            if (!File.Exists(document.FilePath)) return null;

            var memory = new MemoryStream();
            using (var stream = new FileStream(document.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return (memory, GetContentType(document.FilePath), document.OriginalFileName);
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
    }
}
