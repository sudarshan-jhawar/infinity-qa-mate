namespace QAMate.Models
{
    // Simple DTO used by API layer. Avoids exposing EF tracking entities directly.
    public class DefectDto
    {
        public int Id { get; set; } // For POST can be 0 or omitted; for PUT should match path.
        public string Title { get; set; }
        public string Description { get; set; }
        public string Module { get; set; }
        public string Environment { get; set; }
        public string Embedding { get; set; }
        public string Priority { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
