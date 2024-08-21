using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Models.Custom;

namespace Mindflur.IMS.Data.Repository
{
	public class AuditProgramRepository : BaseRepository<AuditProgram>, IAuditProgramRepository
	{
		private readonly IConfiguration _configuration;
		private readonly IEmailService _emailService;
		private readonly IUserRepository _userRepository;

		public AuditProgramRepository(IMSDEVContext dbContext, ILogger<AuditProgram> logger, IConfiguration configuration, IEmailService emailService, IUserRepository userRepository) : base(dbContext, logger)
		{
			_configuration = configuration;
			_emailService = emailService;
			_userRepository = userRepository;
		}

		public async Task<IList<AuditItemsView>> GetAuditItemsByProgram(int auditId)
		{
			var items = await (from ai in _context.AuditableItems
                               join dp in _context.DepartmentMasters on ai.DepartmentId equals dp.DepartmentId
                               where ai.AuditProgramId == auditId && ai.Status == (int)IMSItemStatus.Open
							   select new AuditItemsView
							   {
								   AuditItemId = ai.Id,
								   Department = dp.DepartmentName
							   }).ToListAsync();
			return await Task.FromResult(items);
		}

		public async Task<IList<AuditChecklistDetails>> GetAuditChecklistForCompletingMeeting(int tenantId, int auditId)
		{
			var auditChecklistDetails = (from ac in _context.AuditChecklists
										 join ap in _context.AuditPrograms on ac.AuditProgramId equals ap.Id
										 join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
										 join md in _context.MasterData on ac.MasterDataClassificationId equals md.Id into compliance
										 from type in compliance.DefaultIfEmpty()
										 where ac.AuditProgramId == auditId && ap.TenantId == tenantId && ac.Reviewed == false
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
										 }).ToList();
			return await Task.FromResult(auditChecklistDetails);
		}

		public async Task<IList<ManagementProgramView>> GetMeetingsForAuditReport()
		{
			var minutes = await (from meeting in _context.MeetingPlans

								 select new ManagementProgramView
								 {
									 Title = meeting.Title,
									 Date = meeting.StartDate
								 }).ToListAsync();
			return await Task.FromResult(minutes);
		}

		public async Task<PaginatedItems<AuditProgramGridView>> GetAuditProgramList(GetAuditProgramListRequest getListRequest)
		{
			string searchString = string.Empty;

			var rawData = (from ap in _context.AuditPrograms
						   join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId

						   where ap.TenantId == getListRequest.TenantId
						   select new AuditProgramGridView()
						   {
							   Id = ap.Id,
							   Title = ap.Title,
							   FromDate = ap.FromDate,
							   DueDate = ap.DueDate,
							   CreatedBy = ap.CreatedBy,
							   ActualStart = ap.ActualStart,
							   ActualEnd = ap.ActualEnd,
							   // Standards =  GetStandardsForAudit(ap.TenantId,ap.Id)
						   }).OrderByDescending(ap => ap.FromDate).AsQueryable();
			if (getListRequest.ForUserId > 0)
				rawData = rawData.Where(log => log.CreatedBy == getListRequest.ForUserId);

			var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
							  .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
							  .Take(getListRequest.ListRequests.PerPage);

			var totalItems = await rawData.LongCountAsync();

			int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);

