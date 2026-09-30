using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;
using TechServe.Web.Models;

namespace TechServe.Web.Services;

public sealed class RepairService
{
    public static readonly string[] Statuses =
    [
        "Received", "Diagnosis", "In Repair", "Testing", "Ready for Pickup", "Completed"
    ];

    private readonly ApplicationDbContext? database;
    private readonly IReadOnlyList<RepairTicket> tickets =
    [
        new RepairTicket("RJ-2024-001", "Priya Nair", "Dell XPS 15", "In Repair"),
        new RepairTicket("RJ-2024-002", "Marcus Reed", "MacBook Pro 14\"", "Testing"),
        new RepairTicket("RJ-2024-003", "Elena Cruz", "HP EliteBook 840", "Waiting for Parts"),
        new RepairTicket("RJ-2024-004", "Jordan Lee", "Lenovo ThinkPad T14", "Diagnosing")
    ];

    public RepairService()
    {
        database = null;
    }

    public RepairService(ApplicationDbContext database)
    {
        this.database = database;
    }

    public async Task<IReadOnlyList<RepairTicket>> GetTicketsAsync(CancellationToken cancellationToken = default)
    {
        var tracking = await GetTrackingAsync(cancellationToken);
        return tracking.Select(item => new RepairTicket(item.JobId, item.Customer, item.Device, item.Status)).ToList();
    }

    public async Task<IReadOnlyList<RepairTrackingViewModel>> GetTrackingAsync(CancellationToken cancellationToken = default)
    {
        if (database is null)
        {
            return tickets.Select((ticket, index) => new RepairTrackingViewModel
            {
                RepairJobId = index + 1,
                JobId = ticket.JobId,
                Customer = ticket.Customer,
                Device = ticket.Device,
                Status = ticket.Status,
                DateReceived = DateTime.UtcNow.Date,
                History = [new RepairStatusHistoryViewModel { Status = ticket.Status, ChangedAt = DateTime.UtcNow, ChangedBy = "System" }]
            }).ToList();
        }

        var jobs = await database.RepairJobs
            .AsNoTracking()
            .Include(job => job.Customer)
            .Include(job => job.Device)
            .Include(job => job.Technician)
            .Include(job => job.RepairParts)
            .ThenInclude(repairPart => repairPart.Part)
            .Include(job => job.RepairStatusHistories)
            .ThenInclude(history => history.ChangedByUser)
            .OrderByDescending(job => job.RepairJobId)
            .ToListAsync(cancellationToken);

        return jobs.Select(ToViewModel).ToList();
    }

    public async Task<RepairOptionsResponse> GetRepairOptionsAsync(CancellationToken cancellationToken = default)
    {
        if (database is null)
        {
            return new RepairOptionsResponse([], [], []);
        }

        var customers = await database.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.FullName)
            .Select(customer => new RepairCustomerOption(customer.CustomerId, customer.FullName))
            .ToListAsync(cancellationToken);
        var devices = await database.Devices
            .AsNoTracking()
            .Include(device => device.Customer)
            .OrderBy(device => device.Customer.FullName)
            .ThenBy(device => device.DeviceType)
            .Select(device => new RepairDeviceOption(
                device.DeviceId,
                device.CustomerId,
                (device.Brand ?? "") + " " + (device.Model ?? "") + " " + device.DeviceType + " - " + device.Customer.FullName))
            .ToListAsync(cancellationToken);
        var technicians = await database.Technicians
            .AsNoTracking()
            .Where(technician => database.Users.Any(user =>
                user.IsActive && user.Role == "TECHNICIAN" && user.FullName == technician.FullName))
            .OrderBy(technician => technician.FullName)
            .Select(technician => new RepairTechnicianOption(technician.TechnicianId, technician.FullName))
            .ToListAsync(cancellationToken);

