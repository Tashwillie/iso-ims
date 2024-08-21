using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class IncidentMasterClassificationTagRepsository : BaseRepository<IncidentMasterClassificationTag>, IIncidentMasterClassificationTagRepository
    {
       

        public IncidentMasterClassificationTagRepsository(IMSDEVContext dbContext, ILogger<IncidentMasterClassificationTag> logger) : base(dbContext, logger)
        {
           
        }

        public async Task<IList<TagDataView>> GetIncidentClassificationtTags(int incidentId)
        {
            return await (from incidentTag in _context.IncidentMasterClassificationTags
                          join md in _context.MasterData on incidentTag.IncidentClassificationTags equals md.Id
                          where incidentTag.IncidentId == incidentId

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