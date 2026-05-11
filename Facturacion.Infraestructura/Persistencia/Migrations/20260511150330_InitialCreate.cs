using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "facturacion");

            migrationBuilder.CreateTable(
                name: "empresas",
                schema: "facturacion",
                columns: table => new
                {
                    ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    dir_matriz = table.Column<string>(type: "text", nullable: false),
                    nombre_comercial = table.Column<string>(type: "text", nullable: true),
                    obligado_contabilidad = table.Column<bool>(type: "boolean", nullable: false),
                    certificado_p12 = table.Column<byte[]>(type: "bytea", nullable: false),
                    cert_password = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.ruc);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    ambiente = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    estab = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    pto_emi = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    secuencial = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    clave_acceso = table.Column<string>(type: "character varying(49)", maxLength: 49, nullable: false),
                    fecha_emision = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo_identificacion_comprador = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    identificacion_comprador = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    razon_social_comprador = table.Column<string>(type: "text", nullable: false),
                    direccion_comprador = table.Column<string>(type: "text", nullable: true),
                    dir_establecimiento = table.Column<string>(type: "text", nullable: true),
                    total_sin_impuestos = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    total_descuento = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    base_imponible_ice = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    valor_ice = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    base_imponible_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    propina = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    importe_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    guia_remision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    formas_pago = table.Column<string>(type: "jsonb", nullable: false),
                    info_adicional = table.Column<string>(type: "jsonb", nullable: false),
                    estado_sri = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado_correo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_autorizacion = table.Column<string>(type: "character varying(49)", maxLength: 49, nullable: true),
                    fecha_autorizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sri_respuesta = table.Column<string>(type: "jsonb", nullable: true),
                    xml_firmado_path = table.Column<string>(type: "text", nullable: true),
                    xml_autorizado_path = table.Column<string>(type: "text", nullable: true),
                    pdf_path = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notas_credito",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    ambiente = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    estab = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    pto_emi = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    secuencial = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    clave_acceso = table.Column<string>(type: "character varying(49)", maxLength: 49, nullable: false),
                    fecha_emision = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo_identificacion_comprador = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    identificacion_comprador = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    razon_social_comprador = table.Column<string>(type: "text", nullable: false),
                    direccion_comprador = table.Column<string>(type: "text", nullable: true),
                    dir_establecimiento = table.Column<string>(type: "text", nullable: true),
                    doc_modificado_tipo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    doc_modificado_numero = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    doc_modificado_fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    doc_modificado_clave_acceso = table.Column<string>(type: "character varying(49)", maxLength: 49, nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    total_sin_impuestos = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    total_descuento = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    base_imponible_ice = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    valor_ice = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    base_imponible_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_modificacion = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    info_adicional = table.Column<string>(type: "jsonb", nullable: false),
                    estado_sri = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado_correo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_autorizacion = table.Column<string>(type: "character varying(49)", maxLength: 49, nullable: true),
                    fecha_autorizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sri_respuesta = table.Column<string>(type: "jsonb", nullable: true),
                    xml_firmado_path = table.Column<string>(type: "text", nullable: true),
                    xml_autorizado_path = table.Column<string>(type: "text", nullable: true),
                    pdf_path = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notas_credito", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "retenciones",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    ambiente = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    estab = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    pto_emi = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    secuencial = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    clave_acceso = table.Column<string>(type: "character varying(49)", maxLength: 49, nullable: false),
                    fecha_emision = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo_identificacion_sujeto = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    identificacion_sujeto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    razon_social_sujeto = table.Column<string>(type: "text", nullable: false),
                    direccion_sujeto = table.Column<string>(type: "text", nullable: true),
                    periodo_fiscal = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    total_base_imponible = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    total_retencion_renta = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    total_retencion_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    total_retenido = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    info_adicional = table.Column<string>(type: "jsonb", nullable: false),
                    estado_sri = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado_correo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_autorizacion = table.Column<string>(type: "character varying(49)", maxLength: 49, nullable: true),
                    fecha_autorizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sri_respuesta = table.Column<string>(type: "jsonb", nullable: true),
                    xml_firmado_path = table.Column<string>(type: "text", nullable: true),
                    xml_autorizado_path = table.Column<string>(type: "text", nullable: true),
                    pdf_path = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_retenciones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "facturas_detalle",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    codigo_principal = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    codigo_auxiliar = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    descuento = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    precio_total_sin_impuesto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ice_codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ice_tarifa = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ice_base = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    ice_valor = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    iva_codigo = table.Column<int>(type: "integer", nullable: false),
                    iva_tarifa = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    iva_base = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    iva_valor = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas_detalle", x => x.id);
                    table.ForeignKey(
                        name: "FK_facturas_detalle_facturas_factura_id",
                        column: x => x.factura_id,
                        principalSchema: "facturacion",
                        principalTable: "facturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notas_credito_detalle",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nota_credito_id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    codigo_principal = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    codigo_auxiliar = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    descuento = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    precio_total_sin_impuesto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ice_codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ice_tarifa = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ice_base = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    ice_valor = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    iva_codigo = table.Column<int>(type: "integer", nullable: false),
                    iva_tarifa = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    iva_base = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    iva_valor = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notas_credito_detalle", x => x.id);
                    table.ForeignKey(
                        name: "FK_notas_credito_detalle_notas_credito_nota_credito_id",
                        column: x => x.nota_credito_id,
                        principalSchema: "facturacion",
                        principalTable: "notas_credito",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "retenciones_detalle",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    retencion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    codigo_impuesto = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    codigo_retencion = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    base_imponible = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    porcentaje_retener = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    valor_retenido = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    cod_doc_sustento = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    num_doc_sustento = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    fecha_emision_doc_sustento = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_retenciones_detalle", x => x.id);
                    table.ForeignKey(
                        name: "FK_retenciones_detalle_retenciones_retencion_id",
                        column: x => x.retencion_id,
                        principalSchema: "facturacion",
                        principalTable: "retenciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_facturas_clave_acceso",
                schema: "facturacion",
                table: "facturas",
                column: "clave_acceso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facturas_empresa_ruc_estado_sri_fecha_emision",
                schema: "facturacion",
                table: "facturas",
                columns: new[] { "empresa_ruc", "estado_sri", "fecha_emision" });

            migrationBuilder.CreateIndex(
                name: "IX_facturas_detalle_factura_id",
                schema: "facturacion",
                table: "facturas_detalle",
                column: "factura_id");

            migrationBuilder.CreateIndex(
                name: "IX_notas_credito_clave_acceso",
                schema: "facturacion",
                table: "notas_credito",
                column: "clave_acceso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notas_credito_empresa_ruc_estado_sri_fecha_emision",
                schema: "facturacion",
                table: "notas_credito",
                columns: new[] { "empresa_ruc", "estado_sri", "fecha_emision" });

            migrationBuilder.CreateIndex(
                name: "IX_notas_credito_detalle_nota_credito_id",
                schema: "facturacion",
                table: "notas_credito_detalle",
                column: "nota_credito_id");

            migrationBuilder.CreateIndex(
                name: "IX_retenciones_clave_acceso",
                schema: "facturacion",
                table: "retenciones",
                column: "clave_acceso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_retenciones_empresa_ruc_estado_sri_fecha_emision",
                schema: "facturacion",
                table: "retenciones",
                columns: new[] { "empresa_ruc", "estado_sri", "fecha_emision" });

            migrationBuilder.CreateIndex(
                name: "IX_retenciones_detalle_retencion_id",
                schema: "facturacion",
                table: "retenciones_detalle",
                column: "retencion_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "empresas",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "facturas_detalle",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "notas_credito_detalle",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "retenciones_detalle",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "facturas",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "notas_credito",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "retenciones",
                schema: "facturacion");
        }
    }
}
