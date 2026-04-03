namespace WebApplication1.Models;

public class Subscription
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member? Member { get; set; }

    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public int? VisitsLeft { get; set; }
}