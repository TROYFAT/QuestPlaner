using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuestPlanner.Pages.Activities
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Trip Trip { get; set; }
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public async Task<IActionResult> OnGetAsync(int? tripId)
        {
            if (tripId == null)
            {
                return NotFound();
            }

            Trip = await _context.Trips
                .Include(t => t.Activities)
                .FirstOrDefaultAsync(m => m.Id == tripId);

            if (Trip == null)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Trip.UserId != currentUserId) return Forbid();

            Activities = Trip.Activities?.ToList() ?? new List<Activity>();

            return Page();
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();

            // Проверка прав доступа
            var trip = await _context.Trips.FindAsync(activity.TripId);
            if (trip.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { tripId = activity.TripId });
        }
    }
}