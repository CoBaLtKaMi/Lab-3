using SportClubApi.Models;

namespace SportClubApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Members.Any()) return; // уже заполнено — пропускаем

        var now = DateTime.UtcNow;

        var members = new List<Member>
        {
            new() { FullName="Иван Иванов", Phone="+79161234567",
                    Email="ivan@example.com",
                    BirthDate=DateTime.SpecifyKind(new DateTime(1995,5,15), DateTimeKind.Utc),
                    RegisteredAt=now },
            new() { FullName="Анна Петрова", Phone="+79169876543",
                    Email="anna@example.com",
                    BirthDate=DateTime.SpecifyKind(new DateTime(2000,3,22), DateTimeKind.Utc),
                    RegisteredAt=now },
            new() { FullName="Сергей Смирнов", Phone="+79991112233",
                    Email="sergey@example.com",
                    BirthDate=DateTime.SpecifyKind(new DateTime(1988,11,5), DateTimeKind.Utc),
                    RegisteredAt=now },
        };
        db.Members.AddRange(members);
        db.SaveChanges();

        var workouts = new List<Workout>
        {
            new() { Name="Силовая тренировка", Description="Базовые упражнения со штангой и гантелями",
                    Trainer="Дмитрий Волков",
                    StartsAt=DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1), DateTimeKind.Utc),
                    DurationMinutes=60, MaxParticipants=15 },
            new() { Name="Йога для начинающих", Description="Расслабляющая практика для новичков",
                    Trainer="Ольга Смирнова",
                    StartsAt=DateTime.SpecifyKind(DateTime.UtcNow.AddDays(2), DateTimeKind.Utc),
                    DurationMinutes=45, MaxParticipants=20 },
            new() { Name="Кроссфит", Description="Высокоинтенсивная интервальная тренировка",
                    Trainer="Алексей Быков",
                    StartsAt=DateTime.SpecifyKind(DateTime.UtcNow.AddDays(3), DateTimeKind.Utc),
                    DurationMinutes=50, MaxParticipants=12 },
        };
        db.Workouts.AddRange(workouts);
        db.SaveChanges();

        db.Memberships.AddRange(
            new Membership
            {
                MemberId = members[0].Id,
                Type = "Premium",
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(335),
                Price = 4500m,
                Status = "Active"
            },
            new Membership
            {
                MemberId = members[1].Id,
                Type = "Standard",
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(20),
                Price = 2500m,
                Status = "Active"
            },
            new Membership
            {
                MemberId = members[2].Id,
                Type = "Student",
                StartDate = now.AddDays(-60),
                EndDate = now.AddDays(-1),
                Price = 1500m,
                Status = "Expired"
            }
        );

        db.WorkoutRegistrations.AddRange(
            new WorkoutRegistration { MemberId = members[0].Id, WorkoutId = workouts[0].Id, RegisteredAt = now },
            new WorkoutRegistration { MemberId = members[1].Id, WorkoutId = workouts[0].Id, RegisteredAt = now },
            new WorkoutRegistration { MemberId = members[0].Id, WorkoutId = workouts[1].Id, RegisteredAt = now }
        );
        db.SaveChanges();
    }
}
