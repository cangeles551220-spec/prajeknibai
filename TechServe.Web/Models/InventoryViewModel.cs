namespace TechServe.Web.Models;

public sealed class InventoryPartViewModel
{
    public int PartId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public int? SupplierId { get; set; }
    public string? Supplier { get; set; }
}

public sealed class SaveInventoryPartRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; } = 5;
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public int? SupplierId { get; set; }
}

public sealed class AdjustInventoryRequest
{
    public string Direction { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int? SupplierId { get; set; }
    public string? ReferenceNumber { get; set; }
}