using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedControl.Migrations;

/// <inheritdoc />
public partial class AddDoctorProfile : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "doctor_profiles",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                crm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                council_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                specialty = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_doctor_profiles", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_doctor_profiles_tenant_crm_state",
            table: "doctor_profiles",
#pragma warning disable CA1861
            columns: new[] { "tenant_id", "crm", "council_state" },
#pragma warning restore CA1861
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_doctor_profiles_tenant_id",
            table: "doctor_profiles",
            column: "tenant_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "doctor_profiles");
    }
}
