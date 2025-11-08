// NOTE: DbContext and entity for existing SQLite database (app.db).
// Updated Defect entity to reflect software defect tracking fields.
using Microsoft.EntityFrameworkCore;

namespace QAMate.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Defect> Defect { get; set; } = null!; // singular due to --no-pluralize

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Defect>(entity =>
            {
                entity.ToTable("defects");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.Severity).HasColumnName("severity");
                entity.Property(e => e.Priority).HasColumnName("priority");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.LastModifiedAt).HasColumnName("last_modified_at");
            });
        }
    }

    // Software defect entity (tracking fields)
    public class Defect
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; } // e.g. Open, InProgress, Resolved, Closed
        public int Severity { get; set; } // e.g. 1=Critical .. 5=Minor
        public int Priority { get; set; } // e.g. 1=High .. 5=Low
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
