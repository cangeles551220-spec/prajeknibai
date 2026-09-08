using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var webRoot = Path.Combine(app.Environment.ContentRootPath, "TechServe.Web", "wwwroot");
var fileProvider = new PhysicalFileProvider(webRoot);

app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
app.MapFallback(() => Results.File(Path.Combine(webRoot, "index.html"), "text/html"));
app.Run();
