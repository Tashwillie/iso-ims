using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class AuditFindingMappingRepository : BaseRepository<AuditFindingsMapping>, IAuditFindingMappingRepository
    {
       
        public AuditFindingMappingRepository(IMSDEVContext dbContext, ILogger<AuditFindingsMapping> logger) : base(dbContext, logger)
        {
            
        }
    }
}
