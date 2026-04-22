using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Prometheus;
using SportClubApi.Data;
using SportClubApi.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Строка подключения к PostgreSQL ─────────────────────────────
var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Database=sportclub;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connStr));

// ── 2. Redis ────────────────────────────────────────────────────────
var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddScoped<CacheService>();

// ── 3. Стандартные сервисы ──────────────────────────────────────────
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ── 4. Статические файлы для веб-интерфейсов ───────────────────────


// ── 5. CORS — разрешаем запросы из браузера ────────────────────────
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();


// ── 6. Миграции и тестовые данные ──────────────────────────────────
Console.WriteLine("🚀 Применяем миграции...");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    Console.WriteLine("✅ Миграции успешно применены.");
    DbSeeder.Seed(db);
    Console.WriteLine("✅ Seeding завершён.");
}

// ── 7. Middleware ───────────────────────────────────────────────────
app.UseHttpMetrics();
app.UseCors();
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SportClub API v1");
    c.RoutePrefix = "swagger";
});

app.MapMetrics();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

// ── 8. Редиректы для открытия веб-интерфейсов ─────────────────────
app.MapGet("/admin", ctx => { ctx.Response.Redirect("/admin/index.html"); return Task.CompletedTask; });
app.MapGet("/client", ctx => { ctx.Response.Redirect("/client/index.html"); return Task.CompletedTask; });

app.Run();
