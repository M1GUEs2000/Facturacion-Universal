# Facturacion-Universal

API .NET 8 para facturacion electronica SRI Ecuador.

## Estado de conexion

- Supabase conectado y verificado contra el proyecto `vqrhoihdhilhktgqdnxn`.
- EF Core usa PostgreSQL/Supabase mediante `ConnectionStrings:DefaultConnection`.
- La cadena de conexion local esta configurada con `dotnet user-secrets` en `Facturacion.Api`; no se guarda password en `appsettings.json`.
- La migracion inicial `20260511150330_InitialCreate` quedo registrada como aplicada en `public.__EFMigrationsHistory`.
- La migracion `20260511200328_AddParametrosFacturacion` quedo aplicada; agrega secuenciales SRI y `parametros_facturacion`.
- La migracion `20260511202300_RemoveParametrosCorreoYCamposNoUsados` quedo aplicada; elimina correo/SMTP, numero de digitos y tipo identificador comprador de `parametros_facturacion`.
- `dotnet ef database update` termino correctamente y la API compila.

## Base de datos

La app usa el schema `facturacion` en Supabase. Las tablas existentes fueron conservadas; se completaron las columnas faltantes necesarias para que coincidan con el modelo EF Core actual.

Parametros disponibles:
- `secuenciales_sri`: secuenciales y codigo numerico por empresa/tipo de comprobante (`01`, `04`, `07`).
- `parametros_facturacion`: ambiente, punto de emision, RIMPE, contribuyente especial, obligado contabilidad e IVA por defecto por empresa.
