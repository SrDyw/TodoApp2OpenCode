namespace TodoApp2OpenCode.Constants
{
    public static class SystemMessages
    {
        public const string PENDING_OPERATION = "Hay una operación pendiente, intentalo de nuevo cuando termine.";
        public const string ITEM_UPDATE_ERROR = "Ocurrió un error durante la actualización del elemento";
        public const string ITEM_ADDED = "La tarea :task fué añadida con éxito";
        public const string ITEM_UPDATED = "La tarea :task fué actualizada con éxito";
        public const string ITEM_NOT_EXISTS = "La tarea :task no existe o ha sido eliminada";
        public const string OPERATION_AUTH_REQUIRED = "\"Inicia sesión para poder usar esta funcionalidad.\"";
        public const string DASHBOARD_NOT_EXISTS = "El solicitado tablero no existe";
        public const string PERMISSION_DENIED = "No tienes permiso para realizar o acceder a esta funcionaliad";
        public const string COLUMN_ADDED = "La columna :column ha sido agredada con éxito";
        public const string COLUMN_DOESNT_EXISTS = "La columna objetivo no existe o ha sido eliminada";
        public const string COLUN_MOVED = "La columna se ha movido con éxito";
        public const string ADD_COLUMN_DENIED = "No tienes permiso para crear columnas";

        public const string NETWORK_OR_INTERNAL_ERROR = "Error de conexión o interno con la base de datos";
    }
}
