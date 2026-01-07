using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LoanManagementSystem.Api.Models
{
    public class LoanDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        public int LoanApplicationId { get; set; }

        [ForeignKey("LoanApplicationId")]
        [JsonIgnore]
        public LoanApplication? LoanApplication { get; set; }

        [Required]
        public string DocumentType { get; set; } = string.Empty; 

        [Required]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty; 

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    }
}
