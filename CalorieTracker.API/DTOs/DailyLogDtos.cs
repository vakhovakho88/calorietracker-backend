using System;
using System.Collections.Generic;

namespace CalorieTracker.API.DTOs
{
    public class DailyLogDto
    {
        public Guid DailyLogId { get; set; }
        public DateTime Date { get; set; }
        public int KcalsBurn { get; set; }
        public int KcalsIntake { get; set; }
        
        // Calculated fields
        public int KcalsDiff { get; set; }
        public decimal SumDiffs { get; set; }
        public decimal GoalDelta { get; set; }
        public decimal Avg4Days { get; set; }
        public decimal Avg7Days { get; set; }
        public decimal AvgAll { get; set; }
        public int DayNum { get; set; }
    }

    public class CreateDailyLogDto
    {
        public DateTime Date { get; set; }
        public int KcalsBurn { get; set; }
        public int KcalsIntake { get; set; }
    }

    public class UpdateDailyLogDto
    {
        public int KcalsBurn { get; set; }
        public int KcalsIntake { get; set; }
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}