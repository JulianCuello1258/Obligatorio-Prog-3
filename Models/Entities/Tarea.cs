using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BeeKeeperApp.Models.Entities
{
    public class Tarea : IValidatableObject
    {
        [Key]
        public int Id { get; set; }
        public int? ApiarioId { get; set; }
        public int? ColmenaId { get; set; }
        [Required]
        public string Titulo { get; set; } = string.Empty;
        [Required]
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaProgramada { get; set; }
        public bool Completada { get; set; }
        public Colmena? Colmena { get; set; }
        public Apiario? Apiario { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!ApiarioId.HasValue && !ColmenaId.HasValue)
            {
                yield return new ValidationResult(
                    "Una tarea debe estar asignada a un Apiario o a una Colmena como mínimo.",
                    new[] { nameof(ApiarioId), nameof(ColmenaId) }
                );
            }
        }
    }
}
