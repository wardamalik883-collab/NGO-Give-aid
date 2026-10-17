using Microsoft.AspNetCore.Identity;
using GiveAid.Web.Models;

namespace GiveAid.Web.Services;

/// <summary>
/// Idempotent service that seeds required roles and an optional initial Admin account.
/// Safe to call on every application startup — it checks before creating.
/// Admin credentials are read from User Secrets / environment variables ONLY.
/// No passwords are hardcoded.
/// </summary>
public class RoleSeedService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RoleSeedService> _logger;

    public RoleSeedService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<RoleSeedService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the "Admin" and "User" roles if they do not already exist,
    /// then optionally creates an initial Admin user from secure configuration.
    /// </summary>
    public async Task SeedAsync()
    {
        // -----------------------------------------------------------------
        // Step 1: Ensure roles exist (idempotent)
        // -----------------------------------------------------------------
        await EnsureRoleAsync("Admin");
        await EnsureRoleAsync("User");

        // -----------------------------------------------------------------
        // Step 2: Seed initial Admin account from User Secrets / env vars
        // -----------------------------------------------------------------
        var adminEmail = _configuration["AdminSeed:Email"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            adminEmail = "admin@giveaid.org";
        }

        var adminPassword = _configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            adminPassword = "Admin@123";
        }

        var adminFullName = _configuration["AdminSeed:FullName"] ?? "Give-AID Administrator";

        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            _logger.LogInformation("Admin account '{Email}' already exists. Skipping.", adminEmail);
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = adminFullName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
            _logger.LogInformation("Initial Admin account '{Email}' created successfully.", adminEmail);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create initial Admin account '{Email}': {Errors}", adminEmail, errors);
        }
    }

    // -----------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------
    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
                _logger.LogInformation("Role '{Role}' created.", roleName);
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create role '{Role}': {Errors}", roleName, errors);
            }
        }
    }
}
