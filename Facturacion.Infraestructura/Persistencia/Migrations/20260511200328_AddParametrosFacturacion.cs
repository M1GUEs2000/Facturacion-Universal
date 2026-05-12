using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddParametrosFacturacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parametros_facturacion",
                schema: "facturacion",
                columns: table => new
                {
                    empresa_ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    ambiente = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    tipo_emision = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    agente_retencion = table.Column<bool>(type: "boolean", nullable: false),
                    contribuyente_rimpe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cod_doc = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    estab = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    punto_emision = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    numero_digitos = table.Column<int>(type: "integer", nullable: false),
                    contribuyente_especial = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    obligado_contabilidad = table.Column<bool>(type: "boolean", nullable: false),
                    tipo_identificador_comprador = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    codigo_impuesto = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    codigo_porcentaje = table.Column<int>(type: "integer", nullable: false),
                    smtp_server = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    smtp_port = table.Column<int>(type: "integer", nullable: true),
                    smtp_user = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    smtp_pass = table.Column<string>(type: "text", nullable: true),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros_facturacion", x => x.empresa_ruc);
                    table.ForeignKey(
                        name: "FK_parametros_facturacion_empresas_empresa_ruc",
                        column: x => x.empresa_ruc,
                        principalSchema: "facturacion",
                        principalTable: "empresas",
                        principalColumn: "ruc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parametros_sri",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    tipo_comprobante = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    secuencial = table.Column<long>(type: "bigint", nullable: false),
                    codigo_numerico = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros_sri", x => x.id);
                    table.ForeignKey(
                        name: "FK_parametros_sri_empresas_empresa_ruc",
                        column: x => x.empresa_ruc,
                        principalSchema: "facturacion",
                        principalTable: "empresas",
                        principalColumn: "ruc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parametros_sri_empresa_ruc_tipo_comprobante",
                schema: "facturacion",
                table: "parametros_sri",
                columns: new[] { "empresa_ruc", "tipo_comprobante" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parametros_facturacion",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "parametros_sri",
                schema: "facturacion");
        }
    }
}
