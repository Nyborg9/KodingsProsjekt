using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;
using WebApplication2.API_Models;
using WebApplication2.Data;
using WebApplication2.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddHttpClient<MunicipalityFinderService>();
builder.Services.AddScoped<MunicipalityFinderService>();
builder.Services.AddAntiforgery();


builder.Services.AddControllersWithViews();
// Configure the database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(10, 5, 9)),
    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

// Setup Authentication
SetupAuthentication(builder);

var app = builder.Build();


// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.CanConnect();
        Console.WriteLine("Connection successful.");

        // Ensure the database is created
        await dbContext.Database.EnsureCreatedAsync();

        // Get pending migrations
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        // Apply any pending migrations
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Connection failed: {ex.Message}");
    }
}

// Create roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "User", "Caseworker" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Create user
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string email = "admin@admin.com";
    string password = "Admin123";

    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
            Console.WriteLine($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void SetupAuthentication(WebApplicationBuilder builder)
{
    // Setup for Authentication
    builder.Services.Configure<IdentityOptions>(options =>
    {
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;      
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
    });

    builder.Services
        .AddIdentity<ApplicationUser, IdentityRole>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
}