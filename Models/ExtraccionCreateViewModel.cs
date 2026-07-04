using System;
using System.ComponentModel.DataAnnotations;

namespace BeeKeeperApp.Models
{
    public class ExtraccionCreateViewModel
    {
        [Required]
        public string TipoRegistro { get; set; } = "Colmena"; // "Colmena" or "Apiario"

        public int? ApiarioId { get; set; }

        public int? ColmenaId { get; set; }

        [Required(ErrorMessage = "La cantidad en Kg es requerida.")]
        [Range(0.1, 10000.0, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public double CantidadKg { get; set; }

        [Required(ErrorMessage = "La fecha es requerida.")]
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
