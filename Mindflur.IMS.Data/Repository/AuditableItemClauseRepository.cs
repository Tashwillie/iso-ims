using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class AuditableItemClauseRepository : BaseRepository<AuditableItemClause>, IAuditableItemClauseRepository
    {
       
        public AuditableItemClauseRepository(IMSDEVContext dbContext, ILogger<AuditableItemClause> logger) : base(dbContext, logger)
        {
            
        }
        public async Task<IList<AuditableItemClause>> GetAuditableItemClause()
        {
            var clauses = await _context.AuditableItemClauses.ToListAsync();
            return await Task.FromResult(clauses);
        }

        public async Task<AuditableItemClause> UpdateAuditableItemClause(int Id, PutAuditClauseViewModel putAuditClause)
        {
            var auditClauses = await _context.AuditableItemClauses.Where(aic => aic.AuditableItemId == putAuditClause.Id).ToListAsync();
            if (auditClauses == null)
            {
                throw new NotFoundException(string.Format(RepositoryConstant.IdNotFoundErrorMessage), Id);
            }
            else
            {
                foreach (var auditClause in auditClauses)
                {
                    var existingAuditItem = _context.AuditItemClauses.Where(x => x.AuditableItemClauseId == auditClause.AuditableItemClauseId);
                    _context.AuditItemClauses.RemoveRange(existingAuditItem);
                    await _context.SaveChangesAsync(); // removed data

                    var clauseForAuditItems = new List<AuditItemClause>();
                    foreach (int clause in putAuditClause.Clauses)
                    {
                        var newClause = new AuditItemClause
                        {
                            AuditableItemClauseId = auditClause.AuditableItemClauseId,
                            ClauseMasterId = clause,
                        };
                        clauseForAuditItems.Add(newClause);
                    }

                    await _context.AuditItemClauses.AddRangeAsync(clauseForAuditItems);
                    await _context.SaveChangesAsync();


                    auditClause.MasterDataStandardId = putAuditClause.MasterDataStandardId;

                    await _context.SaveChangesAsync();
                }
                //collect existing records and remove it


                return auditClauses.FirstOrDefault();
            }
        }
    }
}
