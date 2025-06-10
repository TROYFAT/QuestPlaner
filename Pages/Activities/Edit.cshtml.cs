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
            // ������� ������ ��������� ��� ������������� �������
            ModelState.Remove("Activity.Trip");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ��������� ��������� ������� ��� �������� ����
            var trip = await _context.Trips.FindAsync(Activity.TripId);
            if (trip == null)
            {
                ModelState.AddModelError(string.Empty, "������� �� �������");
                return Page();
            }

            // �������� ��� ����������
            if (Activity.EndTime <= Activity.StartTime)
            {
                ModelState.AddModelError("Activity.EndTime", "���� ��������� ������ ���� ����� ���� ������");
                return Page();
            }

            // �������� ������������ ��� �������
            if (Activity.StartTime < trip.StartDate)
            {
                ModelState.AddModelError("Activity.StartTime",
                    $"���� ������ ���������� �� ����� ���� ������ ������ ������� ({trip.StartDate:dd.MM.yyyy})");
            }

            if (Activity.EndTime > trip.EndDate)
            {
                ModelState.AddModelError("Activity.EndTime",
                    $"���� ��������� ���������� �� ����� ���� ����� ��������� ������� ({trip.EndDate:dd.MM.yyyy})");
            }

            if (!ModelState.IsValid)
            {
                Trip = trip; // ����������� ������������� Trip ��� ����������� �� ��������
                return Page();
            }

            // �������� ���� �������
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (trip.UserId != currentUserId)
            {
                return Forbid();
            }

            try
            {
                // ������������� ���������� kind ��� ���
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