			var model = new PaginatedItems<AuditProgramGridView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
			return await Task.FromResult(model);
		}

		public IList<AuditStandardsForGrid> GetStandardsForAudit(int tenantId, int auditId)
		{
			var standards = (from ap in _context.AuditPrograms
							 join ps in _context.ProgramStandards on ap.Id equals ps.AuditProgramId
							 join md in _context.MasterData on ps.MasterDataStandardId equals md.Id
							 where ap.TenantId == tenantId && ap.Id == auditId
							 select new AuditStandardsForGrid
							 {
								 standardId = ps.MasterDataStandardId,
								 Standards = md.Items,
							 }).ToList();
			return standards;
		}

		public async Task<IList<AuditProgramSchedule>> GetScheduledProgamByID(int auditProgramId)
		{
			var auditItems = await (from ap in _context.AuditPrograms
									join ai in _context.AuditableItems on ap.Id equals ai.AuditProgramId
                                    join dp in _context.DepartmentMasters on ai.DepartmentId equals dp.DepartmentId
                                    join ac in _context.AuditableItemClauses on ai.Id equals ac.AuditableItemId
									join aic in _context.AuditItemClauses on ac.AuditableItemClauseId equals aic.AuditableItemClauseId
									join cm in _context.ClauseMasters on aic.ClauseMasterId equals cm.Id
									where auditProgramId == ap.Id
									select new AuditProgramSchedule
									{
										Department = dp.DepartmentName,
										Clauses = cm.ClauseNo
									}).ToListAsync();
			return await Task.FromResult(auditItems);
		}

		public async Task<AuditProgramDetailView> GetAuditPreview(int? auditProgramId, int tenantId)
		{
			var audit = (from ap in _context.AuditPrograms
						 join mdt in _context.MasterData on ap.MasterDataCategoryId equals mdt.Id
						 join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
						 join user in _context.UserMasters on ap.ApprovedBy equals user.UserId into status1
						 from subUser in status1.DefaultIfEmpty()
						 join mdt1 in _context.MasterData on ap.Status equals mdt1.Id into status
						 from substatus in status.DefaultIfEmpty()
						 where ap.Id == auditProgramId && ap.TenantId == tenantId
						 select new AuditProgramDetailView
						 {
							 Id = ap.Id,
							 Title = ap.Title,
							 FromDate = ap.FromDate,
							 DueDate = ap.DueDate,
							 Category = mdt.Items,
							 CategoryId = ap.MasterDataCategoryId,
							 IsPublished = ap.IsPublish,
							 Scope = ap.Scope,
							 Objective = ap.Objectives,
							 AuditProgramStatus = substatus.Items,
							 ActualStart = ap.ActualStart,
							 ActualEnd = ap.ActualEnd,
							 Status = substatus.Items,
							 ApprovedById = ap.ApprovedBy,
							 ApprovedBy = $"{subUser.FirstName}{subUser.LastName}",
							 ApprovedOn = ap.ApprovedOn
						 }).AsQueryable();
			return audit.FirstOrDefault();
		}

		public async Task<AuditProgram> UpdateAuditProgram(int Id, PutAuditProgramViewModel putAuditProgram, int userId, int tenantId)
		{
			var auditProgram = await _context.AuditPrograms.Where(ap => ap.Id == Id).FirstOrDefaultAsync();
			if (auditProgram.Id == Id && auditProgram.TenantId == tenantId)
			{
				//Collect existing program standards and remove it.
				var existingProgramStandards = _context.ProgramStandards.Where(ps => ps.AuditProgramId == putAuditProgram.Id).ToList();
				_context.ProgramStandards.RemoveRange(existingProgramStandards);
				await _context.SaveChangesAsync(); //remove range

				var standardsForAuditProgram = new List<ProgramStandard>();

				//Add standard for a audit program
				foreach (int standard in putAuditProgram.Standards)
				{
					var newProgramStandards = new ProgramStandard
					{
						AuditProgramId = putAuditProgram.Id,
						MasterDataStandardId = standard
					};
					standardsForAuditProgram.Add(newProgramStandards);
				}

				await _context.ProgramStandards.AddRangeAsync(standardsForAuditProgram);
				await _context.SaveChangesAsync(); //add new standard for a audit program

				auditProgram.Title = putAuditProgram.Title;
				auditProgram.MasterDataCategoryId = putAuditProgram.Category;
				auditProgram.FromDate = putAuditProgram.FromDate;
				auditProgram.DueDate = putAuditProgram.DueDate;
				auditProgram.UpdatedBy = userId;
				auditProgram.UpdatedOn = DateTime.UtcNow;
				auditProgram.IsPublish = false;
				auditProgram.Status = (int)IMSItemStatus.Draft;
				auditProgram.Scope = putAuditProgram.Scope;
				auditProgram.Objectives = putAuditProgram.Objectives;
				await _context.SaveChangesAsync();
				AuditPlan auditPlan = new AuditPlan();
				auditPlan.AuditProgramId = auditProgram.Id;
				auditPlan.Scope = auditProgram.Scope;
				auditPlan.Objectives = auditProgram.Objectives;
				await _context.AuditPlans.AddRangeAsync(auditPlan);
				await _context.SaveChangesAsync();
				var usersList = await _userRepository.GetUserBytenantId(tenantId);
				var userList = usersList.Where(t => t.RoleId == (int)IMSRolesMaster.ISOChampion || t.RoleId == (int)IMSRolesMaster.Manager).ToList();

				foreach (var details in userList)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
					keyValuePairs.Add("#AUDITORS_NAME#", details.FullName);
					keyValuePairs.Add("#AUDIT_PROGRAM_ID#", auditProgram.Id.ToString());
					keyValuePairs.Add("#AUDIT_TITLE#", auditProgram.Title);
					keyValuePairs.Add("#AUDIT_CATEGORY#", auditProgram.MasterDataCategoryId.ToString());//Hack For Now
					keyValuePairs.Add("#START_DATE#", auditProgram.FromDate.ToString());
					keyValuePairs.Add("#END_DATE#", auditProgram.DueDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.FullName, "AuditPlanUpdate.html", $"Audit Plan Update > {auditProgram.Id} - {auditProgram.Title} ", keyValuePairs);
				}
				ActivityLog activity = new ActivityLog();
				activity.TenantId = tenantId;
				activity.ControllerId = (int)IMSControllerCategory.InternalAudit;
				activity.EntityId = auditProgram.Id;
				activity.ModuleAction = (int)IMSControllerActionCategory.Edit;
				activity.Description = "InternalAudit has been Edited";
				activity.Details = System.Text.Json.JsonSerializer.Serialize(putAuditProgram);
				activity.Status = true;
				activity.CreatedBy = userId;
				activity.CreatedOn = DateTime.UtcNow;
				await _context.ActivityLogs.AddAsync(activity);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new NotFoundException(string.Format(RepositoryConstant.IdNotFoundErrorMessage), Id);
			}
			return auditProgram;
		}

		public async Task<IList<AuditProgramEmail>> GetAuditEmailDetails(int auditId, int tenantId, int moduleId)
		{
			var emaildetails = await (from ap in _context.AuditPrograms
									  join md in _context.MasterData on ap.MasterDataCategoryId equals md.Id
									  join apar in _context.Participants on ap.Id equals apar.ModuleEntityId
									  join us in _context.UserMasters on apar.UserId equals us.UserId
									  join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
									  where ap.Id == auditId && ap.TenantId == tenantId && apar.ModuleId == moduleId && apar.DeletedBy == null
									  select new AuditProgramEmail
									  {
										  Name = us.FirstName,
										  EmailAddress = us.EmailId,
										  AuditProgramId = ap.Id,
										  Title = ap.Title,
										  Category = md.Items,
										  StartDate = ap.FromDate,
										  EndDate = ap.DueDate
									  }).ToListAsync();
			return await Task.FromResult(emaildetails);
		}

		public async Task<IList<AuditProgramEmail>> GetAuditEmailDetailsForpublish(int auditId, int tenantId, int moduleId)
		{
			var emaildetails = await (from ap in _context.AuditPrograms
									  join md in _context.MasterData on ap.MasterDataCategoryId equals md.Id
									  join apar in _context.Participants on ap.Id equals apar.ModuleEntityId
									  join us in _context.UserMasters on apar.UserId equals us.UserId
									  join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
									  where ap.Id == auditId && ap.TenantId == tenantId && apar.ModuleId == moduleId && apar.DeletedBy == null && us.RoleId == (int)IMSRolesMaster.ISOChampion || us.RoleId == (int)IMSRolesMaster.Manager
									  select new AuditProgramEmail
									  {
										  Name = us.FirstName,
										  EmailAddress = us.EmailId,
										  AuditProgramId = ap.Id,
										  Title = ap.Title,
										  Category = md.Items,
										  StartDate = ap.FromDate,
										  EndDate = ap.DueDate
									  }).ToListAsync();
			return await Task.FromResult(emaildetails);
		}

		public async Task<IList<AuditProgramEmail>> NightlyRemiderMailAuditProgram()
		{
			var emaildetails = await (from ap in _context.AuditPrograms

									  join p in _context.Participants on ap.Id equals p.ModuleEntityId
									  join us in _context.UserMasters on p.UserId equals us.UserId
									  join md in _context.MasterData on ap.MasterDataCategoryId equals md.Id

									  where ap.FromDate == DateTime.UtcNow.Date.AddDays(2) && ap.Status == (int)IMSItemStatus.Open && ap.TenantId != 12 && p.ModuleId == 1

									  select new AuditProgramEmail
									  {
										  Name = us.FirstName,
										  EmailAddress = us.EmailId,
										  AuditProgramId = ap.Id,
										  Title = ap.Title,
										  Category = md.Items,
										  StartDate = ap.FromDate,
										  EndDate = ap.DueDate
									  }).ToListAsync();
			return await Task.FromResult(emaildetails);
		}

		public async Task<IList<AuditProgramEmail>> NightlyRemiderMailToAuditorForItems()
		{
			var emaildetails = await (from ap in _context.AuditPrograms
									  join ai in _context.AuditableItems on ap.Id equals ai.AuditProgramId
									  join dm in _context.DepartmentMasters on ai.DepartmentId equals dm.DepartmentId

									  join us in _context.UserMasters on ai.AuditorName equals us.UserId

									  where ai.StartDate == DateTime.UtcNow.Date.AddDays(2) && ap.TenantId != 12

									  select new AuditProgramEmail
									  {
										  Name = us.FirstName,
										  EmailAddress = us.EmailId,
										  AuditProgramId = ap.Id,
										  Title = dm.DepartmentName,
										  Category = dm.DepartmentName,
										  StartDate = ai.StartDate,
										  EndDate = ai.EndDate
									  }).ToListAsync();
			return await Task.FromResult(emaildetails);
		}

		public async Task<AuditReportDetails> AuditProgramDetails(int auditId)

		{
			var rawdata = (from ap in _context.AuditPrograms
						   //join apl in _context.AuditPlans on ap.Id equals apl.AuditProgramId
						   join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
						   where ap.Id == auditId 
						   select new AuditReportDetails
						   {
							   Id = ap.Id,
							   CompanyName = tm.Name,
							   StartDate = ap.FromDate,
							   EndDate = ap.DueDate,
							   Objectives = ap.Objectives,
							   Scope = ap.Scope,
						   }).AsQueryable();
			return rawdata.FirstOrDefault();
		}

		public async Task<IList<AuditProgramObjective>> AuditProgramObjectives(int auditId)

		{
			var rawdata = await (from ap in _context.AuditPrograms
								 join apl in _context.AuditPlans on ap.Id equals apl.AuditProgramId
								 where ap.Id == auditId
								 select new AuditProgramObjective
								 {
									 Objectives = apl.Objectives,
								 }).ToListAsync();
			return await Task.FromResult(rawdata);
		}

		public async Task<IList<AuditProgramReport>> GetAuditDetailsForReport(int auditId)
		{
			var standards = await (from ap in _context.AuditPrograms
								   join ps in _context.ProgramStandards on ap.Id equals ps.AuditProgramId
								   join md in _context.MasterData on ps.MasterDataStandardId equals md.Id
								   where ap.Id == auditId
								   select new AuditProgramReport
								   {
									   Standards = md.Items
								   }).ToListAsync();
			return await Task.FromResult(standards);
		}

		public async Task<IList<AuditReportNonConfirmities>> GetAuditNonCobfirmityForReport(int auditId)
		{
			var ncConfirmityforaudit = await (from ai in _context.AuditableItems
											  join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
											  join af in _context.AuditFindings on ai.Id equals af.AuditableItemId
											  join acf in _context.AuditChecklistFindings on af.Id equals acf.AuditFindingId
											  join ac in _context.AuditChecklists on acf.AuditChecklistId equals ac.Id
											  join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
											  join cl in _context.ClauseMasters on cm.ClauseMasterId equals cl.Id
											  join md in _context.MasterData on ai.DepartmentId equals md.Id
											  join md1 in _context.MasterData on aic.MasterDataStandardId equals md1.Id
											  join md2 in _context.MasterData on ac.MasterDataClassificationId equals md2.Id
											  where ai.AuditProgramId == auditId && (ac.MasterDataClassificationId == (int)IMSMasterAuditChecklistCategory.Major || ac.MasterDataClassificationId == (int)IMSMasterAuditChecklistCategory.Minor)
											  select new AuditReportNonConfirmities
											  {
												  NcId = af.Id,
												  Description = af.Description,
												  Department = md.Items,
												  Clause = cl.ClauseNo,
												  Standards = md1.Items,
												  Classification = md2.Items
											  }).ToListAsync();
			return await Task.FromResult(ncConfirmityforaudit);
		}

		public async Task<IList<AuditReportOpportunitiesAndObservations>> GetAuditObservationOpportunityForReport(int auditId)
		{
			var ncConfirmityforaudit = await (from ai in _context.AuditableItems
											  join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
											  join af in _context.AuditFindings on ai.Id equals af.AuditableItemId
											  join acf in _context.AuditChecklistFindings on af.Id equals acf.AuditFindingId
											  join ac in _context.AuditChecklists on acf.AuditChecklistId equals ac.Id
											  join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
											  join cl in _context.ClauseMasters on cm.ClauseMasterId equals cl.Id
											  join md in _context.MasterData on ai.DepartmentId equals md.Id
											  join md1 in _context.MasterData on aic.MasterDataStandardId equals md1.Id
											  join md2 in _context.MasterData on ac.MasterDataClassificationId equals md2.Id
											  where ai.AuditProgramId == auditId && (ac.MasterDataClassificationId == (int)IMSMasterAuditChecklistCategory.Observation || ac.MasterDataClassificationId == (int)IMSMasterAuditChecklistCategory.OpportunitiesForImprovement)
											  select new AuditReportOpportunitiesAndObservations
											  {
												  Id = af.Id,
												  Department = md.Items,
												  Classification = md2.Items,
												  Standards = md1.Items,
												  Description = af.Description,
											  }).ToListAsync();
			return await Task.FromResult(ncConfirmityforaudit);
		}

		public async Task<IList<PreviousAuidtNonconformance>> GetPreviousAuditNonConformities(int auditId)
		{
			var previousNonConformity = await (from af in _context.WorkItemMasters
											   join ap in _context.AuditPrograms on af.SourceItemId equals ap.Id
											   join md in _context.MasterData on ap.MasterDataCategoryId equals md.Id
											   join md1 in _context.MasterData on af.CategoryId equals md1.Id

											   where ap.Id == auditId && af.WorkItemTypeId == (int)IMSModules.NonConformity && af.SourceId == (int)IMSModules.InternalAudit
											   select new PreviousAuidtNonconformance
											   {
												   Id = af.WorkItemId,
												   AuditType = md.Items,
												   ClassificationOfNc = md1.Items,
												   DescriptionOfNc = af.Description
											   }).ToListAsync();
			return await Task.FromResult(previousNonConformity);
		}

		public async Task<AuditProgramCreatedOn> GetAuditCreatedDate(int auditId)
		{
			var createdDate = (from ap in _context.AuditPrograms
							   where ap.Id == auditId
							   select new AuditProgramCreatedOn
							   {
								   CreatedOn = ap.CreatedOn,
								   AuditStartDate = ap.ActualStart,
							   }).AsQueryable();
			return createdDate.FirstOrDefault();
		}

		public async Task<AuditProgramDetailsFormCreatedOn> GetAuditIdFromCreatedOnDate(DateTime createdOn)
		{
			var auditId = (from ap in _context.AuditPrograms
						   where ap.CreatedOn < createdOn
						   orderby ap.CreatedOn descending
						   select new AuditProgramDetailsFormCreatedOn
						   {
							   AuditId = ap.Id,
						   }).AsQueryable();
			return auditId.FirstOrDefault();
		}

		public async Task<AuditProgram> GetAuditDetails(int auditId, int tenantId)
		{
			var auditprogram = (from tm in _context.TenanttMasters
								join ap in _context.AuditPrograms on tm.TenantId equals ap.TenantId
								into audit
								from subaudit in audit.DefaultIfEmpty()
								where subaudit.Id == auditId && subaudit.TenantId == tenantId
								select new AuditProgram
								{
									Id = subaudit.Id,
									TenantId = subaudit.TenantId,
									Title = subaudit.Title,
									MasterDataCategoryId = subaudit.MasterDataCategoryId,
									FromDate = subaudit.FromDate,
									DueDate = subaudit.DueDate,
									CreatedBy = subaudit.CreatedBy,
									CreatedOn = subaudit.CreatedOn,
									UpdatedBy = subaudit.UpdatedBy,
									UpdatedOn = subaudit.UpdatedOn,
									IsPublish = subaudit.IsPublish,
									PublishedOn = subaudit.PublishedOn,
									ApprovedBy = subaudit.ApprovedBy,
									ApprovedOn = subaudit.ApprovedOn,
									ActualStart = subaudit.ActualStart,
									ActualEnd = subaudit.ActualEnd,
									Status = subaudit.Status,
								}).AsQueryable();
			return auditprogram.FirstOrDefault();
		}

		public async Task<IList<AuditProgramDropDown>> GetAuditDropdownList(int tenantId)
		{
			var dropdwn = await (from ap in _context.AuditPrograms
								 where ap.TenantId == tenantId
								 select new AuditProgramDropDown
								 {
									 auditId = ap.Id,
									 Title = $"{ap.Title} {ap.ActualStart}",
								 }).OrderByDescending(x => x.auditId).ToListAsync();
			return await Task.FromResult(dropdwn);
		}

		public async Task<IList<AuditableItem>> GetAuditItemsToCompleteAudit(int auditId, int tenantId)
		{
			var auditItems = await (from ai in _context.AuditableItems
									join ap in _context.AuditPrograms on ai.AuditProgramId equals ap.Id
									join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
									where ap.Id == auditId && ap.TenantId == tenantId && ai.Status != (int)IMSItemStatus.Closed
									select new AuditableItem
									{
										Id = ai.Id,
										AuditProgramId = ai.AuditProgramId,
										//AuditableItems = ai.AuditableItems,
										AuditorName = ai.AuditorName,
										StartDate = ai.StartDate,
										EndDate = ai.EndDate,
										Status = ai.Status,
										DepartmentId = ai.DepartmentId,
										//Type = ai.Type,
										CreatedBy = ai.CreatedBy,
										CreatedOn = ai.CreatedOn,
										UpdatedBy = ai.UpdatedBy,
										UpdatedOn = ai.UpdatedOn
									}).ToListAsync();
			return await Task.FromResult(auditItems);
		}

		public async Task<CompliancePercentageValue> getCompliancePerCentage(int tenantId, int auditId)
		{
			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();
			return await conn.QueryFirstAsync<CompliancePercentageValue>(
				@"select count(*) * 100.0 / (select count(*) from AuditCheckList ) as PercentageValue
                from AuditCheckList as ac join AuditProgram as ap on ac.AuditPRogramId=ap.Id
                where ap.Id=@auditId and ac.compliance=1 and ap.TenantId=@tenantId", new { auditId, tenantId }

				);
		}

		public async Task<ReviewedPercentageValue> getReviewPerCentage(int auditId, int tenantId)
		{
			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();
			return await conn.QueryFirstAsync<ReviewedPercentageValue>(
					@"select count(*) * 100.0 / (select count(*) from AuditCheckList ) as PercentageValue
                from AuditCheckList as ac join AuditProgram as ap on ac.AuditPRogramId=ap.Id
                where ap.Id=@auditId and ac.Reviewed=1 and ap.TenantId=@tenantId", new { auditId, tenantId }

					);
		}

		public async Task<IList<AuditDepartmentForReport>> GetAuditDepartmentListForReport(int auditId)
		{
			var rawData = await (from ai in _context.AuditableItems
								 join ap in _context.AuditPrograms on ai.AuditProgramId equals ap.Id
								 join dept in _context.DepartmentMasters on ai.DepartmentId equals dept.DepartmentId
								 where ap.Id == auditId
								 select new AuditDepartmentForReport()
								 {
									 Department = dept.DepartmentName
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<IList<GetTotalMajorNonconformances>> getTotalMajorNc(int auditId)
		{
			var rawData = await (from wt in _context.WorkItemMasters
								 join ap in _context.AuditPrograms on wt.SourceItemId equals ap.Id
								 where wt.SourceId == (int)IMSModules.InternalAudit && wt.CategoryId == (int)IMSModules.MajorNonConformance && wt.SourceItemId == auditId
								 select new GetTotalMajorNonconformances()
								 {
									 MajorNc = wt.CategoryId
								 }
							  ).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<IList<GetTotalMinorNonconformances>> getTotalMinorNc(int auditId)
		{
			var rawData = await (from wt in _context.WorkItemMasters
								 join ap in _context.AuditPrograms on wt.SourceItemId equals ap.Id
								 where wt.SourceId == (int)IMSModules.InternalAudit && wt.CategoryId == (int)IMSModules.MinorNonConformance && wt.SourceItemId == auditId
								 select new GetTotalMinorNonconformances()
								 {
									 MinorNc = wt.CategoryId
								 }
							  ).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<IList<GetTotalObservation>> getTotalObservation(int auditId)
		{
			var rawData = await (from wt in _context.WorkItemMasters
								 join ap in _context.AuditPrograms on wt.SourceItemId equals ap.Id
								 where wt.SourceId == (int)IMSModules.InternalAudit && wt.WorkItemTypeId == (int)IMSModules.Observation && wt.SourceItemId == auditId
								 select new GetTotalObservation()
								 {
									 Observation = wt.CategoryId
								 }
							  ).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<IList<GetNonConformances>> GetNonConformances(int auditId)
		{
			var rawData = await (from wt in _context.WorkItemMasters
								 join ap in _context.AuditPrograms on wt.SourceItemId equals ap.Id
								 join md in _context.MasterData on wt.CategoryId equals md.Id into md
								 from subCategory in md.DefaultIfEmpty()
								 join dept in _context.DepartmentMasters on wt.DepartmentId equals dept.DepartmentId into dept
								 from subDept in dept.DefaultIfEmpty()
								 join ai in _context.AuditableItems on ap.Id equals ai.AuditProgramId
								 join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
								 join ac in _context.AuditItemClauses on aic.AuditableItemClauseId equals ac.AuditableItemClauseId
								 join clause in _context.Clauses on ac.ClauseMasterId equals clause.ClauseId
								 join standard in _context.MasterData on clause.StandardId equals standard.Id
								 where wt.SourceId == (int)IMSModules.InternalAudit && wt.WorkItemTypeId == (int)IMSModules.NonConformity && wt.SourceItemId == auditId
								 select new GetNonConformances()
								 {
									 NCRNo = wt.WorkItemId,
									 Description = wt.Description,
									 Department = subDept.DepartmentName,
									 Clause = clause.ClauseNumberText,
									 Standard = standard.Items,
									 Classification = subCategory.Items,
								 }
							  ).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<IList<GetObservations>> GetObservations(int auditId)
		{
			var rawData = await (from wt in _context.WorkItemMasters
								 join ap in _context.AuditPrograms on wt.SourceItemId equals ap.Id
								 join md in _context.MasterData on wt.WorkItemTypeId equals md.Id into md
								 from subCategory in md.DefaultIfEmpty()
								 join dept in _context.DepartmentMasters on wt.DepartmentId equals dept.DepartmentId into dept
								 from subDept in dept.DefaultIfEmpty()
								 join ai in _context.AuditableItems on ap.Id equals ai.AuditProgramId
								 join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
								 // join ac in _context.AuditItemClauses on aic.AuditableItemClauseId equals ac.AuditableItemClauseId
								 // join clause in _context.Clauses on ac.ClauseMasterId equals clause.ClauseId
								 join standard in _context.MasterData on aic.MasterDataStandardId equals standard.Id
								 where wt.SourceId == (int)IMSModules.InternalAudit && wt.WorkItemTypeId == (int)IMSModules.Observation && wt.SourceItemId == auditId
								 select new GetObservations()
								 {
									 NCRNo = wt.WorkItemId,
									 Department = subDept.DepartmentName,
									 Observation = subCategory.Items,
									 Standard = standard.Items,
									 Description = wt.Description,
								 }
							  ).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<IList<GetNonConformanceLegend>> NonConformanceLegendForReport(int auditId)
		{
			var rawData = await (from wt in _context.WorkItemMasters
								 join ap in _context.AuditPrograms on wt.SourceItemId equals ap.Id
								 join md in _context.MasterData on wt.CategoryId equals md.Id into md
								 from subCategory in md.DefaultIfEmpty()

								 where wt.SourceId == (int)IMSModules.InternalAudit && wt.WorkItemTypeId == (int)IMSModules.NonConformity && wt.SourceItemId == auditId
								 select new GetNonConformanceLegend()
								 {
									 Grading = subCategory.Items,
									 Explanation = wt.Description
								 }
							  ).ToListAsync();
			return await Task.FromResult(rawData);
		}
		public async Task<IList<AuditPlanByDepartmentList>> GetDepartmentListfromAuditId(int auditId, int tenantId)
		{
			var rawdata = await (from ai in _context.AuditPrograms
								 join ap in _context.AuditableItems on ai.Id equals ap.AuditProgramId
								 join dp in _context.DepartmentMasters on ap.DepartmentId equals dp.DepartmentId
								 join tm in _context.TenanttMasters on ai.TenantId equals tm.TenantId
								 where ai.Id == auditId && ai.TenantId == tenantId
								 select new AuditPlanByDepartmentList
								 {
									 DepartmentId = dp.DepartmentId,
									 DepartmentName = dp.DepartmentName,

								 }).ToListAsync();
			return await Task.FromResult(rawdata);

		}

        public async Task<IList<StandardList>> GetStandardsLitFromAuditId(int AuditId, int tenantId)
		{
			var rawData = await (from ap in _context.AuditPrograms
								 join ps in _context.ProgramStandards on ap.Id equals ps.AuditProgramId
								 join md in _context.MasterData on ps.MasterDataStandardId equals md.Id
                                 join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
                                 where ap.Id == AuditId && ap.TenantId == tenantId
								 select new StandardList
								 {
									 StandardId = ps.MasterDataStandardId,
									 Standard = md.Items
								 }
								 ).ToListAsync();
			return await Task.FromResult(rawData);
		}


    }
}