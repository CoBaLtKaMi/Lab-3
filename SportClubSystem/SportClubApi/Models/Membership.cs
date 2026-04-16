using System.ComponentModel.DataAnnotations;

namespace SportClubApi.Models;

public class Membership
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    [Required]
    public string Type { get; set; } = "Standard"; // Standard, Premium, Student

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime EndDate { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; }

    public string Status { get; set; } = "Active"; // Active, Expired, Cancelled

    // Навигационное свойство
    public Member Member { get; set; } = null!;
}