using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class MigrateEmpresaFilesToStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "certificado_p12",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "logo",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.AlterColumn<string>(
                name: "sri_respuesta",
                schema: "facturacion",
                table: "retenciones",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "sri_respuesta",
                schema: "facturacion",
                table: "notas_credito",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "sri_respuesta",
                schema: "facturacion",
                table: "facturas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "certificado_path",
                schema: "facturacion",
                table: "empresas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "logo_path",
                schema: "facturacion",
                table: "empresas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "certificado_path",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "logo_path",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.AlterColumn<string>(
                name: "sri_respuesta",
                schema: "facturacion",
                table: "retenciones",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "sri_respuesta",
                schema: "facturacion",
                table: "notas_credito",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "sri_respuesta",
                schema: "facturacion",
                table: "facturas",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "certificado_p12",
                schema: "facturacion",
                table: "empresas",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "logo",
                schema: "facturacion",
                table: "empresas",
                type: "bytea",
                nullable: true);
        }
    }
}
