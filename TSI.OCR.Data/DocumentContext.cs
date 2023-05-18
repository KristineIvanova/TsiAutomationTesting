using Microsoft.EntityFrameworkCore;
using TSI.OCR.Data.Entities;

namespace TSI.OCR.Data;

public class DocumentContext: DbContext {
    private readonly string _filePath;

    public DocumentContext(string filePath = "identifier.db") {
        _filePath = filePath;
    }
    
    public DbSet<Document> Documents { get; set; } = default!;

    public DbSet<Field> Fields { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite($"Data Source={_filePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Document>(x => {
            x.HasMany(p => p.Fields)
                .WithOne(p => p.Document)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Field>(x => {
            x.HasOne(p => p.Document)
                .WithMany(p => p.Fields)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}