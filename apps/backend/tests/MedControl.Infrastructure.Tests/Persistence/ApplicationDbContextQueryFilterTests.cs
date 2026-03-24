using FluentAssertions;
using MedControl.Domain.Doctors;
using MedControl.Domain.HealthPlans;
using MedControl.Domain.Payments;
using MedControl.Domain.Procedures;
using MedControl.Domain.Tenants;
using MedControl.Infrastructure.Tests.Helpers;

namespace MedControl.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextQueryFilterTests(DbContextModelFixture fixture)
    : IClassFixture<DbContextModelFixture>
{
    // Quando o JWT não tem tenant_id (ex: login mobile sem switch-tenant),
    // o global query filter NÃO deve lançar InvalidOperationException ao ser avaliado.
    // Bug: currentUser.TenantId!.Value com TenantId=null lançava a exceção.

    [Theory]
    [InlineData(typeof(TenantMember))]
    [InlineData(typeof(DoctorProfile))]
    [InlineData(typeof(HealthPlan))]
    [InlineData(typeof(Procedure))]
    [InlineData(typeof(ProcedureImport))]
    [InlineData(typeof(Payment))]
    public void QueryFilter_QuandoTenantIdNulo_NaoDeveLancar(Type entityType)
    {
        var et = fixture.Model.FindEntityType(entityType)!;
        var queryFilter = et.GetDeclaredQueryFilters().FirstOrDefault();
        queryFilter.Should().NotBeNull();

        var compiled = queryFilter!.Expression!.Compile();
        var instance = Activator.CreateInstance(entityType, nonPublic: true)!;

        var act = () => compiled.DynamicInvoke(instance);

        act.Should().NotThrow<InvalidOperationException>(
            "o global query filter não deve chamar .Value em um Guid? nulo");
    }

    [Theory]
    [InlineData(typeof(TenantMember))]
    [InlineData(typeof(DoctorProfile))]
    [InlineData(typeof(HealthPlan))]
    [InlineData(typeof(Procedure))]
    [InlineData(typeof(ProcedureImport))]
    [InlineData(typeof(Payment))]
    public void QueryFilter_QuandoTenantIdNulo_DeveRetornarFalse(Type entityType)
    {
        var et = fixture.Model.FindEntityType(entityType)!;
        var queryFilter = et.GetDeclaredQueryFilters().FirstOrDefault();
        var compiled = queryFilter!.Expression!.Compile();
        var instance = Activator.CreateInstance(entityType, nonPublic: true)!;

        var result = (bool)compiled.DynamicInvoke(instance)!;

        result.Should().BeFalse("sem tenant no jwt, nenhum dado deve ser retornado");
    }
}
