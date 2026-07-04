using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeKeeperApp.Models.Entities
{
    public class Trashumancia
    {
        [Key]
        public int Id { get; set; }
        public int ApiarioOrigenId { get; set; }
        public int ApiarioDestinoId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public double DistanciaKm { get; set; }
        [Required]
        public int ColmenaId { get; set; }
        [ForeignKey("ColmenaId")]
        public Colmena? Colmena { get; set; }

        [ForeignKey("ApiarioOrigenId")]
        public Apiario? ApiarioOrigen { get; set; }
        [ForeignKey("ApiarioDestinoId")]
        public Apiario? ApiarioDestino { get; set; }
    }
}
