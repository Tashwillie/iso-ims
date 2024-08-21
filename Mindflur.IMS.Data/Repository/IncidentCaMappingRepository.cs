using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class IncidentCaMappingRepository : BaseRepository<IncidentCorrectiveActionMapping>, IIncidentCorrectibveActionMappingRepository
    {
        

        public IncidentCaMappingRepository(IMSDEVContext dbContext, ILogger<IncidentCorrectiveActionMapping> logger) : base(dbContext, logger)
        {
            
        }
    }
}