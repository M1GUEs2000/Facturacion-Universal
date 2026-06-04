using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    public partial class MakeCuentaIdNullable : Migration
    {
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldNullable: true,
                oldType: "uuid");

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
