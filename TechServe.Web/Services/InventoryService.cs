using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;
using TechServe.Web.Models;

namespace TechServe.Web.Services;

public sealed class InventoryService
{
    private readonly ApplicationDbContext database;

    public InventoryService(ApplicationDbContext database)
    {
        this.database = database;
    }

    public async Task<IReadOnlyList<InventoryPartViewModel>> GetPartsAsync(CancellationToken cancellationToken = default)
    {
        return await database.Parts
            .AsNoTracking()
            .OrderBy(part => part.PartName)
            .Select(part => new InventoryPartViewModel
            {
                PartId = part.PartId,
                Name = part.PartName,
                Category = part.Category,
                Brand = part.Brand,
                Description = part.Description,
                Quantity = part.Quantity,
                ReorderLevel = part.ReorderLevel,
                UnitCost = part.UnitCost,
                SellingPrice = part.SellingPrice,
                SupplierId = part.SupplierId,
                Supplier = part.Supplier == null ? null : part.Supplier.CompanyName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, InventoryPartViewModel? Part)> CreatePartAsync(
        SaveInventoryPartRequest request, CancellationToken cancellationToken = default)
    {
        var error = await ValidateAsync(request, cancellationToken);
        if (error is not null) return (false, error, null);
        if (await IsDuplicateAsync(request, null, cancellationToken))
        {
            return (false, "A part with the same name, brand, and supplier already exists.", null);
        }

        var part = new Part
        {
            PartName = request.Name.Trim(),
            Category = Normalize(request.Category),
            Brand = Normalize(request.Brand),
            Description = Normalize(request.Description),
            Quantity = request.Quantity,
            ReorderLevel = request.ReorderLevel,
            UnitCost = request.UnitCost,
            SellingPrice = request.SellingPrice,
            SupplierId = request.SupplierId
        };
        database.Parts.Add(part);
        await database.SaveChangesAsync(cancellationToken);
        await database.Entry(part).Reference(item => item.Supplier).LoadAsync(cancellationToken);
        return (true, null, ToViewModel(part));
    }

    public async Task<(bool Success, string? Error, InventoryPartViewModel? Part)> UpdatePartAsync(
        int partId, SaveInventoryPartRequest request, CancellationToken cancellationToken = default)
    {
        var error = await ValidateAsync(request, cancellationToken);
        if (error is not null) return (false, error, null);
        var part = await database.Parts.Include(item => item.Supplier).FirstOrDefaultAsync(item => item.PartId == partId, cancellationToken);
        if (part is null) return (false, "Part was not found.", null);
        if (await IsDuplicateAsync(request, partId, cancellationToken))
        {
            return (false, "A part with the same name, brand, and supplier already exists.", null);
        }

        part.PartName = request.Name.Trim();
        part.Category = Normalize(request.Category);
        part.Brand = Normalize(request.Brand);
        part.Description = Normalize(request.Description);
        part.Quantity = request.Quantity;
        part.ReorderLevel = request.ReorderLevel;
        part.UnitCost = request.UnitCost;
        part.SellingPrice = request.SellingPrice;
        part.SupplierId = request.SupplierId;
        await database.SaveChangesAsync(cancellationToken);
        return (true, null, ToViewModel(part));
    }

    public async Task<(bool Success, string? Error, InventoryPartViewModel? Part)> AdjustStockAsync(
        int partId, string? direction, int quantity, int? supplierId, string? referenceNumber, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return (false, "Quantity must be greater than zero.", null);
        var normalizedDirection = direction?.Trim().ToUpperInvariant();
        if (normalizedDirection is not ("IN" or "OUT")) return (false, "Choose stock-in or stock-out.", null);

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var part = await database.Parts.Include(item => item.Supplier).FirstOrDefaultAsync(item => item.PartId == partId, cancellationToken);
        if (part is null) return (false, "Part was not found.", null);
        if (normalizedDirection == "OUT" && part.Quantity < quantity)
        {
            return (false, "Stock-out quantity exceeds available stock.", null);
        }

        if (normalizedDirection == "IN")
        {
            var resolvedSupplierId = supplierId ?? part.SupplierId;
            if (!resolvedSupplierId.HasValue || !await database.Suppliers.AnyAsync(supplier => supplier.SupplierId == resolvedSupplierId && supplier.IsActive, cancellationToken))
            {
                return (false, "Select an active supplier before recording stock-in.", null);
            }

            var unitCost = part.UnitCost;
            database.Purchases.Add(new Purchase
            {
                SupplierId = resolvedSupplierId.Value,
                PurchaseDate = DateTime.UtcNow.Date,
                ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber)
                    ? $"STOCK-{DateTime.UtcNow:yyyyMMddHHmmssfff}"
                    : referenceNumber.Trim(),
                TotalAmount = unitCost * quantity,
                PurchaseDetails =
                {
                    new PurchaseDetail { PartId = part.PartId, Quantity = quantity, UnitCost = unitCost }
                }
            });
        }

        part.Quantity += normalizedDirection == "IN" ? quantity : -quantity;
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (true, null, ToViewModel(part));
    }

    private async Task<string?> ValidateAsync(SaveInventoryPartRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 150)
            return "Part name is required and must be 150 characters or fewer.";
        if (request.Category?.Length > 80 || request.Brand?.Length > 80 || request.Description?.Length > 400)
            return "Part details exceed the allowed length.";
        if (request.Quantity < 0 || request.ReorderLevel < 0 || request.UnitCost < 0 || request.SellingPrice < 0)
            return "Stock and prices cannot be negative.";
        if (request.SupplierId.HasValue && !await database.Suppliers.AnyAsync(supplier => supplier.SupplierId == request.SupplierId && supplier.IsActive, cancellationToken))
            return "Select an active supplier.";
        return null;
    }

    private Task<bool> IsDuplicateAsync(SaveInventoryPartRequest request, int? excludingPartId, CancellationToken cancellationToken) =>
        database.Parts.AnyAsync(part => part.PartId != excludingPartId &&
            part.PartName == request.Name.Trim() && part.Brand == Normalize(request.Brand) && part.SupplierId == request.SupplierId,
            cancellationToken);

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static InventoryPartViewModel ToViewModel(Part part) => new()
    {
        PartId = part.PartId,
        Name = part.PartName,
        Category = part.Category,
        Brand = part.Brand,
        Description = part.Description,
        Quantity = part.Quantity,
        ReorderLevel = part.ReorderLevel,
        UnitCost = part.UnitCost,
        SellingPrice = part.SellingPrice,
        SupplierId = part.SupplierId,
        Supplier = part.Supplier?.CompanyName
    };
}