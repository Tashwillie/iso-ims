using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ProjectTaskRepository : BaseRepository<ProjectTask>, IProjectTaskRepository
    {
       
        public ProjectTaskRepository(IMSDEVContext dbContext, ILogger<ProjectTask> logger) : base(dbContext, logger)
        {
            
        }
    }
}
