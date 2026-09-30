using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;

namespace TechServe.Web.Services;

public sealed class CustomerService
{
    private readonly ApplicationDbContext database;

    public CustomerService(ApplicationDbContext database)
    {
        this.database = database;
    }

    public async Task<IReadOnlyList<CustomerItem>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await database.Customers
            .AsNoTracking()
            .OrderByDescending(customer => customer.CustomerId)
            .Select(customer => new CustomerItem(
                customer.FullName,
                $"CUS-{customer.CustomerId:0000}",
                customer.CustomerStatus))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerItem> AddCustomerAsync(string fullName, string contactNumber, string email, string? address = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Customer name is required.", nameof(fullName));
        }

        var normalizedName = fullName.Trim();
        var normalizedContact = string.IsNullOrWhiteSpace(contactNumber) ? null : contactNumber.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        var normalizedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();

        var hasEmail = normalizedEmail != null;
        var hasContact = normalizedContact != null;

        var existingCustomer = await database.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer =>
                (hasEmail && customer.Email != null && customer.Email.Trim().Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)) ||
                (hasContact && customer.ContactNumber != null && customer.ContactNumber.Trim().Equals(normalizedContact, StringComparison.OrdinalIgnoreCase)) ||
                (hasContact && customer.FullName.Trim().Equals(normalizedName, StringComparison.OrdinalIgnoreCase) &&
                 customer.ContactNumber != null && customer.ContactNumber.Trim().Equals(normalizedContact, StringComparison.OrdinalIgnoreCase)),
                cancellationToken);

        if (existingCustomer is not null)
        {
            throw new InvalidOperationException("A customer with the same contact number or email already exists.");
        }

        var customer = new Customer
        {
            FullName = normalizedName,
            ContactNumber = normalizedContact,
            Email = normalizedEmail,
            Address = normalizedAddress,
            CustomerStatus = "Active",
            CreatedAt = DateTime.UtcNow
        };

        database.Customers.Add(customer);
        await database.SaveChangesAsync(cancellationToken);

        return new CustomerItem(customer.FullName, $"CUS-{customer.CustomerId:0000}", customer.CustomerStatus);
    }
}

public sealed record CustomerItem(string Name, string CustomerId, string Status);
