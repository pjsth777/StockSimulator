using Microsoft.EntityFrameworkCore;
using StockSimulator.Domain.Entities;

namespace StockSimulator.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPortfolio> UserPortfolios => Set<UserPortfolio>();
    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasIndex(s => s.Symbol).IsUnique();
            entity.Property(s => s.Symbol).HasMaxLength(10).IsRequired();
            entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
            entity.Property(s => s.CurrentPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<UserPortfolio>(entity =>
        {
            entity.Property(p => p.Username).HasMaxLength(50).IsRequired();
            entity.Property(p => p.AvailableCash).HasPrecision(18, 2);
            entity.HasIndex(p => p.UserId).IsUnique();

            entity.HasOne(p => p.User)
                .WithOne(u => u.UserPortfolio)
                .HasForeignKey<UserPortfolio>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioItem>(entity =>
        {
            entity.HasIndex(pi => new { pi.UserPortfolioId, pi.StockId }).IsUnique();

            entity.HasOne(pi => pi.UserPortfolio)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.UserPortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pi => pi.Stock)
                .WithMany()
                .HasForeignKey(pi => pi.StockId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Username)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(t => t.Symbol)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(t => t.PricePerShare)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Ignore(t => t.TotalAmount);

            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => t.Symbol);
            entity.HasIndex(t => t.ExecuteAt);

            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Stock)
                .WithMany()
                .HasForeignKey(t => t.StockId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
