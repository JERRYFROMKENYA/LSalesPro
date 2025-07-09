using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique();
            
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Description).HasMaxLength(200);
        });

        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Name).IsUnique();
            
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(200);
            entity.Property(p => p.Category).IsRequired().HasMaxLength(50);
        });

        // UserRole (many-to-many) configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RolePermission (many-to-many) configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            
            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.Token).IsUnique();
            
            entity.Property(rt => rt.Token).IsRequired();
            
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PasswordResetToken configuration
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(prt => prt.Id);
            entity.HasIndex(prt => prt.Token).IsUnique();
            
            entity.Property(prt => prt.Token).IsRequired();
            
            entity.HasOne(prt => prt.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles
        var salesManagerRoleId = Guid.NewGuid();
        var salesRepRoleId = Guid.NewGuid();

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = salesManagerRoleId,
                Name = "Sales Manager",
                Description = "Full access to sales operations and team management",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = salesRepRoleId,
                Name = "Sales Representative",
                Description = "Access to own sales operations",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed Permissions
        var viewAllSalesId = Guid.NewGuid();
        var createSalesId = Guid.NewGuid();
        var approveSalesId = Guid.NewGuid();
        var manageInventoryId = Guid.NewGuid();
        var viewOwnSalesId = Guid.NewGuid();
        var viewInventoryId = Guid.NewGuid();

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = viewAllSalesId, Name = "view_all_sales", Description = "View all sales data", Category = "Sales" },
            new Permission { Id = createSalesId, Name = "create_sales", Description = "Create sales orders", Category = "Sales" },
            new Permission { Id = approveSalesId, Name = "approve_sales", Description = "Approve sales orders", Category = "Sales" },
            new Permission { Id = manageInventoryId, Name = "manage_inventory", Description = "Manage inventory items", Category = "Inventory" },
            new Permission { Id = viewOwnSalesId, Name = "view_own_sales", Description = "View own sales data", Category = "Sales" },
            new Permission { Id = viewInventoryId, Name = "view_inventory", Description = "View inventory data", Category = "Inventory" }
        );

        // Seed Role-Permission mappings
        modelBuilder.Entity<RolePermission>().HasData(
            // Sales Manager permissions
            new RolePermission { RoleId = salesManagerRoleId, PermissionId = viewAllSalesId },
            new RolePermission { RoleId = salesManagerRoleId, PermissionId = createSalesId },
            new RolePermission { RoleId = salesManagerRoleId, PermissionId = approveSalesId },
            new RolePermission { RoleId = salesManagerRoleId, PermissionId = manageInventoryId },
            
            // Sales Representative permissions
            new RolePermission { RoleId = salesRepRoleId, PermissionId = viewOwnSalesId },
            new RolePermission { RoleId = salesRepRoleId, PermissionId = createSalesId },
            new RolePermission { RoleId = salesRepRoleId, PermissionId = viewInventoryId }
        );

        // Seed Users (based on provided JSON data)
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = user1Id,
                Username = "LEYS-1001",
                Email = "david.kariuki@leysco.co.ke",
                PasswordHash = "$2a$11$example_hash_for_SecurePass123!", // Would be properly hashed in real implementation
                FirstName = "David",
                LastName = "Kariuki",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = user2Id,
                Username = "LEYS-1002",
                Email = "jane.njoki@leysco.co.ke",
                PasswordHash = "$2a$11$example_hash_for_SecurePass456!", // Would be properly hashed in real implementation
                FirstName = "Jane",
                LastName = "Njoki",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed User-Role mappings
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = user1Id, RoleId = salesManagerRoleId, AssignedAt = DateTime.UtcNow },
            new UserRole { UserId = user2Id, RoleId = salesRepRoleId, AssignedAt = DateTime.UtcNow }
        );
    }
}
