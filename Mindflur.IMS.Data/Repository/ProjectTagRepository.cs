using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ProjectTagRepository : BaseRepository<ProjectTag>, IProjectTagRepository
    {
       

        public ProjectTagRepository(IMSDEVContext dbContext, ILogger<ProjectTag> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<IList<TagDataView>> GetProjectTags(int workItemId)
        {
            return await (from projectTag in _context.ProjectTags
                          join md in _context.MasterData on projectTag.MasterDataProjectTagId equals md.Id
                          where projectTag.WorkItemId == workItemId

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