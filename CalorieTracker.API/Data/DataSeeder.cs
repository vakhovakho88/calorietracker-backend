using CalorieTracker.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieTracker.API.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(ApplicationDbContext context)
        {
            // Only seed if the database is empty
            if (await context.Goals.AnyAsync() || await context.DailyLogs.AnyAsync())
            {
                return;
            }

            // Use a transaction for better performance
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Create test users (in a real app with authentication, these would be actual users)
                var users = new List<Guid>
                {
                    Guid.Parse("00000000-0000-0000-0000-000000000001"), // Test User 1
                    Guid.Parse("00000000-0000-0000-0000-000000000002"), // Test User 2
                    Guid.Parse("00000000-0000-0000-0000-000000000003")  // Test User 3
                };

                // Seed goals for each user
                var goals = new List<Goal>
                {
                    // User 1: Weight loss goal (calorie deficit)
                    new Goal
                    {
                        GoalId = Guid.NewGuid(),
                        UserId = users[0],
                        TargetKcals = 5000,            // Target to burn 5000 more calories than intake
                        TimeWindowDays = 30,           // Over 30 days
                        StartDate = DateTime.Now.Date.AddDays(-15)  // Started 15 days ago
                    },
                    
                    // User 2: Muscle building goal (calorie surplus)
                    new Goal
                    {
                        GoalId = Guid.NewGuid(),
                        UserId = users[1],
                        TargetKcals = -3000,           // Target to consume 3000 more calories than burned (for muscle gain)
                        TimeWindowDays = 60,           // Over 60 days
                        StartDate = DateTime.Now.Date.AddDays(-30)  // Started 30 days ago
                    },
                    
                    // User 3: Maintenance goal (balanced calories)
                    new Goal
                    {
                        GoalId = Guid.NewGuid(),
                        UserId = users[2],
                        TargetKcals = 0,               // Target to maintain current weight (balanced calories)
                        TimeWindowDays = 90,           // Over 90 days 
                        StartDate = DateTime.Now.Date.AddDays(-45)  // Started 45 days ago
                    }
                };

                await context.Goals.AddRangeAsync(goals);
                await context.SaveChangesAsync();

                // Seed daily logs for each user
                var dailyLogs = new List<DailyLog>();

                // User 1: Weight loss journey (calorie deficit)
                // Create realistic logs with weekday variations (more active on weekends, occasional cheat days)
                for (int i = 0; i < 15; i++)
                {
                    var date = goals[0].StartDate.AddDays(i);
                    var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    var isCheatDay = i % 7 == 6; // One cheat day per week (Sunday)

                    dailyLogs.Add(new DailyLog
                    {
                        DailyLogId = Guid.NewGuid(),
                        UserId = users[0],
                        Date = date,
                        KcalsBurn = isWeekend ? 2300 + Random(100, 300) : 2000 + Random(50, 150), // More active on weekends
                        KcalsIntake = isCheatDay ? 2400 + Random(100, 200) : 1600 + Random(100, 200) // Occasional cheat days
                    });
                }

                // User 2: Muscle building (calorie surplus with consistent gym routine)
                for (int i = 0; i < 30; i++)
                {
                    var date = goals[1].StartDate.AddDays(i);
                    var isGymDay = i % 7 != 0 && i % 7 != 3; // Gym 5 days a week, rest on Wednesday and Sunday
                    
                    dailyLogs.Add(new DailyLog
                    {
                        DailyLogId = Guid.NewGuid(),
                        UserId = users[1],
                        Date = date,
                        KcalsBurn = isGymDay ? 2800 + Random(100, 300) : 2000 + Random(50, 150), // Higher burn on gym days
                        KcalsIntake = 3000 + Random(200, 500) // High calorie diet for muscle building
                    });
                }

                // User 3: Maintenance (balanced with active lifestyle)
                for (int i = 0; i < 45; i++)
                {
                    var date = goals[2].StartDate.AddDays(i);
                    var activityLevel = GetActivityVariation(date); // Simulate different activity levels through the week
                    
                    dailyLogs.Add(new DailyLog
                    {
                        DailyLogId = Guid.NewGuid(),
                        UserId = users[2],
                        Date = date,
                        KcalsBurn = 2200 + activityLevel + Random(-100, 100), // Base metabolic rate + activity + variation
                        KcalsIntake = 2200 + Random(-200, 200) // Trying to match burn rate with small variations
                    });
                }

                // Insert logs in batches of 100 for better performance
                const int batchSize = 100;
                for (int i = 0; i < dailyLogs.Count; i += batchSize)
                {
                    var batch = dailyLogs.Skip(i).Take(batchSize).ToList();
                    await context.DailyLogs.AddRangeAsync(batch);
                    await context.SaveChangesAsync();
                }

                // Commit the transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // If an error occurs, roll back the transaction
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static readonly Random _random = new Random();

        private static int Random(int min, int max)
        {
            return _random.Next(min, max + 1);
        }

        private static int GetActivityVariation(DateTime date)
        {
            // Simulate a realistic weekly activity pattern
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday: return 200;     // Morning run
                case DayOfWeek.Tuesday: return 300;    // Gym workout
                case DayOfWeek.Wednesday: return 100;  // Light activity
                case DayOfWeek.Thursday: return 300;   // Gym workout
                case DayOfWeek.Friday: return 200;     // Evening jog
                case DayOfWeek.Saturday: return 400;   // Long hike or sports
                case DayOfWeek.Sunday: return 100;     // Rest day, light activity
                default: return 200;
            }
        }
    }
}