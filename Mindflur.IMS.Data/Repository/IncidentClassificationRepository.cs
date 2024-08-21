using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class IncidentClassificationRepository : BaseRepository<IncidentManagementAccidentClassification>, IIncidentClassificationRepository
    {
       

        public IncidentClassificationRepository(IMSDEVContext dbContext, ILogger<IncidentManagementAccidentClassification> logger) : base(dbContext, logger)
        {
           
        }

        public async Task <IncidentManagementAccidentClassification>GetByIncidentId(int incidentId)
        {
            var rawData=await _context.IncidentManagementAccidentClassifications.Where(data=>data.IncidentId==incidentId).FirstOrDefaultAsync();
            return rawData;
        }
    }
}