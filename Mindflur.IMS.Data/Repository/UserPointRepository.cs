using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class UserPointRepository : BaseRepository<UserPoints>, IUserPointRepository
    {

        public UserPointRepository(IMSDEVContext dbContext, ILogger<UserPoints> logger) : base(dbContext, logger)
        {
        }
        public async Task<IList<PointsForUserView>> GetPointsByUserId(int userId)
        {
            var points = await (from pu in _context.UserPoints
                                join us in _context.UserMasters on pu.UserId equals us.UserId
                                join md in _context.MasterData on pu.ModuleId equals md.Id
                                join ts in _context.TaskMasters on pu.ModuleItemId equals ts.TaskId
                                join wm in _context.WorkItemMasters on ts.WorkItemId equals wm.WorkItemId
                                where pu.UserId == userId
                                select new PointsForUserView
                                {
                                    UserId = pu.UserId,
                                    UserName = $"{us.FirstName} {us.LastName}",
                                    ModuleName = md.Items,
                                    ModuleTitle = wm.Title,
                                    Points = pu.Points,
                                    Comments = pu.Comments

                                }).ToListAsync();
            return await Task.FromResult(points);
        }

        public async Task<IList<PointsForUserView>> GetPoints()
        {
            var points = await (from pu in _context.UserPoints
                                join us in _context.UserMasters on pu.UserId equals us.UserId
                                join md in _context.MasterData on pu.ModuleId equals md.Id
                                join ts in _context.TaskMasters on pu.ModuleItemId equals ts.TaskId
                                join wm in _context.WorkItemMasters on ts.WorkItemId equals wm.WorkItemId

                                select new PointsForUserView
                                {
                                    UserId = pu.UserId,
                                    UserName = $"{us.FirstName} {us.LastName}",
                                    ModuleName = md.Items,
                                    ModuleTitle = wm.Title,
                                    Points = pu.Points,
                                    Comments = pu.Comments

                                }).ToListAsync();
            return await Task.FromResult(points);
        }

    }
}
