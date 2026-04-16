using System.ComponentModel.DataAnnotations;

namespace SportClubApi.Models;

public class Workout
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Trainer { get; set; } = string.Empty;

    public DateTime StartsAt { get; set; }

    [Range(10, 300)]
    public int DurationMinutes { get; set; }

    [Range(1, 100)]
    public int Capacity { get; set; }

    // Навигационное свойство
    public ICollection<WorkoutRegistration> Registrations { get; set; } = new List<WorkoutRegistration>();
}