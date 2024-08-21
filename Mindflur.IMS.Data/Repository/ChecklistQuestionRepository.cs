using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ChecklistQuestionRepository : BaseRepository<ChecklistMaster>, IChecklistQuestionRepository
    {

        public ChecklistQuestionRepository(IMSDEVContext dbContext, ILogger<ChecklistMaster> logger) : base(dbContext, logger)
        {

        }

        public async Task<PaginatedItems<CheckListMasterView>> GetCheckListMasterList(CheckListView checkListView)
        {
            var rawData = (from check in _context.ChecklistMasters
                           join tm in _context.TenanttMasters on check.TenantId equals tm.TenantId
                           join clause in _context.Clauses on check.ClauseMasterId equals clause.ClauseId
                           join md in _context.MasterData on clause.StandardId equals md.Id
                           select new CheckListMasterView
                           {
                               id = check.Id,
                               ClauseId = clause.ClauseId,  
                               Questions = check.Questions,
                               OrderNo = check.OrderNo,
                               ClauseNumberText= clause.ClauseNumberText,
                               StandardId = clause.StandardId,
                               Standard = md.Items

                           }).OrderByDescending(rawData => rawData.id).AsQueryable().AsQueryable();
            if (checkListView.StandardId > 0)
            {
                rawData = rawData.Where(log => log.StandardId == checkListView.StandardId);
            }
            
            if (checkListView.ClauseId > 0)
            {
                rawData=rawData.Where(log=>log.ClauseId == checkListView.ClauseId);
            }

            var filteredData = DataExtensions.OrderBy(rawData, checkListView.ListRequests.SortColumn, checkListView.ListRequests.Sort == "asc")
                            .Skip(checkListView.ListRequests.PerPage * (checkListView.ListRequests.Page - 1))
                            .Take(checkListView.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)checkListView.ListRequests.PerPage);
            var model = new PaginatedItems<CheckListMasterView>(checkListView.ListRequests.Page, checkListView.ListRequests.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);
        }


        public async Task<GetCheckListById> GetChecklistMasterById(int tenantId, int Id)
        {
            var rawData = (from check in _context.ChecklistMasters
                           join clause in _context.Clauses on check.ClauseMasterId equals clause.ClauseId
                           join md in _context.MasterData on clause.StandardId equals md.Id
                           join tm in _context.TenanttMasters on check.TenantId equals tm.TenantId
                           where check.Id == Id
                           select new GetCheckListById
                           {
                               Id = check.Id,
                               ClauseMasterId = check.ClauseMasterId,
                               TenantId = check.TenantId,
                               Questions = check.Questions,
                               OrderNo = check.OrderNo,
							   clauseMaster = $" {clause.ClauseNumberText}  {clause.DisplayText}",
                               ParentId = clause.ParentId,
                               StandardId = clause.StandardId,
                               Standard = md.Items
                           }
                           ).AsQueryable();
            return rawData.FirstOrDefault();

        }

        public async Task<IList<GetCheckList>> GetCheckListDropdown(int tenantId)
        {
            var rawData = await (from clause in _context.Clauses                               
                                 select new GetCheckList
                                 {
                                     ClauseNumberText= clause.ClauseNumberText,
                                     DisplayText= $"{clause.ClauseNumberText}   {clause.DisplayText}",
                                     DisplayTextId = clause.ClauseId,
                                 }
                                 ).ToListAsync();
            return await Task.FromResult(rawData);
        }

        public async Task<IList<GetCheckListAuditProgramIdview>> GetChecklistAuditProgramId(int tenantId ,int auditProgramId)
        {
            var rawData = await(from checkList in _context.ChecklistMasters
                           join clause in _context.Clauses on checkList.ClauseMasterId equals clause.ClauseId
                           join audititemclause in _context.AuditItemClauses on checkList.ClauseMasterId equals audititemclause.ClauseMasterId
                           join AIC in _context.AuditableItemClauses on audititemclause.AuditableItemClauseId equals AIC.AuditableItemClauseId
                           join AI in _context.AuditableItems on AIC.AuditableItemId equals AI.Id
                           join AP in _context.AuditPrograms on AI.AuditProgramId equals AP.Id
                           join check in _context.AuditChecklists on checkList.Id equals check.Id
                           where AP.Id == auditProgramId
                                select new GetCheckListAuditProgramIdview
                           {
                              AuditCheckListId = checkList.Id,
                              ComplianceComments= check.Comments,
                              HasCompliance = check.Compliance,
                              Questions = checkList.Questions,
                              Reviewed = check.Reviewed,
                              Tags = clause.ClauseNumberText,

                           }).ToListAsync();
            return await Task.FromResult(rawData);
        }

            public async Task<IList<ChecklistMaster>> GetChecklistQuestions()
        {
            var checklistQuestion = await _context.ChecklistMasters.ToListAsync();
            return await Task.FromResult(checklistQuestion);
        }

        public async Task<ChecklistQuestionView> GetChecklistQuestionByAuditChecklistId(int auditChecklistId)
        {
            var checklistQuestion = (from ac in _context.AuditChecklists
                                     join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
                                     where ac.Id == auditChecklistId
                                     select new ChecklistQuestionView
                                     {
                                         Question = cm.Questions,
                                     }).AsQueryable();
            return checklistQuestion.FirstOrDefault();
        }

    }
}
