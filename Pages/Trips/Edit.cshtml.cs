using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPlanner.Models;
using QuestPlanner.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace QuestPlanner.Pages.Trips
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
        public Trip Trip { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Trip = await _context.Trips
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Trip == null)
            {
                return NotFound();
            }

            // �������� ���� �������
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Trip.UserId != currentUserId)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ������� ������ ��������� ��� �����, ������� �� �������������
            ModelState.Remove("Trip.UserId");
            ModelState.Remove("Trip.User");
            ModelState.Remove("Trip.Activities");

            var activities = await _context.Activities
                .Where(a => a.TripId == Trip.Id)
                .ToListAsync();

            foreach (var activity in activities)
            {
                if (activity.StartTime < Trip.StartDate)
                {
                    ModelState.AddModelError("Trip.StartDate",
                        $"���� ������ ������� �� ����� ���� ����� ���������� '{activity.Name}' ({activity.StartTime:dd.MM.yyyy})");
                }

                if (activity.EndTime > Trip.EndDate)
                {
                    ModelState.AddModelError("Trip.EndDate",
                        $"���� ��������� ������� �� ����� ���� ������ ���������� '{activity.Name}' ({activity.EndTime:dd.MM.yyyy})");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // �������� ���
            if (Trip.EndDate <= Trip.StartDate)
            {
                ModelState.AddModelError("Trip.EndDate", "���� ��������� ������ ���� ����� ���� ������");
                return Page();
            }

            // �������� ���� �������
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Trip.UserId != currentUserId)
            {
                return Forbid();
            }

            try
            {
                // ������������� ���������� kind ��� ���
                Trip.StartDate = DateTime.SpecifyKind(Trip.StartDate, DateTimeKind.Unspecified);
                Trip.EndDate = DateTime.SpecifyKind(Trip.EndDate, DateTimeKind.Unspecified);

                _context.Attach(Trip).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TripExists(Trip.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }
    }
}