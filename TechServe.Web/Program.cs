var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = Path.Combine("TechServe.Web", "wwwroot")
});
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/TechServe.Web/Views/{1}/{0}.cshtml");
    });
builder.Services.AddSingleton<TechServeDatabase>();
var app = builder.Build();
await app.Services.GetRequiredService<TechServeDatabase>().InitializeAsync(CancellationToken.None);

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
app.MapGet("/api/customers", async (TechServeDatabase database, CancellationToken cancellationToken) =>
    Results.Ok(await database.GetCustomersAsync(cancellationToken)));
app.MapPost("/api/customers", async (CustomerInput input, TechServeDatabase database, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Contact) ||
        string.IsNullOrWhiteSpace(input.Email))
    {
        return Results.BadRequest(new { error = "Name, contact, and email are required." });
    }
    return Results.Created("/api/customers", await database.AddCustomerAsync(input, cancellationToken));
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
