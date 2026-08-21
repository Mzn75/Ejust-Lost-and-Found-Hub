using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.DataProtection;
using EjustLostAndFoundHub.Data;

// Intialize the web application builder with default configurations and services
var builder = WebApplication.CreateBuilder(args);

// Configure Entity Framework to use SQL Server with the connection string from configuration
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container for MVC Architecture
builder.Services.AddControllersWithViews();

// Add Razor Pages support
builder.Services.AddRazorPages();

// Configure Data Protection to save encryption keys to a physical folder named "Keys"
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Keys")));

// Build the application based on all the services and configurations defined above
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Catch 404 errors and redirect to a custom error page
app.UseStatusCodePagesWithReExecute("/Home/Error404");

// Enable HTTPS redirection and routing for incoming requests
app.UseHttpsRedirection();

// Enable the routing system
app.UseRouting();

// Enable Authentication
app.UseAuthentication();

// Enable Authorization
app.UseAuthorization();

// Optimize and serve static files (like CSS, JS, images) from the wwwroot folder
app.MapStaticAssets();
app.UseStaticFiles();

// Define the default route for MVC controllers, specifying the default controller and action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Start the application and listen for incoming HTTP requests
app.Run();
