using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeKeeperApp.Models.Entities
{
    public class Extraccion
    {
        [Key]
        public int Id { get; set; }
        public int? ColmenaId { get; set; }
        public int? ApiarioId { get; set; }
        public double CantidadKg { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public int? ExportacionId { get; set; }

        // Navigation
        [ForeignKey("ColmenaId")]
        public Colmena? Colmena { get; set; }

        [ForeignKey("ApiarioId")]
        public Apiario? Apiario { get; set; }

        [ForeignKey("ExportacionId")]
        public Exportacion? Exportacion { get; set; }
    }
}
