using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class RemoveParametrosCorreoYCamposNoUsados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "numero_digitos",
                schema: "facturacion",
                table: "parametros_facturacion");

            migrationBuilder.DropColumn(
                name: "smtp_pass",
                schema: "facturacion",
                table: "parametros_facturacion");

            migrationBuilder.DropColumn(
                name: "smtp_port",
                schema: "facturacion",
                table: "parametros_facturacion");

            migrationBuilder.DropColumn(
                name: "smtp_server",
                schema: "facturacion",
                table: "parametros_facturacion");

            migrationBuilder.DropColumn(
                name: "smtp_user",
                schema: "facturacion",
                table: "parametros_facturacion");

            migrationBuilder.DropColumn(
                name: "tipo_identificador_comprador",
                schema: "facturacion",
                table: "parametros_facturacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "numero_digitos",
                schema: "facturacion",
                table: "parametros_facturacion",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "smtp_pass",
                schema: "facturacion",
                table: "parametros_facturacion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "smtp_port",
                schema: "facturacion",
                table: "parametros_facturacion",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "smtp_server",
                schema: "facturacion",
                table: "parametros_facturacion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "smtp_user",
                schema: "facturacion",
                table: "parametros_facturacion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_identificador_comprador",
                schema: "facturacion",
                table: "parametros_facturacion",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }
    }
}
