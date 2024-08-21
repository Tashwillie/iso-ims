using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Mindflur.IMS.Data.Repository
{
	public class AuditChecklistRepository : BaseRepository<AuditChecklist>, IAuditChecklistRepository
	{

		private readonly IChecklistQuestionRepository _checklistQuestionRepository;

		private readonly IWorkItemBusiness _workItemBusiness;

		public AuditChecklistRepository(IMSDEVContext dbContext, ILogger<AuditChecklist> logger, IChecklistQuestionRepository checklistQuestionRepository,
			 IWorkItemBusiness workItemBusiness) : base(dbContext, logger)
		{

			_checklistQuestionRepository = checklistQuestionRepository;

			_workItemBusiness = workItemBusiness;

		}




		public async Task<IList<ClauseDetails>> GetClauseIdFromAuditProgram(int auditId, int tenantId)
		{
			var clause = await (from ai in _context.AuditableItems
								join ap in _context.AuditPrograms on ai.AuditProgramId equals ap.Id
								join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
								join acc in _context.AuditItemClauses on aic.AuditableItemClauseId equals acc.AuditableItemClauseId
								join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
								where ai.AuditProgramId == auditId && ap.TenantId == tenantId
								select new ClauseDetails
								{
									ClauseId = acc.ClauseMasterId
								}).ToListAsync();
			return await Task.FromResult(clause);
		}

		public async Task<IList<ClauseDetails>> GetClauseIdFromDepartmentId(int auditId, int tenantId, int departmentId)
		{
			var clause = await (from ai in _context.AuditableItems
								join ap in _context.AuditPrograms on ai.AuditProgramId equals ap.Id
								join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
								join acc in _context.AuditItemClauses on aic.AuditableItemClauseId equals acc.AuditableItemClauseId
								join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
								where ai.AuditProgramId == auditId && ap.TenantId == tenantId && ai.DepartmentId == departmentId
								select new ClauseDetails
								{
									ClauseId = acc.ClauseMasterId
								}).ToListAsync();
			return await Task.FromResult(clause);
		}

		public async Task<IList<AuditChecklistView>> GetAuditChecklistForClause(int auditId, int clauseId, int tenantId)
		{


			var checklist = await (from ac in _context.AuditChecklists
								   join ap in _context.AuditPrograms on ac.AuditProgramId equals ap.Id
								   join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
								   join clm in _context.Clauses on cm.ClauseMasterId equals clm.ClauseId
								   join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
								   where ap.Id == auditId && clm.ParentId == clauseId && ap.TenantId == tenantId
								   select new AuditChecklistView
								   {
									   AuditChecklistId = ac.Id,
									   Questions = cm.Questions,
									   ComplianceComments = ac.Comments,
									   hasCompliance = ac.Compliance,
									   Reviewed = ac.Reviewed == true ? "Reviewed" : "Not Reviewed",
									   Tags = clm.ClauseNumberText,
								   }).ToListAsync();
			return await Task.FromResult(checklist);
		}

		public async Task<IList<AuditChecklistView>> GetAuditChecklistWithComplianceFilter(int auditId, bool compliance, int tenantId)
		{
			var checklist = await (from ac in _context.AuditChecklists
								   join ap in _context.AuditPrograms on ac.AuditProgramId equals ap.Id
								   join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
								   join clm in _context.Clauses on cm.ClauseMasterId equals clm.ClauseId
								   join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
								   where ap.Id == auditId && ac.Compliance == compliance && ap.TenantId == tenantId
								   select new AuditChecklistView
								   {
									   AuditChecklistId = ac.Id,
									   Questions = cm.Questions,
									   ComplianceComments = ac.Comments,
									   hasCompliance = ac.Compliance,
									   Reviewed = ac.Reviewed == true ? "Reviewed" : "Not Reviewed",
									   Tags = clm.ClauseNumberText,
								   }).ToListAsync();
			return await Task.FromResult(checklist);
		}

		public async Task UpdateChecklist(int Id, PutAuditChecklistViewModel responce, int userId, int tenantId, string path)
		{
			var checklistResponce = await _context.AuditChecklists.FindAsync(Id);

			if (checklistResponce == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ChecklistNotFoundErrorMessage), Id);
			}
			else
			{
				checklistResponce.AuditProgramId = responce.AuditProgramId;
				checklistResponce.Compliance = responce.Compliance;
				checklistResponce.MasterDataClassificationId = responce.MasterDataClassificationId;
				checklistResponce.Comments = responce.Discription;

				checklistResponce.Reviewed = true;
				await _context.SaveChangesAsync();

				if (checklistResponce.Compliance == false)
				{
					var existingMapping = await _context.AuditFindingsMappings.Where(x => x.AuditChecklistId == Id).FirstOrDefaultAsync();
					if (existingMapping != null)
					{
						var existingworkitem = await _context.WorkItemMasters.Where(x => x.WorkItemId == existingMapping.WorkItemId).FirstOrDefaultAsync();
						_context.Remove(existingworkitem);
						_context.Remove(existingMapping);
					}

					var auidtitems = await _context.AuditableItems.FindAsync(responce.AuditableItemId);
					var question = await _checklistQuestionRepository.GetChecklistQuestionByAuditChecklistId(Id);

					var findingPostView = new FindingPostView();
					findingPostView.AuditChecklistId = checklistResponce.Id;
					findingPostView.Title = responce.Title;
					findingPostView.Description = responce.Discription;
					findingPostView.AuditableItemId = responce.AuditableItemId;
					findingPostView.CategoryId = checklistResponce.MasterDataClassificationId;
					if (checklistResponce.MasterDataClassificationId == (int)IMSModules.MajorNonConformance)
					{
						findingPostView.DueDate= DateTime.Now.AddDays(30);
					}
					else if(checklistResponce.MasterDataClassificationId == (int)IMSModules.MinorNonConformance)
					{
						findingPostView.DueDate = DateTime.Now.AddDays(60);
					}
					findingPostView.ResponsibleUserId = auidtitems.AuditorName;
					findingPostView.DepartmentId = auidtitems.DepartmentId;
					findingPostView.FollowUp = responce.FollowUp;

					await _workItemBusiness.AddFinding(findingPostView, responce.AuditProgramId, userId, tenantId, path);
				}
			}
		}

		public async Task<IList<ChecklistQuestionsForReport>> GetChecklistQuestionsForClauseReport(int auditid, int tenantId, int clauseId)
		{
			var clause = await (from ac in _context.AuditChecklists
								join ap in _context.AuditPrograms on ac.AuditProgramId equals ap.Id
								join cqm in _context.ChecklistMasters on ac.ChecklistMasterId equals cqm.Id
								join cm in _context.Clauses on cqm.ClauseMasterId equals cm.ClauseId
								join md in _context.MasterData on ac.MasterDataClassificationId equals md.Id into md
								from severity in md.DefaultIfEmpty()
								where ap.Id == auditid && ap.TenantId == tenantId && cm.ClauseId == clauseId
								select new ChecklistQuestionsForReport
								{
									ClauseNo = cm.ClauseNumberText,
									Questions = cqm.Questions,
									hasCompliance = ac.Compliance == true ? "Yes" : "No",
									Severity = severity.Items == null ? "N/A" : severity.Items,
									ComplianceComments = ac.Comments == null ? "   " : ac.Comments
								}).ToListAsync();
			return await Task.FromResult(clause);
		}
		public async Task<IList<AuditChecklistView>> GetAuditChecklistByAuditId(int auditId, int tenantId, int clauseId)
		{


			var checklist = await (from ac in _context.AuditChecklists
								   join ap in _context.AuditPrograms on ac.AuditProgramId equals ap.Id
								   join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
								   join clm in _context.Clauses on cm.ClauseMasterId equals clm.ClauseId
								   join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
								   where ap.Id == auditId && ap.TenantId == tenantId && clm.ClauseId == clauseId
								   select new AuditChecklistView
								   {
									   AuditChecklistId = ac.Id,
									   Questions = cm.Questions,
									   ComplianceComments = ac.Comments,
									   hasCompliance = ac.Compliance,
									   HasReviewed = ac.Reviewed,
									   Reviewed = ac.Reviewed == true ? "Reviewed" : "Not Reviewed",
									   Tags = clm.ClauseNumberText
								   }).ToListAsync();
			return await Task.FromResult(checklist);
		}
		public async Task<IList<AuditReportBarChartDataForCompliance>> GetChecklistForBarChartByAuditId(int auditId, int tenantId, int clauseId)
		{


			var checklist = await (from ac in _context.AuditChecklists
								   join ap in _context.AuditPrograms on ac.AuditProgramId equals ap.Id
								   join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
								   join clm in _context.Clauses on cm.ClauseMasterId equals clm.ClauseId
								   join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
								   where ap.Id == auditId && ap.TenantId == tenantId && clm.ClauseId == clauseId
								   select new AuditReportBarChartDataForCompliance
								   {
									   ClauseId = clm.ClauseId,
									   ClauseNumber = clm.ClauseNumberText,
									   ClauseText = clm.DisplayText,
									   hasCompliance = ac.Compliance
								   }).ToListAsync();
			return await Task.FromResult(checklist);
		}

		public async Task<ClauseDetails> GetClauseIdFromChecklistId(int auditId, int checklistId, int tenantId)
		{
			var clause = (from ac in _context.AuditChecklists
						  join ap in _context.AuditPrograms on ac.AuditProgramId equals ap.Id
						  join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
						  join clm in _context.ChecklistMasters on ac.ChecklistMasterId equals clm.Id
						  join cm in _context.Clauses on clm.ClauseMasterId equals cm.ClauseId
						  where ac.AuditProgramId == auditId && ac.Id == checklistId && ap.TenantId == tenantId
						  select new ClauseDetails
						  {
							  ClauseId = cm.ClauseId
						  }).AsQueryable();
			return clause.FirstOrDefault();
		}


		public async Task<IList<AuditItemDetailsFromClauses>> GetAuditItemsFromClauseId(int auditId, int clauseId, int tenantId)
		{
			var auditItems = await (from ai in _context.AuditableItems
									join ap in _context.AuditPrograms on ai.AuditProgramId equals ap.Id
									join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
									join ic in _context.AuditItemClauses on aic.AuditableItemClauseId equals ic.AuditableItemClauseId
									join cm in _context.Clauses on ic.ClauseMasterId equals cm.ClauseId
									join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
									join dep in _context.DepartmentMasters on ai.DepartmentId equals dep.DepartmentId
									where ap.Id == auditId && cm.ClauseId == clauseId && ap.TenantId == tenantId && ai.Status == (int)IMSItemStatus.Open
									select new AuditItemDetailsFromClauses
									{
										AuditItemId = ai.Id,
										//AuditItemTitle = ai.AuditableItems
										Department = dep.DepartmentName
									}).ToListAsync();

			return await Task.FromResult(auditItems);
		}

		public async Task<AuditChecklistDetails> GetAuditChecklistDetailsFromChecklistId(int checklistId, int tenantId)
		{
			var auditChecklistDetails = (from ac in _context.AuditChecklists
										 join ap in _context.AuditPrograms on ac.AuditProgramId equals ap.Id
										 join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
										 join md in _context.MasterData on ac.MasterDataClassificationId equals md.Id into compliance
										 from type in compliance.DefaultIfEmpty()
										 join afm in _context.AuditFindingsMappings on ac.Id equals afm.AuditChecklistId into mapping
										 from submapping in mapping.DefaultIfEmpty()
										 join ai in _context.AuditableItems on submapping.AuditableItemId equals ai.Id into items
										 from subitems in items.DefaultIfEmpty()
										 where ac.Id == checklistId && ap.TenantId == tenantId
										 select new AuditChecklistDetails
										 {
											 AuditCheckListId = ac.Id,
											 AuditProgramId = ac.AuditProgramId,
											 Compliance = ac.Compliance,
											 MasterDataClassificationId = ac.MasterDataClassificationId,
											 MasterDataClassification = type.Items,
											 Comments = ac.Comments,
											 Reviewed = ac.Reviewed == true ? "Reviewed" : "Not Reviewed",
											 ReviewedDbt = ac.Reviewed,
											 AuditItemId = subitems.Id,
											 //AuditItem = subitems.AuditableItems,
											 FollowUp = submapping.FollowUp
										 }).AsQueryable();
			return auditChecklistDetails.FirstOrDefault();
		}



		public async Task<BackTrace> GetAuditCheckListByObservationId(int moduleEntitiyId)
		{
			var rawdata = (from ac in _context.AuditChecklists
						   join acf in _context.AuditChecklistFindings on ac.Id equals acf.AuditChecklistId
						   join af in _context.AuditFindings on acf.AuditFindingId equals af.Id
						   join ob in _context.ObservationMasters on af.Id equals ob.SourceId
						   join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
						   join um in _context.UserMasters on acf.CreatedBy equals um.UserId
						   where ob.Id == moduleEntitiyId && ob.Source == 90
						   select new BackTrace
						   {
							   ModuleId = (int)IMSControllerCategory.AuditCheckList,
							   ModuleName = "AuditChecklist",
							   ModuleItemId = ac.Id,
							   Title = cm.Questions,
							   Content = ac.Comments,
							   CreatedOn = acf.CreatedOn,
							   CreatedBy = $"{um.FirstName} {um.LastName}",
							   OrderNumber = 3
						   }).AsQueryable();
			return rawdata.FirstOrDefault();
		}

		public async Task<BackTrace> GetAuditCheckListByOpportunitiesId(int moduleEntitiyId)
		{
			var rawdata = (from ac in _context.AuditChecklists
						   join acf in _context.AuditChecklistFindings on ac.Id equals acf.AuditChecklistId
						   join af in _context.AuditFindings on acf.AuditFindingId equals af.Id
						   join om in _context.OpportunitiesMasters on af.Id equals om.SourceId
						   join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
						   join um in _context.UserMasters on acf.CreatedBy equals um.UserId
						   where om.Id == moduleEntitiyId && om.Source == 90
						   select new BackTrace
						   {
							   ModuleId = (int)IMSControllerCategory.AuditCheckList,
							   ModuleName = "AuditChecklist",
							   ModuleItemId = ac.Id,
							   Title = cm.Questions,
							   Content = ac.Comments,
							   CreatedOn = acf.CreatedOn,
							   CreatedBy = $"{um.FirstName} {um.LastName}",
							   OrderNumber = 3
						   }).AsQueryable();
			return rawdata.FirstOrDefault();
		}





		public async Task<IList<CategoryListView>> GetcategoryList(int masterDataGroupId)
		{
			var category = await (from md in _context.MasterData

								  where md.MasterDataGroupId == masterDataGroupId && md.Id != (int)IMSMasterWorkItemCategory.Risk
								  select new CategoryListView
								  {
									  CategoryId = md.Id,
									  Category = md.Items

								  }).ToListAsync();
			return await Task.FromResult(category);
		}

		public async Task<IList<ClauseDetails>> GetChecklistByStandard(int auditId, int tenantId, int standardId)
		{
			var rawdata = await (from ap in _context.AuditPrograms
								 join ai in _context.AuditableItems on ap.Id equals ai.AuditProgramId
								 join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
								 join ac in _context.AuditItemClauses on aic.AuditableItemClauseId equals ac.AuditableItemClauseId
								 join cs in _context.Clauses on ac.ClauseMasterId equals cs.ClauseId
								 join md in _context.MasterData on cs.StandardId equals md.Id
								 join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
								 where ap.Id == auditId && ap.TenantId == tenantId && aic.MasterDataStandardId == standardId
								 select new ClauseDetails
								 {
									 ClauseId = cs.ClauseId
								 }).ToListAsync();
			return await Task.FromResult(rawdata);
		}
	}
}