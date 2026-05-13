using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeKeeperApp.Models.Entities
{
    public class Revision
    {
        public int Id { get; set; }

        [Required]
        public int ColmenaId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public string Tipo { get; set; } = "Rutinaria"; // Rutinaria, Sanitaria, Extraccion

        public string? Observaciones { get; set; }
        public string? Sintomas { get; set; }
        public string? Enfermedades { get; set; }
        public string? Tratamiento { get; set; }
        public string? Dosis { get; set; }
        public DateTime? ProximaDosis { get; set; }

        public string? PoblacionEstimada { get; set; }
        public string? Temperamento { get; set; }

        public bool ReinaPresente { get; set; }
        public string? ReinaSalud { get; set; }
        public bool HayCrias { get; set; }

        // Nuevos campos
        public string? Plagas { get; set; }
        public string? NivelInfestacion { get; set; }
        public string? ResultadoTratamiento { get; set; }
        public bool Enjambrazon { get; set; }
        public double? Temperatura { get; set; }
        public double? Humedad { get; set; }
        public double? Presion { get; set; }
        public double? VelocidadViento { get; set; }
        public string? DireccionViento { get; set; }

        // Navigation
        [ForeignKey("ColmenaId")]
        public Colmena? Colmena { get; set; }
    }

    public class Extraccion
    {
        public int Id { get; set; }

        [Required]
        public int ColmenaId { get; set; }

        public double CantidadKg { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ColmenaId")]
        public Colmena? Colmena { get; set; }
    }

    public class Tarea
    {
        public int Id { get; set; }

        public int? ApiarioId { get; set; }
        public int? ColmenaId { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaProgramada { get; set; }
        public bool Completada { get; set; }

        // Navigation
        public Colmena? Colmena { get; set; }
        public Apiario? Apiario { get; set; }
    }

    public class Trashumancia
    {
        public int Id { get; set; }

        public int ApiarioOrigenId { get; set; }
        public int ApiarioDestinoId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
        public double DistanciaKm { get; set; }

        // Navigation
        [ForeignKey("ApiarioOrigenId")]
        public Apiario? ApiarioOrigen { get; set; }

        [ForeignKey("ApiarioDestinoId")]
        public Apiario? ApiarioDestino { get; set; }
    }

    public class Exportacion
    {
        public int Id { get; set; }
        
        [Required]
        public int CantidadBarriles { get; set; }
        
        [Required]
        public string Destino { get; set; } = string.Empty;
        
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
