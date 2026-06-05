using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_empresas_cuentas_cuenta_id",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.AlterColumn<Guid>(
                name: "cuenta_id",
                schema: "facturacion",
                table: "empresas",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "facturacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CuentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    ResponseBody = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_keys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_keys_ExpiresAt",
                schema: "facturacion",
                table: "idempotency_keys",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_keys_Key_CuentaId",
                schema: "facturacion",
                table: "idempotency_keys",
                columns: new[] { "Key", "CuentaId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_empresas_cuentas_cuenta_id",
                schema: "facturacion",
                table: "empresas",
                column: "cuenta_id",
                principalSchema: "facturacion",
                principalTable: "cuentas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_empresas_cuentas_cuenta_id",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.DropTable(
                name: "idempotency_keys",
                schema: "facturacion");

            migrationBuilder.AlterColumn<Guid>(
                name: "cuenta_id",
                schema: "facturacion",
                table: "empresas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_empresas_cuentas_cuenta_id",
                schema: "facturacion",
                table: "empresas",
                column: "cuenta_id",
                principalSchema: "facturacion",
                principalTable: "cuentas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
