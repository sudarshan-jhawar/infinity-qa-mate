using System.ComponentModel.DataAnnotations;

namespace QAMate.Models
{
    // DTO used by API layer for software defects
    public class DefectDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string? Title { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        [Range(1,5)]
        public int Severity { get; set; }
        [Range(1,5)]
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
