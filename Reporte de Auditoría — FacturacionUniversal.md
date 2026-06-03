Reporte de Auditoría — FacturacionUniversal — 2026-06-03
Resumen Ejecutivo
Severidad	Hallazgos
🔴 CRITICAL	8
🟠 HIGH	12
🟡 MEDIUM	13
🟢 LOW / INFO	10
Stack auditado: .NET 8 · ASP.NET Core Minimal APIs · EF Core + PostgreSQL · JWT Bearer · Supabase Storage · XAdES Digital Signature · QuestPDF · Serilog

🔴 CRITICAL
C-1 | Emisión de facturas completamente anónima
FacturasEndpoints.cs:18-19

El grupo /facturas usa .AllowAnonymous(). Cualquier persona en internet puede emitir documentos tributarios reales ante el SRI, reintentar emisiones y generar PDFs fraudulentos con datos de terceros — sin ninguna autenticación.

Fix: Cambiar .AllowAnonymous() por .RequireAuthorization() en el grupo. Si /preview necesita acceso diferenciado, moverlo a un subgrupo propio.

C-2 | /empresas y /parametros sin autorización
EmpresasEndpoints.cs:13 | ParametrosEndpoints.cs:12

Ninguno de los dos grupos declara .RequireAuthorization() ni .AllowAnonymous(). En .NET 8 sin FallbackPolicy, los endpoints son públicos por defecto. Cualquiera puede listar RUCs de empresas, reemplazar certificados P12 y modificar parámetros de ambiente SRI (pruebas ↔ producción).

Fix: Agregar .RequireAuthorization() a ambos grupos.

C-3 | CertPassword almacenada en texto plano en la BD
Empresa.cs:14

CertPassword se persiste directamente sin cifrado. El certificado P12 es la firma digital tributaria de la empresa — equivalente legal a la firma manuscrita. Si la base de datos es comprometida, todas las contraseñas quedan expuestas.

Fix: Cifrar la columna con AES-256-GCM usando una clave derivada de un secrets manager (Azure Key Vault, Supabase Vault). Implementar como ValueConverter en EmpresaConfiguracion.

C-4 | GlobalExceptionHandler expone internals en HTTP 500
GlobalExceptionHandler.cs:17-22

exception.GetType().Name y exception.Message se devuelven al cliente en producción. Los mensajes de EF Core incluyen nombres de tablas y columnas; los de X509 incluyen rutas de certificados; los de Npgsql pueden incluir fragmentos de queries SQL.

Fix:


var correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
logger.LogError(exception, "Excepción no manejada. CorrelationId: {CorrelationId}", correlationId);
await ctx.Response.WriteAsJsonAsync(new ProblemDetails {
    Status = 500, Title = "Error interno del servidor",
    Detail = $"Referencia: {correlationId}"
}, ct);
C-5 | Race condition en generación de secuencial — números tributarios quemados
EmitirFactura.cs:74 | EmitirNotaCredito.cs:76 | EmitirRetencion.cs:61

El secuencial se incrementa en BD (línea 74) antes de verificar si ya existe un documento activo con ese número (línea 81). Si ExisteSecuencialActivoAsync retorna true, el error se devuelve pero el contador ya fue consumido, quemando un número de secuencial irrecuperable ante el SRI.

Fix: Invertir el orden: verificar duplicado primero, luego incrementar. O combinar ambas operaciones en una sola transacción atómica en el repositorio.

C-6 | InvalidOperationException desde repositorio rompe el pipeline ErrorOr
SecuencialesSriRepositorio.cs:51

IncrementarYObtenerAsync lanza InvalidOperationException cuando no hay secuencial configurado. La excepción escapa al middleware genérico sin pasar por el pipeline ErrorOr, perdiendo todo el contexto de error estructurado.

Fix: Cambiar firma a Task<ErrorOr<long>> y retornar Error.NotFound("Secuencial.NoConfigurado", ...).

C-7 | Bug latente: FacturaId de los detalles puede no coincidir con Factura.Id
EmitirFactura.cs:86-91

Se genera facturaId = Guid.NewGuid() en el caso de uso y se pasa a FacturaDetalle.Crear(facturaId, ...), pero Factura.Crear(...) genera su propio Guid.NewGuid() internamente. Los detalles tienen una FK apuntando a un ID que no coincide con el ID real de la factura.

Fix: Factura.Crear debe recibir el Id externo como parámetro, o los detalles deben agregarse post-creación vía factura.AgregarDetalle(...) que use factura.Id.

