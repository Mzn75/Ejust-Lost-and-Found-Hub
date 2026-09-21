using EjustLostAndFoundHub.Data;
using EjustLostAndFoundHub.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

// Intialize the web application builder with default configurations and services
var builder = WebApplication.CreateBuilder(args);

// Configure Entity Framework to use SQL Server with the connection string from configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Redirect users if they are not authenticated or lack the required role
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

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

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Create Roles
    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 2. Hardcoded Admin
    string adminNationalId = "N01234567";

    if (await userManager.Users.FirstOrDefaultAsync(u => u.NationalId == adminNationalId) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = "admin_user", // Identity requires a UserName
            NationalId = adminNationalId
        };

        // Identity requires a dummy password to create the record
        await userManager.CreateAsync(admin, "Admin@123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // 3. Hardcoded User
    string testUserNationalId = "30012345678910";

    if (await userManager.Users.FirstOrDefaultAsync(u => u.NationalId == testUserNationalId) == null)
    {
        var student = new ApplicationUser
        {
            UserName = "test_student",
            NationalId = testUserNationalId
        };

        await userManager.CreateAsync(student, "Student@123!");
        await userManager.AddToRoleAsync(student, "User");
    }
}

// Start the application and listen for incoming HTTP requests
app.Run();
