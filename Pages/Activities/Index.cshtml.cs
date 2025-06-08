using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Data;
using QuestPlanner.Models;
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

            return Page();
        }
    }
}