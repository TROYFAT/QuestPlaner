using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuestPlanner.Data;
using QuestPlanner.Models;
using System.Diagnostics;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var config = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var mainConnectionString = config.GetConnectionString("DefaultConnection");
        var connBuilder = new NpgsqlConnectionStringBuilder(mainConnectionString);
        var dumpPath = Path.Combine(env.ContentRootPath, "Data", "dump.sql");
        bool databaseCreated = false;

        // 1. Проверка существования БД
        bool databaseExists;
        try
        {
            db.Database.OpenConnection();
            db.Database.CloseConnection();
            databaseExists = true;
        }
        catch (PostgresException ex) when (ex.SqlState == "3D000") // БД не существует
        {
            databaseExists = false;
        }

        // 2. Создание БД если не существует
        if (!databaseExists)
        {
            logger.LogInformation("База данных не существует, создаем...");

            using (var conn = new NpgsqlConnection($"Host={connBuilder.Host};Port={connBuilder.Port};Username={connBuilder.Username};Password={connBuilder.Password}"))
            {
                conn.Open();
                using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{connBuilder.Database}\"", conn);
                cmd.ExecuteNonQuery();
            }

            logger.LogInformation("База данных {Database} создана", connBuilder.Database);
            databaseExists = true;
            databaseCreated = true;
        }

        // 3. Восстановление из дампа для новой БД
        if (databaseCreated && File.Exists(dumpPath))
        {
            logger.LogInformation("Восстановление данных из дампа...");

            Environment.SetEnvironmentVariable("PGPASSWORD", connBuilder.Password);
            var processInfo = new ProcessStartInfo
            {
                FileName = "psql",
                Arguments = $"-U {connBuilder.Username} -d {connBuilder.Database} -f \"{dumpPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            using (var process = new Process())
            {
                process.StartInfo = processInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        logger.LogError("Ошибка psql: {Error}", e.Data);
                };

                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    logger.LogInformation("Данные успешно восстановлены из дампа");
                }
                else
                {
                    logger.LogError("Ошибка восстановления. Код: {ExitCode}", process.ExitCode);
                }
            }
        }

        // 4. Всегда применяем миграции (для новых изменений)
        if (databaseExists)
        {
            if (db.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Применение миграций...");
                db.Database.Migrate();
                logger.LogInformation("Миграции успешно применены");
            }
        }

        // 5. Восстановление для существующей БД (если пустая)
        if (databaseExists && !databaseCreated && File.Exists(dumpPath) && !db.Users.Any())
        {
            logger.LogInformation("Восстановление данных из дампа...");

            Environment.SetEnvironmentVariable("PGPASSWORD", connBuilder.Password);
            var processInfo = new ProcessStartInfo
            {
                FileName = "psql",
                Arguments = $"-U {connBuilder.Username} -d {connBuilder.Database} -f \"{dumpPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            using (var process = new Process())
            {
                process.StartInfo = processInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        logger.LogError("Ошибка psql: {Error}", e.Data);
                };

                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    logger.LogInformation("Данные успешно восстановлены из дампа");
                }
                else
                {
                    logger.LogError("Ошибка восстановления. Код: {ExitCode}", process.ExitCode);
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Критическая ошибка инициализации БД");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated &&
        context.Request.Path == "/")
    {
        context.Response.Redirect("/Identity/Account/Login");
        return;
    }
    await next();
});

app.MapRazorPages();

app.MapGet("/api/trips/gantt", async (ApplicationDbContext context, IHttpContextAccessor httpContext) => {
    if (!httpContext.HttpContext.User.Identity.IsAuthenticated)
        return Results.Unauthorized();

    var userId = httpContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

    return Results.Ok(await context.Trips
        .Where(t => t.UserId == userId)
        .Include(t => t.Activities)
        .Select(t => new {
            id = t.Id,
            title = t.Title,
            startDate = t.StartDate,
            endDate = t.EndDate,
            activities = t.Activities.Select(a => new {
                id = a.Id,
                name = a.Name,
                startTime = a.StartTime,
                endTime = a.EndTime,
                progress = a.Progress
            })
        })
        .ToListAsync());
}).RequireAuthorization();

app.Run();