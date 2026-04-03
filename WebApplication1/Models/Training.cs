namespace WebApplication1.Models;

public class Training
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member? Member { get; set; }

    public DateTime DateTime { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}