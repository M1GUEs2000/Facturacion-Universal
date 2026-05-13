using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    public partial class SriRespuestaToText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE facturacion.facturas ALTER COLUMN sri_respuesta TYPE text USING sri_respuesta::text;");
            migrationBuilder.Sql("ALTER TABLE facturacion.notas_credito ALTER COLUMN sri_respuesta TYPE text USING sri_respuesta::text;");
            migrationBuilder.Sql("ALTER TABLE facturacion.retenciones ALTER COLUMN sri_respuesta TYPE text USING sri_respuesta::text;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE facturacion.facturas ALTER COLUMN sri_respuesta TYPE jsonb USING sri_respuesta::jsonb;");
            migrationBuilder.Sql("ALTER TABLE facturacion.notas_credito ALTER COLUMN sri_respuesta TYPE jsonb USING sri_respuesta::jsonb;");
            migrationBuilder.Sql("ALTER TABLE facturacion.retenciones ALTER COLUMN sri_respuesta TYPE jsonb USING sri_respuesta::jsonb;");
        }
    }
}
