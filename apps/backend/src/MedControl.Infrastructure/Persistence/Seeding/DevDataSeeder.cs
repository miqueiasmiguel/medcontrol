using MedControl.Domain.Doctors;
using MedControl.Domain.HealthPlans;
using MedControl.Domain.Payments;
using MedControl.Domain.Procedures;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedControl.Infrastructure.Persistence.Seeding;

public sealed class DevDataSeeder(
    ApplicationDbContext context,
    ILogger<DevDataSeeder> logger)
{
    private static readonly Guid SeedTenantId = new("10000000-0000-0000-0000-000000000001");
    private static readonly Guid SeedGlobalAdminUserId = new("20000000-0000-0000-0000-000000000001");
    private static readonly Guid SeedTenantAdminUserId = new("20000000-0000-0000-0000-000000000002");
    private static readonly Guid SeedDoctorUserId = new("20000000-0000-0000-0000-000000000003");
    private static readonly Guid SeedDoctorProfileId = new("30000000-0000-0000-0000-000000000001");
    private static readonly Guid SeedHealthPlanId = new("40000000-0000-0000-0000-000000000001");
    private static readonly Guid SeedProcedureId = new("50000000-0000-0000-0000-000000000001");
    private static readonly Guid SeedPaymentId1 = new("60000000-0000-0000-0000-000000000001");
    private static readonly Guid SeedPaymentId2 = new("60000000-0000-0000-0000-000000000002");
    private static readonly Guid SeedPaymentId3 = new("60000000-0000-0000-0000-000000000003");
    private static readonly Guid SeedPaymentId4 = new("60000000-0000-0000-0000-000000000004");
    private static readonly Guid SeedPaymentId5 = new("60000000-0000-0000-0000-000000000005");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Running development data seed...");

        await SeedTenantAsync(ct);
        await SeedGlobalAdminUserAsync("miqueias.s.filho@gmail.com", "Miquéias Filho", SeedGlobalAdminUserId, ct);
        await SeedUserAsync("miqueias.o.branco@gmail.com", "Miquéias Branco", SeedTenantAdminUserId, ct);
        await SeedUserAsync("mariaclara.mr09@gmail.com", "Maria Clara", SeedDoctorUserId, ct);
        await SeedMembersAsync(ct);
        await SeedDoctorProfileAsync(ct);
        await SeedHealthPlanAsync(ct);
        await SeedProcedureAsync(ct);
        await SeedPaymentsAsync(ct);

        logger.LogInformation("Development seed complete.");
    }

    private async Task SeedTenantAsync(CancellationToken ct)
    {
        var existing = await context.Tenants
            .FirstOrDefaultAsync(t => t.Name == "MedControl Demo", ct);

        if (existing is not null)
        {
            logger.LogDebug("Tenant 'MedControl Demo' already exists ({Id})", existing.Id);
            return;
        }

        var result = Tenant.Create("MedControl Demo");
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create seed tenant: {result.Error.Description}");
        }

        var tenant = result.Value;
        context.Entry(tenant).Property(t => t.Id).CurrentValue = SeedTenantId;

        await context.Tenants.AddAsync(tenant, ct);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created seed tenant ({Id})", SeedTenantId);
    }

    private async Task SeedGlobalAdminUserAsync(string email, string displayName, Guid fixedId, CancellationToken ct)
    {
        var existing = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (existing is not null)
        {
            if (existing.GlobalRole != GlobalRole.Admin)
            {
                existing.SetGlobalRole(GlobalRole.Admin);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Updated user '{Email}' to GlobalRole.Admin", email);
            }
            else
            {
                logger.LogDebug("Global admin user '{Email}' already exists ({Id})", email, existing.Id);
            }

            return;
        }

        var result = User.Create(email, displayName);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create seed global admin '{email}': {result.Error.Description}");
        }

        var user = result.Value;
        user.SetGlobalRole(GlobalRole.Admin);
        context.Entry(user).Property(u => u.Id).CurrentValue = fixedId;

        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created global admin user '{Email}' ({Id})", email, fixedId);
    }

    private async Task SeedUserAsync(string email, string displayName, Guid fixedId, CancellationToken ct)
    {
        var existing = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (existing is not null)
        {
            logger.LogDebug("User '{Email}' already exists ({Id})", email, existing.Id);
            return;
        }

        var result = User.Create(email, displayName);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create seed user '{email}': {result.Error.Description}");
        }

        var user = result.Value;
        context.Entry(user).Property(u => u.Id).CurrentValue = fixedId;

        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created seed user '{Email}' ({Id})", email, fixedId);
    }

    private async Task SeedMembersAsync(CancellationToken ct)
    {
        var adminExists = await context.TenantMembers
            .IgnoreQueryFilters()
            .AnyAsync(m => m.TenantId == SeedTenantId && m.UserId == SeedTenantAdminUserId, ct);

        var doctorExists = await context.TenantMembers
            .IgnoreQueryFilters()
            .AnyAsync(m => m.TenantId == SeedTenantId && m.UserId == SeedDoctorUserId, ct);

        if (adminExists && doctorExists)
        {
            logger.LogDebug("Seed members already exist for tenant {TenantId}", SeedTenantId);
            return;
        }

        var tenant = await context.Tenants
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == SeedTenantId, ct)
            ?? throw new InvalidOperationException("Seed tenant not found.");

        if (!adminExists)
        {
            var addAdmin = tenant.AddMember(SeedTenantAdminUserId, TenantRole.Admin);
            if (addAdmin.IsFailure)
            {
                throw new InvalidOperationException($"Failed to add admin member: {addAdmin.Error.Description}");
            }
        }

        if (!doctorExists)
        {
            var addDoctor = tenant.AddMember(SeedDoctorUserId, TenantRole.Doctor);
            if (addDoctor.IsFailure)
            {
                throw new InvalidOperationException($"Failed to add doctor member: {addDoctor.Error.Description}");
            }
        }

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Seeded members for tenant {TenantId}", SeedTenantId);
    }

    private async Task SeedDoctorProfileAsync(CancellationToken ct)
    {
        var existing = await context.DoctorProfiles
            .IgnoreQueryFilters()
            .AnyAsync(d => d.TenantId == SeedTenantId && d.Crm == "123456", ct);

        if (existing)
        {
            logger.LogDebug("Doctor profile CRM 123456 already exists");
            return;
        }

        var result = DoctorProfile.Create(SeedTenantId, "Maria Clara", "123456", "SP", "Cardiologia");
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create seed doctor profile: {result.Error.Description}");
        }

        var profile = result.Value;
        context.Entry(profile).Property(p => p.Id).CurrentValue = SeedDoctorProfileId;

        var link = profile.LinkUser(SeedDoctorUserId);
        if (link.IsFailure)
        {
            throw new InvalidOperationException($"Failed to link doctor user: {link.Error.Description}");
        }

        await context.DoctorProfiles.AddAsync(profile, ct);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created seed doctor profile ({Id})", SeedDoctorProfileId);
    }

    private async Task SeedHealthPlanAsync(CancellationToken ct)
    {
        var existing = await context.HealthPlans
            .IgnoreQueryFilters()
            .AnyAsync(hp => hp.TenantId == SeedTenantId && hp.TissCode == "000701", ct);

        if (existing)
        {
            logger.LogDebug("HealthPlan TissCode '000701' already exists");
            return;
        }

        var result = HealthPlan.Create(SeedTenantId, "Unimed Nacional", "000701");
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create seed health plan: {result.Error.Description}");
        }

        var hp = result.Value;
        context.Entry(hp).Property(h => h.Id).CurrentValue = SeedHealthPlanId;

        await context.HealthPlans.AddAsync(hp, ct);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created seed health plan ({Id})", SeedHealthPlanId);
    }

    private async Task SeedProcedureAsync(CancellationToken ct)
    {
        var existing = await context.Procedures
            .IgnoreQueryFilters()
            .AnyAsync(p => p.TenantId == SeedTenantId && p.Code == "10101012", ct);

        if (existing)
        {
            logger.LogDebug("Procedure code '10101012' already exists");
            return;
        }

        var result = Procedure.Create(
            SeedTenantId,
            "10101012",
            "Consulta em consultório (no horário normal ou preestabelecido)",
            150.00m,
            new DateOnly(2025, 1, 1));

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create seed procedure: {result.Error.Description}");
        }

        var procedure = result.Value;
        context.Entry(procedure).Property(p => p.Id).CurrentValue = SeedProcedureId;

        await context.Procedures.AddAsync(procedure, ct);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created seed procedure ({Id})", SeedProcedureId);
    }

    private async Task SeedPaymentsAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var defs = new[]
        {
            (Id: SeedPaymentId1, Appt: "ATD-001", Name: "João Silva",     Date: today.AddDays(-30), Status: (PaymentStatus?)PaymentStatus.Paid),
            (Id: SeedPaymentId2, Appt: "ATD-002", Name: "Maria Santos",   Date: today.AddDays(-20), Status: (PaymentStatus?)null),
            (Id: SeedPaymentId3, Appt: "ATD-003", Name: "Carlos Oliveira",Date: today.AddDays(-15), Status: (PaymentStatus?)PaymentStatus.Refused),
            (Id: SeedPaymentId4, Appt: "ATD-004", Name: "Ana Lima",       Date: today.AddDays(-10), Status: (PaymentStatus?)null),
            (Id: SeedPaymentId5, Appt: "ATD-005", Name: "Pedro Costa",    Date: today.AddDays(-5),  Status: (PaymentStatus?)PaymentStatus.Paid),
        };

        foreach (var def in defs)
        {
            var exists = await context.Payments
                .IgnoreQueryFilters()
                .AnyAsync(p => p.TenantId == SeedTenantId && p.AppointmentNumber == def.Appt, ct);

            if (exists)
            {
                logger.LogDebug("Payment '{Appt}' already exists", def.Appt);
                continue;
            }

            var result = Payment.Create(
                SeedTenantId,
                doctorId: SeedDoctorUserId,
                healthPlanId: SeedHealthPlanId,
                executionDate: def.Date,
                appointmentNumber: def.Appt,
                authorizationCode: null,
                beneficiaryCard: "000000001",
                beneficiaryName: def.Name,
                executionLocation: "Consultório MedControl",
                paymentLocation: "São Paulo - SP",
                notes: null,
                items: [(SeedProcedureId, 150.00m)]);

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Failed to create seed payment '{def.Appt}': {result.Error.Description}");
            }

            var payment = result.Value;
            context.Entry(payment).Property(p => p.Id).CurrentValue = def.Id;

            if (def.Status.HasValue)
            {
                payment.Items[0].UpdateStatus(def.Status.Value);
            }

            await context.Payments.AddAsync(payment, ct);
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Created seed payment '{Appt}' ({Id})", def.Appt, def.Id);
        }
    }
}
