using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedControl.Migrations;

/// <inheritdoc />
public partial class AddProcedures : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "procedures",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_procedures", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_procedures_tenant_code",
            table: "procedures",
#pragma warning disable CA1861
            columns: new[] { "tenant_id", "code" },
#pragma warning restore CA1861
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_procedures_tenant_id",
            table: "procedures",
            column: "tenant_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "procedures");
    }
}
