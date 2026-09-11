//ApplicationDbContext.cs file 
using Microsoft.EntityFrameworkCore;
using SimpleInventorySystem.Model;
using SimpleInventorySystem.Model.Entities;

namespace SimpleInventorySystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<ItemSupplier> ItemSuppliers => Set<ItemSupplier>();

    public DbSet<Customer> Customers => Set<Customer>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Item
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("items");

            entity.HasKey(e => e.ItemId);

            entity.Property(e => e.ItemId)
                .HasColumnName("item_id");

            entity.Property(e => e.ItemCode)
                .HasColumnName("item_code")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(e => e.ItemCode)
                .IsUnique();

            entity.Property(e => e.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            entity.Property(e => e.Unit)
                .HasColumnName("unit")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .HasPrecision(12, 3);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
        });

        // Supplier
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");

            entity.HasKey(e => e.SupplierId);

            entity.Property(e => e.SupplierId)
                .HasColumnName("supplier_id");

            entity.Property(e => e.SupplierCode)
                .HasColumnName("supplier_code")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(e => e.SupplierCode)
                .IsUnique();

            entity.Property(e => e.SupplierName)
                .HasColumnName("supplier_name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.ContactPerson)
                .HasColumnName("contact_person")
                .HasMaxLength(100);

            entity.Property(e => e.ContactNumber)
                .HasColumnName("contact_number")
                .HasMaxLength(50);

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(150);

            entity.Property(e => e.Address)
                .HasColumnName("address")
                .HasMaxLength(255);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
        });

        // Stock Transaction
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.ToTable("stock_transactions");

            entity.HasKey(e => e.TransactionId);

            entity.Property(e => e.TransactionId)
                .HasColumnName("transaction_id");

            entity.Property(e => e.ItemId)
                .HasColumnName("item_id");

            entity.Property(e => e.SupplierId)
                .HasColumnName("supplier_id");

            entity.Property(e => e.TransactionType)
                .HasColumnName("transaction_type")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .HasPrecision(12, 3);

            entity.Property(e => e.TransactionDate)
                .HasColumnName("transaction_date");

            entity.Property(e => e.Remarks)
                .HasColumnName("remarks")
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(e => e.Item)
                .WithMany(e => e.StockTransactions)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                .WithMany(e => e.StockTransactions)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Audit Log
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");

            entity.HasKey(e => e.AuditLogId);

            entity.Property(e => e.AuditLogId)
                .HasColumnName("audit_log_id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.Action)
                .HasColumnName("action")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.EntityType)
                .HasColumnName("entity_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.EntityId)
                .HasColumnName("entity_id");

            entity.Property(e => e.OldValues)
                .HasColumnName("old_values")
                .HasColumnType("json");

            entity.Property(e => e.NewValues)
                .HasColumnName("new_values")
                .HasColumnType("json");

            entity.Property(e => e.Remarks)
                .HasColumnName("remarks")
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
        });

        // ItemSupplier mapping
        modelBuilder.Entity<ItemSupplier>(entity =>
        {
            entity.ToTable("item_suppliers");

            entity.HasKey(e => e.ItemSupplierId);

            entity.Property(e => e.ItemSupplierId)
                .HasColumnName("item_supplier_id");

            entity.Property(e => e.ItemId)
                .HasColumnName("item_id");

            entity.Property(e => e.SupplierId)
                .HasColumnName("supplier_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ItemId, e.SupplierId })
                .IsUnique();
        });

        // Customer mapping
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");

            entity.HasKey(e => e.CustomerId);

            entity.Property(e => e.CustomerId)
                .HasColumnName("customer_id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Address)
                .HasColumnName("address")
                .HasMaxLength(55);
        });
    }
}