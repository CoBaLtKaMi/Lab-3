using Microsoft.EntityFrameworkCore;
using SportClubApi.Models;

namespace SportClubApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        var now = DateTime.UtcNow;

        // Проверяем, есть ли уже данные
        if (context.Members.Any())
        {
            Console.WriteLine("✅ Данные уже существуют, seeding пропущен.");
            return;
        }

        Console.WriteLine("🌱 Добавляем тестовые данные...");

        // Участники
        var member1 = new Member
        {
            FullName = "Иван Иванов",
            Phone = "+79161234567",
            Email = "ivan@example.com",
            BirthDate = DateTime.SpecifyKind(new DateTime(1995, 5, 15), DateTimeKind.Utc),
            RegisteredAt = now
        };

        var member2 = new Member
        {
            FullName = "Анна Петрова",
            Phone = "+79169876543",
            Email = "anna@example.com",
            BirthDate = DateTime.SpecifyKind(new DateTime(2000, 3, 22), DateTimeKind.Utc),
            RegisteredAt = now
        };

        context.Members.AddRange(member1, member2);
        context.SaveChanges();

        // Тренировки
        var workout1 = new Workout
        {
            Name = "Силовая тренировка",
            Description = "Тренировка на основные группы мышц",
            DurationMinutes = 60,
            MaxParticipants = 15
        };

        var workout2 = new Workout
        {
            Name = "Йога для начинающих",
            Description = "Расслабляющая практика",
            DurationMinutes = 45,
            MaxParticipants = 20
        };

        context.Workouts.AddRange(workout1, workout2);
        context.SaveChanges();

        // Абонемент
        var membership1 = new Membership
        {
            MemberId = member1.Id,
            Type = "Premium",
            StartDate = now.AddDays(-30),
            EndDate = now.AddDays(300),
            Price = 4500.00m,
            Status = "Active"
        };

        context.Memberships.Add(membership1);
        context.SaveChanges();

        // Запись на тренировку
        var registration1 = new WorkoutRegistration
        {
            MemberId = member1.Id,
            WorkoutId = workout1.Id,
            RegisteredAt = now
        };

        context.WorkoutRegistrations.Add(registration1);
        context.SaveChanges();

        Console.WriteLine($"✅ Seeding успешно завершён! Добавлено участников: {context.Members.Count()}");
    }
}