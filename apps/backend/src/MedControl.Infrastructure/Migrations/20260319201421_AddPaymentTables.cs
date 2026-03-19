using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedControl.Migrations;

/// <inheritdoc />
public partial class AddPaymentTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "payments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                health_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                execution_date = table.Column<DateOnly>(type: "date", nullable: false),
                appointment_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                authorization_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                beneficiary_card = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                beneficiary_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                execution_location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                payment_location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                notes = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_payments", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "payment_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                procedure_id = table.Column<Guid>(type: "uuid", nullable: false),
                value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                notes = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_payment_items", x => x.id);
                table.ForeignKey(
                    name: "FK_payment_items_payments_payment_id",
                    column: x => x.payment_id,
                    principalTable: "payments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_payment_items_payment_id",
            table: "payment_items",
            column: "payment_id");

        migrationBuilder.CreateIndex(
            name: "ix_payment_items_procedure_id",
            table: "payment_items",
            column: "procedure_id");

        migrationBuilder.CreateIndex(
            name: "ix_payments_doctor_id",
            table: "payments",
            column: "doctor_id");

        migrationBuilder.CreateIndex(
            name: "ix_payments_health_plan_id",
            table: "payments",
            column: "health_plan_id");

        migrationBuilder.CreateIndex(
            name: "ix_payments_tenant_id",
            table: "payments",
            column: "tenant_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "payment_items");

        migrationBuilder.DropTable(
            name: "payments");
    }
}
