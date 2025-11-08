// NOTE: This file represents the scaffolded DbContext and entity from the existing SQLite database (app.db).
// Generated via EF Core Database-First. Example scaffolding command used:
// dotnet ef dbcontext scaffold "Data Source=app.db" Microsoft.EntityFrameworkCore.Sqlite -o Data -c AppDbContext --no-pluralize --force
using Microsoft.EntityFrameworkCore;

namespace QAMate.Data
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet for the defects table.
        public virtual DbSet<Defect> Defect { get; set; } = null!; // --no-pluralize keeps singular name.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapping for defects table.
            modelBuilder.Entity<Defect>(entity =>
            {
                entity.ToTable("defects"); // matches existing table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.Price).HasColumnName("Price");
            });
        }
    }

    // Scaffolded entity representing a row in defects table.
    public partial class Defect
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double? Price { get; set; }
    }
}
