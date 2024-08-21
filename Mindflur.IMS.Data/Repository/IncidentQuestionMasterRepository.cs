using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class IncidentQuestionMasterRepository : BaseRepository<IncidentQuestionMaster>, IIncidentQuestionMasterRepository
    {
        

        public IncidentQuestionMasterRepository(IMSDEVContext dbContext, ILogger<IncidentQuestionMaster> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<IList<IncidentQuestionMaster>> GetAllQuestionMaster()
        {
            var rawData = await _context.IncidentQuestionMasters.ToListAsync();
            return rawData;
        }
    }
}