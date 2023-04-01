using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Shop.ComponentTests.Db.Models;

namespace Shop.ComponentTests.Db;

public partial class ShopContext : DbContext
{
    public ShopContext(DbContextOptions<ShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Basket> Baskets { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Basket>(entity =>
        {
            entity.HasIndex(e => e.Userid, "IX_Baskets_Userid");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.PromoCode).HasMaxLength(200);

            entity.HasOne(d => d.User).WithMany(p => p.Baskets).HasForeignKey(d => d.Userid);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.BasketId, "IX_Orders_BasketId").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Status).HasMaxLength(32);

            entity.HasOne(d => d.Basket).WithOne(p => p.Order).HasForeignKey<Order>(d => d.BasketId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.BasketId, "IX_OrderItems_BasketId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Basket).WithMany(p => p.OrderItems).HasForeignKey(d => d.BasketId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
