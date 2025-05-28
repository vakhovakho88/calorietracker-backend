using CalorieTracker.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieTracker.API.Data.Repositories
{
    public interface IGoalRepository : IRepository<Goal>
    {
        Task<Goal> GetActiveGoalForUserAsync(Guid userId);
    }

    public class GoalRepository : Repository<Goal>, IGoalRepository
    {
        public GoalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Goal> GetActiveGoalForUserAsync(Guid userId)
        {
            return await _context.Goals
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.StartDate)
                .FirstOrDefaultAsync();
        }
    }
}