C-8 | Domain logic construida en el endpoint de Preview
FacturasEndpoints.cs:38-61

El handler Preview construye FacturaDetalle, FormaPago, InfoAdicional e invoca GeneradorClaveAcceso.Generar(...) directamente. Duplica la lógica de EmitirFactura; cualquier cambio en las reglas de construcción debe actualizarse en dos lugares.

Fix: Crear ComandoPreviewFactura y mover toda la construcción al caso de uso GenerarPreviewPdf. El endpoint solo mapea el request al comando.

🟠 HIGH
H-1 | IDOR en /empresas/{ruc} — sin verificación de ownership
EmpresasEndpoints.cs:63-71

Un usuario autenticado puede consultar y modificar empresas de otras cuentas conociendo su RUC. El espacio de búsqueda (13 dígitos) es trivialmente enumerable contra el padrón SRI público.

Fix: Extraer CuentaId del JWT y filtrar en todos los repositorios: .Where(e => e.Ruc == ruc && e.CuentaId == cuentaIdDelToken).

H-2 | Sin rate limiting en endpoints de emisión
FacturasEndpoints.cs:21 | NotasCreditoEndpoints.cs:21 | RetencionesEndpoints.cs:21

No hay AddRateLimiter() ni UseRateLimiter(). Un atacante puede agotar el secuencial SRI, saturar el storage de Supabase o generar cargos ilimitados.

Fix: services.AddRateLimiter(o => o.AddFixedWindowLimiter("emisiones", opts => { opts.PermitLimit = 60; opts.Window = TimeSpan.FromMinutes(1); })) y .RequireRateLimiting("emisiones") en los grupos.

H-3 | Multi-tenancy no enforced — EmpresaRuc viene del body, no del JWT
FacturasEndpoints.cs:80-120

Incluso después de agregar autenticación, un usuario autenticado puede emitir con el RUC de cualquier otra empresa del sistema porque ningún endpoint extrae ni valida claims del token.

Fix: Cruzar el sub claim del JWT con Empresa.CuentaId antes de procesar cualquier operación sobre una empresa. Implementar como IAuthorizationHandler o validación en cada caso de uso.

H-4 | JWT sin parámetros de validación explícitos
ApiExtensions.cs:36-39

Solo se configura Authority y Audience. ClockSkew es 5 minutos por defecto (tokens expirados siguen válidos), y no se restringe el algoritmo permitido.

Fix:


options.TokenValidationParameters = new() {
    ValidateIssuerSigningKey = true, ValidateLifetime = true,
    ClockSkew = TimeSpan.FromSeconds(30),
    ValidAlgorithms = ["RS256"]
};
H-5 | Validación de logo por Content-Type declarado — sin magic bytes
EmpresaContratos.cs:128-140

El LogoContentType lo envía el cliente en el body. Un ejecutable con ContentType: "image/png" pasa la validación y se almacena en Supabase.

Fix: Verificar firma de archivo: PNG [0x89, 0x50, 0x4E, 0x47], JPEG [0xFF, 0xD8], WebP "WEBP" en offset 8. Rechazar o sanitizar SVG (puede contener scripts).

H-6 | PDF generado sincrónico envuelto en Task.FromResult — bloquea threadpool
ServicioPdf.cs:24-56

Document.Create(...).GeneratePdf() (QuestPDF) es síncrono pero retorna Task.FromResult(bytes). Bajo carga concurrente agota los threads del pool ASP.NET.

Fix: return await Task.Run(() => (ErrorOr<byte[]>)Document.Create(...).GeneratePdf(), ct);

H-7 | XmlSerializer instanciado en cada llamada — sin caché
ServicioXml.cs:292-307

new XmlSerializer(typeof(T)) compila código dinámico en cada llamada. En .NET Core no hay memory leak pero sí overhead de CPU significativo por documento emitido.

Fix:


private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializers = new();
private static XmlSerializer GetSerializer<T>() =>
    _serializers.GetOrAdd(typeof(T), t => new XmlSerializer(t));
H-8 | OperationCanceledException tragado como error genérico
ServicioSri.cs:44-48 | ServicioSri.cs:98-102

catch (Exception ex) engloba las cancelaciones de CancellationToken. Se loguea como "error inesperado" y el documento puede quedar en estado inconsistente.

Fix: Agregar catch (OperationCanceledException) { throw; } antes del catch general en ambos métodos.

