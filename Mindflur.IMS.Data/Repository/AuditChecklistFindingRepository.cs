using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class AuditChecklistFindingRepository : BaseRepository<AuditChecklistFinding>, IAuditChecklistFindingRepository
    {
       

        public AuditChecklistFindingRepository(IMSDEVContext dbContext, ILogger<AuditChecklistFinding> logger) : base(dbContext, logger)
        {
           
        }
    }
}