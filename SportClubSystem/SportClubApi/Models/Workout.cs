using System.ComponentModel.DataAnnotations;

namespace SportClubApi.Models;

public class Workout
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public int MaxParticipants { get; set; }

    // Навигационное свойство
    public ICollection<WorkoutRegistration> Registrations { get; set; } = new List<WorkoutRegistration>();
}