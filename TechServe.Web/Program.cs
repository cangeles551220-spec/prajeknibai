using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot"
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("TechServeCookie")
    .AddCookie("TechServeCookie", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "TechServe.Auth";
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<TechServeDatabase>();
builder.Services.AddScoped<TechServe.Web.Services.UserService>();
builder.Services.AddScoped<TechServe.Web.Services.IEmailSender, TechServe.Web.Services.SmtpEmailSender>();
builder.Services.AddScoped<TechServe.Web.Services.UserService>();
builder.Services.AddScoped<TechServe.Web.Services.ProductService>();
builder.Services.AddScoped<TechServe.Web.Services.SupplierService>();
builder.Services.AddScoped<TechServe.Web.Services.SalesService>();
builder.Services.AddScoped<TechServe.Web.Services.CustomerService>();
builder.Services.AddScoped<TechServe.Web.Services.RepairService>();
builder.Services.AddScoped<TechServe.Web.Services.DeviceService>();
builder.Services.AddScoped<TechServe.Web.Services.InventoryService>();
builder.Services.AddScoped<TechServe.Web.Services.ReportService>();
builder.Services.AddScoped<TechServe.Web.Services.BillingService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var userService = scope.ServiceProvider.GetRequiredService<TechServe.Web.Services.UserService>();
    await userService.EnsureSeedUsersAsync(CancellationToken.None);
    var repairService = scope.ServiceProvider.GetRequiredService<TechServe.Web.Services.RepairService>();
    await repairService.EnsureDemoRepairsAsync(CancellationToken.None);
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; object-src 'none'; base-uri 'self'; frame-ancestors 'none'";
    await next();
});

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/customers", async (TechServeDatabase database, CancellationToken cancellationToken) =>
    Results.Ok(await database.GetCustomersAsync(cancellationToken)))
    .RequireAuthorization(new AuthorizeAttribute { Roles = "ADMIN,MANAGER,STAFF,BILLING" });
app.MapGet("/api/technicians", async (ApplicationDbContext database, CancellationToken cancellationToken) =>
{
    var technicians = await database.Users
        .AsNoTracking()
        .Where(user => user.IsActive && user.Role == "TECHNICIAN")
        .Select(user => new
        {
            id = user.UserId,
            name = user.FullName,
            technicianId = database.Technicians
                .Where(technician => technician.FullName == user.FullName)
                .Select(technician => (int?)technician.TechnicianId)
                .FirstOrDefault(),
            specialty = database.Technicians
                .Where(technician => technician.FullName == user.FullName)
                .Select(technician => technician.Specialization)
                .FirstOrDefault(),
            currentJobs = database.RepairJobs.Count(job =>
                job.Technician != null &&
                job.Technician.FullName == user.FullName &&
                job.RepairStatus != "Completed" &&
                job.RepairStatus != "Cancelled"),
            completed = database.RepairJobs.Count(job =>
                job.Technician != null &&
                job.Technician.FullName == user.FullName &&
                job.RepairStatus == "Completed"),
            availability = database.RepairJobs.Any(job =>
                job.Technician != null &&
                job.Technician.FullName == user.FullName &&
                job.RepairStatus != "Completed" &&
                job.RepairStatus != "Cancelled")
                ? "Busy"
                : database.Technicians
                    .Where(technician => technician.FullName == user.FullName)
                    .Select(technician => technician.Availability)
                    .FirstOrDefault() ?? "Available",
            capacity = 5
        })
        .OrderBy(technician => technician.name)
        .ToListAsync(cancellationToken);

    return Results.Ok(technicians);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADMIN,MANAGER" });
app.MapPost("/api/customers", async (CustomerInput input, TechServeDatabase database, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Contact) ||
        string.IsNullOrWhiteSpace(input.Email))
    {
        return Results.BadRequest(new { error = "Name, contact, and email are required." });
    }
    return Results.Created("/api/customers", await database.AddCustomerAsync(input, cancellationToken));
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADMIN,MANAGER,STAFF,BILLING" });
app.MapPost("/api/customers/{customerId:int}/archive", async (int customerId, TechServeDatabase database, CancellationToken cancellationToken) =>
{
    var archived = await database.ArchiveCustomerAsync(customerId, cancellationToken);
    return archived ? Results.Ok(new { customerId, status = "Archived" }) : Results.NotFound(new { error = "Customer was not found or is already archived." });
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADMIN,MANAGER" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
