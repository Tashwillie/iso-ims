using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class CorrectiveActionMetaDataRepository : BaseRepository<CorrectiveActionMetadata>, ICorrectiveActionMetaDataRepository
	{
		public CorrectiveActionMetaDataRepository(IMSDEVContext dbContext, ILogger<CorrectiveActionMetadata> logger) : base(dbContext, logger)
		{
		}

		public async Task<CorrectiveActionMetadata> getCorrectiveActionMetaDataByWorkItemId(int workItemId)
		{
			var data = await _context.CorrectiveActionMetaDatas.FirstOrDefaultAsync(t => t.WorkItemId == workItemId);

			return data;
		}

		public async Task<GetCorrectiveActionMetadataView> GetMetadata(int workItemId, int tenantId)
		{
			var metadata = (from rca in _context.CorrectiveActionMetaDatas
							join user in _context.UserMasters on rca.UpdatedBy equals user.UserId into user
							from subuser in user.DefaultIfEmpty()
							where rca.WorkItemId == workItemId
							select new GetCorrectiveActionMetadataView()
							{
								Id = rca.Id,
								WorkItemId = rca.WorkItemId,
								IsApproved = rca.IsApproved,
								WhyAnalysis1 = rca.WhyAnalysis1,
								WhyAnalysis2 = rca.WhyAnalysis2,
								WhyAnalysis3 = rca.WhyAnalysis3,
								WhyAnalysis4 = rca.WhyAnalysis4,
								WhyAnalysis5 = rca.WhyAnalysis5,
								RootCauseAnalysis = rca.RootCauseAnalysis,
								UpdatedById = rca.UpdatedBy,
								UpdatedOn = rca.UpdatedOn,
								UpdatedBy = $"{subuser.FirstName} {subuser.LastName}"
							}).AsQueryable();
			return metadata.FirstOrDefault();
		}

		public async Task<GetCorrectiveActionDataForReport> GetCorrectiveActionReportData(int workItemId, int tenantId)
		{
			var metadata = (from workItem in _context.WorkItemMasters
							join rca in _context.CorrectiveActionMetaDatas on workItem.WorkItemId equals rca.WorkItemId
							join nonconformance in _context.WorkItemMasters on workItem.SourceItemId equals nonconformance.WorkItemId
							join source in _context.MasterData on nonconformance.SourceId equals source.Id into source
							from subSource in source.DefaultIfEmpty()
							join user in _context.UserMasters on nonconformance.CreatedBy equals user.UserId into user
							from subuser in user.DefaultIfEmpty()
							join user1 in _context.UserMasters on workItem.ResponsibleUserId equals user1.UserId into user1
							from subuser1 in user1.DefaultIfEmpty()
							join staus in _context.MasterData on workItem.StatusMasterDataId equals staus.Id into staus
							from substaus in staus.DefaultIfEmpty()
							join dept in _context.DepartmentMasters on nonconformance.DepartmentId equals dept.DepartmentId into dept
							from subdept in dept.DefaultIfEmpty()
							where workItem.WorkItemId == workItemId
							select new GetCorrectiveActionDataForReport()
							{
								CorrectiveActionNo = workItem.WorkItemId,
								Originator = $"{subuser.FirstName} {subuser.LastName}",
								WhyAnalysis1 = rca.WhyAnalysis1 == null ? "N/A" : rca.WhyAnalysis1,
								WhyAnalysis2 = rca.WhyAnalysis2 == null ? "N/A" : rca.WhyAnalysis2,
								WhyAnalysis3 = rca.WhyAnalysis3 == null ? "N/A" : rca.WhyAnalysis3,
								WhyAnalysis4 = rca.WhyAnalysis4 == null ? "N/A" : rca.WhyAnalysis4,
								WhyAnalysis5 = rca.WhyAnalysis5 == null ? "N/A" : rca.WhyAnalysis5,
								RootCauseAnalysis = rca.RootCauseAnalysis == null ? "N/A" : rca.RootCauseAnalysis,
								Source = subSource.Items == null ? "N/A" : subSource.Items,
								NonConformanceDescription = nonconformance.Description == null ? "N/A" : nonconformance.Description,
								ResponsiblePerson = $"{subuser1.FirstName} {subuser1.LastName}",
								Status = substaus.Items == null ? "N/A" : substaus.Items,
								Department = subdept.DepartmentName == null ? "N/A" : subdept.DepartmentName,
								DueDate = workItem.DueDate,
								NcDate = nonconformance.CreatedOn,
								EndDate = workItem.UpdatedOn,
								ActionRequired = workItem.Title
							}).AsQueryable();
			return metadata.FirstOrDefault();
		}

		public async Task<IList<CAListByIncidentId>> GetCaListByIncidentId(int incidentId, int tenantId)
		{
			var rawData = await (from correctiveAction in _context.WorkItemMasters
								 join incident in _context.WorkItemMasters on correctiveAction.SourceItemId equals incident.WorkItemId
								 join user in _context.UserMasters on correctiveAction.ResponsibleUserId equals user.UserId into user
								 from subuser in user.DefaultIfEmpty()
								 where correctiveAction.SourceItemId == incidentId && correctiveAction.SourceId == (int)IMSModules.IncidentManagement && correctiveAction.TenantId == tenantId
								 select new CAListByIncidentId()
								 {
									 Title = correctiveAction.Title,
									 ResponsiblePerson = $"{subuser.FirstName} {subuser.LastName}",
									 DueDate = correctiveAction.DueDate,
									 FollowUpDate = correctiveAction.DueDate
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<PaginatedItems<GetTaskListByCorrectiveAction>> GetTaskListByCorrectiveAction(GetTaskLists getAllTaskByCA)
		{
			var rawData = (from Task in _context.WorkItemMasters
						   join Ca in _context.WorkItemMasters on Task.SourceItemId equals Ca.WorkItemId
						   join tm in _context.TenanttMasters on Task.TenantId equals tm.TenantId
						   join Um in _context.UserMasters on Task.ResponsibleUserId equals Um.UserId

						   where Task.WorkItemTypeId == 231 && getAllTaskByCA.SourceItemId == Task.SourceItemId

						   select new GetTaskListByCorrectiveAction
						   {
							   Id = Task.WorkItemId,
							   Title = Task.Title,
							   Description = Task.Description,
							   DueDate = Task.DueDate,
							   ResponsiblePerson = $"{Um.FirstName}{Um.LastName}",
							   SourceItemId = Task.SourceItemId,
							   WorkItemTypeId = Ca.WorkItemTypeId,
						   }).AsQueryable();
			if (getAllTaskByCA.WorkItemTypeId > 0)
				rawData = rawData.Where(log => log.WorkItemTypeId == getAllTaskByCA.WorkItemTypeId);

			var filteredData = DataExtensions.OrderBy(rawData, getAllTaskByCA.ListRequests.SortColumn, getAllTaskByCA.ListRequests.Sort == "asc")
							  .Skip(getAllTaskByCA.ListRequests.PerPage * (getAllTaskByCA.ListRequests.Page - 1))
							  .Take(getAllTaskByCA.ListRequests.PerPage);

			var totalItems = await rawData.LongCountAsync();

			int totalPages = (int)Math.Ceiling(totalItems / (double)getAllTaskByCA.ListRequests.PerPage);
			var model = new PaginatedItems<GetTaskListByCorrectiveAction>(getAllTaskByCA.ListRequests.Page, getAllTaskByCA.ListRequests.PerPage, totalPages, filteredData);
			return await Task.FromResult(model);
		}
	}
}