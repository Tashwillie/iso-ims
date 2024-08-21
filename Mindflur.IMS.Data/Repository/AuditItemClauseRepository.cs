using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class AuditItemClauseRepository : BaseRepository<AuditItemClause>, IAuditItemClauseRepository
    {
       
        public AuditItemClauseRepository(IMSDEVContext dbContext, ILogger<AuditItemClause> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<IList<AuditItemClause>> GetAuditItemClauses()
        {
            var clauses = await _context.AuditItemClauses.ToListAsync();
            return await Task.FromResult(clauses);
        }
        public async Task<List<ItemClauses>> GetClausesByAuditItemId(int itemId)
        {
            var clauses = await (from ai in _context.AuditableItems
                           join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
                           join aiic in _context.AuditItemClauses on aic.AuditableItemClauseId equals aiic.AuditableItemClauseId
                           join cl in _context.Clauses on aiic.ClauseMasterId equals cl.ClauseId
                           where ai.Id == itemId
                           select new ItemClauses
                           {
                               ClauseName = cl.ClauseNumberText
                           }).ToListAsync();

           return await Task.FromResult(clauses);
        }
    }
}