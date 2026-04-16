using System.ComponentModel.DataAnnotations;

namespace SportClubApi.Models;

public class Member
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<WorkoutRegistration> WorkoutRegistrations { get; set; } = new List<WorkoutRegistration>();
}