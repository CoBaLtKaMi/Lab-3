namespace SportClubApi.Models;

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;   // ФИО
    public string Phone { get; set; } = string.Empty;      // Телефон
    public string Email { get; set; } = string.Empty;      // Email (уникальный)
    public DateTime BirthDate { get; set; }                // Дата рождения
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства (EF Core заполнит автоматически)
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<WorkoutRegistration> WorkoutRegistrations { get; set; } = new List<WorkoutRegistration>();
}
