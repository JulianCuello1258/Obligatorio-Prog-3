using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeKeeperApp.Models.Entities
{
    public class Revision
    {
        [Key]
        public int Id { get; set; }
        public int? ApiarioId { get; set; }
        public int? ColmenaId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        [Required]
        public TipoRevision Tipo { get; set; } = TipoRevision.Rutinaria;
        public string? Observaciones { get; set; }
        public string? Sintomas { get; set; }
        public string? Enfermedades { get; set; }
        public string? Tratamiento { get; set; }
        public string? Dosis { get; set; }
        public DateTime? ProximaDosis { get; set; }
        public NivelPoblacion? PoblacionEstimada { get; set; }
        public TipoTemperamento? Temperamento { get; set; }
        public bool ReinaPresente { get; set; }
        public SaludReina? ReinaSalud { get; set; }
        public bool HayCrias { get; set; }
        public string? Plagas { get; set; }
        public GradoInfestacion? NivelInfestacion { get; set; }
        public string? ResultadoTratamiento { get; set; }
        public bool Enjambrazon { get; set; }
        public Clima? CondicionesClimaticas { get; set; }
        [ForeignKey("ColmenaId")]
        public Colmena? Colmena { get; set; }
        [ForeignKey("ApiarioId")]
        public Apiario? Apiario { get; set; }
    }
}
