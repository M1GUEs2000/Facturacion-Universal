using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    public partial class AddSecuencialActivoUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Índice único parcial: solo aplica a documentos activos (no Pendiente ni NoAutorizado).
            // Permite que el mismo número secuencial exista si el documento previo falló antes
            // de llegar al SRI o fue rechazado, pero lo bloquea en cuanto el SRI lo recibe.
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX uq_facturas_secuencial_activo
                ON facturacion.facturas(empresa_ruc, estab, pto_emi, secuencial, ambiente)
                WHERE estado_sri NOT IN ('PENDIENTE', 'NO_AUTORIZADO');
            ");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX uq_notas_credito_secuencial_activo
                ON facturacion.notas_credito(empresa_ruc, estab, pto_emi, secuencial, ambiente)
                WHERE estado_sri NOT IN ('PENDIENTE', 'NO_AUTORIZADO');
            ");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX uq_retenciones_secuencial_activo
                ON facturacion.retenciones(empresa_ruc, estab, pto_emi, secuencial, ambiente)
                WHERE estado_sri NOT IN ('PENDIENTE', 'NO_AUTORIZADO');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.uq_facturas_secuencial_activo;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.uq_notas_credito_secuencial_activo;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.uq_retenciones_secuencial_activo;");
        }
    }
}
