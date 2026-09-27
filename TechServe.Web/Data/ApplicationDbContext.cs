using Microsoft.EntityFrameworkCore;

namespace TechServe.Web.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Technician> Technicians { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<RepairJob> RepairJobs { get; set; }
    public DbSet<RepairPart> RepairParts { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CustomerNote> CustomerNotes { get; set; }
    public DbSet<RepairStatusHistory> RepairStatusHistories { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseDetail> PurchaseDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Username).HasMaxLength(60).IsRequired();
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(30).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
            entity.Property(x => x.InvitationTokenHash).HasMaxLength(64);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.CustomerId);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ContactNumber).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(180);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.CustomerStatus).HasMaxLength(30).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(x => x.DeviceId);
            entity.Property(x => x.DeviceType).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Brand).HasMaxLength(60);
            entity.Property(x => x.Model).HasMaxLength(80);
            entity.Property(x => x.SerialNumber).HasMaxLength(120);
            entity.Property(x => x.OperatingSystem).HasMaxLength(80);
            entity.Property(x => x.DeviceCondition).HasMaxLength(40);
            entity.Property(x => x.Accessories).HasMaxLength(300);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Devices)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.HasKey(x => x.TechnicianId);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ContactNumber).HasMaxLength(30);
            entity.Property(x => x.Specialization).HasMaxLength(120);
            entity.Property(x => x.Availability).HasMaxLength(30).HasDefaultValue("Available").IsRequired();
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(x => x.SupplierId);
            entity.Property(x => x.CompanyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(80);
            entity.Property(x => x.ContactPerson).HasMaxLength(120);
            entity.Property(x => x.ContactNumber).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(180);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.PaymentTerms).HasMaxLength(40);
            entity.Property(x => x.LeadTime).HasMaxLength(40);
            entity.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(x => x.PartId);
            entity.Property(x => x.PartName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(80);
            entity.Property(x => x.Brand).HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(400);
            entity.Property(x => x.Quantity).HasDefaultValue(0);
            entity.Property(x => x.ReorderLevel).HasDefaultValue(5);
            entity.Property(x => x.UnitCost).HasColumnType("decimal(12,2)");
            entity.Property(x => x.SellingPrice).HasColumnType("decimal(12,2)");
            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.Parts)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RepairJob>(entity =>
        {
            entity.HasKey(x => x.RepairJobId);
            entity.Property(x => x.Complaint).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Diagnosis).HasMaxLength(1000);
            entity.Property(x => x.RepairDescription).HasMaxLength(1000);
            entity.Property(x => x.EstimatedCost).HasColumnType("decimal(12,2)");
            entity.Property(x => x.ActualCost).HasColumnType("decimal(12,2)");
            entity.Property(x => x.Priority).HasMaxLength(20).HasDefaultValue("Normal").IsRequired();
            entity.Property(x => x.RepairStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.RepairJobs)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Device)
                .WithMany(x => x.RepairJobs)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Technician)
                .WithMany(x => x.RepairJobs)
                .HasForeignKey(x => x.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RepairPart>(entity =>
        {
            entity.HasKey(x => new { x.RepairJobId, x.PartId });
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(12,2)");
            entity.HasOne(x => x.RepairJob)
                .WithMany(x => x.RepairParts)
                .HasForeignKey(x => x.RepairJobId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany(x => x.RepairParts)
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(x => x.InvoiceId);
            entity.Property(x => x.LaborCost).HasDefaultValue(0m).HasColumnType("decimal(12,2)");
            entity.Property(x => x.PartsCost).HasDefaultValue(0m).HasColumnType("decimal(12,2)");
            entity.Property(x => x.OtherCharges).HasDefaultValue(0m).HasColumnType("decimal(12,2)");
            entity.Property(x => x.Discount).HasDefaultValue(0m).HasColumnType("decimal(12,2)");
            entity.Property(x => x.PaymentStatus).HasMaxLength(30).HasDefaultValue("Unpaid").IsRequired();
            entity.Property(x => x.InvoiceDate).HasDefaultValueSql("GETDATE()");
            entity.HasOne(x => x.RepairJob)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.RepairJobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.PaymentId);
            entity.Property(x => x.Amount).HasColumnType("decimal(12,2)");
            entity.Property(x => x.PaymentMethod).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PaidAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerNote>(entity =>
        {
            entity.HasKey(x => x.NoteId);
            entity.Property(x => x.NoteText).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.CustomerNotes)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CustomerNotes)
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RepairStatusHistory>(entity =>
        {
            entity.HasKey(x => x.HistoryId);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.Property(x => x.ChangedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.RepairJob)
                .WithMany(x => x.RepairStatusHistories)
                .HasForeignKey(x => x.RepairJobId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChangedByUser)
                .WithMany(x => x.RepairStatusHistories)
                .HasForeignKey(x => x.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(x => x.PurchaseId);
            entity.Property(x => x.ReferenceNumber).HasMaxLength(60);
            entity.Property(x => x.TotalAmount).HasDefaultValue(0m).HasColumnType("decimal(12,2)");
            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.Purchases)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseDetail>(entity =>
        {
            entity.HasKey(x => new { x.PurchaseId, x.PartId });
            entity.Property(x => x.UnitCost).HasColumnType("decimal(12,2)");
            entity.HasOne(x => x.Purchase)
                .WithMany(x => x.PurchaseDetails)
                .HasForeignKey(x => x.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany(x => x.PurchaseDetails)
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public sealed class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? InvitationTokenHash { get; set; }
    public DateTime? InvitationExpiresAt { get; set; }

    public ICollection<CustomerNote> CustomerNotes { get; set; } = new List<CustomerNote>();
    public ICollection<RepairStatusHistory> RepairStatusHistories { get; set; } = new List<RepairStatusHistory>();
}

public sealed class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string CustomerStatus { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }

    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
    public ICollection<CustomerNote> CustomerNotes { get; set; } = new List<CustomerNote>();
}

public sealed class Device
{
    public int DeviceId { get; set; }
    public int CustomerId { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? OperatingSystem { get; set; }
    public string? DeviceCondition { get; set; }
    public string? Accessories { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
}

public sealed class Technician
{
    public int TechnicianId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? Specialization { get; set; }
    public string Availability { get; set; } = "Available";

    public ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
}

public sealed class Supplier
{
    public int SupplierId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PaymentTerms { get; set; }
    public string? LeadTime { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Part> Parts { get; set; } = new List<Part>();
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}

public sealed class Part
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public int? SupplierId { get; set; }

    public Supplier? Supplier { get; set; }
    public ICollection<RepairPart> RepairParts { get; set; } = new List<RepairPart>();
    public ICollection<PurchaseDetail> PurchaseDetails { get; set; } = new List<PurchaseDetail>();
}

public sealed class RepairJob
{
    public int RepairJobId { get; set; }
    public int CustomerId { get; set; }
    public int DeviceId { get; set; }
    public int? TechnicianId { get; set; }
    public string Complaint { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? RepairDescription { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public DateTime DateReceived { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Priority { get; set; } = "Normal";
    public string RepairStatus { get; set; } = "Pending";

    public Customer Customer { get; set; } = null!;
    public Device Device { get; set; } = null!;
    public Technician? Technician { get; set; }
    public ICollection<RepairPart> RepairParts { get; set; } = new List<RepairPart>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<RepairStatusHistory> RepairStatusHistories { get; set; } = new List<RepairStatusHistory>();
}

public sealed class RepairPart
{
    public int RepairJobId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public RepairJob RepairJob { get; set; } = null!;
    public Part Part { get; set; } = null!;
}

public sealed class Invoice
{
    public int InvoiceId { get; set; }
    public int RepairJobId { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal Discount { get; set; }
    public string PaymentStatus { get; set; } = "Unpaid";
    public DateTime InvoiceDate { get; set; }

    public RepairJob RepairJob { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public sealed class Payment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
}

public sealed class CustomerNote
{
    public int NoteId { get; set; }
    public int CustomerId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}

public sealed class RepairStatusHistory
{
    public int HistoryId { get; set; }
    public int RepairJobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }

    public RepairJob RepairJob { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}

public sealed class Purchase
{
    public int PurchaseId { get; set; }
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal TotalAmount { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseDetail> PurchaseDetails { get; set; } = new List<PurchaseDetail>();
}

public sealed class PurchaseDetail
{
    public int PurchaseId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }

    public Purchase Purchase { get; set; } = null!;
    public Part Part { get; set; } = null!;
}
