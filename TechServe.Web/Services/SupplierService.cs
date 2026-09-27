using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;

namespace TechServe.Web.Services;

public sealed class SupplierService
{
    private readonly ApplicationDbContext database;

    public SupplierService(ApplicationDbContext database)
    {
        this.database = database;
    }

    public async Task<IReadOnlyList<SupplierItem>> GetSuppliersAsync(CancellationToken cancellationToken = default)
    {
        return await database.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.CompanyName)
            .Select(supplier => new SupplierItem
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.CompanyName,
                Category = supplier.Category ?? string.Empty,
                ContactPerson = supplier.ContactPerson ?? string.Empty,
                ContactEmail = supplier.Email ?? string.Empty,
                Phone = supplier.ContactNumber ?? string.Empty,
                Address = supplier.Address ?? string.Empty,
                Status = supplier.IsActive ? "Active" : "Inactive",
                PaymentTerms = supplier.PaymentTerms ?? string.Empty,
                LeadTime = supplier.LeadTime ?? string.Empty
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SupplierItem> AddSupplierAsync(string name, string category,
        string? contactPerson = null,
        string? contactEmail = null,
        string? phone = null,
        string? address = null,
        string status = "Active",
        string paymentTerms = "Net 30",
        string leadTime = "3-5 Days",
        CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier
        {
            CompanyName = name.Trim(),
            Category = category.Trim(),
            ContactPerson = Normalize(contactPerson),
            Email = Normalize(contactEmail),
            ContactNumber = Normalize(phone),
            Address = Normalize(address),
            IsActive = !string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase),
            PaymentTerms = Normalize(paymentTerms),
            LeadTime = Normalize(leadTime)
        };
        database.Suppliers.Add(supplier);
        await database.SaveChangesAsync(cancellationToken);
        return ToItem(supplier);
    }

    public async Task<SupplierItem?> UpdateSupplierAsync(SupplierItem supplier, CancellationToken cancellationToken = default)
    {
        var current = await database.Suppliers.FirstOrDefaultAsync(item => item.SupplierId == supplier.SupplierId, cancellationToken);
        if (current is null)
        {
            return null;
        }

        current.CompanyName = supplier.Name.Trim();
        current.Category = Normalize(supplier.Category);
        current.ContactPerson = Normalize(supplier.ContactPerson);
        current.Email = Normalize(supplier.ContactEmail);
        current.ContactNumber = Normalize(supplier.Phone);
        current.Address = Normalize(supplier.Address);
        current.IsActive = !string.Equals(supplier.Status, "Inactive", StringComparison.OrdinalIgnoreCase);
        current.PaymentTerms = Normalize(supplier.PaymentTerms);
        current.LeadTime = Normalize(supplier.LeadTime);
        await database.SaveChangesAsync(cancellationToken);
        return ToItem(current);
    }

    public async Task<bool> DeleteSupplierAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var supplier = await database.Suppliers.FirstOrDefaultAsync(item => item.SupplierId == supplierId, cancellationToken);
        if (supplier is null || !supplier.IsActive)
        {
            return false;
        }

        supplier.IsActive = false;
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static SupplierItem ToItem(Supplier supplier) => new()
    {
        SupplierId = supplier.SupplierId,
        Name = supplier.CompanyName,
        Category = supplier.Category ?? string.Empty,
        ContactPerson = supplier.ContactPerson ?? string.Empty,
        ContactEmail = supplier.Email ?? string.Empty,
        Phone = supplier.ContactNumber ?? string.Empty,
        Address = supplier.Address ?? string.Empty,
        Status = supplier.IsActive ? "Active" : "Inactive",
        PaymentTerms = supplier.PaymentTerms ?? string.Empty,
        LeadTime = supplier.LeadTime ?? string.Empty
    };
}

public sealed class SupplierItem
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string PaymentTerms { get; set; } = "Net 30";
    public string LeadTime { get; set; } = "3-5 Days";
}
