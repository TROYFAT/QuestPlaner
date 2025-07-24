using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuestPlanner.Pages.Trips
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Trip Trip { get; set; }

        public int TotalTimelineWidth
        {
            get
            {
                if (Trip == null) return 0;
                var days = (Trip.EndDate.Date - Trip.StartDate.Date).Days + 1; 
                return days * 30;
            }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Trip = await _context.Trips
                .Include(t => t.Activities)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Trip == null)
            {
                return NotFound();
            }

            return Page();
        }

        public int GetDayOffset(DateTime date)
        {
            var days = (date.Date - Trip.StartDate.Date).Days;
            return days * 30;
        }

        public int GetDurationWidth(DateTime start, DateTime end)
        {
            var days = (end.Date - start.Date).Days + 1; 
            return days * 30;
        }

        public decimal GetTotalCost()
        {
            return Trip.BasePrice + Trip.Activities.Sum(a => a.Price);
        }
    }
}