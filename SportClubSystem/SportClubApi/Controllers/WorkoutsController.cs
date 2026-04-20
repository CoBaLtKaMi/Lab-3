using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;

namespace SportClubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkoutsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workout>>> GetWorkouts()
    {
        return await _context.Workouts.ToListAsync();
    }

    // POST api/workouts/{workoutId}/register
    [HttpPost("{workoutId}/register")]
    public async Task<IActionResult> Register(int workoutId, [FromBody] int memberId)
    {
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

        return Ok(new { message = "Запись отменена" });
    }
}