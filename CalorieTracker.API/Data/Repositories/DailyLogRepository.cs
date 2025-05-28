using CalorieTracker.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieTracker.API.Data.Repositories
{
    public interface IDailyLogRepository : IRepository<DailyLog>
    {
        Task<DailyLog> GetDailyLogByDateAsync(Guid userId, DateTime date);
        Task<List<DailyLog>> GetDailyLogsByDateRangeAsync(Guid userId, DateTime? from, DateTime? to);
        Task<List<DailyLog>> GetAllUserLogsAsync(Guid userId);
        Task<bool> HasLogsForGoalPeriodAsync(Guid userId, DateTime startDate, DateTime endDate);
    }

    public class DailyLogRepository : Repository<DailyLog>, IDailyLogRepository
    {
        public DailyLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DailyLog> GetDailyLogByDateAsync(Guid userId, DateTime date)
        {
            return await _context.DailyLogs
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Date.Date == date.Date);
        }

        public async Task<List<DailyLog>> GetDailyLogsByDateRangeAsync(Guid userId, DateTime? from, DateTime? to)
        {
            IQueryable<DailyLog> query = _context.DailyLogs
                .Where(d => d.UserId == userId);

            if (from.HasValue)
            {
                query = query.Where(d => d.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(d => d.Date <= to.Value.Date);
            }

            return await query.OrderBy(d => d.Date).ToListAsync();
        }

        public async Task<List<DailyLog>> GetAllUserLogsAsync(Guid userId)
        {
            return await _context.DailyLogs
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        public async Task<bool> HasLogsForGoalPeriodAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _context.DailyLogs
                .AnyAsync(d => d.UserId == userId && 
                             d.Date >= startDate && 
                             d.Date <= endDate);
        }
    }
}