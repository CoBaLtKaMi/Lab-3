namespace SportClubApi.Models;

public class Workout
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;         // Название тренировки
    public string Description { get; set; } = string.Empty;  // Описание
    public string Trainer { get; set; } = string.Empty;      // ФИО тренера
    public DateTime StartsAt { get; set; }                   // Дата и время начала
    public int DurationMinutes { get; set; }                 // Длительность (минуты)
    public int MaxParticipants { get; set; }                 // Макс. участников

    public ICollection<WorkoutRegistration> Registrations { get; set; } = new List<WorkoutRegistration>();
}
