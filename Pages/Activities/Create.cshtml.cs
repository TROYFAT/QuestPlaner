using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace QuestPlanner.Pages.Activities
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            ApplicationDbContext context,
            ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int TripId { get; set; }

        [BindProperty]
        public Activity Activity { get; set; }

        public Trip Trip { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // �������� ������� � ��������� ����� �������
            Trip = await _context.Trips
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == TripId);

            if (Trip == null)
            {
                return NotFound();
            }

            // ���������, ��� ������� ������������ - �������� �������
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Trip.UserId != currentUserId)
            {
                return Forbid();
            }

            // ������������� �������� �� ���������
            Activity = new Activity
            {
                TripId = TripId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                Progress = 0
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Activity.Trip");
            ModelState.Remove("Activity.TripId");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ��������� ��������� �������
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

            // ��������� ����� �������
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (trip.UserId != currentUserId)
            {
                return Forbid();
            }

            // ��������� ������
            if (!ModelState.IsValid)
            {
                // ������� ����� ������������� �����������
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Page();
            }

            // �������� ��� ����������
            if (Activity.EndTime <= Activity.StartTime)
            {
                ModelState.AddModelError("Activity.EndTime", "���� ��������� ������ ���� ����� ���� ������");
                return Page();
            }

            try
            {
                // ��������� ������� ����
                Activity.StartTime = DateTime.SpecifyKind(Activity.StartTime, DateTimeKind.Unspecified);
                Activity.EndTime = DateTime.SpecifyKind(Activity.EndTime, DateTimeKind.Unspecified);
                // ��������� ����������
                _context.Activities.Add(Activity);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index", new { tripId = Activity.TripId });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "������ ��� ���������� ����������");
                ModelState.AddModelError(string.Empty, "������ ��� ���������� ������");
                Trip = trip;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�������������� ������ ��� �������� ����������");
                ModelState.AddModelError(string.Empty, "��������� �������������� ������");
                Trip = trip;
                return Page();
            }
        }
    }
}