namespace TechServe.Web.Models;

public sealed class DeviceViewModel
{
    public int DeviceId { get; set; }
    public int CustomerId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Condition { get; set; }
    public string? Accessories { get; set; }
    public int RepairCount { get; set; }
}

public sealed class SaveDeviceRequest
{
    public int CustomerId { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Condition { get; set; }
    public string? Accessories { get; set; }
}

public sealed record DeviceOperationResult(bool Success, string? Error, DeviceViewModel? Device)
{
    public static DeviceOperationResult Failure(string error) => new(false, error, null);
    public static DeviceOperationResult Succeeded(DeviceViewModel device) => new(true, null, device);
}