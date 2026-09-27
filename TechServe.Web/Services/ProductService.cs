namespace TechServe.Web.Services;

public sealed class ProductService
{
    private readonly IReadOnlyList<ProductItem> catalog =
    [
        new ProductItem("Laptop", "Core repair device"),
        new ProductItem("Monitor", "Display unit"),
        new ProductItem("Keyboard", "Input device"),
        new ProductItem("Desktop", "Tower workstation"),
        new ProductItem("Printer", "Office output device")
    ];

    public Task<IReadOnlyList<ProductItem>> GetProductsAsync()
    {
        return Task.FromResult(catalog);
    }
}

public sealed record ProductItem(string Name, string Description);
