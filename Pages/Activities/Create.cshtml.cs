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
            // Получаем поездку и проверяем права доступа
            Trip = await _context.Trips
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == TripId);

            if (Trip == null)
            {
                return NotFound();
            }

            // Проверяем, что текущий пользователь - владелец поездки
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Trip.UserId != currentUserId)
            {
                return Forbid();
            }

            // Устанавливаем значения по умолчанию
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

            // Загружаем связанную поездку
            var trip = await _context.Trips.FindAsync(Activity.TripId);
            if (trip == null)
            {
                ModelState.AddModelError(string.Empty, "Поездка не найдена");
                return Page();
            }

            // Проверка дат активности
            if (Activity.EndTime <= Activity.StartTime)
            {
                ModelState.AddModelError("Activity.EndTime", "Дата окончания должна быть позже даты начала");
            }

            // Проверка соответствия дат поездке
            if (Activity.StartTime < trip.StartDate)
            {
                ModelState.AddModelError("Activity.StartTime",
                    $"Дата начала активности не может быть раньше начала поездки ({trip.StartDate:dd.MM.yyyy})");
            }

            if (Activity.EndTime > trip.EndDate)
            {
                ModelState.AddModelError("Activity.EndTime",
                    $"Дата окончания активности не может быть позже окончания поездки ({trip.EndDate:dd.MM.yyyy})");
            }

            if (!ModelState.IsValid)
            {
                Trip = trip; // Обязательно устанавливаем Trip для отображения на странице
                return Page();
            }

            // Проверяем права доступа
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (trip.UserId != currentUserId)
            {
                return Forbid();
            }

            // Валидация модели
            if (!ModelState.IsValid)
            {
                // Добавим более информативное логирование
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Page();
            }

            // Проверка дат активности
            if (Activity.EndTime <= Activity.StartTime)
            {
                ModelState.AddModelError("Activity.EndTime", "Дата окончания должна быть позже даты начала");
                return Page();
            }

            try
            {
                // Фиксируем часовой пояс
                Activity.StartTime = DateTime.SpecifyKind(Activity.StartTime, DateTimeKind.Unspecified);
                Activity.EndTime = DateTime.SpecifyKind(Activity.EndTime, DateTimeKind.Unspecified);
                // Добавляем активность
                _context.Activities.Add(Activity);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index", new { tripId = Activity.TripId });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении активности");
                ModelState.AddModelError(string.Empty, "Ошибка при сохранении данных");
                Trip = trip;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка при создании активности");
                ModelState.AddModelError(string.Empty, "Произошла непредвиденная ошибка");
                Trip = trip;
                return Page();
            }
        }
    }
}