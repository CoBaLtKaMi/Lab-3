namespace SportClubApi.Models;

public class Membership
{
    public int Id { get; set; }
    public int MemberId { get; set; }              // Внешний ключ на Member
    // Типы: Standard, Premium, Student
    public string Type { get; set; } = "Standard";
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    // Статусы: Active, Expired, Cancelled
    public string Status { get; set; } = "Active";

    public Member? Member { get; set; }
}
