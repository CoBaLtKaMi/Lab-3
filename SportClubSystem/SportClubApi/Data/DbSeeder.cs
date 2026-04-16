using SportClubApi.Models;

namespace SportClubApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Members.Any())
            return; // уже засеяно

        // Создаём тестовых членов клуба
        var members = new List<Member>
        {
            new Member
            {
                FullName = "Иван Петров",
                Phone = "+79001112233",
                Email = "ivan@mail.ru",
                BirthDate = new DateTime(1990, 5, 12, 0, 0, 0, DateTimeKind.Utc)
            },
            new Member
            {
                FullName = "Мария Сидорова",
                Phone = "+79004445566",
                Email = "maria@mail.ru",
                BirthDate = new DateTime(1995, 8, 23, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        db.Members.AddRange(members);
        db.SaveChanges();

        // Создаём тренировки
        var workouts = new List<Workout>
        {
            new Workout
            {
                Title = "Йога",
                Trainer = "Ольга Смирнова",
                StartsAt = DateTime.UtcNow.AddDays(1),
                DurationMinutes = 60,
                Capacity = 15
            },
            new Workout
            {
                Title = "Бокс",
                Trainer = "Дмитрий Волков",
                StartsAt = DateTime.UtcNow.AddDays(2),
                DurationMinutes = 90,
                Capacity = 10
            }
        };

        db.Workouts.AddRange(workouts);
        db.SaveChanges();

        // Создаём абонемент для первого члена
        db.Memberships.Add(new Membership
        {
            MemberId = members[0].Id,
            Type = "Premium",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(335),
            Price = 5000m,
            Status = "Active"
        });

        // Записываем первого члена на первую тренировку
        db.WorkoutRegistrations.Add(new WorkoutRegistration
        {
            MemberId = members[0].Id,
            WorkoutId = workouts[0].Id
        });

        db.SaveChanges();
    }
}