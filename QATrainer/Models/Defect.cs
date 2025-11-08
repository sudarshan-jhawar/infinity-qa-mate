using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Defect")]
public class Defect
{
    [Key]
    [Column("Defect_ID")]
    public string? DefectId { get; set; }

    [Required]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    [Column("Module")]
    public string? Module { get; set; }

    [Column("Severity")]
    public string? Severity { get; set; }

    [Column("Priority")]
    public string? Priority { get; set; }

    [Column("Created_Date")]
    public string? CreatedDate { get; set; }

    [Column("Environment")]
    public string? Environment { get; set; }

    // If Embedding stores JSON or text representation of embedding, keep as string.
    // Change to byte[] if it's stored as BLOB.
    [Column("Embedding")]
    public string? Embedding { get; set; }
}