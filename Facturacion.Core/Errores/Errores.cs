using ErrorOr;

namespace Facturacion.Core;

public static class Errores
{
    public static class Empresa
    {
        public static readonly Error NoEncontrada =
            Error.NotFound("Empresa.NoEncontrada", "La empresa no existe.");

        public static readonly Error RucDuplicado =
            Error.Conflict("Empresa.RucDuplicado", "Ya existe una empresa con ese RUC.");
    }

    public static class Factura
    {
        public static readonly Error NoEncontrada =
            Error.NotFound("Factura.NoEncontrada", "La factura no existe.");

        public static readonly Error ClaveAccesoDuplicada =
            Error.Conflict("Factura.ClaveAccesoDuplicada", "Ya existe una factura con esa clave de acceso.");

        public static readonly Error EstadoInvalido =
            Error.Validation("Factura.EstadoInvalido", "La factura no está en un estado válido para esta operación.");
    }

    public static class NotaCredito
    {
        public static readonly Error NoEncontrada =
            Error.NotFound("NotaCredito.NoEncontrada", "La nota de crédito no existe.");

        public static readonly Error ClaveAccesoDuplicada =
            Error.Conflict("NotaCredito.ClaveAccesoDuplicada", "Ya existe una nota de crédito con esa clave de acceso.");

        public static readonly Error EstadoInvalido =
            Error.Validation("NotaCredito.EstadoInvalido", "La nota de crédito no está en un estado válido para esta operación.");
    }

    public static class Retencion
    {
        public static readonly Error NoEncontrada =
            Error.NotFound("Retencion.NoEncontrada", "La retención no existe.");

        public static readonly Error ClaveAccesoDuplicada =
            Error.Conflict("Retencion.ClaveAccesoDuplicada", "Ya existe una retención con esa clave de acceso.");

        public static readonly Error EstadoInvalido =
            Error.Validation("Retencion.EstadoInvalido", "La retención no está en un estado válido para esta operación.");
    }

    public static class Sri
    {
        public static Error NoAutorizado(string? mensaje = null) =>
            Error.Failure("Sri.NoAutorizado", mensaje ?? "El SRI no autorizó el documento.");

        public static Error Devuelta(string? detalle = null) =>
            Error.Failure("Sri.Devuelta", detalle is not null
                ? $"El SRI devolvió el documento en recepción: {detalle}"
                : "El SRI devolvió el documento en recepción.");

        public static readonly Error SecuencialDuplicado =
            Error.Conflict("Sri.SecuencialDuplicado", "El secuencial ya fue registrado en el SRI (CLAVE ACCESO REGISTRADA).");

        public static readonly Error EnProcesamiento =
            Error.Failure("Sri.EnProcesamiento", "El SRI recibió el documento pero aún está en procesamiento.");

        public static readonly Error ErrorComunicacion =
            Error.Failure("Sri.ErrorComunicacion", "Error al comunicarse con el SRI.");

        public static readonly Error SinRespuesta =
            Error.Failure("Sri.SinRespuesta", "El SRI no devolvió una respuesta reconocible.");
    }

    public static class Firma
    {
        public static readonly Error ErrorFirma =
            Error.Failure("Firma.ErrorFirma", "No se pudo firmar el documento.");

        public static readonly Error CertificadoInvalido =
            Error.Validation("Firma.CertificadoInvalido", "El certificado .p12 es inválido o la contraseña es incorrecta.");
    }

    public static class Storage
    {
        public static readonly Error ErrorGuardar =
            Error.Failure("Storage.ErrorGuardar", "No se pudo guardar el archivo.");

        public static readonly Error ArchivoNoEncontrado =
            Error.NotFound("Storage.ArchivoNoEncontrado", "El archivo no existe en el storage.");
    }

    public static class Xml
    {
        public static readonly Error ErrorSerializacion =
            Error.Failure("Xml.ErrorSerializacion", "No se pudo serializar el documento XML.");
    }
}
