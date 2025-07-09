using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<StockReservation> StockReservations { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Sku).IsUnique();
            
            entity.Property(p => p.Sku).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.TaxRate).HasPrecision(5, 2);
        });

        // Warehouse configuration
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.HasIndex(w => w.Code).IsUnique();
            
            entity.Property(w => w.Code).IsRequired().HasMaxLength(10);
            entity.Property(w => w.Name).IsRequired().HasMaxLength(200);
            entity.Property(w => w.Type).IsRequired().HasMaxLength(20);
            entity.Property(w => w.Address).IsRequired().HasMaxLength(500);
        });

        // InventoryItem configuration
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(ii => ii.Id);
            entity.HasIndex(ii => new { ii.ProductId, ii.WarehouseId }).IsUnique();
            
            entity.HasOne(ii => ii.Product)
                .WithMany(p => p.InventoryItems)
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(ii => ii.Warehouse)
                .WithMany(w => w.InventoryItems)
                .HasForeignKey(ii => ii.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StockReservation configuration
        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.HasKey(sr => sr.Id);
            entity.HasIndex(sr => sr.ReservationId).IsUnique();
            
            entity.Property(sr => sr.ReservationId).IsRequired().HasMaxLength(50);
            
            entity.HasOne(sr => sr.Product)
                .WithMany(p => p.StockReservations)
                .HasForeignKey(sr => sr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(sr => sr.Warehouse)
                .WithMany(w => w.StockReservations)
                .HasForeignKey(sr => sr.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StockMovement configuration
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(sm => sm.Id);
            
            entity.Property(sm => sm.MovementType).IsRequired().HasMaxLength(20);
            entity.Property(sm => sm.Reason).IsRequired().HasMaxLength(50);
            
            entity.HasOne(sm => sm.Product)
                .WithMany()
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(sm => sm.Warehouse)
                .WithMany()
                .HasForeignKey(sm => sm.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Warehouses
        var nairobiWarehouseId = Guid.NewGuid();
        var mombasaWarehouseId = Guid.NewGuid();

        modelBuilder.Entity<Warehouse>().HasData(
            new Warehouse
            {
                Id = nairobiWarehouseId,
                Code = "NCW",
                Name = "Nairobi Central Warehouse",
                Type = "Main",
                Address = "Enterprise Road, Industrial Area, Nairobi",
                ManagerEmail = "warehouse.nairobi@leysco.co.ke",
                Phone = "+254-20-5551234",
                Capacity = 50000,
                Latitude = -1.308971,
                Longitude = 36.851523,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Warehouse
            {
                Id = mombasaWarehouseId,
                Code = "MRW",
                Name = "Mombasa Regional Warehouse",
                Type = "Regional",
                Address = "Port Reitz Road, Changamwe, Mombasa",
                ManagerEmail = "warehouse.mombasa@leysco.co.ke",
                Phone = "+254-41-2224567",
                Capacity = 30000,
                Latitude = -4.034396,
                Longitude = 39.647446,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed Products
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = product1Id,
                Sku = "SF-MAX-20W50",
                Name = "SuperFuel Max 20W-50",
                Category = "Engine Oils",
                Subcategory = "Mineral Oils",
                Description = "High-performance mineral oil for heavy-duty engines",
                Price = 4500.00m,
                TaxRate = 16.0m,
                Unit = "Liter",
                Packaging = "5L Container",
                MinOrderQuantity = 1,
                ReorderLevel = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = product2Id,
                Sku = "ED-SYN-5W30",
                Name = "EcoDrive Synthetic 5W-30",
                Category = "Engine Oils",
                Subcategory = "Synthetic Oils",
                Description = "Fully synthetic oil for modern passenger vehicles",
                Price = 7200.00m,
                TaxRate = 16.0m,
                Unit = "Liter",
                Packaging = "4L Container",
                MinOrderQuantity = 1,
                ReorderLevel = 40,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed Inventory Items
        modelBuilder.Entity<InventoryItem>().HasData(
            // Product 1 in both warehouses
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                ProductId = product1Id,
                WarehouseId = nairobiWarehouseId,
                AvailableQuantity = 150,
                ReservedQuantity = 0,
                LastUpdated = DateTime.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                ProductId = product1Id,
                WarehouseId = mombasaWarehouseId,
                AvailableQuantity = 80,
                ReservedQuantity = 0,
                LastUpdated = DateTime.UtcNow
            },
            // Product 2 in both warehouses
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                ProductId = product2Id,
                WarehouseId = nairobiWarehouseId,
                AvailableQuantity = 200,
                ReservedQuantity = 0,
                LastUpdated = DateTime.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                ProductId = product2Id,
                WarehouseId = mombasaWarehouseId,
                AvailableQuantity = 120,
                ReservedQuantity = 0,
                LastUpdated = DateTime.UtcNow
            }
        );
    }
}
