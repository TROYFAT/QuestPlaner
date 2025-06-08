using Microsoft.AspNetCore.Identity;

namespace QuestPlanner.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public List<Trip> Trips { get; set; } = new List<Trip>();
    }
}
