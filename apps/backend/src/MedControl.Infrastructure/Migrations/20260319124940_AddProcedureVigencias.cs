using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedControl.Migrations;

/// <inheritdoc />
public partial class AddProcedureVigencias : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_procedures_tenant_code",
            table: "procedures");

        migrationBuilder.AddColumn<DateOnly>(
            name: "effective_from",
            table: "procedures",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(1, 1, 1));

        migrationBuilder.AddColumn<DateOnly>(
            name: "effective_to",
            table: "procedures",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "source",
            table: "procedures",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "procedure_imports",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                total_rows = table.Column<int>(type: "integer", nullable: false),
                imported_rows = table.Column<int>(type: "integer", nullable: false),
                skipped_rows = table.Column<int>(type: "integer", nullable: false),
                error_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_procedure_imports", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_procedures_tenant_code_effective_from",
            table: "procedures",
#pragma warning disable CA1861
            columns: new[] { "tenant_id", "code", "effective_from" },
#pragma warning restore CA1861
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_procedures_tenant_effective_dates",
            table: "procedures",
#pragma warning disable CA1861
            columns: new[] { "tenant_id", "effective_from", "effective_to" });
#pragma warning restore CA1861

        migrationBuilder.CreateIndex(
            name: "ix_procedure_imports_tenant_id",
            table: "procedure_imports",
            column: "tenant_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "procedure_imports");

        migrationBuilder.DropIndex(
            name: "ix_procedures_tenant_code_effective_from",
            table: "procedures");

        migrationBuilder.DropIndex(
            name: "ix_procedures_tenant_effective_dates",
            table: "procedures");

        migrationBuilder.DropColumn(
            name: "effective_from",
            table: "procedures");

        migrationBuilder.DropColumn(
            name: "effective_to",
            table: "procedures");

        migrationBuilder.DropColumn(
            name: "source",
            table: "procedures");

        migrationBuilder.CreateIndex(
            name: "ix_procedures_tenant_code",
            table: "procedures",
#pragma warning disable CA1861
            columns: new[] { "tenant_id", "code" },
#pragma warning restore CA1861
            unique: true);
    }
}
