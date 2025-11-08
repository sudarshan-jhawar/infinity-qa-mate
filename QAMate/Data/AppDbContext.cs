// NOTE: This file represents the scaffolded DbContext and entity from the existing SQLite database (app.db).
// Generated via EF Core Database-First. Example scaffolding command used:
// dotnet ef dbcontext scaffold "Data Source=app.db" Microsoft.EntityFrameworkCore.Sqlite -o Data -c AppDbContext --no-pluralize --force
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
namespace QAMate.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(new DbContextOptionsBuilder<AppDbContext>().Options)
        {
            //var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            //optionsBuilder.UseSqlite("Data Source=infinityQA.db");
            //_dbContext = new AppDbContext(optionsBuilder.Options);
        }       

        // DbSet for the defects table.
        public virtual DbSet<Defect> Defect { get; set; } // --no-pluralize keeps singular name.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapping for defects table.
            //modelBuilder.Entity<Defect>(entity =>
            //{
            //    entity.ToTable("Defects"); // matches existing table name
            //    entity.HasKey(e => e.Id);
            //    entity.Property(e => e.Id).HasColumnName("Defect_ID");
            //    entity.Property(e => e.Title).HasColumnName("Title");
            //    entity.Property(e => e.Description).HasColumnName("Description");
            //    entity.Property(e => e.Module).HasColumnName("Module");
            //    entity.Property(e => e.Severity).HasColumnName("Severity");
            //    entity.Property(e => e.Priority).HasColumnName("Priority");
            //    entity.Property(e => e.CreatedDate).HasColumnName("Created_Date");
            //    entity.Property(e => e.Environment).HasColumnName("Environment");
            //    entity.Property(e => e.Embedding).HasColumnName("Embedding");
            //});
            base.OnModelCreating(modelBuilder);
        }
    }

    // Scaffolded entity representing a row in defects table.
    public partial class Defect
    {
        [Key]
        public int? Defect_ID { get; set; } // For POST can be 0 or omitted; for PUT should match path.
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Module { get; set; }
        public string? Environment { get; set; }
        public string? Embedding { get; set; }
        public string? Priority { get; set; }
        public string? Severity { get; set; }
        public string? CreatedDate { get; set; }
    }

}
