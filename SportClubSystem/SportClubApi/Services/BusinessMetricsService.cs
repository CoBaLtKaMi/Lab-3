using Prometheus;
using SportClubApi.Data;

namespace SportClubApi.Services;

public class BusinessMetricsService
{
    private readonly AppDbContext _db;
    
    // Бизнес-метрики
    private static readonly Gauge ActiveMembershipsCount = Metrics.CreateGauge(
        "sportclub_active_memberships_total",
        "Количество активных абонементов"
    );
    
    private static readonly Gauge TotalMembersCount = Metrics.CreateGauge(
        "sportclub_members_total",
        "Всего членов клуба"
    );
    
    private static readonly Gauge RevenueCount = Metrics.CreateGauge(
        "sportclub_revenue_total",
        "Общая выручка от активных абонементов",
        new GaugeConfiguration { LabelNames = new[] { "currency" } }
    );
    
    private static readonly Gauge AverageMembershipPrice = Metrics.CreateGauge(
        "sportclub_membership_average_price",
        "Средняя стоимость абонемента",
        new GaugeConfiguration { LabelNames = new[] { "type" } }
    );
    
    private static readonly Gauge WorkoutFillPercentage = Metrics.CreateGauge(
        "sportclub_workout_fill_percentage",
        "Процент заполненности тренировки",
        new GaugeConfiguration { LabelNames = new[] { "workout_name" } }
    );
    
    private static readonly Gauge MembershipsByType = Metrics.CreateGauge(
        "sportclub_memberships_by_type",
        "Абонементы по типам",
        new GaugeConfiguration { LabelNames = new[] { "type", "status" } }
    );

    public BusinessMetricsService(AppDbContext db)
    {
        _db = db;
    }

    public Task UpdateMetricsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            // 1. Активные абонементы
            var activeMemberships = _db.Memberships.Where(m => m.EndDate > now).ToList();
            ActiveMembershipsCount.Set(activeMemberships.Count);

            // 2. Всего членов
            var totalMembers = _db.Members.Count();
            TotalMembersCount.Set(totalMembers);

            // 3. Выручка
            var revenue = activeMemberships.Sum(m => m.Price);
            RevenueCount.Labels("RUB").Set((double)revenue);

            // 4. Средняя цена по типам
            var membershipsByType = activeMemberships.GroupBy(m => m.Type)
                .Select(g => new { Type = g.Key, AvgPrice = g.Average(m => m.Price) })
                .ToList();
            
            foreach (var group in membershipsByType)
            {
                AverageMembershipPrice.Labels(group.Type).Set((double)group.AvgPrice);
            }

            // 5. Абонементы по типам и статусам
            var allMemberships = _db.Memberships.ToList();
            var membershipCounts = allMemberships
                .GroupBy(m => new { Type = m.Type, Status = m.EndDate > now ? "Active" : "Expired" })
                .Select(g => new { g.Key.Type, g.Key.Status, Count = g.Count() })
                .ToList();
            
            foreach (var group in membershipCounts)
            {
                MembershipsByType.Labels(group.Type, group.Status).Set(group.Count);
            }

            // 6. Заполненность тренировок
            var workouts = _db.Workouts.ToList();
            foreach (var workout in workouts)
            {
                var registered = _db.WorkoutRegistrations.Count(r => r.WorkoutId == workout.Id);
                var fillPct = workout.MaxParticipants > 0 
                    ? (double)registered / workout.MaxParticipants * 100 
                    : 0;
                WorkoutFillPercentage.Labels(workout.Name).Set(fillPct);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка обновления метрик: {ex.Message}");
        }
        return Task.CompletedTask;
    }
}
