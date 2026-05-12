using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class MoveEmpresaFiscalFieldsToParametros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_parametros_sri_empresas_empresa_ruc",
                schema: "facturacion",
                table: "parametros_sri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_parametros_sri",
                schema: "facturacion",
                table: "parametros_sri");

            migrationBuilder.DropColumn(
                name: "cod_doc",
                schema: "facturacion",
                table: "parametros_facturacion");

            migrationBuilder.DropColumn(
                name: "obligado_contabilidad",
                schema: "facturacion",
                table: "empresas");

            migrationBuilder.RenameTable(
                name: "parametros_sri",
                schema: "facturacion",
                newName: "secuenciales_sri",
                newSchema: "facturacion");

            migrationBuilder.RenameIndex(
                name: "IX_parametros_sri_empresa_ruc_tipo_comprobante",
                schema: "facturacion",
                table: "secuenciales_sri",
                newName: "IX_secuenciales_sri_empresa_ruc_tipo_comprobante");

            migrationBuilder.AddPrimaryKey(
                name: "PK_secuenciales_sri",
                schema: "facturacion",
                table: "secuenciales_sri",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_secuenciales_sri_empresas_empresa_ruc",
                schema: "facturacion",
                table: "secuenciales_sri",
                column: "empresa_ruc",
                principalSchema: "facturacion",
                principalTable: "empresas",
                principalColumn: "ruc",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_secuenciales_sri_empresas_empresa_ruc",
                schema: "facturacion",
                table: "secuenciales_sri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_secuenciales_sri",
                schema: "facturacion",
                table: "secuenciales_sri");

            migrationBuilder.RenameTable(
                name: "secuenciales_sri",
                schema: "facturacion",
                newName: "parametros_sri",
                newSchema: "facturacion");

            migrationBuilder.RenameIndex(
                name: "IX_secuenciales_sri_empresa_ruc_tipo_comprobante",
                schema: "facturacion",
                table: "parametros_sri",
                newName: "IX_parametros_sri_empresa_ruc_tipo_comprobante");

            migrationBuilder.AddColumn<string>(
                name: "cod_doc",
                schema: "facturacion",
                table: "parametros_facturacion",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "obligado_contabilidad",
                schema: "facturacion",
                table: "empresas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_parametros_sri",
                schema: "facturacion",
                table: "parametros_sri",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_parametros_sri_empresas_empresa_ruc",
                schema: "facturacion",
                table: "parametros_sri",
                column: "empresa_ruc",
                principalSchema: "facturacion",
                principalTable: "empresas",
                principalColumn: "ruc",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
