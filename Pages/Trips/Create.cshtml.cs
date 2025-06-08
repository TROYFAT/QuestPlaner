using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QuestPlanner.Pages.Trips
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Trip Trip { get; set; }

        public IActionResult OnGet()
        {
            // ������������� �������� �� ���������
            Trip = new Trip
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Progress = 0,
                PeopleCount = 1,
                BasePrice = 0
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("������ ��������� POST ������� ��� �������� �������");

            // ������� ������ ��������� ��� UserId � User, ��� ��� ��� ��������������� ��������
            ModelState.Remove("Trip.UserId");
            ModelState.Remove("Trip.User");

            // �������� ������
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("������ �� �������. ������: {@Errors}",
                    ModelState.Values.SelectMany(v => v.Errors));
                return Page();
            }

            // �������� ���
            if (Trip.EndDate <= Trip.StartDate)
            {
                _logger.LogWarning("���� ��������� {EndDate} ������ ���� ����� ���� ������ {StartDate}",
                    Trip.EndDate, Trip.StartDate);
                ModelState.AddModelError("Trip.EndDate", "���� ��������� ������ ���� ����� ���� ������");
                return Page();
            }

            // �������� � �������� ������������
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("������������ �� ����������������. ��������������� �� �������� �����");
                return RedirectToPage("/Identity/Account/Login");
            }

            // ������������� UserId
            Trip.UserId = userId;
            _logger.LogInformation("������� ��������� � ������������ {UserId}", userId);

            try
            {
                // ��������, ��� ���� ����� ���������� Kind
                Trip.StartDate = DateTime.SpecifyKind(Trip.StartDate, DateTimeKind.Unspecified);
                Trip.EndDate = DateTime.SpecifyKind(Trip.EndDate, DateTimeKind.Unspecified);

                _context.Trips.Add(Trip);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                // �������� ���������� ������
                _logger.LogError(ex.InnerException ?? ex, "������ ���� ������ ��� �������� �������");

                // �������� SQL-������ � ���������
                try
                {
                    var entries = _context.ChangeTracker.Entries()
                        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                        .ToList();

                    _logger.LogInformation("Entries to save: {Count}", entries.Count);
                    foreach (var entry in entries)
                    {
                        _logger.LogInformation("Entry: {Entity} - {State}", entry.Entity.GetType().Name, entry.State);
                        foreach (var property in entry.Properties)
                        {
                            _logger.LogInformation("  {Property}: {Value}", property.Metadata.Name, property.CurrentValue);
                        }
                    }
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "������ ��� ����������� ��������� ���������");
                }

                ModelState.AddModelError(string.Empty, "������ ��� ���������� ������. ����������, ���������� �����.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "����������� ������ ��� �������� �������");
                ModelState.AddModelError(string.Empty, "��������� �������������� ������. ����������, ���������� �����.");
                return Page();
            }
        }
    }
}