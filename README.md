# Kanvax - Kanban Task Board

FlowBoard es una aplicación moderna de tablero Kanban construida con Blazor y MudBlazor. Permite a los usuarios gestionar sus tareas en un formato visual de columnas con soporte para arrastrar y soltar.

## Características

### Autenticación
- Registro de usuarios con nombre de usuario y contraseña
- Inicio de sesión con validación
- Persistencia de sesión en localStorage

### Tableros
- Crear múltiples tableros con nombre y descripción
- Compartir tableros con otros usuarios (participantes)
-URLs dedicadas por tablero (`/board/{boardId}`)
- Dashboard con lista de tableros del usuario

### Kanban
- Columnas personalizables (nombre, color)
- Tareas con:
  - Título y descripción
  - Prioridad (Normal, Alta, Baja)
  - Completar/marcar tareas
  - Asignar usuarios
- Drag & drop para mover tareas entre columnas
- Drag & drop para reordenar columnas

### Sistema de Participantes
- Propietario del tablero tiene control total
- Participantes pueden ver y editar el tablero
- Solo el propietario puede añadir/remover participantes
- Lista de usuarios en menú desplegable

### Logs de Actividad
- Registro de todas las operaciones
- Vista de tabla con filtros (CRUD)
- Carga bajo demanda

### UI/UX
- Diseño moderno estilo Windows 11
- Actualizaciones optimistas (sin skeletons después de la carga inicial)
- Barra de estado de sincronización
- Componentes MudBlazor

## Tecnologías

- **Frontend**: Blazor Server (.NET 9)
- **UI Framework**: MudBlazor
- **Persistencia**: localStorage
- **Estilos**: CSS con variables de MudBlazor

## Estructura del Proyecto

```
TodoApp2OpenCode/
├── Components/
│   ├── Pages/          # Páginas razor
│   │   ├── Home.razor       # Landing page
│   │   ├── Dashboard.razor # Dashboard de tableros
│   │   ├── Todo.razor      # Vista principal del kanban
│   │   └── BoardNotFound.razor
│   ├── Dialogs/        # Componentes de diálogo
│   ├── Layout/         # Layouts
│   └── Routes.razor    # Configuración de rutas
├── Services/           # Servicios
│   ├── AuthService.cs      # Autenticación
│   ├── BoardService.cs     # Gestión de tableros
│   ├── TodoService.cs      # Gestión de tareas/columnas
│   ├── LogService.cs       # Logs de actividad
│   └── BottomNavService.cs # Navegación inferior
├── Models/             # Modelos de datos
└── Program.cs          # Punto de entrada
```

## Cómo Ejecutar

1. Restaurar dependencias:
   ```bash
   dotnet restore
   ```

2. Compilar:
   ```bash
   dotnet build
   ```

3. Ejecutar:
   ```bash
   dotnet run
   ```

4. Abrir navegador en: `http://localhost:5000`

## Usage

### Crear un Tablero
1. Iniciar sesión o registrarse
2. En el dashboard, hacer click en "Nuevo Tablero"
3. Ingresar nombre y descripción (opcional)
4. El tablero se crea y redirige automáticamente

### Añadir Tareas
1. Estar en un tablero
2. Click en "Nueva Tarea"
3. Llenar título, descripción, prioridad y columna
4. Guardar

### Gestionar Columnas
1. Click en los 3 puntos de una columna
2. Editar nombre/color o eliminar
3. Arrastrar el encabezado para reordenar

### Compartir Tablero
1. Click en menú "Participantes"
2. Click en "Añadir participante"
3. Buscar usuario por nombre
4. Confirmar

### Ver Logs
1. Click en botón "Ver Logs"
2. Ver tabla de actividad del tablero
3. Click en "Ver Board" para volver
