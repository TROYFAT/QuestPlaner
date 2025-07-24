using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPlanner.Models;
using QuestPlanner.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace QuestPlanner.Pages.Activities
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ApplicationDbContext context, ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Activity Activity { get; set; }

        public Trip Trip { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Activity = await _context.Activities
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (Activity == null)
            {
                return NotFound();
            }

            Trip = Activity.Trip;

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Trip.UserId != currentUserId)
            {
                return Forbid();
            }
            if (Activity.TripId != Trip.Id)
            {
                ModelState.AddModelError(string.Empty, "Несоответствие идентификатора поездки");
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Trip = await _context.Trips.FindAsync(Activity.TripId);
            if (Trip == null)
            {
                return NotFound();
            }

            ModelState.Remove("Activity.Trip");

            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Trip.UserId != currentUserId)
            {
                return Forbid();
            }

            if (Activity.EndTime <= Activity.StartTime)
            {
                ModelState.AddModelError("Activity.EndTime", "Дата окончания должна быть позже даты начала");
                return Page(); 
            }

            if (Activity.StartTime < Trip.StartDate)
            {
                ModelState.AddModelError("Activity.StartTime",
                    $"Дата начала активности не может быть раньше начала поездки ({Trip.StartDate:dd.MM.yyyy})");
            }

            if (Activity.EndTime > Trip.EndDate)
            {
                ModelState.AddModelError("Activity.EndTime",
                    $"Дата окончания активности не может быть позже окончания поездки ({Trip.EndDate:dd.MM.yyyy})");
            }

            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            try
            {
                Activity.StartTime = DateTime.SpecifyKind(Activity.StartTime, DateTimeKind.Unspecified);
                Activity.EndTime = DateTime.SpecifyKind(Activity.EndTime, DateTimeKind.Unspecified);

                _context.Attach(Activity).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index", new { tripId = Activity.TripId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityExists(Activity.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ActivityExists(int id)
        {
            return _context.Activities.Any(e => e.Id == id);
        }
    }
}