using SQLite;

namespace Examen_Empleados.Models
{
    public class Empleado
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public decimal Salario { get; set; }
        public string Correo { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
    }
}