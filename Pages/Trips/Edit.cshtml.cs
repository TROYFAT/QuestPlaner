using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuestPlanner.Pages.Trips
{
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (Trip == null)
            {
                return NotFound();
            }

            if (Trip.Status == TripStatus.Completed)
            {
                return RedirectToPage("./Details", new { id });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ��������� ��������� �������
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingTrip = await _context.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == Trip.Id && t.UserId == userId);

            if (existingTrip == null)
            {
                return NotFound();
            }

            if (existingTrip.Status == TripStatus.Completed)
            {
                ModelState.AddModelError("", "����������� ������� ������ �������������");
                return Page();
            }

            // �������� ���
            if (Trip.EndDate <= Trip.StartDate)
            {
                ModelState.AddModelError("Trip.EndDate", "���� ��������� ������ ���� ����� ���� ������");
                return Page();
            }

            // ������������� ������������ ����
            Trip.UserId = existingTrip.UserId;
            Trip.Status = existingTrip.Status;

            try
            {
                // ��������� ������ ���������� ����
                var tripToUpdate = await _context.Trips.FindAsync(Trip.Id);
                if (tripToUpdate == null)
                {
                    return NotFound();
                }

                tripToUpdate.Title = Trip.Title;
                tripToUpdate.Destination = Trip.Destination;
                tripToUpdate.PeopleCount = Trip.PeopleCount;
                tripToUpdate.BasePrice = Trip.BasePrice;
                tripToUpdate.StartDate = Trip.StartDate;
                tripToUpdate.EndDate = Trip.EndDate;
                tripToUpdate.Progress = Trip.Progress;

                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "������ ��� ���������� �������");
                if (!TripExists(Trip.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "������ ��� ���������� ���������. ����������, ���������� �����.");
                    return Page();
                }
            }
        }

        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }
    }
}