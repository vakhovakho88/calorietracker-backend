using System;

namespace CalorieTracker.API.DTOs
{
    public class GoalDto
    {
        public Guid GoalId { get; set; }
        public int TargetKcals { get; set; }
        public int TimeWindowDays { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate => StartDate.AddDays(TimeWindowDays - 1);
    }

    public class CreateGoalDto
    {
        public int TargetKcals { get; set; }
        public int TimeWindowDays { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class UpdateGoalDto
    {
        public int TargetKcals { get; set; }
        public int TimeWindowDays { get; set; }
        public DateTime StartDate { get; set; }
    }
}