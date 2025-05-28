using CalorieTracker.API.Data;
using CalorieTracker.API.Data.Repositories;
using CalorieTracker.API.DTOs;
using CalorieTracker.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieTracker.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GoalsController : ControllerBase
    {
        private readonly IGoalRepository _goalRepository;
        private readonly IDailyLogRepository _dailyLogRepository;

        public GoalsController(
            IGoalRepository goalRepository,
            IDailyLogRepository dailyLogRepository)
        {
            _goalRepository = goalRepository;
            _dailyLogRepository = dailyLogRepository;
        }

        // GET: api/v1/goals/active
        [HttpGet("active")]
        public async Task<ActionResult<GoalDto>> GetActiveGoal()
        {
            // For now, we'll use a mock user ID. In a real application,
            // this would come from the JWT claims
            var userId = GetUserId();

            var goal = await _goalRepository.GetActiveGoalForUserAsync(userId);

            if (goal == null)
            {
                return NotFound(new { message = "No active goal found" });
            }

            return MapToGoalDto(goal);
        }

        // POST: api/v1/goals
        [HttpPost]
        public async Task<ActionResult<GoalDto>> CreateGoal(CreateGoalDto createGoalDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();

            // Validate that StartDate is not in the past
            if (createGoalDto.StartDate.Date < DateTime.Today)
            {
                return BadRequest(new { message = "Start date cannot be in the past" });
            }

            var goal = new Goal
            {
                GoalId = Guid.NewGuid(),
                UserId = userId,
                TargetKcals = createGoalDto.TargetKcals,
                TimeWindowDays = createGoalDto.TimeWindowDays,
                StartDate = createGoalDto.StartDate.Date
            };

            await _goalRepository.AddAsync(goal);
            
            try
            {
                await _goalRepository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Failed to create goal" });
            }

            return CreatedAtAction(
                nameof(GetActiveGoal),
                MapToGoalDto(goal));
        }

        // PUT: api/v1/goals/{goalId}
        [HttpPut("{goalId}")]
        public async Task<ActionResult<GoalDto>> UpdateGoal(Guid goalId, UpdateGoalDto updateGoalDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            
            var goal = await _goalRepository.GetByIdAsync(goalId);

            if (goal == null || goal.UserId != userId)
            {
                return NotFound(new { message = "Goal not found" });
            }

            // Check if there are any logs for this goal
            bool hasLogs = await _dailyLogRepository.HasLogsForGoalPeriodAsync(
                userId, 
                goal.StartDate, 
                goal.StartDate.AddDays(goal.TimeWindowDays - 1));

            // If there are logs, implement a soft-lock (or add additional validation)
            if (hasLogs)
            {
                // Here you could add specific rules for what can be changed when logs exist
                // For example, you might allow extending the time window but not changing the start date
                
                // For simplicity, we'll allow changes but warn about existing logs
                // In a full implementation, you might want more sophisticated logic
                Response.Headers.Add("X-Warning", "Goal updated with existing logs. Recalculations required.");
            }

            // Update goal properties
            goal.TargetKcals = updateGoalDto.TargetKcals;
            goal.TimeWindowDays = updateGoalDto.TimeWindowDays;
            goal.StartDate = updateGoalDto.StartDate.Date;

            await _goalRepository.UpdateAsync(goal);

            try
            {
                await _goalRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Goal was modified by another process" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Failed to update goal" });
            }

            return Ok(MapToGoalDto(goal));
        }

        // Helper methods
        private Guid GetUserId()
        {
            // In a real application, this would extract the user ID from the JWT token
            // For now, we'll use a mock user ID
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        private GoalDto MapToGoalDto(Goal goal)
        {
            return new GoalDto
            {
                GoalId = goal.GoalId,
                TargetKcals = goal.TargetKcals,
                TimeWindowDays = goal.TimeWindowDays,
                StartDate = goal.StartDate
            };
        }
    }
}