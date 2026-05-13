using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Servicios;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facturacion.Infraestructura.Servicios.Pdf;

public class ServicioPdf : IServicioPdf
{
    static ServicioPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ── Factura ──────────────────────────────────────────────────────────────

    public Task<ErrorOr<byte[]>> GenerarRideFacturaAsync(
        Factura factura, Empresa empresa, ParametrosFacturacion? parametros,
        byte[]? logoBytes, CancellationToken ct = default)
    {
        var bytes = Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(t => t.FontSize(8));

            page.Content().Column(col =>
            {
                col.Spacing(4);

                col.Item().Element(c => RenderEncabezado(c, empresa, parametros, logoBytes,
                    "FACTURA", $"{factura.Estab}-{factura.PtoEmi}-{factura.Secuencial}",
                    factura.NumeroAutorizacion, factura.FechaAutorizacion,
                    factura.Ambiente, factura.ClaveAcceso));

                col.Item().Element(c => RenderComprador(c,
                    factura.TipoIdentificacionComprador, factura.IdentificacionComprador,
                    factura.RazonSocialComprador, factura.DireccionComprador,
                    factura.FechaEmision, factura.GuiaRemision));

                col.Item().Element(c => RenderTablaItems(c, MapDetalleFactura(factura.Detalle)));

                col.Item().Element(c => RenderTotalesFactura(c, factura));

                if (factura.FormasPago.Count > 0)
                    col.Item().Element(c => RenderFormasPago(c, factura.FormasPago));

                if (factura.InfoAdicional.Count > 0)
                    col.Item().Element(c => RenderInfoAdicional(c, factura.InfoAdicional));
            });
        })).GeneratePdf();

        return Task.FromResult<ErrorOr<byte[]>>(bytes);
    }

    // ── Nota de Crédito ───────────────────────────────────────────────────────

    public Task<ErrorOr<byte[]>> GenerarRideNotaCreditoAsync(
        NotaCredito nc, Empresa empresa, ParametrosFacturacion? parametros,
        byte[]? logoBytes, CancellationToken ct = default)
    {
        var bytes = Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(t => t.FontSize(8));

            page.Content().Column(col =>
            {
                col.Spacing(4);

                col.Item().Element(c => RenderEncabezado(c, empresa, parametros, logoBytes,
                    "NOTA DE CRÉDITO", $"{nc.Estab}-{nc.PtoEmi}-{nc.Secuencial}",
                    nc.NumeroAutorizacion, nc.FechaAutorizacion,
                    nc.Ambiente, nc.ClaveAcceso));

                col.Item().Element(c => RenderComprador(c,
                    nc.TipoIdentificacionComprador, nc.IdentificacionComprador,
                    nc.RazonSocialComprador, nc.DireccionComprador,
                    nc.FechaEmision));

                col.Item().Element(c => RenderDocumentoModificado(c, nc));

                col.Item().Element(c => RenderTablaItems(c, MapDetalleNc(nc.Detalle)));

                col.Item().Element(c => RenderTotalesNc(c, nc));

                if (nc.InfoAdicional.Count > 0)
                    col.Item().Element(c => RenderInfoAdicional(c, nc.InfoAdicional));
            });
        })).GeneratePdf();

        return Task.FromResult<ErrorOr<byte[]>>(bytes);
    }

    // ── Retención ─────────────────────────────────────────────────────────────

    public Task<ErrorOr<byte[]>> GenerarRideRetencionAsync(
        Retencion retencion, Empresa empresa, ParametrosFacturacion? parametros,
        byte[]? logoBytes, CancellationToken ct = default)
    {
        var bytes = Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(t => t.FontSize(8));

            page.Content().Column(col =>
            {
                col.Spacing(4);

                col.Item().Element(c => RenderEncabezado(c, empresa, parametros, logoBytes,
                    "COMPROBANTE DE RETENCIÓN", $"{retencion.Estab}-{retencion.PtoEmi}-{retencion.Secuencial}",
                    retencion.NumeroAutorizacion, retencion.FechaAutorizacion,
                    retencion.Ambiente, retencion.ClaveAcceso));

                col.Item().Element(c => RenderSujeto(c,
                    retencion.TipoIdentificacionSujeto, retencion.IdentificacionSujeto,
                    retencion.RazonSocialSujeto, retencion.DireccionSujeto,
                    retencion.FechaEmision, retencion.PeriodoFiscal));

                col.Item().Element(c => RenderTablaRetencion(c,
                    retencion.Detalle.OrderBy(d => d.Orden).ToList()));

                col.Item().Element(c => RenderTotalesRetencion(c, retencion));

                if (retencion.InfoAdicional.Count > 0)
                    col.Item().Element(c => RenderInfoAdicional(c, retencion.InfoAdicional));
            });
        })).GeneratePdf();

        return Task.FromResult<ErrorOr<byte[]>>(bytes);
    }

    // ── Encabezado ────────────────────────────────────────────────────────────

    private static void RenderEncabezado(IContainer container,
        Empresa empresa, ParametrosFacturacion? p, byte[]? logoBytes,
        string tipoDocumento, string numeroDocumento,
        string? numAutorizacion, DateTimeOffset? fechaAuth,
        Ambiente ambiente, string claveAcceso)
    {
        container.Border(0.5f).BorderColor(Colors.Black).Row(row =>
        {
            // Columna izquierda: logo + datos del emisor
            row.RelativeItem(55).Border(0.5f).BorderColor(Colors.Black).Padding(6).Column(col =>
            {
                col.Spacing(2);

                if (logoBytes is not null)
                {
                    col.Item().MaxHeight(55).AlignCenter().Image(logoBytes).FitArea();
                    col.Item().Height(4);
                }

                col.Item().AlignCenter().Text(empresa.Nombre).Bold().FontSize(10);

                if (!string.IsNullOrWhiteSpace(empresa.NombreComercial))
                    col.Item().AlignCenter().Text(empresa.NombreComercial).FontSize(9);

                col.Item().Height(3);
                col.Item().Text($"Dir. Matriz: {empresa.DirMatriz}").FontSize(7);

                if (p is not null)
                {
                    if (!string.IsNullOrWhiteSpace(p.ContribuyenteEspecial))
                        col.Item().Text($"Contribuyente Especial No. {p.ContribuyenteEspecial}").FontSize(7);

                    col.Item().Text($"Obligado a llevar Contabilidad: {(p.ObligadoContabilidad ? "SI" : "NO")}").FontSize(7);

                    if (p.AgenteRetencion)
                        col.Item().Text("AGENTE DE RETENCIÓN").Bold().FontSize(7);

                    if (!string.IsNullOrWhiteSpace(p.ContribuyenteRimpe))
                        col.Item().Text($"RIMPE: {p.ContribuyenteRimpe}").FontSize(7);
                }
            });

            // Columna derecha: tipo de doc, número, autorización
            row.RelativeItem(45).Border(0.5f).BorderColor(Colors.Black).Padding(6).Column(col =>
            {
                col.Spacing(2);

                col.Item().Text($"R.U.C.: {empresa.Ruc}").Bold().FontSize(9);
                col.Item().Height(2);

                col.Item().AlignCenter()
                    .Background(Colors.Grey.Lighten3)
                    .Padding(3)
                    .Text(tipoDocumento).Bold().FontSize(11);

                col.Item().AlignCenter().Text($"No. {numeroDocumento}").Bold().FontSize(9);
                col.Item().Height(3);

                col.Item().Text("NÚMERO DE AUTORIZACIÓN:").Bold().FontSize(7);
                col.Item().Text(numAutorizacion ?? "Pendiente de autorización").FontSize(7);

                col.Item().Height(2);
                col.Item().Text("FECHA Y HORA DE AUTORIZACIÓN:").Bold().FontSize(7);
                col.Item().Text(FormatFechaAuth(fechaAuth)).FontSize(7);

                col.Item().Height(2);
                col.Item().Text($"AMBIENTE: {(ambiente == Ambiente.Produccion ? "PRODUCCIÓN" : "PRUEBAS")}").FontSize(7);
                col.Item().Text("EMISIÓN: NORMAL").FontSize(7);

                col.Item().Height(3);
                col.Item().Text("CLAVE DE ACCESO:").Bold().FontSize(7);
                col.Item().Text(claveAcceso).FontSize(6);
            });
        });
    }

    // ── Comprador (Factura / NC) ──────────────────────────────────────────────

    private static void RenderComprador(IContainer container,
        string tipoId, string identificacion, string razonSocial,
        string? direccion, DateOnly fechaEmision, string? guiaRemision = null)
    {
        container.Border(0.5f).BorderColor(Colors.Black).Padding(5).Column(col =>
        {
            col.Spacing(2);

            col.Item().Row(row =>
            {
                row.RelativeItem(2).Text(t =>
                {
                    t.Span("RAZÓN SOCIAL / APELLIDOS Y NOMBRES: ").Bold().FontSize(7);
                    t.Span(razonSocial).FontSize(7);
                });
                row.RelativeItem().Text(t =>
                {
                    t.Span("IDENTIFICACIÓN: ").Bold().FontSize(7);
                    t.Span($"{tipoId}: {identificacion}").FontSize(7);
                });
            });

            col.Item().Row(row =>
            {
                row.RelativeItem(2).Text(t =>
                {
                    t.Span("DIRECCIÓN: ").Bold().FontSize(7);
                    t.Span(direccion ?? "—").FontSize(7);
                });
                row.RelativeItem().Text(t =>
                {
                    t.Span("FECHA EMISIÓN: ").Bold().FontSize(7);
                    t.Span(fechaEmision.ToString("dd/MM/yyyy")).FontSize(7);
                });
            });

            if (!string.IsNullOrWhiteSpace(guiaRemision))
            {
                col.Item().Text(t =>
                {
                    t.Span("GUÍA DE REMISIÓN: ").Bold().FontSize(7);
                    t.Span(guiaRemision).FontSize(7);
                });
            }
        });
    }

    // ── Documento Modificado (NC) ─────────────────────────────────────────────

    private static void RenderDocumentoModificado(IContainer container, NotaCredito nc)
    {
        container.Border(0.5f).BorderColor(Colors.Black).Padding(5).Column(col =>
        {
            col.Spacing(2);

            col.Item().Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("TIPO COMPROBANTE MODIFICADO: ").Bold().FontSize(7);
                    t.Span(nc.DocModificadoTipo).FontSize(7);
                });
                row.RelativeItem().Text(t =>
                {
                    t.Span("NÚMERO: ").Bold().FontSize(7);
                    t.Span(nc.DocModificadoNumero).FontSize(7);
                });
                row.RelativeItem().Text(t =>
                {
                    t.Span("FECHA: ").Bold().FontSize(7);
                    t.Span(nc.DocModificadoFecha.ToString("dd/MM/yyyy")).FontSize(7);
                });
            });

            col.Item().Text(t =>
            {
                t.Span("MOTIVO: ").Bold().FontSize(7);
                t.Span(nc.Motivo).FontSize(7);
            });
        });
    }

    // ── Sujeto (Retención) ────────────────────────────────────────────────────

    private static void RenderSujeto(IContainer container,
        string tipoId, string identificacion, string razonSocial,
        string? direccion, DateOnly fechaEmision, string periodoFiscal)
    {
        container.Border(0.5f).BorderColor(Colors.Black).Padding(5).Column(col =>
        {
            col.Spacing(2);

            col.Item().Row(row =>
            {
                row.RelativeItem(2).Text(t =>
                {
                    t.Span("RAZÓN SOCIAL / APELLIDOS Y NOMBRES: ").Bold().FontSize(7);
                    t.Span(razonSocial).FontSize(7);
                });
                row.RelativeItem().Text(t =>
                {
                    t.Span("IDENTIFICACIÓN: ").Bold().FontSize(7);
                    t.Span($"{tipoId}: {identificacion}").FontSize(7);
                });
            });

            col.Item().Row(row =>
            {
                row.RelativeItem(2).Text(t =>
                {
                    t.Span("DIRECCIÓN: ").Bold().FontSize(7);
                    t.Span(direccion ?? "—").FontSize(7);
                });
                row.RelativeItem().Text(t =>
                {
                    t.Span("FECHA EMISIÓN: ").Bold().FontSize(7);
                    t.Span(fechaEmision.ToString("dd/MM/yyyy")).FontSize(7);
                });
            });

            col.Item().Text(t =>
            {
                t.Span("PERÍODO FISCAL: ").Bold().FontSize(7);
                t.Span(periodoFiscal).FontSize(7);
            });
        });
    }

    // ── Tabla de ítems (Factura / NC) ─────────────────────────────────────────

    private record LineaItem(
        string CodigoPrincipal, string? CodigoAuxiliar, string Descripcion,
        decimal Cantidad, decimal PrecioUnitario, decimal Descuento, decimal PrecioTotal);

    private static List<LineaItem> MapDetalleFactura(IEnumerable<FacturaDetalle> detalle) =>
        detalle.OrderBy(d => d.Orden)
               .Select(d => new LineaItem(d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
                   d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto))
               .ToList();

    private static List<LineaItem> MapDetalleNc(IEnumerable<NotaCreditoDetalle> detalle) =>
        detalle.OrderBy(d => d.Orden)
               .Select(d => new LineaItem(d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
                   d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto))
               .ToList();

    private static void RenderTablaItems(IContainer container, List<LineaItem> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(8);  // Cant.
                cols.RelativeColumn(12); // Cód. principal
                cols.RelativeColumn(10); // Cód. auxiliar
                cols.RelativeColumn(40); // Descripción
                cols.RelativeColumn(10); // P. Unit.
                cols.RelativeColumn(10); // Descuento
                cols.RelativeColumn(10); // P. Total
            });

            table.Header(header =>
            {
                static IContainer Th(IContainer c) =>
                    c.Background(Colors.Grey.Lighten3)
                     .Border(0.5f).BorderColor(Colors.Black)
                     .Padding(3).AlignCenter();

                header.Cell().Element(Th).Text("CANT.").Bold().FontSize(7);
                header.Cell().Element(Th).Text("CÓD. PRINCIPAL").Bold().FontSize(7);
                header.Cell().Element(Th).Text("CÓD. AUXILIAR").Bold().FontSize(7);
                header.Cell().Element(Th).Text("DESCRIPCIÓN").Bold().FontSize(7);
                header.Cell().Element(Th).Text("P. UNIT.").Bold().FontSize(7);
                header.Cell().Element(Th).Text("DESCUENTO").Bold().FontSize(7);
                header.Cell().Element(Th).Text("P. TOTAL").Bold().FontSize(7);
            });

            static IContainer Td(IContainer c) =>
                c.Border(0.5f).BorderColor(Colors.Black).Padding(3);

            foreach (var item in items)
            {
                table.Cell().Element(Td).AlignCenter().Text(item.Cantidad.ToString("0.##")).FontSize(7);
                table.Cell().Element(Td).Text(item.CodigoPrincipal).FontSize(7);
                table.Cell().Element(Td).Text(item.CodigoAuxiliar ?? string.Empty).FontSize(7);
                table.Cell().Element(Td).Text(item.Descripcion).FontSize(7);
                table.Cell().Element(Td).AlignRight().Text(item.PrecioUnitario.ToString("0.000000")).FontSize(7);
                table.Cell().Element(Td).AlignRight().Text(item.Descuento.ToString("0.00")).FontSize(7);
                table.Cell().Element(Td).AlignRight().Text(item.PrecioTotal.ToString("0.00")).FontSize(7);
            }
        });
    }

    // ── Tabla de retención ────────────────────────────────────────────────────

    private static void RenderTablaRetencion(IContainer container, List<RetencionDetalle> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(14); // Cod. Sustento
                cols.RelativeColumn(20); // Nro. Doc.
                cols.RelativeColumn(12); // Fecha
                cols.RelativeColumn(10); // Impuesto
                cols.RelativeColumn(12); // Cód. Retención
                cols.RelativeColumn(14); // Base Imponible
                cols.RelativeColumn(8);  // %
                cols.RelativeColumn(10); // Valor Retenido
            });

            table.Header(header =>
            {
                static IContainer Th(IContainer c) =>
                    c.Background(Colors.Grey.Lighten3)
                     .Border(0.5f).BorderColor(Colors.Black)
                     .Padding(3).AlignCenter();

                header.Cell().Element(Th).Text("COD. SUSTENTO").Bold().FontSize(6);
                header.Cell().Element(Th).Text("NRO. DOC. SUSTENTO").Bold().FontSize(6);
                header.Cell().Element(Th).Text("FECHA EMISIÓN").Bold().FontSize(6);
                header.Cell().Element(Th).Text("IMPUESTO").Bold().FontSize(6);
                header.Cell().Element(Th).Text("CÓD. RETENCIÓN").Bold().FontSize(6);
                header.Cell().Element(Th).Text("BASE IMPONIBLE").Bold().FontSize(6);
                header.Cell().Element(Th).Text("%").Bold().FontSize(6);
                header.Cell().Element(Th).Text("VALOR RETENIDO").Bold().FontSize(6);
            });

            static IContainer Td(IContainer c) =>
                c.Border(0.5f).BorderColor(Colors.Black).Padding(3);

            foreach (var item in items)
            {
                table.Cell().Element(Td).AlignCenter().Text(item.CodDocSustento).FontSize(7);
                table.Cell().Element(Td).Text(item.NumDocSustento).FontSize(7);
                table.Cell().Element(Td).AlignCenter().Text(item.FechaEmisionDocSustento.ToString("dd/MM/yyyy")).FontSize(7);
                table.Cell().Element(Td).AlignCenter().Text(item.CodigoImpuesto).FontSize(7);
                table.Cell().Element(Td).AlignCenter().Text(item.CodigoRetencion).FontSize(7);
                table.Cell().Element(Td).AlignRight().Text(item.BaseImponible.ToString("0.00")).FontSize(7);
                table.Cell().Element(Td).AlignRight().Text(item.PorcentajeRetener.ToString("0.##") + "%").FontSize(7);
                table.Cell().Element(Td).AlignRight().Text(item.ValorRetenido.ToString("0.00")).FontSize(7);
            }
        });
    }

    // ── Totales Factura ───────────────────────────────────────────────────────

    private static void RenderTotalesFactura(IContainer container, Factura f)
    {
        var gruposIva = f.Detalle
            .GroupBy(d => d.IvaTarifa)
            .OrderBy(g => g.Key)
            .ToList();

        container.Row(row =>
        {
            row.RelativeItem(2);
            row.RelativeItem(3).Column(col =>
            {
                foreach (var grupo in gruposIva)
                {
                    var label = grupo.Key == 0
                        ? "SUBTOTAL 0%:"
                        : $"SUBTOTAL IVA {grupo.Key:0.##}%:";
                    RenderFilaTotales(col, label, grupo.Sum(d => d.PrecioTotalSinImpuesto).ToString("0.00"));
                }

                RenderFilaTotales(col, "DESCUENTO:", f.TotalDescuento.ToString("0.00"));

                if (f.BaseImponibleIce.HasValue && f.BaseImponibleIce > 0)
                    RenderFilaTotales(col, "ICE:", f.ValorIce!.Value.ToString("0.00"));

                foreach (var grupo in gruposIva.Where(g => g.Key > 0))
                    RenderFilaTotales(col, $"IVA {grupo.Key:0.##}%:", grupo.Sum(d => d.IvaValor).ToString("0.00"));

                if (f.Propina > 0)
                    RenderFilaTotales(col, "PROPINA:", f.Propina.ToString("0.00"));

                RenderFilaTotales(col, "VALOR TOTAL:", f.ImporteTotal.ToString("0.00"), bold: true);
            });
        });
    }

    // ── Totales Nota de Crédito ───────────────────────────────────────────────

    private static void RenderTotalesNc(IContainer container, NotaCredito nc)
    {
        var gruposIva = nc.Detalle
            .GroupBy(d => d.IvaTarifa)
            .OrderBy(g => g.Key)
            .ToList();

        container.Row(row =>
        {
            row.RelativeItem(2);
            row.RelativeItem(3).Column(col =>
            {
                foreach (var grupo in gruposIva)
                {
                    var label = grupo.Key == 0
                        ? "SUBTOTAL 0%:"
                        : $"SUBTOTAL IVA {grupo.Key:0.##}%:";
                    RenderFilaTotales(col, label, grupo.Sum(d => d.PrecioTotalSinImpuesto).ToString("0.00"));
                }

                RenderFilaTotales(col, "DESCUENTO:", nc.TotalDescuento.ToString("0.00"));

                if (nc.BaseImponibleIce.HasValue && nc.BaseImponibleIce > 0)
                    RenderFilaTotales(col, "ICE:", nc.ValorIce!.Value.ToString("0.00"));

                foreach (var grupo in gruposIva.Where(g => g.Key > 0))
                    RenderFilaTotales(col, $"IVA {grupo.Key:0.##}%:", grupo.Sum(d => d.IvaValor).ToString("0.00"));

                RenderFilaTotales(col, "VALOR DE MODIFICACIÓN:", nc.ValorModificacion.ToString("0.00"), bold: true);
            });
        });
    }

    // ── Totales Retención ─────────────────────────────────────────────────────

    private static void RenderTotalesRetencion(IContainer container, Retencion r)
    {
        container.Row(row =>
        {
            row.RelativeItem(2);
            row.RelativeItem(3).Column(col =>
            {
                RenderFilaTotales(col, "TOTAL RETENCIÓN RENTA:", r.TotalRetencionRenta.ToString("0.00"));
                RenderFilaTotales(col, "TOTAL RETENCIÓN IVA:", r.TotalRetencionIva.ToString("0.00"));
                RenderFilaTotales(col, "TOTAL RETENIDO:", r.TotalRetenido.ToString("0.00"), bold: true);
            });
        });
    }

    // ── Fila de totales (helper compartido) ───────────────────────────────────

    private static void RenderFilaTotales(ColumnDescriptor col, string label, string valor, bool bold = false)
    {
        col.Item().Row(row =>
        {
            var labelCell = row.RelativeItem(2)
                .Border(0.5f).BorderColor(Colors.Black)
                .Padding(3);
            var valorCell = row.RelativeItem()
                .Border(0.5f).BorderColor(Colors.Black)
                .Padding(3).AlignRight();

            if (bold)
            {
                labelCell.Text(label).Bold().FontSize(7);
                valorCell.Text(valor).Bold().FontSize(7);
            }
            else
            {
                labelCell.Text(label).FontSize(7);
                valorCell.Text(valor).FontSize(7);
            }
        });
    }

    // ── Formas de pago ────────────────────────────────────────────────────────

    private static void RenderFormasPago(IContainer container, List<FormaPago> formasPago)
    {
        container.Column(col =>
        {
            col.Item()
                .Background(Colors.Grey.Lighten3)
                .Border(0.5f).BorderColor(Colors.Black)
                .Padding(3)
                .Text("FORMA DE PAGO").Bold().FontSize(7);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    static IContainer Th(IContainer c) =>
                        c.Background(Colors.Grey.Lighten3)
                         .Border(0.5f).BorderColor(Colors.Black)
                         .Padding(3).AlignCenter();

                    header.Cell().Element(Th).Text("FORMA DE PAGO").Bold().FontSize(7);
                    header.Cell().Element(Th).Text("VALOR").Bold().FontSize(7);
                    header.Cell().Element(Th).Text("PLAZO").Bold().FontSize(7);
                    header.Cell().Element(Th).Text("TIEMPO").Bold().FontSize(7);
                });

                static IContainer Td(IContainer c) =>
                    c.Border(0.5f).BorderColor(Colors.Black).Padding(3);

                foreach (var fp in formasPago)
                {
                    table.Cell().Element(Td).Text(fp.Codigo).FontSize(7);
                    table.Cell().Element(Td).AlignRight().Text(fp.Total.ToString("0.00")).FontSize(7);
                    table.Cell().Element(Td).AlignCenter().Text(fp.Plazo?.ToString() ?? string.Empty).FontSize(7);
                    table.Cell().Element(Td).AlignCenter().Text(fp.UnidadTiempo ?? string.Empty).FontSize(7);
                }
            });
        });
    }

    // ── Información adicional ─────────────────────────────────────────────────

    private static void RenderInfoAdicional(IContainer container, List<InfoAdicional> infoAdicional)
    {
        container.Column(col =>
        {
            col.Item()
                .Background(Colors.Grey.Lighten3)
                .Border(0.5f).BorderColor(Colors.Black)
                .Padding(3)
                .Text("INFORMACIÓN ADICIONAL").Bold().FontSize(7);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(5);
                });

                static IContainer Td(IContainer c) =>
                    c.Border(0.5f).BorderColor(Colors.Black).Padding(3);

                foreach (var info in infoAdicional)
                {
                    table.Cell().Element(Td).Text(info.Nombre).Bold().FontSize(7);
                    table.Cell().Element(Td).Text(info.Valor).FontSize(7);
                }
            });
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string FormatFechaAuth(DateTimeOffset? fechaAuth)
    {
        if (!fechaAuth.HasValue) return "Pendiente de autorización";
        // Ecuador es UTC-5; se muestra la hora del offset almacenado
        return fechaAuth.Value.ToOffset(TimeSpan.FromHours(-5)).ToString("dd/MM/yyyy HH:mm:ss");
    }
}
