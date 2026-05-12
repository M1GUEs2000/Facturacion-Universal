using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddNumeroUsuariosToEmpresas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "numero_usuarios",
                schema: "facturacion",
                table: "empresas",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "numero_usuarios",
                schema: "facturacion",
                table: "empresas");
        }
    }
}
