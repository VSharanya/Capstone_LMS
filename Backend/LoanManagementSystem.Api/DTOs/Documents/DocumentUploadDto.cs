using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Api.DTOs.Documents
{
    public class DocumentUploadDto
    {
        [Required]
        public int LoanApplicationId { get; set; }
        
        [Required]
        public string DocumentType { get; set; } = "Other";
        
        [Required]
        public IFormFile? File { get; set; }
    }
}
