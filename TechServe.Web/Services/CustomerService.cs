namespace TechServe.Web.Services;

public sealed class CustomerService
{
    private readonly IReadOnlyList<CustomerItem> customers =
    [
        new CustomerItem("Priya Nair", "CUS-0204", "Active"),
        new CustomerItem("Marcus Reed", "CUS-0203", "Returning"),
        new CustomerItem("Elena Cruz", "CUS-0202", "Active"),
        new CustomerItem("Jordan Lee", "CUS-0201", "Returning")
    ];

    public Task<IReadOnlyList<CustomerItem>> GetCustomersAsync()
    {
        return Task.FromResult(customers);
    }
}

public sealed record CustomerItem(string Name, string CustomerId, string Status);
