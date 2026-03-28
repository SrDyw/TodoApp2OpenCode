namespace TodoApp2OpenCode.Models
{

    public class UsuarioDTO
    {
        public string Id { get; set; }
        public string Usuario { get; set; }
        public string Nombre_Completo { get; set; }
        public bool Esta_Activo { get; set; }
        public DateTime Fecha_Alta { get; set; }
        public DateTime? Fecha_Baja { get; set; }
        public UnidadDto Unidad { get; set; }
        public SexoDto Sexo { get; set; }
        public AreaDto Area { get; set; }
        public CargoDto Cargo { get; set; }
        public List<RolDto> Roles { get; set; }
       
    }
    public class UnidadDto
    {
        public string Dpa { get; set; }
        public NivelDto Nivel { get; set; }

       

    }

    public class NivelDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }

       

    }

    public class SexoDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }

       

    }

    public class AreaDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }

       

    }

    public class CargoDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }

       

    }

    public class RolDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public bool EsAdministrativo { get; set; }

       
    }
}
