#  **Restaurante Reservas App - Backend Solution**

##  _Overview_

Este repositorio contiene una **solución .NET 8 Web API** para la gestión de **reservas de restaurante**.  
El backend maneja entidades como **Mesas**, **Reservas** y **Clientes**, con:

- Autenticación **JWT**
- **Entity Framework Core** para acceso a datos
- Detección avanzada de conflictos de reservas

> ⚠️ El frontend (React) aún no está desarrollado, por lo que este README se centra en el **backend**.

---

##  _Arquitectura del proyecto_

La solución sigue una **arquitectura limpia** con separación por capas:

- **Restaurante.Api** → Controladores, DTOs y configuración del API  
- **Restaurante.Aplicacion** → Servicios de lógica de negocio e interfaces de repositorios  
- **Restaurante.Infraestructura** → Entidades, `DbContext`, e implementación de repositorios (incluye `IBaseRepository` genérico)  
- **Restaurante.Tests** → Pruebas unitarias con **xUnit** y **Moq**

---

##  _Características principales_

-  Autenticación y autorización basada en **JWT**
-  Operaciones CRUD para **Mesas**, **Reservas** y **Clientes**
-  Validación de reservas:
  - Sin solapamientos (se permiten reservas contiguas)
  - Verificación de capacidad por mesa
-  Consulta de disponibilidad basada en:
  - Tamaño del grupo
  - Fecha y hora
  - Duración

---

##  _Requisitos previos_

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- **SQL Server** o **LocalDB** (configuración en `appsettings.json`)
- Opcional:  
  - **Visual Studio 2022**
  - **VS Code** con extensiones de C#

---

##  _Cómo compilar_

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/softwareengdev/restaurante-reservas-app.git
   cd restaurante-reservas-app
   ```

2. **Restaurar los paquetes NuGet:**
   ```bash
   dotnet restore
   ```

3. **Compilar la solución:**
   ```bash
   dotnet build
   ```

Esto compila todos los proyectos: API, capa de aplicación, infraestructura y pruebas.

---

##  _Cómo ejecutar_

### 1. Configurar la base de datos

- Actualiza la cadena de conexión en  
  `Restaurante.Api/appsettings.Development.json`, por ejemplo:
  ```json
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RestauranteDb;Trusted_Connection=True;"
  ```

- Aplica las migraciones:
  ```bash
  dotnet ef migrations add InitialMigration --project Restaurante.Infraestructura --startup-project Restaurante.Api
  dotnet ef database update --project Restaurante.Infraestructura --startup-project Restaurante.Api
  ```

Esto crea las tablas para **Mesas**, **Reservas**, **Clientes** y **ASP.NET Identity**.

---

### 2. Ejecutar la API

```bash
dotnet run --project Restaurante.Api
```

Por defecto, la API se ejecuta en:  
🔗 [https://localhost:5001](https://localhost:5001)

- Accede a **Swagger** en `/swagger` para probar los endpoints.  
- Autentícate vía `/api/auth/login` o `/api/auth/register` para obtener tokens JWT.  
- Los roles iniciales (`Admin`, `User`) se crean automáticamente.

---

##  _Pruebas unitarias_

Las pruebas están en `Restaurante.Tests`, usando **xUnit** y **Moq**, centradas en la lógica de negocio.

Ejecutar todas las pruebas:
```bash
dotnet test
```

**Casos cubiertos:**
- Permitir reservas contiguas (sin solapamiento)
- Rechazar solapamientos de tiempo
- Verificar capacidad de mesa
- Consultas de disponibilidad correctas  
  (solo mesas adecuadas y sin conflictos)

---

##  _Modelado de entidades_

### **Mesa**
- `Capacidad` → tamaño máximo del grupo  
- `Numero`, `Ubicacion`, `Estado`  
- Relación uno-a-muchos con `Reservas`

### **Reserva**
- `FechaInicio` (`DateTime`)
- `Duracion` (`TimeSpan`, formato HH:MM)
- `Estado` ("Pendiente", "Confirmada", "Cancelada")
- `NumeroPersonas`
- FK → `Mesa`, `Cliente`

### **Cliente**
- `Nombre`, `Apellidos`, `Telefono`, `Email` (único)
- `PuntosLealtad`
- Relación uno-a-muchos con `Reservas`

---

##  _Validaciones y decisiones de diseño_

- **Relaciones:**  
  Uno-a-muchos (Mesa–Reserva, Cliente–Reserva), con delete restrict/cascade según el caso.  
- **Validación de solapamientos:**  
  `(start1 < end2) && (end1 > start2)`  
  → permite reservas contiguas (abutting).  
- **Capacidad:**  
  `Mesa.Capacidad >= NumeroPersonas`  
- **IDs:**  
  GUIDs para unicidad.  
- **Tiempo:**  
  UTC para coherencia; duración con `TimeSpan`.  
- **Seguridad:**  
  JWT + refresh tokens; rol `Admin` para operaciones sensibles.  
- **Base de datos:**  
  SQL Server; sin “soft delete” (eliminación directa).  
- **Rendimiento:**  
  Paginación y filtrado básico; índices sugeridos en `FechaInicio` y `Email`.  
- **Errores:**  
  Excepciones específicas (e.g., `InvalidOperationException` para conflictos).  
- **Frontend:**  
  Aún no implementado; API lista para integración React.  
- **Internacionalización:**  
  Mezcla EN/ES; tiempos en UTC.

---

##  _Contribuciones y soporte_

¿Encontraste un problema o quieres contribuir?  
👉 Abre un [**issue en GitHub**](https://github.com/softwareengdev/restaurante-reservas-app/issues)

---

##  _Licencia_
Este proyecto está bajo la licencia **MIT**.  
Consulta el archivo [`LICENSE`](./LICENSE) para más detalles.

---

> _Desarrollado con ❤️ en .NET 8 para mejorar la gestión de reservas de restaurantes._
