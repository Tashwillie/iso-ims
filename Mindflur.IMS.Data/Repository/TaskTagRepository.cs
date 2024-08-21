using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class TaskTagRepository : BaseRepository<TaskTag>, ITaskTagRepository
    {

        public TaskTagRepository(IMSDEVContext dbContext, ILogger<TaskTag> logger) : base(dbContext, logger)
        {
        }

        public async Task<IList<TagDataView>> GetTaskTags(int taskId)
        {
            return await (from taskTag in _context.TaskTags
                          join md in _context.MasterData on taskTag.MasterDataTaskTagId equals md.Id
                          where taskTag.TaskId == taskId

                          select new TagDataView
                          {
                              TagId = md.Id,
                              TagName = md.Items,
                          })
                          .OrderByDescending(md => md.TagId)
                          .ToListAsync();
        }
    }
}