H-9 | Rutas de certificado y logo hardcodeadas fuera de RutasStorage
RegistrarEmpresa.cs:33 | GuardarEmpresa.cs:32

$"{cmd.Ruc}/certificado.p12" y $"{cmd.Ruc}/logo" duplicados en dos clases, ignorando el patrón RutasStorage ya existente.

Fix: Agregar RutasStorage.Certificado(ruc) y RutasStorage.Logo(ruc) en RutasStorage.cs y usar en ambos casos de uso.

H-10 | Duplicación masiva del comportamiento de estado entre entidades
Factura.cs | Retencion.cs | NotaCredito.cs

Las tres entidades tienen implementaciones idénticas de RegistrarXmlFirmado, RegistrarEnvioSri, RegistrarNumeroAutorizacion, RegistrarAutorizacionSri, RegistrarPdf, RegistrarNoAutorizacion, Anular, MarcarCorreoEnviado. Cualquier cambio en la máquina de estados requiere actualizar tres archivos.

Fix: Extraer clase base abstracta DocumentoElectronico con todos los campos y métodos compartidos.

H-11 | FluentValidation versiones divididas — 11.3.1 y 12.1.1
Facturacion.Api.csproj:16

FluentValidation.AspNetCore 11.3.1 (deprecated) mezclado con FluentValidation 12.1.1. El paquete AspNetCore fue discontinuado en v12.

Fix: Eliminar FluentValidation.AspNetCore. Usar solo FluentValidation 12.x con services.AddValidatorsFromAssemblyContaining<Program>() (ya configurado en ApiExtensions.cs:56).

H-12 | BuildTotalImpuestosFactura y BuildTotalImpuestosNota — duplicación lógica
ServicioXml.cs:170-224

Dos métodos con lógica idéntica para calcular impuestos por tipo. Cualquier corrección en el algoritmo de agrupación debe aplicarse en ambos.

Fix: Extraer método genérico BuildTotalImpuestos<T>(IEnumerable<T> detalle, ...) con funciones selectoras.

🟡 MEDIUM
M-1 | XML Injection en BuildSoapAutorizacion — claveAcceso sin escapado
ServicioSri.cs:197-207
Fix: SecurityElement.Escape(claveAcceso) al interpolar, o construir con XDocument/XElement.

M-2 | JSON manual con interpolación en EliminarAsync — riesgo de JSON injection
ServicioStorageSupabase.cs:79-80
Fix: JsonSerializer.Serialize(new { prefixes = new[] { ruta } }).

M-3 | CORS hardcodeado a localhost — sin configuración para producción
ApiExtensions.cs:27-31
Fix: policy.WithOrigins(config.GetSection("Cors:Origins").Get<string[]>()!) con appsettings.Production.json.

M-4 | Sin HSTS — degradación a HTTP posible ante MitM
Program.cs:26-27
Fix: app.UseHsts() junto a UseHttpsRedirection() en el bloque de producción.

M-5 | Sin headers de seguridad HTTP
Falta X-Content-Type-Options: nosniff, X-Frame-Options: DENY, Referrer-Policy: no-referrer.
Fix: Middleware inline antes de UseAuthentication(), o paquete NWebSec.AspNetCore.Middleware.

M-6 | Sin Correlation ID / Request Tracing
GlobalExceptionHandler.cs:11
Fix: app.Use(async (ctx, next) => { ctx.Response.Headers["X-Correlation-Id"] = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString(); ... }).

M-7 | IP del cliente capturada sin considerar proxies
FacturasEndpoints.cs:107
Fix: app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | XForwardedProto }).

M-8 | Sin límites en el cuerpo del request
Sin MaxRequestBodySize global ni límite en cmd.Detalle.Count. Una factura con 100,000 líneas se procesa completamente en memoria.
Fix: builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 1_048_576) + validador RuleFor(x => x.Detalle).Must(d => d.Count <= 500).

M-9 | FacturaId expuesto públicamente en FacturaDetalle
FacturaDetalle.cs:9
Los detalles pueden cargarse independientemente de su aggregate root.
Fix: Usar shadow property en FacturaDetalleConfiguracion para la FK, sin exponerla en la entidad.

M-10 | OrquestadorReintento compara enum con <
OrquestadorReintento.cs:50
Dependencia implícita del orden numérico del enum. Un nuevo estado añadido puede romper la lógica silenciosamente.
Fix: doc.EstadoSri is EstadoSri.Pendiente or EstadoSri.Enviado.

