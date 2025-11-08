namespace QAMate.Models
{
    // DTO used by API layer for software defects
    public class DefectDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int Severity { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
