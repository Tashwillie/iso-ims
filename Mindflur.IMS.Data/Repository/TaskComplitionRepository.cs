using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class TaskComplitionRepository : BaseRepository<TaskComplition>, ITaskCompletionRepository
    {

        public TaskComplitionRepository(IMSDEVContext dbContext, ILogger<TaskComplition> logger) : base(dbContext, logger)
        {
        }

        public TaskComplition GetByTaskId(int taskId)
        {
            return _context.TaskComplitions.SingleOrDefault(tc => tc.TaskId == taskId);
        }
    }
}