namespace QAMate.Models
{
    // Simple DTO used by API layer. Avoids exposing EF tracking entities directly.
    public class DefectDto
    {
        public int Id { get; set; } // For POST can be 0 or omitted; for PUT should match path.
        public string? Name { get; set; }
        public double? Price { get; set; }
    }
}
