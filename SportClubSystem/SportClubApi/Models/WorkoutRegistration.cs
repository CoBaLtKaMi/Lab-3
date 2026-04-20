namespace SportClubApi.Models;

public class WorkoutRegistration
{
    public int Id { get; set; }
    public int MemberId { get; set; }     // Внешний ключ на Member
    public int WorkoutId { get; set; }    // Внешний ключ на Workout
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public Member Member { get; set; } = null!;
    public Workout Workout { get; set; } = null!;
}
