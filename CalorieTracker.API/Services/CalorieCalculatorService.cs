using CalorieTracker.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CalorieTracker.API.Services
{
    public interface ICalorieCalculatorService
    {
        List<DailyLog> CalculateDerivedMetrics(List<DailyLog> dailyLogs, Goal goal);
        DailyLog CalculateDerivedMetrics(DailyLog dailyLog, Goal goal, List<DailyLog> allUserLogs);
    }

    public class CalorieCalculatorService : ICalorieCalculatorService
    {
        public List<DailyLog> CalculateDerivedMetrics(List<DailyLog> dailyLogs, Goal goal)
        {
            if (dailyLogs == null || !dailyLogs.Any() || goal == null)
                return dailyLogs;

            // Sort logs by date
            var sortedLogs = dailyLogs.OrderBy(d => d.Date).ToList();
            decimal runningSum = 0;

            for (int i = 0; i < sortedLogs.Count; i++)
            {
                var log = sortedLogs[i];
                
                // Calculate KcalsDiff is done automatically via the property in the model
                
                // Calculate running sum
                runningSum += log.KcalsDiff;
                log.SumDiffs = runningSum;
                
                // Calculate GoalDelta
                log.GoalDelta = goal.TargetKcals - log.SumDiffs;
                
                // Calculate DayNum (1-based index relative to StartDate)
                log.DayNum = (int)(log.Date - goal.StartDate).TotalDays + 1;
                
                // Calculate averages
                CalculateAverages(sortedLogs, i);
            }

            return sortedLogs;
        }

        public DailyLog CalculateDerivedMetrics(DailyLog dailyLog, Goal goal, List<DailyLog> allUserLogs)
        {
            if (dailyLog == null || goal == null)
                return dailyLog;

            // Add the current log to the list if it's not already there
            var logsToCalculate = new List<DailyLog>(allUserLogs);
            if (!logsToCalculate.Any(l => l.DailyLogId == dailyLog.DailyLogId))
            {
                logsToCalculate.Add(dailyLog);
            }
            else
            {
                // Update the log in the list
                var existingLog = logsToCalculate.First(l => l.DailyLogId == dailyLog.DailyLogId);
                existingLog.KcalsBurn = dailyLog.KcalsBurn;
                existingLog.KcalsIntake = dailyLog.KcalsIntake;
                existingLog.Date = dailyLog.Date;
            }

            // Calculate metrics for all logs
            var calculatedLogs = CalculateDerivedMetrics(logsToCalculate, goal);
            
            // Find and return the calculated log
            return calculatedLogs.First(l => l.DailyLogId == dailyLog.DailyLogId);
        }

        private void CalculateAverages(List<DailyLog> logs, int currentIndex)
        {
            var currentLog = logs[currentIndex];
            
            // Calculate AvgAll
            currentLog.AvgAll = (decimal)logs.Take(currentIndex + 1).Average(l => (double)l.KcalsDiff);
            
            // Calculate Avg4Days
            if (currentIndex >= 3) // Need at least 4 days
            {
                currentLog.Avg4Days = (decimal)logs.Skip(currentIndex - 3).Take(4).Average(l => (double)l.KcalsDiff);
            }
            else
            {
                currentLog.Avg4Days = (decimal)logs.Take(currentIndex + 1).Average(l => (double)l.KcalsDiff);
            }
            
            // Calculate Avg7Days
            if (currentIndex >= 6) // Need at least 7 days
            {
                currentLog.Avg7Days = (decimal)logs.Skip(currentIndex - 6).Take(7).Average(l => (double)l.KcalsDiff);
            }
            else
            {
                currentLog.Avg7Days = (decimal)logs.Take(currentIndex + 1).Average(l => (double)l.KcalsDiff);
            }
        }
    }
}