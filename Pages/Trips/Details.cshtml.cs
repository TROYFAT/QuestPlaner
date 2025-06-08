using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System;
using System.Linq;

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
            var days = (date - Trip.StartDate).TotalDays;
            return (int)(days * 30); // 30px на день
        }

        public int GetDurationWidth(DateTime start, DateTime end)
        {
            var days = (end - start).TotalDays + 1;
            return (int)(days * 30); // 30px на день
        }

        public decimal GetTotalCost()
        {
            return Trip.BasePrice + Trip.Activities.Sum(a => a.Price);
        }
    }
}