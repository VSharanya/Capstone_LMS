using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims; // Required for User.FindFirst
using System.Linq;
using LoanManagementSystem.Api.DTOs.Documents;

namespace LoanManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoanRepository _loanRepository;

        public DocumentsController(IDocumentService documentService, ILoanRepository loanRepository)
        {
            _documentService = documentService;
            _loanRepository = loanRepository;
        }

        // Uploads a document for a specific loan application.
        [HttpPost("upload")]
        [Authorize(Roles = "Customer, LoanOfficer")]
        public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadDto dto)
        {
            try
            {
                int? userId = null;
                if (User.IsInRole("Customer"))
                {
                    var userIdClaim = User.FindFirst("UserId");
                    if (userIdClaim == null) return Unauthorized();
                    userId = int.Parse(userIdClaim.Value);
                }

                var document = await _documentService.UploadDocumentAsync(dto, userId);
                return Ok(new { Message = "Document uploaded successfully", DocumentId = document.DocumentId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Retrieves metadata for all documents associated with a loan.
        [HttpGet("{loanId}")]
        [Authorize]
        public async Task<IActionResult> GetDocuments(int loanId)
        {
            try
            {
                int? userId = null;
                if (User.IsInRole("Customer"))
                {
                    var userIdClaim = User.FindFirst("UserId");
                    if (userIdClaim == null) return Unauthorized();
                    userId = int.Parse(userIdClaim.Value);
                }

                var documents = await _documentService.GetDocumentsByLoanIdAsync(loanId, userId);
                return Ok(documents.Select(d => new
                {
                    d.DocumentId,
                    d.DocumentType,
                    d.OriginalFileName,
                    d.UploadedDate
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        // Downloads the actual file content of a specific document.
        [HttpGet("download/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                // Note: GetDocumentByIdAsync is just a metadata check, actual download validation happens in GetDocumentFileAsync
                // But for safety and consistency, we can rely on GetDocumentFileAsync's checks or add logic here.
                // Since GetDocumentFileAsync handles the heavy lifting and validation, we'll let it handle it.
                // However, the original code had checks. Let's trust the Service refactor.

                int? userId = null;
                if (User.IsInRole("Customer"))
                {
                    var userIdClaim = User.FindFirst("UserId");
                    if (userIdClaim == null) return Unauthorized();
                    userId = int.Parse(userIdClaim.Value);
                }

                // Proceed to download
                var result = await _documentService.GetDocumentFileAsync(id, userId);

                if (result == null)
                    return NotFound("File not found on disk.");

                var (fileStream, contentType, fileName) = result.Value;
                return File(fileStream, contentType, fileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
    }


}
