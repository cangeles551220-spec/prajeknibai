namespace TechServe.Web.Models;

public sealed class RepairViewModel
{
    public string JobId { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public sealed class RepairTrackingViewModel
{
    public int RepairJobId { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string Complaint { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string? CustomerContact { get; set; }
    public string? CustomerEmail { get; set; }
    public string Device { get; set; } = string.Empty;
    public string? DeviceBrand { get; set; }
    public string? DeviceModel { get; set; }
    public string? DeviceSerialNumber { get; set; }
    public string? DeviceOperatingSystem { get; set; }
    public int? TechnicianId { get; set; }
    public string Technician { get; set; } = "Unassigned";
    public string? Diagnosis { get; set; }
    public string? RepairDescription { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public DateTime DateReceived { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public decimal Amount { get; set; }
    public List<RepairStatusHistoryViewModel> History { get; set; } = [];
    public List<RepairPartUsedViewModel> PartsUsed { get; set; } = [];
}

public sealed class CreateRepairJobRequest
{
    public int CustomerId { get; set; }
    public int DeviceId { get; set; }
    public int? TechnicianId { get; set; }
    public string Complaint { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public DateTime? ExpectedCompletionDate { get; set; }
    public decimal? EstimatedCost { get; set; }
}

public sealed class AssignRepairTechnicianRequest
{
    public int TechnicianId { get; set; }
}

public sealed record RepairOptionsResponse(
    IReadOnlyList<RepairCustomerOption> Customers,
    IReadOnlyList<RepairDeviceOption> Devices,
    IReadOnlyList<RepairTechnicianOption> Technicians);

public sealed record RepairCustomerOption(int CustomerId, string Name);

public sealed record RepairDeviceOption(int DeviceId, int CustomerId, string Label);

public sealed record RepairTechnicianOption(int TechnicianId, string Name);

public sealed class RepairPartUsedViewModel
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public sealed class AvailableRepairPartViewModel
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
}

public sealed class RepairStatusHistoryViewModel
{
    public string Status { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = "System";
    public string? Note { get; set; }
}

public sealed class UpdateRepairStatusViewModel
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? Diagnosis { get; set; }
    public string? RepairDescription { get; set; }
    public int? PartId { get; set; }
    public int? PartQuantity { get; set; }
}
