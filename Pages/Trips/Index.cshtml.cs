using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuestPlanner.Pages.Trips
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Trip> Trips { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var tripsToUpdate = await _context.Trips
                    .Where(t => t.UserId == userId && t.Status != TripStatus.Completed)
                    .ToListAsync();

                foreach (var trip in tripsToUpdate)
                {
                    if (trip.EndDate < DateTime.Today)
                    {
                        trip.Status = TripStatus.Completed;
                    }
                    else if (trip.StartDate <= DateTime.Today)
                    {
                        trip.Status = TripStatus.InProgress;
                    }
                }
                await _context.SaveChangesAsync();

                Trips = await _context.Trips
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.StartDate)
                    .ToListAsync();
            }
        }
        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
            }

            trip.Status = TripStatus.Completed;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}