namespace SportClubApi.Models;

public class WorkoutRegistration
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public int WorkoutId { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public Member Member { get; set; } = null!;
    public Workout Workout { get; set; } = null!;
}