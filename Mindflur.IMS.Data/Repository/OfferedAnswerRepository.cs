using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class OfferedAnswerRepository : BaseRepository<OfferedAnswerMaster>, IOfferedAnswerMasterRepository
    {
        
        public OfferedAnswerRepository(IMSDEVContext dbContext, ILogger<OfferedAnswerMaster> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<IList<OfferedAnswerMaster>> getAllOfferedAnswer()
        {
            var offeredAnswer = await _context.OfferedAnswerMasters.ToListAsync();
            return offeredAnswer;
        }
    }
}