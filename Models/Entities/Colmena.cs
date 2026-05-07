using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeKeeperApp.Models.Entities
{
    public enum EstadoColmena
    {
        Activa,
        Inactiva,
        Perdida
    }

    public class Colmena
    {
        public int Id { get; set; }

        [Required]
        public int ApiarioId { get; set; }

        [Required]
        public EstadoColmena Estado { get; set; } = EstadoColmena.Activa;

        [Required]
        public string Tipo { get; set; } = string.Empty; // e.g., Langstroth, Dadant

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public string? Poblacion { get; set; } // Fuerza (Fuerte, Media, Debil)
        public string? Temperamento { get; set; } // Mansa, Agresiva

        // Navigation
        [ForeignKey("ApiarioId")]
        public Apiario? Apiario { get; set; }

        public Reina? Reina { get; set; }
        public ICollection<Revision> Revisiones { get; set; } = new List<Revision>();
        public ICollection<Extraccion> Extracciones { get; set; } = new List<Extraccion>();
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }

    public class Reina
    {
        [Key, ForeignKey("Colmena")]
        public int ColmenaId { get; set; }

        public string Salud { get; set; } = "Buena";
        public bool Presencia { get; set; } = true;
        public DateTime? FechaNacimiento { get; set; }

        // Navigation
        public Colmena? Colmena { get; set; }
    }
}
