using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedControl.Migrations;

/// <inheritdoc />
public partial class AddHealthPlans : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "health_plans",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                tiss_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_health_plans", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_health_plans_tenant_id",
            table: "health_plans",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_health_plans_tenant_tiss_code",
            table: "health_plans",
#pragma warning disable CA1861
            columns: new[] { "tenant_id", "tiss_code" },
#pragma warning restore CA1861
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "health_plans");
    }
}
