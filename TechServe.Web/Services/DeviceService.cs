using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;
using TechServe.Web.Models;

namespace TechServe.Web.Services;

public sealed class DeviceService
{
    private readonly ApplicationDbContext database;

    public DeviceService(ApplicationDbContext database)
    {
        this.database = database;
    }

    public async Task<IReadOnlyList<DeviceViewModel>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await database.Devices
            .AsNoTracking()
            .OrderBy(device => device.DeviceId)
            .Select(device => new DeviceViewModel
            {
                DeviceId = device.DeviceId,
                CustomerId = device.CustomerId,
                Customer = device.Customer.FullName,
                DeviceType = device.DeviceType,
                Brand = device.Brand,
                Model = device.Model,
                SerialNumber = device.SerialNumber,
                OperatingSystem = device.OperatingSystem,
                Condition = device.DeviceCondition,
                Accessories = device.Accessories,
                RepairCount = device.RepairJobs.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DeviceOperationResult> CreateAsync(SaveDeviceRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateAsync(request, cancellationToken);
        if (validationError is not null)
        {
            return DeviceOperationResult.Failure(validationError);
        }

        if (!string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            var duplicate = await database.Devices
                .AnyAsync(device => device.CustomerId == request.CustomerId && device.SerialNumber == request.SerialNumber.Trim(), cancellationToken);
            if (duplicate)
            {
                return DeviceOperationResult.Failure("That serial number is already registered for this customer.");
            }
        }

        var device = new Device
        {
            CustomerId = request.CustomerId,
            DeviceType = request.DeviceType.Trim(),
            Brand = Normalize(request.Brand),
            Model = Normalize(request.Model),
            SerialNumber = Normalize(request.SerialNumber),
            OperatingSystem = Normalize(request.OperatingSystem),
            DeviceCondition = Normalize(request.Condition) ?? "Good",
            Accessories = Normalize(request.Accessories)
        };
        database.Devices.Add(device);
        await database.SaveChangesAsync(cancellationToken);

        await database.Entry(device).Reference(item => item.Customer).LoadAsync(cancellationToken);
        return DeviceOperationResult.Succeeded(ToViewModel(device));
    }

    public async Task<DeviceOperationResult> UpdateAsync(int deviceId, SaveDeviceRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateAsync(request, cancellationToken);
        if (validationError is not null)
        {
            return DeviceOperationResult.Failure(validationError);
        }

        var device = await database.Devices
            .Include(item => item.Customer)
            .Include(item => item.RepairJobs)
            .FirstOrDefaultAsync(item => item.DeviceId == deviceId, cancellationToken);
        if (device is null)
        {
            return DeviceOperationResult.Failure("Device was not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.SerialNumber) &&
            await database.Devices.AnyAsync(item => item.DeviceId != deviceId && item.CustomerId == request.CustomerId && item.SerialNumber == request.SerialNumber.Trim(), cancellationToken))
        {
            return DeviceOperationResult.Failure("That serial number is already registered for this customer.");
        }

        device.CustomerId = request.CustomerId;
        device.DeviceType = request.DeviceType.Trim();
        device.Brand = Normalize(request.Brand);
        device.Model = Normalize(request.Model);
        device.SerialNumber = Normalize(request.SerialNumber);
        device.OperatingSystem = Normalize(request.OperatingSystem);
        device.DeviceCondition = Normalize(request.Condition) ?? "Good";
        device.Accessories = Normalize(request.Accessories);
        await database.SaveChangesAsync(cancellationToken);

        return DeviceOperationResult.Succeeded(ToViewModel(device));
    }

    private async Task<string?> ValidateAsync(SaveDeviceRequest request, CancellationToken cancellationToken)
    {
        if (request.CustomerId <= 0 || string.IsNullOrWhiteSpace(request.DeviceType) || request.DeviceType.Trim().Length > 60)
        {
            return "Select a customer and provide a device type.";
        }
        if (request.Brand?.Length > 60 || request.Model?.Length > 80 || request.SerialNumber?.Length > 120 ||
            request.OperatingSystem?.Length > 80 || request.Condition?.Length > 40 || request.Accessories?.Length > 300)
        {
            return "One or more device fields exceed the allowed length.";
        }
        return await database.Customers.AnyAsync(customer => customer.CustomerId == request.CustomerId, cancellationToken)
            ? null
            : "Customer was not found.";
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DeviceViewModel ToViewModel(Device device) => new()
    {
        DeviceId = device.DeviceId,
        CustomerId = device.CustomerId,
        Customer = device.Customer.FullName,
        DeviceType = device.DeviceType,
        Brand = device.Brand,
        Model = device.Model,
        SerialNumber = device.SerialNumber,
        OperatingSystem = device.OperatingSystem,
        Condition = device.DeviceCondition,
        Accessories = device.Accessories,
        RepairCount = device.RepairJobs.Count
    };
}