using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ClauseMasterRepository : BaseRepository<ClauseMaster>, IClauseMasterRepository
    {
       
        public ClauseMasterRepository(IMSDEVContext dbContext, ILogger<ClauseMaster> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<IList<ClauseMaster>> GetAllClause()
        {
            var clause = await _context.ClauseMasters.ToListAsync();
            return await Task.FromResult(clause);
        }
    }
}