using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Data;

internal static class DevelopmentAdminSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        var email = app.Configuration["DemoAdmin:Email"]?.Trim();
        var password = app.Configuration["DemoAdmin:Password"];

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
        {
            return;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
            !new EmailAddressAttribute().IsValid(email) ||
            !(email.EndsWith("@sakarya.edu.tr", StringComparison.OrdinalIgnoreCase) ||
              email.EndsWith("@ogr.sakarya.edu.tr", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("DemoAdmin için Sakarya Üniversitesi e-postası ve parola gereklidir.");
        }

        await using var scope = app.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await using var transaction = await database.Database.BeginTransactionAsync();
        var existingUser = await users.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            if (!await users.IsInRoleAsync(existingUser, RoleNames.Admin))
            {
                throw new InvalidOperationException("Demo admin e-postası mevcut bir kullanıcıya ait.");
            }

            return;
        }

        var admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

        // Dersin demo parolasını yalnızca bu hesapta kabul ediyorum.
        admin.PasswordHash = users.PasswordHasher.HashPassword(admin, password);
        var createResult = await users.CreateAsync(admin);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException("Demo admin hesabı oluşturulamadı.");
        }

        var roleResult = await users.AddToRoleAsync(admin, RoleNames.Admin);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException("Demo admin rolü atanamadı.");
        }

        await transaction.CommitAsync();
    }
}
