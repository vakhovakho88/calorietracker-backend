using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalorieTracker.API.Models
{
    public class DailyLog
    {
        public DailyLog()
        {
            // Initialize with a new GUID
            DailyLogId = Guid.NewGuid();
            // Initialize concurrency control field
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        [Key]
        public Guid DailyLogId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Calories burned must be a non-negative value")]
        public int KcalsBurn { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Calories intake must be a non-negative value")]
        public int KcalsIntake { get; set; }

        // Change from [Timestamp] to use ConcurrencyCheck with a string that can be initialized
        [ConcurrencyCheck]
        public string ConcurrencyStamp { get; set; }

        // Calculated properties (not stored in DB, but used in DTOs)
        [NotMapped]
        public int KcalsDiff => KcalsBurn - KcalsIntake;

        // The following properties will be calculated by the service
        [NotMapped]
        public decimal SumDiffs { get; set; }

        [NotMapped]
        public decimal GoalDelta { get; set; }

        [NotMapped]
        public decimal Avg4Days { get; set; }

        [NotMapped]
        public decimal Avg7Days { get; set; }

        [NotMapped]
        public decimal AvgAll { get; set; }

        [NotMapped]
        public int DayNum { get; set; }
    }
}