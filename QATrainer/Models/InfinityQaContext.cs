using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QATrainer.Models;

public partial class InfinityQaContext : DbContext
{
    public InfinityQaContext()
    {
    }

    public InfinityQaContext(DbContextOptions<InfinityQaContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=infinityQA.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public virtual DbSet<Defect> Defects { get; set; } = null!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Defect>(entity =>
        {
            entity.ToTable("Defect");

            entity.HasKey(e => e.DefectId);

            entity.Property(e => e.DefectId)
                .HasColumnName("Defect_ID");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("Title");

            entity.Property(e => e.Description)
                .HasColumnName("Description");

            entity.Property(e => e.Module)
                .HasColumnName("Module");

            entity.Property(e => e.Severity)
                .HasColumnName("Severity");

            entity.Property(e => e.Priority)
                .HasColumnName("Priority");

            entity.Property(e => e.CreatedDate)
                .HasColumnName("Created_Date")
                .HasColumnType("TEXT");

            entity.Property(e => e.Environment)
                .HasColumnName("Environment");

            entity.Property(e => e.Embedding)
                .HasColumnName("Embedding");
        });
    }
}
