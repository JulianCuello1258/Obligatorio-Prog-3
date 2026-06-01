using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeKeeperApp.Models.Entities
{
    public class Reina
    {
        [Key]
        public int ColmenaId { get; set; }
        public SaludReina Salud { get; set; } = SaludReina.Buena;
        public bool Presencia { get; set; } = true;
        public DateTime? FechaNacimiento { get; set; }
        [ForeignKey("ColmenaId")]
        public Colmena? Colmena { get; set; }
    }
}
