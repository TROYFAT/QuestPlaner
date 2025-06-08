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
            // Устанавливаем значения по умолчанию
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
            _logger.LogInformation("Начало обработки POST запроса для создания поездки");

            // Удаляем ошибки валидации для UserId и User, так как они устанавливаются сервером
            ModelState.Remove("Trip.UserId");
            ModelState.Remove("Trip.User");

            // Проверка модели
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Модель не валидна. Ошибки: {@Errors}",
                    ModelState.Values.SelectMany(v => v.Errors));
                return Page();
            }

            // Проверка дат
            if (Trip.EndDate <= Trip.StartDate)
            {
                _logger.LogWarning("Дата окончания {EndDate} должна быть позже даты начала {StartDate}",
                    Trip.EndDate, Trip.StartDate);
                ModelState.AddModelError("Trip.EndDate", "Дата окончания должна быть позже даты начала");
                return Page();
            }

            // Привязка к текущему пользователю
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Пользователь не аутентифицирован. Перенаправление на страницу входа");
                return RedirectToPage("/Identity/Account/Login");
            }

            // Устанавливаем UserId
            Trip.UserId = userId;
            _logger.LogInformation("Поездка привязана к пользователю {UserId}", userId);

            try
            {
                // Убедимся, что даты имеют правильный Kind
                Trip.StartDate = DateTime.SpecifyKind(Trip.StartDate, DateTimeKind.Unspecified);
                Trip.EndDate = DateTime.SpecifyKind(Trip.EndDate, DateTimeKind.Unspecified);

                _context.Trips.Add(Trip);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                // Логируем внутреннюю ошибку
                _logger.LogError(ex.InnerException ?? ex, "Ошибка базы данных при создании поездки");

                // Логируем SQL-запрос и параметры
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
                    _logger.LogError(logEx, "Ошибка при логировании состояния контекста");
                }

                ModelState.AddModelError(string.Empty, "Ошибка при сохранении данных. Пожалуйста, попробуйте позже.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при создании поездки");
                ModelState.AddModelError(string.Empty, "Произошла непредвиденная ошибка. Пожалуйста, попробуйте позже.");
                return Page();
            }
        }
    }
}