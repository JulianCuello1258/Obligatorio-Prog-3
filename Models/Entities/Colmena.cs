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
        [Key]
        public int Id { get; set; }
        [Required]
        public int ApiarioId { get; set; }
        [Required]
        public EstadoColmena Estado { get; set; } = EstadoColmena.Activa;
        [Required]
        public string Tipo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public NivelPoblacion? Poblacion { get; set; }
        public TipoTemperamento? Temperamento { get; set; }
        public double ProduccionAcumulada { get; set; } = 0.0;
        [ForeignKey("ApiarioId")]
        public Apiario? Apiario { get; set; }
        public Reina? Reina { get; set; }
        public ICollection<Revision> Revisiones { get; set; } = new List<Revision>();
        public ICollection<Extraccion> Extracciones { get; set; } = new List<Extraccion>();
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
