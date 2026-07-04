using System.ComponentModel.DataAnnotations;

namespace BeeKeeperApp.Models.Entities
{
    public enum TipoApiario
    {
        Fijo,
        Trasladable
    }

    public class Apiario
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public TipoApiario Tipo { get; set; }
        [Required]
        public string SeccionPolicial { get; set; } = string.Empty;
        [Required]
        public string Zona { get; set; } = string.Empty;
        public bool TrashumanciaHabilitada { get; set; }
        public string? Departamento { get; set; }
        public string? Paraje { get; set; }
        public ICollection<Colmena> Colmenas { get; set; } = new List<Colmena>();
        public ICollection<Extraccion> Extracciones { get; set; } = new List<Extraccion>();
    }
}
