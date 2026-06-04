# Política de Privacidad — Facturación Universal API

**Versión:** 1.0  
**Fecha de vigencia:** pendiente de publicación  
**Responsable del tratamiento:** [Nombre legal de la empresa]  
**Contacto:** [correo de privacidad]

---

## 1. Quién somos

Facturación Universal es una API de emisión de comprobantes electrónicos tributarios (facturas, notas de crédito y retenciones) para el Servicio de Rentas Internas del Ecuador (SRI). Actuamos como encargado del tratamiento de datos en nombre de las empresas emisoras que usan el servicio.

---

## 2. Datos que recopilamos

### 2.1 Datos de la cuenta

| Dato | Propósito |
|---|---|
| Plan contratado | Gestión del servicio |
| Límites de uso (empresas, usuarios) | Control de suscripción |
| Fecha de expiración | Renovación del servicio |

### 2.2 Datos de la empresa emisora

| Dato | Propósito |
|---|---|
| RUC | Identificación fiscal obligatoria |
| Razón social y nombre comercial | Emisión de comprobantes |
| Dirección matriz | Emisión de comprobantes |
| Certificado digital (.p12) y contraseña | Firma electrónica (SRI) |
| Logo | Generación del RIDE (representación impresa) |

### 2.3 Datos de comprobantes fiscales

Por cada comprobante se registra:

- Datos del emisor (RUC, razón social, dirección) — **embebidos en el momento de emisión**
- Datos del comprador o sujeto retenido (tipo y número de identificación, razón social, dirección)
- Detalle de productos o servicios facturados
- Valores económicos e impuestos
- Clave de acceso y número de autorización del SRI
- Dirección IP desde la cual se emitió el comprobante (auditoría)

### 2.4 Datos de uso y logs

Registro de operaciones: errores, reintentos y eventos del sistema. No incluyen datos personales de compradores.

---

## 3. Base legal del tratamiento

| Tratamiento | Base legal |
|---|---|
| Emisión de comprobantes | Obligación legal — LORTI y Reglamento de Comprobantes de Venta, Retención y Documentos Complementarios |
| Firma electrónica con SRI | Obligación legal |
| Almacenamiento de documentos fiscales | Obligación legal (ver sección 5) |
| Logs de auditoría | Interés legítimo — seguridad y trazabilidad |
| Datos de cuenta y empresa | Ejecución del contrato de servicio |

---

## 4. Compartición de datos

Los datos se transmiten únicamente a:

- **SRI (Servicio de Rentas Internas del Ecuador):** para la recepción y autorización de comprobantes electrónicos. Es una obligación legal.
- **Proveedor de infraestructura (Supabase):** almacenamiento de base de datos y archivos, ubicado en la región [São Paulo / EU West]. Sujeto a cláusulas estándar de protección de datos.

No vendemos ni cedemos datos personales a terceros para fines comerciales.

---

## 5. Retención de datos — excepción fiscal obligatoria

> **⚠️ Retención mínima de 7 años para documentos fiscales**
>
> El artículo 41 del Reglamento de Comprobantes de Venta, Retención y Documentos Complementarios del Ecuador establece la obligación de conservar los comprobantes electrónicos por un período **mínimo de 7 años** contados desde la fecha de emisión.
>
> En consecuencia, los datos contenidos en facturas, notas de crédito y retenciones **no pueden ser eliminados antes de que transcurra ese período**, incluso si el titular solicita la supresión. Esta excepción está reconocida por el artículo 22(c) del RGPD y normativas equivalentes, que permiten mantener datos cuando existe una obligación legal que lo requiere.

### Tabla de retención por tipo de dato

| Dato | Retención | Motivo |
|---|---|---|
| Comprobantes fiscales (facturas, NC, retenciones) | **7 años desde emisión** | Obligación legal LORTI |
| Documentos XML y RIDE PDF | **7 años desde emisión** | Obligación legal LORTI |
| Certificado digital (.p12) | Hasta eliminación de cuenta | No hay obligación de retención |
| Logo | Hasta eliminación de cuenta | No hay obligación de retención |
| Datos de cuenta y suscripción | Hasta eliminación de cuenta | No hay obligación de retención |
| Logs de sistema | 90 días | Auditoría operacional |

---

## 6. Derechos del titular de los datos

El titular de los datos tiene derecho a:

- **Acceso:** solicitar copia de sus datos.
- **Rectificación:** corregir datos inexactos.
- **Supresión ("derecho al olvido"):** solicitar la eliminación de datos. *Aplica con la excepción fiscal del punto 5.*
- **Portabilidad:** recibir sus datos en formato estructurado.
- **Oposición y limitación:** oponerse o limitar ciertos tratamientos.

Para ejercer estos derechos, contactar a: [correo de privacidad]

### Qué ocurre al eliminar una cuenta

Al solicitar la eliminación de la cuenta (`DELETE /cuenta`):

| Dato | Acción |
|---|---|
| Certificado digital (.p12) | **Eliminado inmediatamente** del almacenamiento |
| Logo | **Eliminado inmediatamente** del almacenamiento |
| Contraseña del certificado | **Eliminada inmediatamente** |
| Dirección y nombre comercial de la empresa | **Anonimizados inmediatamente** |
| Secuenciales y parámetros de facturación | **Eliminados inmediatamente** |
| Datos de la cuenta y suscripción | **Eliminados inmediatamente** |
| Comprobantes fiscales y sus datos | **Retenidos 7 años** por obligación legal; desvinculados de la cuenta |

---

## 7. Seguridad de los datos

- La contraseña del certificado digital se almacena cifrada (AES-256).
- El certificado .p12 y el logo se almacenan en Supabase Storage con acceso restringido; no son accesibles públicamente.
- Las comunicaciones usan TLS 1.2+ en tránsito.
- El acceso a la API requiere autenticación JWT.
- Las tablas de base de datos tienen Row Level Security (RLS) habilitado en producción.
- Se registra la IP de origen de cada comprobante emitido para auditoría.

---

## 8. Transferencias internacionales

Los datos se almacenan en servidores ubicados en [São Paulo, Brasil / EU West]. Las transferencias están cubiertas por las garantías contractuales del proveedor de infraestructura (Supabase, Inc.).

---

## 9. Cookies y tecnologías de seguimiento

Esta API no utiliza cookies. No hay interfaz de usuario que las requiera.

---

## 10. Cambios a esta política

Notificaremos cambios materiales con al menos 30 días de anticipación por correo electrónico al contacto registrado en la cuenta.

---

## 11. Contacto y autoridad de control

- **Responsable:** [Nombre legal] — [correo de privacidad]
- **Autoridad de control:** Dirección Nacional de Registro de Datos Públicos (DINARDAP) — Ecuador

---

*Última revisión: 04-06-2026*
