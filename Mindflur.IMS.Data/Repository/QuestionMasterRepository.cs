using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class QuestionMasterRepository : BaseRepository<QuestionMaster>, IQuestionMasterRepository
    {
      
        public QuestionMasterRepository(IMSDEVContext dbContext, ILogger<QuestionMaster> logger) : base(dbContext, logger)
        {
        }

        public async Task<IList<QuestionMaster>> GetAllQuestionDetails()
        {
            var questions = await _context.QuestionMasters.ToListAsync();
            return questions;
        }
    }
}