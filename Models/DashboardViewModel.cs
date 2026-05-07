using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Models
{
    public class DashboardViewModel
    {
        public int TotalApiarios { get; set; }
        public int TotalColmenasActivas { get; set; }
        public int TareasPendientes { get; set; }
        public List<Revision> UltimasRevisiones { get; set; } = new();
    }
}
