using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System.Threading.Tasks;

namespace QuestPlanner.Pages.Activities
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int TripId { get; set; }

        [BindProperty]
        public Activity Activity { get; set; }

        public Trip Trip { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Trip = await _context.Trips.FindAsync(TripId);

            if (Trip == null)
            {
                return NotFound();
            }

            Activity = new Activity
            {
                StartTime = Trip.StartDate.AddHours(10),
                EndTime = Trip.StartDate.AddHours(12),
                Progress = 0
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Trip = await _context.Trips.FindAsync(TripId);
            if (Trip == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Activity.EndTime <= Activity.StartTime)
            {
                ModelState.AddModelError("Activity.EndTime", "Дата окончания должна быть позже даты начала");
                return Page();
            }

            if (Activity.StartTime < Trip.StartDate || Activity.EndTime > Trip.EndDate)
            {
                ModelState.AddModelError("", "Активность должна полностью находиться в рамках поездки");
                return Page();
            }

            // Устанавливаем TripId и сохраняем
            Activity.TripId = TripId;
            _context.Activities.Add(Activity);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { tripId = TripId });
        }
    }
}