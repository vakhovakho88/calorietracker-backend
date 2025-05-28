using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalorieTracker.API.Models
{
    public class Goal
    {
        public Goal()
        {
            // Initialize with a new GUID
            GoalId = Guid.NewGuid();
            // Initialize concurrency control field
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        [Key]
        public Guid GoalId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Target calories must be a non-zero value")]
        public int TargetKcals { get; set; }

        [Required]
        [Range(1, 3650, ErrorMessage = "Time window must be between 1 and 3650 days")]
        public int TimeWindowDays { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        // Change from [Timestamp] to use ConcurrencyCheck with a string that can be initialized
        [ConcurrencyCheck]
        public string ConcurrencyStamp { get; set; }

        // Navigation properties (optional for now)
        // public virtual ICollection<DailyLog> DailyLogs { get; set; }
    }
}