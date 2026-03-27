using MedControl.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedControl.Infrastructure.Persistence.Seeding;

public sealed class ProdDataSeeder(
    ApplicationDbContext context,
    ILogger<ProdDataSeeder> logger)
{
    private static readonly Guid GlobalAdminUserId = new("20000000-0000-0000-0000-000000000001");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Running production data seed...");
        await EnsureGlobalAdminAsync("miqueias.s.filho@gmail.com", "Miquéias Filho", ct);
        logger.LogInformation("Production seed complete.");
    }

    private async Task EnsureGlobalAdminAsync(string email, string displayName, CancellationToken ct)
    {
        var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (existing is not null)
        {
            if (existing.GlobalRole != GlobalRole.Admin)
            {
                existing.SetGlobalRole(GlobalRole.Admin);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Updated user '{Email}' to GlobalRole.Admin", email);
            }

            return;
        }

        var user = User.Create(email, displayName).Value;
        user.SetGlobalRole(GlobalRole.Admin);
        context.Entry(user).Property(u => u.Id).CurrentValue = GlobalAdminUserId;
        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created global admin '{Email}' ({Id})", email, GlobalAdminUserId);
    }
}
