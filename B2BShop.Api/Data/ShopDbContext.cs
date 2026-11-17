using B2BShop.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace B2BShop.Api.Data;

public class ShopDbContext : IdentityDbContext<ApplicationUser>
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Place> Places => Set<Place>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CartItem>()
            .HasIndex(item => new { item.CartId, item.ProductId })
            .IsUnique();
        modelBuilder.Entity<CartItem>()
            .HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CartItem>()
            .Property(item => item.Quantity)
            .IsConcurrencyToken();
        modelBuilder.Entity<Place>()
            .HasIndex(place => place.NormalizedName)
            .IsUnique();
        modelBuilder.Entity<Place>()
            .OwnsOne(place => place.Address, address =>
            {
                address.OwnsOne(a => a.Location);
            });
    }
}