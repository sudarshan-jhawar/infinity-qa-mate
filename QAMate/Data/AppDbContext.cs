// NOTE: This file represents the scaffolded DbContext and entity from the existing SQLite database (app.db).
// Generated via EF Core Database-First. Example scaffolding command used:
// dotnet ef dbcontext scaffold "Data Source=app.db" Microsoft.EntityFrameworkCore.Sqlite -o Data -c AppDbContext --no-pluralize --force
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
namespace QAMate.Data
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            bool connected = TryOpenConnection();
            bool tableExists = TableExists("Defects");
            Console.WriteLine($"[TABLE] Defects {(tableExists ? "EXISTS" : "DOES NOT EXIST")}");
            Console.WriteLine(" CONNECTED ?  " + connected);
        }

        private bool TableExists(string tableName)
        {
            try
            {
                var conn = Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT name FROM sqlite_master 
                WHERE type='table' AND name=@name COLLATE NOCASE;";

                cmd.Parameters.Add(new SqliteParameter("@name", tableName));

                var tableExists = cmd.ExecuteScalar();

                return tableExists != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TABLE CHECK ERROR] {ex.Message}");
                return false;
            }
        }

        private bool TryOpenConnection()
        {
            try
            {
                // Open + immediately close – this forces SQLite to validate the file
                Database.OpenConnection();
                Database.CloseConnection();
                return true;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"[SQLite ERROR] {ex.Message} (Code: {ex.SqliteErrorCode})");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GENERAL ERROR] {ex.Message}");
                return false;
            }
        }

        // DbSet for the defects table.
        public virtual DbSet<Defect> Defect { get; set; } = null!; // --no-pluralize keeps singular name.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapping for defects table.
            modelBuilder.Entity<Defect>(entity =>
            {
                entity.ToTable("Defects"); // matches existing table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Defect_ID");
                entity.Property(e => e.Title).HasColumnName("Title");
                entity.Property(e => e.Description).HasColumnName("Description");
                entity.Property(e => e.Module).HasColumnName("Module");
                entity.Property(e => e.Severity).HasColumnName("Severity");
                entity.Property(e => e.Priority).HasColumnName("Priority");
                entity.Property(e => e.CreatedDate).HasColumnName("Created_Date");
                entity.Property(e => e.Environment).HasColumnName("Environment");
                entity.Property(e => e.Embedding).HasColumnName("Embedding");
            });
        }
    }

    // Scaffolded entity representing a row in defects table.
    public partial class Defect
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
