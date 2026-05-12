using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "logo",
                schema: "facturacion",
                table: "empresas",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logo_content_type",
                schema: "facturacion",
                table: "empresas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "logo",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "logo_content_type",
                schema: "facturacion",
                table: "empresas");
        }
    }
}