        return new RepairOptionsResponse(customers, devices, technicians);
    }

    public async Task<(bool Success, string? Error, RepairTrackingViewModel? Repair)> CreateRepairAsync(
        CreateRepairJobRequest request,
        string? username,
        CancellationToken cancellationToken = default)
    {
        if (database is null)
        {
            return (false, "Repair database is unavailable.", null);
        }
        if (request.CustomerId <= 0 || request.DeviceId <= 0 ||
            string.IsNullOrWhiteSpace(request.Complaint) || request.Complaint.Trim().Length > 500)
        {
            return (false, "Select a customer and device and enter a repair complaint.", null);
        }
        if (!new[] { "Normal", "High", "Urgent" }.Contains(request.Priority, StringComparer.OrdinalIgnoreCase))
        {
            return (false, "Select a valid repair priority.", null);
        }
        if (request.EstimatedCost is < 0)
        {
            return (false, "Estimated cost cannot be negative.", null);
        }

        var customerExists = await database.Customers.AnyAsync(customer => customer.CustomerId == request.CustomerId, cancellationToken);
        var device = await database.Devices.FirstOrDefaultAsync(device => device.DeviceId == request.DeviceId, cancellationToken);
        if (!customerExists || device is null || device.CustomerId != request.CustomerId)
        {
            return (false, "The selected device does not belong to the selected customer.", null);
        }

        Technician? technician = null;
        if (request.TechnicianId.HasValue)
        {
            technician = await FindActiveTechnicianAsync(request.TechnicianId.Value, cancellationToken);
            if (technician is null)
            {
                return (false, "Select an active technician account.", null);
            }
        }

        var changedBy = string.IsNullOrWhiteSpace(username)
            ? null
            : await database.Users.Where(user => user.Username == username).Select(user => (int?)user.UserId).FirstOrDefaultAsync(cancellationToken);
        var job = new RepairJob
        {
            CustomerId = request.CustomerId,
            DeviceId = request.DeviceId,
            TechnicianId = technician?.TechnicianId,
            Complaint = request.Complaint.Trim(),
            DateReceived = DateTime.UtcNow.Date,
            ExpectedCompletionDate = request.ExpectedCompletionDate?.Date,
            EstimatedCost = request.EstimatedCost,
            Priority = request.Priority,
            RepairStatus = "Pending"
        };
        database.RepairJobs.Add(job);
        database.RepairStatusHistories.Add(new RepairStatusHistory
        {
            RepairJob = job,
            Status = job.RepairStatus,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            Note = "Repair job created"
        });
        await database.SaveChangesAsync(cancellationToken);

        var savedJob = await LoadRepairAsync(job.RepairJobId, cancellationToken);
        return (true, null, savedJob is null ? null : ToViewModel(savedJob));
    }

    public async Task<(bool Success, string? Error, RepairTrackingViewModel? Repair)> AssignTechnicianAsync(
        int repairJobId,
        int technicianId,
        string? username,
        CancellationToken cancellationToken = default)
    {
        if (database is null)
        {
            return (false, "Repair database is unavailable.", null);
        }
        var job = await database.RepairJobs.FirstOrDefaultAsync(item => item.RepairJobId == repairJobId, cancellationToken);
        if (job is null)
        {
            return (false, "Repair job was not found.", null);
        }
        if (job.RepairStatus is "Completed" or "Cancelled")
        {
            return (false, "Completed or cancelled repairs cannot be reassigned.", null);
        }
        var technician = await FindActiveTechnicianAsync(technicianId, cancellationToken);
        if (technician is null)
        {
            return (false, "Select an active technician account.", null);
        }
        if (job.TechnicianId == technician.TechnicianId)
        {
            var unchanged = await LoadRepairAsync(job.RepairJobId, cancellationToken);
            return (true, null, unchanged is null ? null : ToViewModel(unchanged));
        }

        var changedBy = string.IsNullOrWhiteSpace(username)
            ? null
            : await database.Users.Where(user => user.Username == username).Select(user => (int?)user.UserId).FirstOrDefaultAsync(cancellationToken);
        job.TechnicianId = technician.TechnicianId;
        database.RepairStatusHistories.Add(new RepairStatusHistory
        {
            RepairJobId = job.RepairJobId,
            Status = job.RepairStatus,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            Note = $"Assigned to {technician.FullName}"
        });
        await database.SaveChangesAsync(cancellationToken);

        var savedJob = await LoadRepairAsync(job.RepairJobId, cancellationToken);
        return (true, null, savedJob is null ? null : ToViewModel(savedJob));
    }

    private Task<Technician?> FindActiveTechnicianAsync(int technicianId, CancellationToken cancellationToken) =>
        database!.Technicians.FirstOrDefaultAsync(technician =>
            technician.TechnicianId == technicianId &&
            database.Users.Any(user => user.IsActive && user.Role == "TECHNICIAN" && user.FullName == technician.FullName),
            cancellationToken);

    private Task<RepairJob?> LoadRepairAsync(int repairJobId, CancellationToken cancellationToken) => database!.RepairJobs
        .AsNoTracking()
        .Include(job => job.Customer)
        .Include(job => job.Device)
        .Include(job => job.Technician)
        .Include(job => job.RepairParts)
        .ThenInclude(repairPart => repairPart.Part)
        .Include(job => job.RepairStatusHistories)
        .ThenInclude(history => history.ChangedByUser)
        .FirstOrDefaultAsync(job => job.RepairJobId == repairJobId, cancellationToken);

    public Task<(bool Success, string? Error, RepairTrackingViewModel? Repair)> UpdateStatusAsync(
        int repairJobId, string? status, string? note, string? username, CancellationToken cancellationToken = default) =>
        UpdateWorkAsync(repairJobId, new UpdateRepairStatusViewModel { Status = status ?? string.Empty, Note = note }, username, null, cancellationToken);

    public async Task<IReadOnlyList<AvailableRepairPartViewModel>> GetAvailablePartsAsync(CancellationToken cancellationToken = default)
    {
        if (database is null)
        {
            return [];
        }

        return await database.Parts
            .AsNoTracking()
            .Where(part => part.Quantity > 0)
            .OrderBy(part => part.PartName)
            .Select(part => new AvailableRepairPartViewModel
            {
                PartId = part.PartId,
                PartName = part.PartName,
                QuantityAvailable = part.Quantity
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, RepairTrackingViewModel? Repair)> UpdateWorkAsync(
        int repairJobId,
        UpdateRepairStatusViewModel update,
        string? username,
        string? requiredTechnicianName,
        CancellationToken cancellationToken = default)
    {
        if (database is null)
        {
            return (false, "Repair database is unavailable.", null);
        }

        var normalizedStatus = update.Status?.Trim();
        if (normalizedStatus is null || !Statuses.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
        {
            return (false, "Invalid repair status.", null);
        }
        if (update.Diagnosis?.Length > 1000 || update.RepairDescription?.Length > 1000 || update.Note?.Length > 500)
        {
            return (false, "Repair details exceed the allowed length.", null);
        }
        if (update.PartId.HasValue != update.PartQuantity.HasValue || update.PartQuantity is <= 0)
        {
            return (false, "Select a part and a valid quantity.", null);
        }

        var job = await database.RepairJobs
            .Include(item => item.Customer)
            .Include(item => item.Device)
            .Include(item => item.Technician)
            .Include(item => item.RepairParts)
            .ThenInclude(repairPart => repairPart.Part)
            .Include(item => item.RepairStatusHistories)
            .ThenInclude(history => history.ChangedByUser)
            .FirstOrDefaultAsync(item => item.RepairJobId == repairJobId, cancellationToken);
        if (job is null)
        {
            return (false, "Repair job was not found.", null);
        }
        if (!string.IsNullOrWhiteSpace(requiredTechnicianName) &&
            !string.Equals(job.Technician?.FullName, requiredTechnicianName, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "This repair is not assigned to you.", null);
        }

        var canonicalStatus = Statuses.First(item => string.Equals(item, normalizedStatus, StringComparison.OrdinalIgnoreCase));
        var statusChanged = !string.Equals(job.RepairStatus, canonicalStatus, StringComparison.OrdinalIgnoreCase);
        Part? selectedPart = null;
        if (update.PartId.HasValue)
        {
            selectedPart = await database.Parts.FirstOrDefaultAsync(part => part.PartId == update.PartId.Value, cancellationToken);
            if (selectedPart is null || selectedPart.Quantity < update.PartQuantity)
            {
                return (false, "The selected part is unavailable in that quantity.", null);
            }
        }

        var changedBy = string.IsNullOrWhiteSpace(username)
            ? null
            : await database.Users.Where(user => user.Username == username).Select(user => (int?)user.UserId).FirstOrDefaultAsync(cancellationToken);

        if (update.Diagnosis is not null)
        {
            job.Diagnosis = string.IsNullOrWhiteSpace(update.Diagnosis) ? null : update.Diagnosis.Trim();
        }
        if (update.RepairDescription is not null)
        {
            job.RepairDescription = string.IsNullOrWhiteSpace(update.RepairDescription) ? null : update.RepairDescription.Trim();
        }
        if (statusChanged)
        {
            job.RepairStatus = canonicalStatus;
            if (canonicalStatus == "Completed")
            {
                job.CompletionDate = DateTime.UtcNow;
            }
        }

        if (statusChanged || !string.IsNullOrWhiteSpace(update.Note))
        {
            database.RepairStatusHistories.Add(new RepairStatusHistory
            {
                RepairJobId = job.RepairJobId,
                Status = canonicalStatus,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                Note = string.IsNullOrWhiteSpace(update.Note) ? null : update.Note.Trim()
            });
        }

        if (selectedPart is not null)
        {
            var repairPart = job.RepairParts.FirstOrDefault(item => item.PartId == selectedPart.PartId);
            if (repairPart is null)
            {
                job.RepairParts.Add(new RepairPart
                {
                    RepairJobId = job.RepairJobId,
                    PartId = selectedPart.PartId,
                    Quantity = update.PartQuantity!.Value,
                    UnitPrice = selectedPart.SellingPrice,
                    Part = selectedPart
                });
            }
            else
            {
                repairPart.Quantity += update.PartQuantity!.Value;
            }

            selectedPart.Quantity -= update.PartQuantity!.Value;
        }

        await database.SaveChangesAsync(cancellationToken);

        await database.Entry(job).Collection(item => item.RepairStatusHistories).Query().Include(history => history.ChangedByUser).LoadAsync(cancellationToken);
        return (true, null, ToViewModel(job));
    }

    public async Task EnsureDemoRepairsAsync(CancellationToken cancellationToken = default)
    {
        if (database is null)
        {
            return;
        }

        var adminId = await database.Users.Where(user => user.Username == "admin").Select(user => (int?)user.UserId).FirstOrDefaultAsync(cancellationToken);

        if (!await database.RepairJobs.AnyAsync(cancellationToken))
        {
            var seed = new[]
            {
                ("Maria Santos", "Lenovo ThinkPad E15", "Carlo Mendoza", "In Repair", "Laptop will not power on", 3500m),
                ("Jose Reyes", "HP Pavilion TP01", "Diana Aquino", "Testing", "Screen replacement", 1800m),
                ("Ahn Villanueva", "MacBook Air M2", "Carlo Mendoza", "In Repair", "Battery drains quickly", 6500m),
                ("Liza Fernandez", "Dell Inspiron 3891", "Ben Torres", "Diagnosis", "Keyboard not responding", 2200m),
                ("Rafael Cruz", "ASUS VivoBook 15", "Diana Aquino", "Received", "Random restarts", 900m)
            };

            foreach (var item in seed)
            {
                var customer = await database.Customers.FirstOrDefaultAsync(customer => customer.FullName == item.Item1, cancellationToken);
                if (customer is null)
                {
                    customer = new Customer { FullName = item.Item1, CustomerStatus = "Active", CreatedAt = DateTime.UtcNow };
                    database.Customers.Add(customer);
                    await database.SaveChangesAsync(cancellationToken);
                }

                var device = new Device { CustomerId = customer.CustomerId, DeviceType = item.Item2, DeviceCondition = "Fair" };
                database.Devices.Add(device);
                await database.SaveChangesAsync(cancellationToken);
                var technician = await database.Technicians.FirstOrDefaultAsync(value => value.FullName == item.Item3, cancellationToken);
                if (technician is null)
                {
                    technician = new Technician { FullName = item.Item3, Availability = "Available" };
                    database.Technicians.Add(technician);
                    await database.SaveChangesAsync(cancellationToken);
                }

                var job = new RepairJob
                {
                    CustomerId = customer.CustomerId,
                    DeviceId = device.DeviceId,
                    TechnicianId = technician.TechnicianId,
                    Complaint = item.Item5,
                    DateReceived = DateTime.UtcNow.Date.AddDays(-2),
                    ExpectedCompletionDate = DateTime.UtcNow.Date.AddDays(3),
                    Priority = "Normal",
                    RepairStatus = item.Item4,
                    EstimatedCost = item.Item6
                };
                database.RepairJobs.Add(job);
                await database.SaveChangesAsync(cancellationToken);

                database.Invoices.Add(new Invoice
                {
                    RepairJobId = job.RepairJobId,
                    LaborCost = item.Item6,
                    PartsCost = 0m,
                    OtherCharges = 0m,
                    Discount = 0m,
                    InvoiceDate = DateTime.UtcNow.Date.AddDays(-1),
                    PaymentStatus = "Unpaid"
                });
                await database.SaveChangesAsync(cancellationToken);

                database.RepairStatusHistories.Add(new RepairStatusHistory { RepairJobId = job.RepairJobId, Status = item.Item4, ChangedBy = adminId, ChangedAt = DateTime.UtcNow.AddMinutes(-30) });
                await database.SaveChangesAsync(cancellationToken);
            }
        }

        var jobsWithoutInvoices = await database.RepairJobs
            .Where(job => !database.Invoices.Any(invoice => invoice.RepairJobId == job.RepairJobId))
            .ToListAsync(cancellationToken);

        foreach (var job in jobsWithoutInvoices)
        {
            var amount = job.ActualCost ?? job.EstimatedCost ?? 0m;
            if (amount <= 0m)
            {
                continue;
            }

            database.Invoices.Add(new Invoice
            {
                RepairJobId = job.RepairJobId,
                LaborCost = amount,
                PartsCost = 0m,
                OtherCharges = 0m,
                Discount = 0m,
                InvoiceDate = job.DateReceived.Date,
                PaymentStatus = "Unpaid"
            });
        }

        if (jobsWithoutInvoices.Count > 0)
        {
            await database.SaveChangesAsync(cancellationToken);
        }
    }

    private static RepairTrackingViewModel ToViewModel(RepairJob job)
    {
        return new RepairTrackingViewModel
        {
            RepairJobId = job.RepairJobId,
            JobId = $"RJ-{job.RepairJobId:0000}",
            Complaint = job.Complaint,
            Customer = job.Customer.FullName,
            CustomerContact = job.Customer.ContactNumber,
            CustomerEmail = job.Customer.Email,
            Device = job.Device.DeviceType,
            DeviceBrand = job.Device.Brand,
            DeviceModel = job.Device.Model,
            DeviceSerialNumber = job.Device.SerialNumber,
            DeviceOperatingSystem = job.Device.OperatingSystem,
            TechnicianId = job.TechnicianId,
            Technician = job.Technician?.FullName ?? "Unassigned",
            Diagnosis = job.Diagnosis,
            RepairDescription = job.RepairDescription,
            Status = job.RepairStatus,
            Priority = job.Priority,
            DateReceived = job.DateReceived,
            ExpectedCompletionDate = job.ExpectedCompletionDate,
            Amount = job.ActualCost ?? job.EstimatedCost ?? 0,
            History = job.RepairStatusHistories.OrderBy(history => history.ChangedAt).Select(history => new RepairStatusHistoryViewModel
            {
                Status = history.Status,
                ChangedAt = history.ChangedAt,
                ChangedBy = history.ChangedByUser?.FullName ?? "System",
                Note = history.Note
            }).ToList(),
            PartsUsed = job.RepairParts.Select(repairPart => new RepairPartUsedViewModel
            {
                PartId = repairPart.PartId,
                PartName = repairPart.Part.PartName,
                Quantity = repairPart.Quantity
            }).ToList()
        };
    }
}

public sealed record RepairTicket(string JobId, string Customer, string Device, string Status);
