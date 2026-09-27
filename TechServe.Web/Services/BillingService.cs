using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;

namespace TechServe.Web.Services;

public sealed class BillingService
{
    private readonly ApplicationDbContext db;

    public BillingService(ApplicationDbContext db)
    {
        this.db = db;
    }

    public async Task<IReadOnlyList<BillingInvoice>> GetInvoicesAsync(CancellationToken cancellationToken)
    {
        return await db.Invoices
            .AsNoTracking()
            .OrderByDescending(invoice => invoice.InvoiceDate)
            .Select(invoice => new BillingInvoice(
                invoice.InvoiceId,
                invoice.RepairJobId,
                invoice.RepairJob.Customer.FullName,
                invoice.InvoiceDate,
                invoice.LaborCost + invoice.PartsCost + invoice.OtherCharges - invoice.Discount,
                invoice.Payments.Sum(payment => (decimal?)payment.Amount) ?? 0m,
                invoice.PaymentStatus))
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentResult> RecordPaymentAsync(int invoiceId, decimal amount, string paymentMethod, CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return PaymentResult.Failure("Payment amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            return PaymentResult.Failure("Payment method is required.");
        }

        var invoice = await db.Invoices
            .Include(item => item.RepairJob)
                .ThenInclude(job => job.Customer)
            .Include(item => item.Payments)
            .SingleOrDefaultAsync(item => item.InvoiceId == invoiceId, cancellationToken);
        if (invoice == null)
        {
            return PaymentResult.Failure("Invoice was not found.");
        }

        var total = invoice.LaborCost + invoice.PartsCost + invoice.OtherCharges - invoice.Discount;
        var paid = invoice.Payments.Sum(payment => payment.Amount);
        var balance = total - paid;
        if (amount > balance)
        {
            return PaymentResult.Failure($"Payment cannot exceed the outstanding balance of {balance:₱#,##0.00}.");
        }

        invoice.Payments.Add(new Payment
        {
            Amount = amount,
            PaymentMethod = paymentMethod.Trim(),
            PaidAt = DateTime.UtcNow
        });
        invoice.PaymentStatus = paid + amount >= total ? "Paid" : "Partially Paid";
        await db.SaveChangesAsync(cancellationToken);

        return PaymentResult.Succeeded(new BillingInvoice(
            invoice.InvoiceId,
            invoice.RepairJobId,
            invoice.RepairJob.Customer.FullName,
            invoice.InvoiceDate,
            total,
            paid + amount,
            invoice.PaymentStatus), amount, paymentMethod.Trim());
    }
}

public sealed record BillingInvoice(
    int InvoiceId,
    int RepairJobId,
    string Customer,
    DateTime InvoiceDate,
    decimal Amount,
    decimal Paid,
    string Status);

public sealed record PaymentInput(decimal Amount, string PaymentMethod);

public sealed record PaymentResult(bool Success, string? Error, BillingInvoice? Invoice, decimal PaymentAmount, string? PaymentMethod)
{
    public static PaymentResult Failure(string error) => new(false, error, null, 0m, null);

    public static PaymentResult Succeeded(BillingInvoice invoice, decimal amount, string paymentMethod) => new(true, null, invoice, amount, paymentMethod);
}