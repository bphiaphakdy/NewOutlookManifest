
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStaticFiles();

// Serve files from Pages directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Pages")),
    RequestPath = "/Pages"
});

app.UseRouting();

app.MapControllers();

// Default landing page helps when browsing to the root
app.MapGet("/", () => Results.Text("<html><head><meta charset='utf-8'></head><body><h2>Fluent Weather Add-in Web Host</h2><p>Try <code>/Pages/TaskPane.html</code> or <code>/Pages/Commands.html</code>.</p></body></html>", "text/html"));

app.Run();
