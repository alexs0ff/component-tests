using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shop.Orders;

namespace Shop.Entities;

public class ShopContext : DbContext
{
    public ShopContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Basket> Baskets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(cfg =>
        {
            cfg.HasKey(i => i.Id);
            cfg.Property(p => p.Name).HasMaxLength(200);
        });
        modelBuilder.Entity<Basket>(cfg =>
        {
            cfg.HasKey(i => i.Id);
            cfg.Property(i => i.Total).IsRequired();
            cfg.Property(i => i.Userid).IsRequired();
            cfg.Property(i => i.OrderId);
            cfg.Property(i => i.PromoCode).HasMaxLength(200);
            cfg.HasOne(i => i.Order).WithOne(i => i.Basket)
                .HasForeignKey<Basket>(i => i.OrderId);
            cfg.HasMany(i => i.OrderItems).WithOne(i => i.Basket).HasForeignKey(i => i.BasketId);
        });
        modelBuilder.Entity<OrderItem>(cfg =>
        {
            cfg.HasKey(i => i.Id);
            cfg.Property(i => i.Name).IsRequired();
            cfg.Property(i => i.Count).IsRequired();
            cfg.Property(i => i.Price).IsRequired();
            cfg.Property(i => i.BasketId).IsRequired();
        });

        modelBuilder.Entity<Order>(cfg =>
        {
            cfg.HasKey(i => i.Id);
            cfg.Property(i => i.BasketId).IsRequired();
            cfg.Property(i => i.Status).HasMaxLength(32).HasConversion(new EnumToStringConverter<OrderStatus>())
                .IsRequired();
            cfg.HasOne(i => i.Basket).WithOne(i => i.Order)
                .HasForeignKey<Order>(i => i.BasketId);
        });
    }
}
