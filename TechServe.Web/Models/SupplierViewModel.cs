namespace TechServe.Web.Models;

public sealed class SupplierViewModel
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
