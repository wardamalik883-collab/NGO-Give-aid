using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Configure Entity Framework Core with SQL Server
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================================
// Configure ASP.NET Core Identity
// ============================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password policy — reasonable for an NGO demo site
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Account lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Simplified for student project
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure authentication cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Register role and data seeding services
builder.Services.AddScoped<RoleSeedService>();
builder.Services.AddScoped<DataSeedService>();

// Register image upload service for NGO/Partner logo management (Task 10)
builder.Services.AddScoped<ImageUploadService>();

// Register email service for inquiry notifications and admin replies (Task 12)
builder.Services.AddScoped<IEmailService, EmailService>();

// Register memory cache for idempotency protection (Task 13)
builder.Services.AddMemoryCache();

// Register simulated payment service for safe demo donations (Task 13)
builder.Services.AddScoped<ISimulatedPaymentService, SimulatedPaymentService>();

// Add MVC services to the container with global Antiforgery enforcement.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// ============================================================
// Seed roles, initial admin, and baseline data on startup
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var roleSeedService = scope.ServiceProvider.GetRequiredService<RoleSeedService>();
    await roleSeedService.SeedAsync();

    var dataSeedService = scope.ServiceProvider.GetRequiredService<DataSeedService>();
    await dataSeedService.SeedAsync();
}

// ============================================================
// Configure the HTTP request pipeline
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

// User-friendly re-execution for 404, 403, and other HTTP error status codes (Task 17)
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

// Standard HTTP Security Headers (Task 17)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseHttpsRedirection();
app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

