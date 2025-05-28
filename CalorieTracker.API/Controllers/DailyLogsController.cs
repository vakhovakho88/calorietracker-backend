using CalorieTracker.API.Data.Repositories;
using CalorieTracker.API.DTOs;
using CalorieTracker.API.Models;
using CalorieTracker.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieTracker.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DailyLogsController : ControllerBase
    {
        private readonly IDailyLogRepository _dailyLogRepository;
        private readonly IGoalRepository _goalRepository;
        private readonly ICalorieCalculatorService _calculatorService;

        public DailyLogsController(
            IDailyLogRepository dailyLogRepository,
            IGoalRepository goalRepository,
            ICalorieCalculatorService calculatorService)
        {
            _dailyLogRepository = dailyLogRepository;
            _goalRepository = goalRepository;
            _calculatorService = calculatorService;
        }

        // GET: api/v1/dailylogs
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<DailyLogDto>>> GetDailyLogs(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var userId = GetUserId();

            // Get active goal for this user
            var goal = await _goalRepository.GetActiveGoalForUserAsync(userId);

            if (goal == null)
            {
                return BadRequest(new { message = "No active goal found. Please create a goal first." });
            }

            // Get logs for the specified date range
            var dailyLogs = await _dailyLogRepository.GetDailyLogsByDateRangeAsync(userId, from, to);

            // Calculate total count for pagination
            var totalCount = dailyLogs.Count;

            // Apply pagination (manually since we already fetched all logs for calculation)
            var pagedLogs = dailyLogs
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            // Calculate derived metrics for all logs to get running sums correctly
            var calculatedLogs = _calculatorService.CalculateDerivedMetrics(dailyLogs, goal);

            // Extract just the page we want to return
            var pagedCalculatedLogs = calculatedLogs
                .Where(l => pagedLogs.Any(pl => pl.DailyLogId == l.DailyLogId))
                .ToList();

            // Map to DTOs
            var dailyLogDtos = pagedCalculatedLogs.Select(MapToDailyLogDto).ToList();

            // Create paged result
            var pagedResult = new PagedResultDto<DailyLogDto>
            {
                Items = dailyLogDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };

            return Ok(pagedResult);
        }

        // POST: api/v1/dailylogs
        [HttpPost]
        public async Task<ActionResult<DailyLogDto>> CreateDailyLog(CreateDailyLogDto createDailyLogDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();

            // Get active goal for this user
            var goal = await _goalRepository.GetActiveGoalForUserAsync(userId);

            if (goal == null)
            {
                return BadRequest(new { message = "No active goal found. Please create a goal first." });
            }

            // Validate date range
            if (createDailyLogDto.Date.Date < goal.StartDate ||
                createDailyLogDto.Date.Date > goal.StartDate.AddDays(goal.TimeWindowDays - 1))
            {
                return BadRequest(new { message = $"Date must be between {goal.StartDate:yyyy-MM-dd} and {goal.StartDate.AddDays(goal.TimeWindowDays - 1):yyyy-MM-dd}" });
            }

            // Check if a log already exists for this date
            var existingLog = await _dailyLogRepository.GetDailyLogByDateAsync(userId, createDailyLogDto.Date);

            if (existingLog != null)
            {
                return Conflict(new { message = $"A log already exists for {createDailyLogDto.Date:yyyy-MM-dd}" });
            }

            // Create new log
            var dailyLog = new DailyLog
            {
                DailyLogId = Guid.NewGuid(),
                UserId = userId,
                Date = createDailyLogDto.Date.Date,
                KcalsBurn = createDailyLogDto.KcalsBurn,
                KcalsIntake = createDailyLogDto.KcalsIntake
            };

            await _dailyLogRepository.AddAsync(dailyLog);

            try
            {
                await _dailyLogRepository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Failed to create daily log" });
            }

            // Get all logs for this user
            var allUserLogs = await _dailyLogRepository.GetAllUserLogsAsync(userId);

            // Calculate derived metrics
            var calculatedLog = _calculatorService.CalculateDerivedMetrics(dailyLog, goal, allUserLogs);

            return CreatedAtAction(nameof(GetDailyLog), new { id = dailyLog.DailyLogId }, MapToDailyLogDto(calculatedLog));
        }

        // GET: api/v1/dailylogs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DailyLogDto>> GetDailyLog(Guid id)
        {
            var userId = GetUserId();

            var dailyLog = await _dailyLogRepository.GetByIdAsync(id);

            if (dailyLog == null || dailyLog.UserId != userId)
            {
                return NotFound(new { message = "Daily log not found" });
            }

            // Get active goal for this user
            var goal = await _goalRepository.GetActiveGoalForUserAsync(userId);

            if (goal == null)
            {
                return BadRequest(new { message = "No active goal found" });
            }

            // Get all logs for this user
            var allUserLogs = await _dailyLogRepository.GetAllUserLogsAsync(userId);

            // Calculate derived metrics
            var calculatedLog = _calculatorService.CalculateDerivedMetrics(dailyLog, goal, allUserLogs);

            return Ok(MapToDailyLogDto(calculatedLog));
        }

        // PUT: api/v1/dailylogs/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<DailyLogDto>> UpdateDailyLog(Guid id, UpdateDailyLogDto updateDailyLogDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();

            var dailyLog = await _dailyLogRepository.GetByIdAsync(id);

            if (dailyLog == null || dailyLog.UserId != userId)
            {
                return NotFound(new { message = "Daily log not found" });
            }

            // Update log properties
            dailyLog.KcalsBurn = updateDailyLogDto.KcalsBurn;
            dailyLog.KcalsIntake = updateDailyLogDto.KcalsIntake;

            await _dailyLogRepository.UpdateAsync(dailyLog);

            try
            {
                await _dailyLogRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Daily log was modified by another process" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Failed to update daily log" });
            }

            // Get active goal for this user
            var goal = await _goalRepository.GetActiveGoalForUserAsync(userId);

            // Get all logs for this user
            var allUserLogs = await _dailyLogRepository.GetAllUserLogsAsync(userId);

            // Calculate derived metrics
            var calculatedLog = _calculatorService.CalculateDerivedMetrics(dailyLog, goal, allUserLogs);

            return Ok(MapToDailyLogDto(calculatedLog));
        }

        // DELETE: api/v1/dailylogs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyLog(Guid id)
        {
            var userId = GetUserId();

            var dailyLog = await _dailyLogRepository.GetByIdAsync(id);

            if (dailyLog == null || dailyLog.UserId != userId)
            {
                return NotFound(new { message = "Daily log not found" });
            }

            await _dailyLogRepository.DeleteAsync(id);

            try
            {
                await _dailyLogRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Daily log was modified by another process" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Failed to delete daily log" });
            }

            return NoContent();
        }

        // POST: api/v1/dailylogs/bulk
        [HttpPost("bulk")]
        public async Task<ActionResult<object>> BulkImportDailyLogs([FromBody] List<CreateDailyLogDto> dailyLogs)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();

            // Get active goal for this user
            var goal = await _goalRepository.GetActiveGoalForUserAsync(userId);

            if (goal == null)
            {
                return BadRequest(new { message = "No active goal found. Please create a goal first." });
            }

            // Track results for each log
            var results = new List<object>();
            
            foreach (var logDto in dailyLogs)
            {
                try
                {
                    // Validate date range
                    if (logDto.Date.Date < goal.StartDate ||
                        logDto.Date.Date > goal.StartDate.AddDays(goal.TimeWindowDays - 1))
                    {
                        results.Add(new { 
                            date = logDto.Date, 
                            success = false, 
                            message = $"Date must be between {goal.StartDate:yyyy-MM-dd} and {goal.StartDate.AddDays(goal.TimeWindowDays - 1):yyyy-MM-dd}" 
                        });
                        continue;
                    }

                    // Check if a log already exists for this date
                    var existingLog = await _dailyLogRepository.GetDailyLogByDateAsync(userId, logDto.Date);

                    if (existingLog != null)
                    {
                        // Update existing log
                        existingLog.KcalsBurn = logDto.KcalsBurn;
                        existingLog.KcalsIntake = logDto.KcalsIntake;
                        await _dailyLogRepository.UpdateAsync(existingLog);
                        results.Add(new { date = logDto.Date, success = true, action = "updated" });
                    }
                    else
                    {
                        // Create new log
                        var dailyLog = new DailyLog
                        {
                            DailyLogId = Guid.NewGuid(),
                            UserId = userId,
                            Date = logDto.Date.Date,
                            KcalsBurn = logDto.KcalsBurn,
                            KcalsIntake = logDto.KcalsIntake
                        };

                        await _dailyLogRepository.AddAsync(dailyLog);
                        results.Add(new { date = logDto.Date, success = true, action = "created" });
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new { date = logDto.Date, success = false, message = ex.Message });
                }
            }

            try
            {
                await _dailyLogRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Failed to save some daily logs", error = ex.Message });
            }

            return StatusCode(207, new { results });
        }

        // Helper methods
        private Guid GetUserId()
        {
            // In a real application, this would extract the user ID from the JWT token
            // For now, we'll use a mock user ID
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        private DailyLogDto MapToDailyLogDto(DailyLog dailyLog)
        {
            return new DailyLogDto
            {
                DailyLogId = dailyLog.DailyLogId,
                Date = dailyLog.Date,
                KcalsBurn = dailyLog.KcalsBurn,
                KcalsIntake = dailyLog.KcalsIntake,
                KcalsDiff = dailyLog.KcalsDiff,
                SumDiffs = dailyLog.SumDiffs,
                GoalDelta = dailyLog.GoalDelta,
                Avg4Days = dailyLog.Avg4Days,
                Avg7Days = dailyLog.Avg7Days,
                AvgAll = dailyLog.AvgAll,
                DayNum = dailyLog.DayNum
            };
        }
    }
}