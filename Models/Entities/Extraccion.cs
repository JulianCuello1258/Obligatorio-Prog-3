using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeKeeperApp.Models.Entities
{
    public class Extraccion
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ColmenaId { get; set; }
        public double CantidadKg { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public int? ExportacionId { get; set; }

        // Navigation
        [ForeignKey("ColmenaId")]
        public Colmena? Colmena { get; set; }

        [ForeignKey("ExportacionId")]
        public Exportacion? Exportacion { get; set; }
    }
}
