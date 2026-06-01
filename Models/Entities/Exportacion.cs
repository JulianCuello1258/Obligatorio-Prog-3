using System.ComponentModel.DataAnnotations;

namespace BeeKeeperApp.Models.Entities
{
    public class Exportacion
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CantidadBarriles { get; set; }
        [Required]
        public string Destino { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public ICollection<Extraccion> Extracciones { get; set; } = new List<Extraccion>();
    }
}
