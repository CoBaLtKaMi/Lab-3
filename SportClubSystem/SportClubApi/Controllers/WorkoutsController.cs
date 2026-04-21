using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;
using SportClubApi.Services;
using System.Text.Json;

namespace SportClubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CacheService _cache;

    public WorkoutsController(AppDbContext context, CacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workout>>> GetWorkouts()
    {
        return await _context.Workouts.ToListAsync();
    }

    // GET api/workouts/{id}/registrations
    [HttpGet("{id}/registrations")]
    public async Task<IActionResult> GetRegistrationCount(int id)
    {
        var count = await _context.WorkoutRegistrations
            .CountAsync(r => r.WorkoutId == id);
        return Ok(new { count = count });
    }

    // POST api/workouts - создать новую тренировку
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutRequest request)
    {
        if (string.IsNullOrEmpty(request.Name))
            return BadRequest("Название обязательно");
        if (string.IsNullOrEmpty(request.Trainer))
            return BadRequest("Тренер обязателен");
        if (request.DurationMinutes <= 0)
            return BadRequest("Длительность должна быть больше 0");
        if (request.MaxParticipants <= 0)
            return BadRequest("Максимальное количество участников должно быть больше 0");

        var workout = new Workout
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Trainer = request.Trainer,
            StartsAt = request.StartsAt,
            DurationMinutes = request.DurationMinutes,
            MaxParticipants = request.MaxParticipants
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkouts), new { id = workout.Id }, workout);
    }

    // DELETE api/workouts/{id} - удалить тренировку
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var workout = await _context.Workouts.FindAsync(id);
        if (workout == null)
            return NotFound();

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST api/workouts/{workoutId}/register
    [HttpPost("{workoutId}/register")]
    public async Task<IActionResult> Register(int workoutId)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        
        if (string.IsNullOrEmpty(body))
            return BadRequest("Body cannot be empty");

        int memberId;
        
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("memberId", out var memberIdElem))
            {
                memberId = memberIdElem.GetInt32();
            }
            else
            {
                return BadRequest("memberId not found in request body");
            }
        }
        catch
        {
            return BadRequest("Invalid JSON format");
        }

        var workout = await _context.Workouts.FindAsync(workoutId);
        if (workout == null)
            return NotFound("Тренировка не найдена");

        var alreadyExists = await _context.WorkoutRegistrations
            .AnyAsync(r => r.WorkoutId == workoutId && r.MemberId == memberId);

        if (alreadyExists)
            return BadRequest("Участник уже записан на эту тренировку");

        var registration = new WorkoutRegistration
        {
            WorkoutId = workoutId,
            MemberId = memberId,
            RegisteredAt = DateTime.UtcNow
        };

        _context.WorkoutRegistrations.Add(registration);
        await _context.SaveChangesAsync();
        
        // Инвалидировать кэш участника
        await _cache.RemoveAsync($"member:{memberId}");

        return Ok(new { message = "Запись успешно создана" });
    }

    // DELETE api/workouts/{workoutId}/unregister/{memberId}
    [HttpDelete("{workoutId}/unregister/{memberId}")]
    public async Task<IActionResult> Unregister(int workoutId, int memberId)
    {
        var reg = await _context.WorkoutRegistrations
            .FirstOrDefaultAsync(r => r.WorkoutId == workoutId && r.MemberId == memberId);

        if (reg == null)
            return NotFound("Запись не найдена");

        _context.WorkoutRegistrations.Remove(reg);
        await _context.SaveChangesAsync();
        
        // Инвалидировать кэш участника
        await _cache.RemoveAsync($"member:{memberId}");

        return Ok(new { message = "Запись отменена" });
    }
}

public class CreateWorkoutRequest
{
    public string Name { get; set; }
    public string Trainer { get; set; }
    public string Description { get; set; }
    public DateTime StartsAt { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxParticipants { get; set; }
}
