# Sistema de Control de Infracciones de Tránsito

## Autores

ALCIVAR CORDOBA PEDRO LUIS
CEDEÑO RODRIGUEZ CARLOS LUIS
LOOR VERA JORDY LENIN
RODRIGUEZ SALVATIERRA ENIS ANDERI
VALENCIA RAMIREZ JHON ROBERT

---

# Descripción del Proyecto

Este proyecto implementa un sistema distribuido para la gestión de infracciones de tránsito utilizando una arquitectura basada en eventos.

El sistema permite registrar infracciones detectadas por cámaras, enviarlas mediante Azure Service Bus, generar multas automáticamente y aplicar mecanismos de resiliencia para evitar pérdida de información cuando existen fallos en la comunicación.

---

# Objetivos

- Registrar infracciones de tránsito.
- Procesar eventos de forma asíncrona.
- Generar multas automáticamente.
- Implementar mecanismos de resiliencia.
- Gestionar mensajes inválidos mediante DLQ.
- Permitir consultas de multas por placa.

---

# Arquitectura General

ClienteCamara

↓

ApiIngesta

↓

PostgreSQL (Infracciones)

↓

Azure Service Bus

↓

ApiMultas

↓

PostgreSQL (Multas)

↓

PortalCiudadano

---

# Arquitectura de Resiliencia

ApiIngesta

↓

Service Bus caído

↓

MensajesPendientes

↓

Worker de Reintento

↓

Reenvío Automático

---

# Arquitectura DLQ

Mensaje Inválido

↓

Validación

↓

Dead Letter Queue (DLQ)

---

# Tecnologías Utilizadas

- .NET 10
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Azure Service Bus Emulator
- .NET Aspire
- OpenAPI
- PgAdmin

---

# Estructura de la Solución

```text
ControlTransito
│
├── ApiIngesta
├── ApiMultas
├── ClienteCamara
├── PortalCiudadano
├── Shared.Contracts
└── ControlTransito.AppHost
```

# Shared.Contracts

Proyecto compartido que contiene los contratos utilizados entre los microservicios.

Evento utilizado:

```csharp
InfraccionDetectadaEvent
```

Este contrato permite que ApiIngesta y ApiMultas intercambien información utilizando el mismo formato.

---

# ApiIngesta

Responsable de recibir y almacenar las infracciones.

Funciones principales:

- Recibir infracciones.
- Guardar infracciones en PostgreSQL.
- Publicar eventos en Azure Service Bus.
- Aplicar resiliencia cuando el broker está caído.

### Endpoint

```http
POST /api/infracciones
```

### Ejemplo

```json
{
  "placa": "ABC1234",
  "velocidad": 95,
  "limiteVelocidad": 50,
  "fechaDeteccion": "2026-07-20T00:00:00Z"
}
```

---

# Azure Service Bus

Se utiliza para desacoplar los servicios.

Cola utilizada:

```text
infracciones-velocidad
```

Flujo:

```text
ApiIngesta
      ↓
Azure Service Bus
      ↓
ApiMultas
```

---

# ApiMultas

Responsable de consumir eventos y generar multas.

Funciones principales:

- Escuchar la cola de eventos.
- Validar mensajes.
- Crear multas automáticamente.
- Gestionar la Dead Letter Queue.

El consumidor fue implementado utilizando:

```csharp
BackgroundService
```

Proceso:

```text
Mensaje recibido
      ↓
Validación
      ↓
Creación de multa
      ↓
Guardar en PostgreSQL
```

---

# Base de Datos

## Tabla Infracciones

Registra las infracciones recibidas.

Campos principales:

```text
Id
Placa
Velocidad
LimiteVelocidad
FechaDeteccion
```

---

## Tabla Multas

Registra las multas generadas.

Campos principales:

```text
Id
Placa
Valor
FechaEmision
Pagada
```

---

## Tabla MensajesPendientes

Almacena mensajes cuando el broker de mensajería no está disponible.

Campos:

```text
Id
Payload
FechaCreacion
Procesado
```

---

# Resiliencia

## Problema

Cuando Azure Service Bus está apagado, el mensaje no puede enviarse.

Sin resiliencia:

```text
ApiIngesta
      ↓
Error
      ↓
Pérdida de información
```

---

## Solución Implementada

```text
ApiIngesta
      ↓
Intento de envío
      ↓
Error
      ↓
Guardar en MensajesPendientes
```

De esta forma no se pierde información.

---

# Worker de Reintento

Se implementó un servicio en segundo plano encargado de reenviar automáticamente los mensajes pendientes.

Proceso:

```text
Buscar mensajes pendientes
      ↓
Intentar reenviar
      ↓
Éxito
      ↓
Procesado = true
```

Beneficios:

- Recuperación automática.
- No requiere intervención manual.
- Evita pérdida de datos.

---

# Dead Letter Queue (DLQ)

Se implementó para gestionar mensajes inválidos.

Ejemplos:

```json
{
  "placa": "",
  "velocidad": -10
}
```

Validaciones:

- Placa vacía.
- Velocidad menor o igual a cero.
- Mensajes corruptos o inválidos.

Proceso:

```text
Mensaje inválido
      ↓
Validación
      ↓
DeadLetterMessageAsync()
      ↓
DLQ
```

Beneficios:

- Evita ciclos infinitos de reprocesamiento.
- Facilita auditoría y monitoreo.
- Mejora la estabilidad del sistema.

---

# PortalCiudadano

Permite consultar las multas generadas.

### Consultar todas las multas

```http
GET /api/multas
```

### Consultar multas por placa

```http
GET /api/multas/{placa}
```

Ejemplo:

```http
GET /api/multas/ABC1234
```

Respuesta:

```json
[
  {
    "placa": "ABC1234",
    "valor": 150,
    "pagada": false
  }
]
```

---

# Casos de Prueba

## Caso 1: Flujo Normal

```text
Crear infracción
      ↓
ApiIngesta
      ↓
Service Bus
      ↓
ApiMultas
      ↓
Multa creada
```

Resultado esperado:

- Registro en Infracciones.
- Registro en Multas.

---

## Caso 2: Broker Caído

```text
Service Bus apagado
      ↓
Crear infracción
      ↓
MensajesPendientes
```

Resultado esperado:

- Registro en MensajesPendientes.
- No pérdida de información.

---

## Caso 3: Recuperación Automática

```text
Service Bus encendido
      ↓
Worker detecta pendientes
      ↓
Reenvío
      ↓
Multa creada
```

Resultado esperado:

- Procesado = true.
- Registro en Multas.

---

## Caso 4: Dead Letter Queue

```text
Mensaje inválido
      ↓
DLQ
```

Resultado esperado:

- No se genera multa.
- Se registra en la cola de errores.

---

# Conclusiones

El sistema implementa una arquitectura distribuida basada en eventos que permite procesar infracciones de tránsito de forma desacoplada y resiliente.

Características implementadas:

✅ Registro de infracciones

✅ Comunicación asíncrona mediante Service Bus

✅ Generación automática de multas

✅ Persistencia de mensajes fallidos

✅ Reenvío automático de mensajes

✅ Dead Letter Queue (DLQ)

✅ Consulta de multas por placa

✅ Tolerancia a fallos del broker

✅ Arquitectura basada en microservicios con Aspire

El proyecto cumple los requisitos de procesamiento distribuido, resiliencia y tolerancia a fallos solicitados para el sistema de control de tránsito.