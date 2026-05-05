# CLAUDE.md — Facturación Universal API

API .NET 8 universal para facturación electrónica SRI Ecuador. Sin UI — consumible desde cualquier sistema. Para contexto de negocio, decisiones y estado del proyecto ver el vault: `d:\Obsidian\Bovedá\proyectos\facturacion-universal-api.md`.

## Build & Run

Abrir `FacturacionUniversal.sln` en Visual Studio 2022.
- **API**: establecer `Facturacion.Api` como proyecto de inicio, corre en IIS Express.
- **Tests**: `dotnet test` desde la raíz.

## Estructura de proyectos

| Proyecto | Rol |
|---|---|
| `Facturacion.Core` | Dominio puro — entidades, enums, interfaces, casos de uso. Sin dependencias externas |
| `Facturacion.Infraestructura` | EF Core, SRI HttpClient, FirmaXadesNet, QuestPDF, Serilog |
| `Facturacion.Api` | Minimal APIs, JWT, Scalar/OpenAPI, FluentValidation, middleware |
| `Facturacion.Tests` | xUnit — unitarios + integración + E2E SRI |

**Dependencias:**
```
Facturacion.Api → Core + Infraestructura
Facturacion.Infraestructura → Core
Facturacion.Tests → Core + Infraestructura
```

## Estructura de carpetas

```
FacturacionUniversal/
├── Facturacion.Core/
│   ├── CasosDeUso/
│   │   ├── Facturas/
│   │   ├── NotasCredito/
│   │   ├── Retenciones/
│   │   └── Empresas/
│   ├── Entidades/
│   ├── Enums/
│   ├── Errores/
│   └── Interfaces/
│       ├── Repositorios/
│       └── Servicios/
├── Facturacion.Infraestructura/
│   ├── Persistencia/
│   │   ├── Contexto/
│   │   ├── Repositorios/
│   │   └── Configuraciones/    ← configuraciones de entidades EF Core
│   └── Servicios/
│       ├── Sri/                ← HttpClient SOAP al SRI
│       ├── Firma/              ← FirmaXadesNet (XAdES-BES)
│       ├── Pdf/                ← QuestPDF (RIDE)
│       └── Xml/                ← generación XML con XmlSerializer
├── Facturacion.Api/
│   ├── Endpoints/
│   │   ├── Facturas/
│   │   ├── NotasCredito/
│   │   ├── Retenciones/
│   │   └── Empresas/
│   ├── Contratos/              ← DTOs de request/response por módulo
│   │   ├── Facturas/
│   │   ├── NotasCredito/
│   │   ├── Retenciones/
│   │   └── Empresas/
│   ├── Extensions/             ← registro de servicios, middleware, etc.
│   └── Middleware/
└── Facturacion.Tests/
    ├── Unitarios/
    └── Integracion/
```

## Stack

| Capa | Paquete |
|---|---|
| Result pattern | `ErrorOr` |
| ORM | `Npgsql.EntityFrameworkCore.PostgreSQL` + `EF Core Tools` |
| PDF RIDE | `QuestPDF` |
| Logging | `Serilog` / `Serilog.AspNetCore` |
| Validación | `FluentValidation` / `FluentValidation.AspNetCore` |
| Firma XAdES | `FirmaXadesNetCore` |
| Docs API | `Scalar.AspNetCore` |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Tests | `FluentAssertions` + `NSubstitute` |

## Base de datos

Schema `facturacion` en Supabase (PostgreSQL). Tablas: `empresas`, `facturas`, `facturas_detalle`, `notas_credito`, `notas_credito_detalle`, `retenciones`, `retenciones_detalle`, `logs`. Ver vault para schema completo: `d:\Obsidian\Bovedá\research\facturacion-universal-bd.md`.

## Cerebro del proyecto

> ⚠️ **OBLIGATORIO leer el vault PRIMERO.** Todo lo que no sea código técnico vive ahí: estado actual, pendientes, decisiones de arquitectura, análisis y contexto de negocio. **No re-derivar esa información leyendo código — es un gasto de tokens innecesario.**

### Archivos de entrada obligatorios (leer en este orden)

| Archivo | Qué contiene |
|---|---|
| `d:\Obsidian\Bovedá\proyectos\facturacion-universal\README.md` | Estado actual, decisiones clave, stack |
| `d:\Obsidian\Bovedá\proyectos\facturacion-universal\arquitectura-facturacion-universal.md` | Mapa de módulos y qué nodos están ✅ vs ⚠️ |
| `d:\Obsidian\Bovedá\proyectos\facturacion-universal\tareas.md` | Tareas pendientes (P-XXX / CU-XXXXX) |
| `d:\Obsidian\Bovedá\CLAUDE.md` | Convenciones del vault — leer solo si hay dudas de estructura |

**Flujo de sesión:**
1. Leer `README.md` → estado actual
2. Leer `arquitectura-facturacion-universal.md` → qué nodos están ✅ completo vs ⚠️ pendiente
3. Ir al nodo `nodos/[modulo].md` si está ✅ — confiar en él, no leer código fuente
4. Si el nodo está ⚠️ → leer código fuente y documentar el nodo al terminar
5. Al cerrar sesión → actualizar `## 📌 Estado actual` en README

### Post-commit — mantener nodos sincronizados

Después de cada `git push`, GitHub Actions postea un comentario en el commit listando qué nodos del vault pueden estar desactualizados. **Revisar ese comentario y actualizar los nodos afectados en Obsidian antes de cerrar la sesión.** Ver `.github/node-map.yml` para el mapeo completo de archivos → nodos.
