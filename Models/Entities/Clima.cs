using Microsoft.EntityFrameworkCore;

namespace BeeKeeperApp.Models.Entities
{
    [Owned]
    public class Clima
    {
        public double? Temperatura { get; set; }
        public double? Humedad { get; set; }
        public double? Presion { get; set; }
        public double? VelocidadViento { get; set; }
        public string? DireccionViento { get; set; }
    }
}
