using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System.Threading.Tasks;

namespace QuestPlanner.Pages.Activities
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Activity == null)
            {
                return NotFound();
            }

            Trip = Activity.Trip;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Перезагружаем поездку при ошибках валидации
                Trip = await _context.Trips.FindAsync(Activity.TripId);
                return Page();
            }

            // Проверяем существование поездки
            var trip = await _context.Trips.FindAsync(Activity.TripId);
            if (trip == null)
            {
                return NotFound();
            }

            // Проверка дат активности
            if (Activity.EndTime <= Activity.StartTime)
            {
                ModelState.AddModelError("Activity.EndTime", "Дата окончания должна быть позже даты начала");
                Trip = trip;
                return Page();
            }

            // Проверка в рамках поездки
            if (Activity.StartTime < trip.StartDate || Activity.EndTime > trip.EndDate)
            {
                ModelState.AddModelError("", "Активность должна полностью находиться в рамках поездки");
                Trip = trip;
                return Page();
            }

            // Обновляем активность
            var activityToUpdate = await _context.Activities.FindAsync(Activity.Id);
            if (activityToUpdate == null)
            {
                return NotFound();
            }

            activityToUpdate.Name = Activity.Name;
            activityToUpdate.Price = Activity.Price;
            activityToUpdate.Location = Activity.Location;
            activityToUpdate.StartTime = Activity.StartTime;
            activityToUpdate.EndTime = Activity.EndTime;
            activityToUpdate.Progress = Activity.Progress;
            activityToUpdate.TripId = Activity.TripId;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index", new { tripId = Activity.TripId });
        }
    }
}