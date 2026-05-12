using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CreateCuentasAndRefactorEmpresas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cuentas",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    plan = table.Column<string>(type: "text", nullable: false),
                    max_empresas = table.Column<int>(type: "integer", nullable: false),
                    max_usuarios = table.Column<int>(type: "integer", nullable: false),
                    fecha_expira = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas", x => x.id);
                });

            migrationBuilder.DropColumn(
                name: "numero_usuarios",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.AddColumn<Guid>(
                name: "cuenta_id",
                schema: "facturacion",
                table: "empresas",
                type: "uuid",
                nullable: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_empresas_cuentas_cuenta_id",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "cuenta_id",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.DropTable(
                name: "cuentas",
                schema: "facturacion");

            migrationBuilder.AddColumn<int>(
                name: "numero_usuarios",
                schema: "facturacion",
                table: "empresas",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }
    }
}
