using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ProgramStandardsRepository : BaseRepository<ProgramStandard>, IProgramStandardsRepository
    {
       

        public ProgramStandardsRepository(IMSDEVContext dbContext, ILogger<ProgramStandard> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<IList<StandardDataView>> GetStandards(int auditProgramId)
        {
            return await (from standard in _context.ProgramStandards
                          join md in _context.MasterData on standard.MasterDataStandardId equals md.Id
                          where standard.AuditProgramId == auditProgramId

                          select new StandardDataView
                          {
                              StandardId = md.Id,
                              StandardName = md.Items,
                          })
                          .OrderByDescending(md => md.StandardId)
                          .ToListAsync();
        }
    }
}