M-11 | IncrementarYObtener en la interfaz del repositorio — lógica de negocio en puerto de persistencia
ISecuencialesSriRepositorio.cs:11
Fix: Mover a IServicioSecuencial (servicio de dominio). El repositorio solo expone ObtenerAsync / ActualizarAsync.

M-12 | ExtraerMensajesSri — patrón imperativo donde LINQ sería más expresivo
ServicioSri.cs:224-246
Fix: Reescribir con .Descendants("mensaje").Where(...).Select(...).ToList().

M-13 | Sin tests funcionales implementados
Facturacion.Tests/
El proyecto de tests solo contiene archivos generados. Sistema de facturación tributaria sin ningún test de GeneradorClaveAcceso, máquina de estados de EstadoSri, ni OrquestadorReintento.
Fix prioritario: Tests unitarios de GeneradorClaveAcceso.Generar con vectores del SRI, y tests de las transiciones de EstadoSri.

🟢 LOW / Mejoras
#	Archivo	Hallazgo	Fix
L-1	appsettings.json	Serilog solo Console sink — logs de auditoría fiscal no persisten entre reinicios	Agregar sink PostgreSQL o Seq; normativa SRI Ecuador exige retención mínima 7 años
L-2	FacturaContratos.cs	XmlAutorizadoPath y PdfPath expuestos en response — revela topología interna de storage	Exponer endpoints GET /facturas/{id}/pdf que generen signed URLs con TTL
L-3	Factura.cs:33	FormasPago y InfoAdicional como List<T> mutable accesible externamente	Cambiar a IReadOnlyList<T> en getter; mantener List<T> privado
L-4	CodigoRetencionSri.cs:66	.Concat().ToArray() — C# 12 disponible	IReadOnlyList<CodigoRetencionSri> Todos = [.. Renta, .. Iva]
L-5	ServicioXml.cs:258-279	BuildImpuestosDetalle con new List + Add imperativo	Reescribir con collection expression [.. iceCodigo is not null ? [...] : [], new XmlImpuestoDetalle {...}]
L-6	RegistrarEmpresa.cs:31	"Cuenta.NoEncontrada" duplicado como literal en dos clases	Mover a Errores.Cuenta.NoEncontrada siguiendo el patrón existente
L-7	appsettings.json:28	AllowedHosts: "*" — sin restricción de Host header	"AllowedHosts": "api.tudominio.com;localhost"
L-8	ServicioSri.cs:31	Ternario ambiente == Pruebas ? url : url repetido 4 veces	Extraer EndpointRecepcion(Ambiente a) y EndpointAutorizacion(Ambiente a) como métodos privados
L-9	IServicioStorageFirmaYLogo.cs	Tag interface vacía — solo diferencia registros DI	Usar Keyed services de .NET 8: services.AddKeyedScoped<IServicioStorage>("firma")
L-10	RegistrarEmpresa.cs:29	ObtenerPrimeraAsync — modelo multi-tenant incompleto. En producción todas las empresas podrían registrarse bajo la misma cuenta	Clarificar si CuentaId debe venir del JWT claim o del comando
Próximos pasos recomendados
Sprint 1 — Seguridad crítica (esta semana):

FacturasEndpoints.cs:18 — Cambiar .AllowAnonymous() por .RequireAuthorization()
EmpresasEndpoints.cs:13 + ParametrosEndpoints — Agregar .RequireAuthorization()
GlobalExceptionHandler.cs:17 — Devolver correlation ID genérico en producción
Empresa.cs:14 — Cifrar CertPassword en reposo
Sprint 2 — Bugs y robustez:
5. EmitirFactura.cs:86 — Corregir bug de FacturaId desincronizado con detalles
6. EmitirFactura.cs:74 — Invertir orden: verificar duplicado antes de incrementar secuencial
7. SecuencialesSriRepositorio.cs:51 — Convertir excepción a ErrorOr
8. ServicioPdf.cs:24 — Envolver generación PDF en Task.Run

Sprint 3 — Arquitectura y calidad:
9. Extraer clase base DocumentoElectronico para eliminar duplicación en Factura/Retencion/NotaCredito
10. Implementar suite de tests unitarios para GeneradorClaveAcceso y máquina de estados EstadoSri
11. Agregar rate limiting en endpoints de emisión
12. Implementar enforcement de multi-tenancy via JWT claims

Los hallazgos C-1 y C-3 son los de mayor impacto inmediato: la emisión anónima es explotable ahora mismo, y la contraseña del certificado en texto plano compromete la firma digital tributaria de todas las empresas registradas.