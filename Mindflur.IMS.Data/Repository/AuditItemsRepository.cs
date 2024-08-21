using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class AuditItemsRepository : BaseRepository<AuditableItem>, IAuditItemsRepository
    {
       
        public AuditItemsRepository(IMSDEVContext dbContext, ILogger<AuditableItem> logger) : base(dbContext, logger)
        {
           
        }
        public async Task<IList<ParticipantDropDownList>> GetParticipantList(int moduleEntityId, int moduleId)
        {
            var data = await( from user1 in _context.UserMasters
                              join p in _context.Participants on user1.UserId equals p.UserId 
                              join role in _context.MasterData on p.RoleId equals role.Id
                              join ap in _context.AuditPrograms on p.ModuleEntityId equals ap.Id
                              where ap.Id == moduleEntityId && p.ModuleId == moduleId && p.RoleId==(int)IMSRoles.Auditor && p.DeletedOn == null
                              select new ParticipantDropDownList
                              {
                                  Id = user1.UserId,
                                  Name = $"{user1.FirstName} {user1.LastName} {"(" + role.Items +")"}",
                              }

                              ).ToListAsync();
            return await Task.FromResult( data );
        }



        public async Task<IList<AuditItemsView>> GetAuditItemsByProgram(int auditId)
		{
			var items = await (from ai in _context.AuditableItems
                               join dep in _context.DepartmentMasters on ai.DepartmentId equals dep.DepartmentId into dep
                               from subdepartment in dep.DefaultIfEmpty()
							   where ai.AuditProgramId == auditId
							   select new AuditItemsView
							   {
								   AuditItemId = ai.Id,
								   //AuditTitle = ai.AuditableItems,
                                   Department = subdepartment.DepartmentName,
                                   DepartmentId = subdepartment.DepartmentId

                               }).ToListAsync();
			return await Task.FromResult(items);
		}
		public async Task<PaginatedItems<AuditItemView>> GetAuditItems(AuditItemListView auditItemListView)
        {
            string searchString = string.Empty;
            var rawData = (from ai in _context.AuditableItems
                           join dp in _context.DepartmentMasters on ai.DepartmentId equals dp.DepartmentId
                           join us in _context.UserMasters on ai.AuditorName equals us.UserId
                           join ap in _context.AuditPrograms on ai.AuditProgramId equals ap.Id
                           join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
                           join md in _context.MasterData on ai.Status equals md.Id
                           where ai.AuditProgramId == auditItemListView.AuditProgramId && ap.TenantId == auditItemListView.TenantId
                           select new AuditItemView
                           {
                               Id = ai.Id,
                               //AuditableItem = ai.AuditableItems,
                               Department = dp.DepartmentName,
                               Auditor = $"{us.FirstName} {us.LastName}",                               
                               StartDate = ai.StartDate,
                               EndDate = ai.EndDate,
                               Status=md.Items
                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, auditItemListView.ListRequests.SortColumn, auditItemListView.ListRequests.Sort == "asc")
                              .Skip(auditItemListView.ListRequests.PerPage * (auditItemListView.ListRequests.Page - 1))
                              .Take(auditItemListView.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)auditItemListView.ListRequests.PerPage);

            var model = new PaginatedItems<AuditItemView>(auditItemListView.ListRequests.Page, auditItemListView.ListRequests.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);
        }
      

        public async Task<IList<GetAuditDetailsForReport>> GetAuditItemsDetailsForPlan(int auditId)
        {
            var auditItems = await (from ai in _context.AuditableItems
                                    join ap in _context.AuditPrograms on ai.AuditProgramId equals ap.Id
                                    join dp in _context.DepartmentMasters on ai.DepartmentId equals dp.DepartmentId
                                    join us in _context.UserMasters on ai.AuditorName equals us.UserId
                                    where ap.Id == auditId
                                    select new GetAuditDetailsForReport
                                    {
                                        Department = dp.DepartmentName,
                                        AuditorName = $"{us.FirstName} {us.LastName}",
                                        StartDate = ai.StartDate,
                                        EndDate = ai.EndDate,

                                    }).ToListAsync();
            return await Task.FromResult(auditItems);

        }
        public async Task<AuditItemPreview> GetAuditableItemsPreview(int auditableItemId)
        {

            var rawData = (from ai in _context.AuditableItems
                           join user in _context.UserMasters on ai.AuditorName equals user.UserId
                           join dm in _context.DepartmentMasters on ai.DepartmentId equals dm.DepartmentId
                           //join md in _context.MasterData on ai.Type equals md.Id
                           join md1 in _context.MasterData on ai.Status equals md1.Id
                           join user1 in _context.UserMasters on ai.CreatedBy equals user1.UserId into user1
                           from subuser1 in user1.DefaultIfEmpty()
                           join user2 in _context.UserMasters on ai.UpdatedBy equals user2.UpdatedBy into user2
                           from subuser2 in user2.DefaultIfEmpty() 
                           where ai.Id == auditableItemId
                           select new AuditItemPreview()
                           {

                               Id = ai.Id,
                               AuditProgramId = ai.AuditProgramId,
                               //AuditableItems = ai.AuditableItems,
                               Description = ai.Description,
                               //TypeId = ai.Type,
                               //Type = md.Items,
                               StatusId = ai.Status,
                               Status = md1.Items,
                               StartDate = ai.StartDate,
                               EndDate = ai.EndDate,
                               AuditorId = ai.AuditorName,
                               AuditorName = $"{user.FirstName} {user.LastName}",
                               DepartmentId = ai.DepartmentId,
                               DepartmentName = dm.DepartmentName,
                               CreatedById = ai.CreatedBy,
                               CreatedBy = $"{subuser1.FirstName} {subuser1.LastName}",
                               CreatedOn = ai.CreatedOn,
                               UpdatedById = ai.UpdatedBy,
                               UpdatedBy = $"{subuser2.FirstName} {subuser2.LastName}",
                               UpdatedOn = ai.UpdatedOn,


                           }).AsQueryable();
            return rawData.FirstOrDefault();



        }

        public async Task<AuditItemStandards> GetAuditableItemStandrads(int auditableItemId)
        {
            var rawData = (from ai in _context.AuditableItems
                           join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
						   join aiic in _context.AuditItemClauses on aic.AuditableItemClauseId equals aiic.AuditableItemClauseId
						   join cm in _context.Clauses on aiic.ClauseMasterId equals cm.ClauseId
						   join md in _context.MasterData on cm.StandardId equals md.Id
                           where ai.Id == auditableItemId
                           select new AuditItemStandards
                           {
                               MasterDataStandardId = aic.MasterDataStandardId,
                               Standards = md.Items
                           }).AsQueryable();
            return rawData.FirstOrDefault();
        }

        public async Task<IList<ClausesDataView>> GetAuditableItemsClauses(int auditableItemId)
        {
            var clause = await (from ai in _context.AuditableItems
                                join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
                                join aiic in _context.AuditItemClauses on aic.AuditableItemClauseId equals aiic.AuditableItemClauseId
                                join cm in _context.Clauses on aiic.ClauseMasterId equals cm.ClauseId
                                where ai.Id == auditableItemId
                                select new ClausesDataView
                                {
                                    ClauseId = aiic.ClauseMasterId,
                                    ClauseNo = cm.ClauseNumberText +" " + cm.DisplayText
                                }).OrderByDescending(clause => clause.ClauseId).ToListAsync();
            return await Task.FromResult(clause);
        }

        public async Task<AuditableItem> UpdateAuditableItems(int Id, PutAuditableItemViewModel auditableItem, int userId, int tenantId)
        {
            var items = await _context.AuditableItems.FindAsync(Id);
            if (items == null)
            {
                throw new NotFoundException(string.Format(RepositoryConstant.AuditItemNotFoundErrorMessage), Id);
            }
            else
            {
                items.AuditProgramId = auditableItem.AuditProgramId;
                //items.AuditableItems = auditableItem.AuditableItems;
                items.Description = auditableItem.Description;
                //items.Type = auditableItem.Type;
                items.Status = auditableItem.Status;
                items.StartDate = auditableItem.StartDate;
                items.EndDate = auditableItem.EndDate;
                items.AuditorName = auditableItem.AuditorName;
                items.DepartmentId = auditableItem.DepartmentId;
                items.UpdatedBy = userId;
                items.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                ActivityLog activityLog = new ActivityLog();
                activityLog.TenantId = tenantId;
                activityLog.ControllerId = (int)IMSControllerCategory.AuditableItem;
                activityLog.EntityId = items.Id;
                activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
                activityLog.Description = "Audit Item Has Been Updated ";
                activityLog.Details = System.Text.Json.JsonSerializer.Serialize(auditableItem);
                activityLog.Status = true;
                activityLog.CreatedBy = userId;
                activityLog.CreatedOn = DateTime.UtcNow;
                await _context.ActivityLogs.AddAsync(activityLog);
                await _context.SaveChangesAsync();


            }

            var auditClauses = await _context.AuditableItemClauses.Where(aic => aic.AuditableItemId == items.Id).ToListAsync();
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
                    foreach (int clause in auditableItem.Clauses)
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


                    auditClause.MasterDataStandardId = auditableItem.Standards;
                    
                    await _context.SaveChangesAsync();
                }
                //collect existing records and remove it 

                return items;
            }
        }

       
        public async Task<AuditItemStandardsViewModel> GetStandardsForAuditItems(int auditableItemId)
        {
            var auditItems =  await(from ai in _context.AuditableItems
                                    join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
                                    where ai.Id == auditableItemId
                                    select new AuditItemStandardsViewModel
                                    {
                                        StandardsId = aic.MasterDataStandardId
                                    }).ToListAsync();
            return auditItems.FirstOrDefault();

        }





    }

}
