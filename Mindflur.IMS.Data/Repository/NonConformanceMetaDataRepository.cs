using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class NonConformanceMetaDataRepository : BaseRepository<NonConformanceMetadata>, INonConformanceMetaDataRepository
	{
		private readonly ICommentBusiness _commentBusiness;
		private readonly IMessageService _messageService;
		private readonly IWorkItemBusiness _workItemBusiness;
		private readonly IUserRepository _userRepository;

		public NonConformanceMetaDataRepository(IMessageService messageService, IMSDEVContext dbContext, ILogger<NonConformanceMetadata> logger, ICommentBusiness commentBusiness, IWorkItemBusiness workItemBusiness, IUserRepository userRepository) : base(dbContext, logger)
		{
			_messageService = messageService;
			_workItemBusiness = workItemBusiness;
			_userRepository = userRepository;
			_commentBusiness = commentBusiness;
		}

		public async Task<NonConformanceMetadata> getNonConformanceMetaDataByWorkItemId(int workItemId)
		{
			var data = await _context.NonConformanceMetadatas.FirstOrDefaultAsync(t => t.WorkItemId == workItemId);

			return data;
		}

		public async Task<GetNonConformanceMetadataView> GetMetadata(int workItemId, int tenantId)
		{
			var metadata = (from nc in _context.NonConformanceMetadatas
							join user in _context.UserMasters on nc.UpdatedBy equals user.UserId into user
							from subuser in user.DefaultIfEmpty()
							join reviewuser in _context.UserMasters on nc.ReviewedBy equals reviewuser.UserId into ruser
							from subreviewUser in ruser.DefaultIfEmpty()
							join appuser in _context.UserMasters on nc.ApprovedBy equals appuser.UserId into auser
							from approveUser in auser.DefaultIfEmpty()

							where nc.WorkItemId == workItemId
							select new GetNonConformanceMetadataView()
							{
								StartDate = nc.StartDate,
								ImmediateAction = nc.ImmediateAction,
								Documents = nc.Documents,

								IsApproved = nc.IsApproved,
								ApprovedById = nc.ApprovedBy,
								ApprovedBy = $"{approveUser.FirstName} {approveUser.LastName}",
								AppovedOn = nc.ApprovedOn,
								ReviewedById = nc.ReviewedBy,
								ReviewedBy = $"{subreviewUser.FirstName} {subreviewUser.LastName}",
								ReviewedOn = nc.ReviewedOn,
								UpdatedById = nc.UpdatedBy,
								UpdatedBy = $"{subuser.FirstName} {subuser.LastName}",
								UpdatedOn = nc.UpdatedOn,
							}).AsQueryable();

			return metadata.FirstOrDefault();
		}

		public async Task<GetNonConformanceMetadataViewWithTokens> GetNonConformanceMetadata(int workItemId, int tenantId)
		{
			var metadata = (from nc in _context.NonConformanceMetadatas
							join workItem in _context.WorkItemMasters on nc.WorkItemId equals workItem.WorkItemId
							join user in _context.UserMasters on nc.UpdatedBy equals user.UserId into user
							from subuser in user.DefaultIfEmpty()
							join reviewuser in _context.UserMasters on nc.ReviewedBy equals reviewuser.UserId into ruser
							from subreviewUser in ruser.DefaultIfEmpty()
							join appuser in _context.UserMasters on nc.ApprovedBy equals appuser.UserId into auser
							from approveUser in auser.DefaultIfEmpty()
							join assign in _context.UserMasters on workItem.AssignedToUserId equals assign.UserId into assign
							from assignuser in assign.DefaultIfEmpty()
							where nc.WorkItemId == workItemId
							select new GetNonConformanceMetadataViewWithTokens()
							{
								DateOfNc = nc.StartDate,
								ImmediateAction = nc.ImmediateAction,
								Documents = nc.Documents,
								AssignToUserId = workItem.AssignedToUserId,
								AssignToUser = $"{assignuser.FirstName} {assignuser.LastName}",
								IsApproved = nc.IsApproved,
								ApprovedById = nc.ApprovedBy,
								ApprovedBy = $"{approveUser.FirstName} {approveUser.LastName}",
								AppovedOn = nc.ApprovedOn,
								ReviewedById = nc.ReviewedBy,
								ReviewedBy = $"{subreviewUser.FirstName} {subreviewUser.LastName}",
								ReviewedOn = nc.ReviewedOn,
								UpdatedById = nc.UpdatedBy,
								UpdatedBy = $"{subuser.FirstName} {subuser.LastName}",
								UpdatedOn = nc.UpdatedOn,
							}).AsQueryable();

			return metadata.FirstOrDefault();
		}

		public async Task<List<NonConformanceListByMeetingId>> GetAllNcByMeetingId(int meetingId)
		{
			var rawData = await (from wt in _context.WorkItemMasters
								 join ap in _context.AuditPrograms on wt.SourceItemId equals ap.Id
								 join mp in _context.MeetingPlans on ap.Id equals mp.AuditProgramId
								 join dept in _context.DepartmentMasters on wt.DepartmentId equals dept.DepartmentId into dept
								 from department in dept.DefaultIfEmpty()
								 join category in _context.MasterData on wt.CategoryId equals category.Id into category
								 from subCategory in category.DefaultIfEmpty()
									 /*  join ai in _context.AuditableItems on ap.Id equals ai.AuditProgramId
									   join aic in _context.AuditableItemClauses on ai.Id equals aic.AuditableItemId
									   join ac in _context.AuditItemClauses on aic.AuditableItemClauseId equals ac.AuditableItemClauseId
									   join clause in _context.Clauses on ac.ClauseMasterId equals clause.ClauseId*/
								 where wt.SourceId == (int)IMSModules.InternalAudit && wt.WorkItemTypeId == (int)IMSModules.NonConformity && mp.Id == meetingId
								 select new NonConformanceListByMeetingId()
								 {
									 NcId = wt.WorkItemId.ToString(),
									 Title = wt.Description,
									 Department = department.DepartmentName,
									 Classification = subCategory.Items,
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<List<ObservationOpportunitiesListByMeetingId>> GetAllObservationsByMeetingId(int meetingId)
		{
			var rawData = await (from wt in _context.WorkItemMasters
								 join ap in _context.AuditPrograms on wt.SourceItemId equals ap.Id
								 join mp in _context.MeetingPlans on ap.Id equals mp.AuditProgramId
								 join dept in _context.DepartmentMasters on wt.DepartmentId equals dept.DepartmentId into dept
								 from department in dept.DefaultIfEmpty()
								 join category in _context.MasterData on wt.CategoryId equals category.Id into category
								 from subCategory in category.DefaultIfEmpty()

								 where wt.SourceId == (int)IMSModules.InternalAudit && wt.WorkItemTypeId == (int)IMSModules.Observation && mp.Id == meetingId
								 select new ObservationOpportunitiesListByMeetingId()
								 {
									 Id = wt.WorkItemId.ToString(),
									 Description = wt.Description,
									 Department = department.DepartmentName,
									 ObservationRecommendation = subCategory.Items,
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<NonConformanceReportsDataView> GetNonconformanceReportData(int workItemId, int tenantId)
		{
			var metadata = (from nonConformance in _context.WorkItemMasters
							join nc in _context.NonConformanceMetadatas on nonConformance.WorkItemId equals nc.WorkItemId into nc
							from subnc in nc.DefaultIfEmpty()
							join correctiveaction in _context.WorkItemMasters on nonConformance.WorkItemId equals correctiveaction.SourceItemId into correctiveaction
							from subCorrectiveaction in correctiveaction.DefaultIfEmpty()
							join ca in _context.CorrectiveActionMetaDatas on subCorrectiveaction.WorkItemId equals ca.WorkItemId into ca
							from subCa in ca.DefaultIfEmpty()
							join source in _context.MasterData on nonConformance.SourceId equals source.Id into source
							from subSource in source.DefaultIfEmpty()
							join user in _context.UserMasters on nonConformance.CreatedBy equals user.UserId into user
							from subuser in user.DefaultIfEmpty()
							join staus in _context.MasterData on nonConformance.StatusMasterDataId equals staus.Id into staus
							from substaus in staus.DefaultIfEmpty()
							join dept in _context.DepartmentMasters on nonConformance.DepartmentId equals dept.DepartmentId into dept
							from subdept in dept.DefaultIfEmpty()
							join user1 in _context.UserMasters on nonConformance.ResponsibleUserId equals user1.UserId into user1
							from subuser1 in user1.DefaultIfEmpty()
							join tm in _context.TenanttMasters on nonConformance.TenantId equals tm.TenantId

							where nonConformance.WorkItemId == workItemId && nonConformance.TenantId == tenantId
							select new NonConformanceReportsDataView()
							{
								NcId = nonConformance.WorkItemId.ToString(),
								OriginatorName = tm.Name,
								Department = subdept.DepartmentName == null ? "N/A" : subdept.DepartmentName,
								CreatedDate = subnc.StartDate,
								NcDescription = nonConformance.Description,
								RootCauseAnalysis = subCa.RootCauseAnalysis == null ? "N/A" : subCa.RootCauseAnalysis,
								WhyAnalysis1 = subCa.WhyAnalysis1 == null ? "N/A" : subCa.WhyAnalysis1,
								WhyAnalysis2 = subCa.WhyAnalysis2 == null ? "N/A" : subCa.WhyAnalysis2,
								WhyAnalysis3 = subCa.WhyAnalysis3 == null ? "N/A" : subCa.WhyAnalysis3,
								WhyAnalysis4 = subCa.WhyAnalysis4 == null ? "N/A" : subCa.WhyAnalysis4,
								WhyAnalysis5 = subCa.WhyAnalysis5 == null ? "N/A" : subCa.WhyAnalysis5,
								CorrectiveActionDescription = subCorrectiveaction.Description,
								DueDate = nonConformance.DueDate,
								ActionTaken = subnc.ImmediateAction == null ? "N/A" : subnc.ImmediateAction,
								Status = substaus.Items == null ? "N/A" : substaus.Items,
								AssignedTo = $"{subuser1.FirstName} {subuser1.LastName}",
								UpdatedOn = subnc.UpdatedOn,
								CreatedBy = $"{subuser.FirstName} {subuser.LastName}",
								NcSource = subSource.Items
							}).AsQueryable();

			return metadata.FirstOrDefault();
		}

		public async Task<IList<SelectView>> GetNcCategoryDropdown(int tenantId)
		{
			var rawData = await (from md in _context.MasterData
								 where md.Active == true && md.Id == (int)IMSModules.MajorNonConformance || md.Id == (int)IMSModules.MinorNonConformance
								 select new SelectView
								 {
									 Value = md.Id,
									 Label = md.Items,
									 ParentId = md.MasterDataGroupId
								 })
			.OrderByDescending(md => md.Value)
								.ToListAsync();

			return await Task.FromResult(rawData);
		}

		public async Task<IList<GetCAListByIncidentId>> GetCaListByNcId(int tenantId, int workItemId)
		{
			var rawData = await (from nonconformance in _context.WorkItemMasters
								 join metaData in _context.NonConformanceMetadatas on nonconformance.WorkItemId equals metaData.WorkItemId
								 join ca in _context.WorkItemMasters on nonconformance.WorkItemId equals ca.SourceItemId
								 join status in _context.MasterData on nonconformance.StatusMasterDataId equals status.Id into status
								 from subStatus in status.DefaultIfEmpty()
								 where nonconformance.WorkItemId == workItemId && nonconformance.TenantId == tenantId && nonconformance.WorkItemTypeId == (int)IMSModules.NonConformity && ca.WorkItemTypeId == (int)IMSModules.CorrectiveAction
								 select new GetCAListByIncidentId()
								 {
									 Id = nonconformance.WorkItemId,
									 StatusId = ca.StatusMasterDataId
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task UpdateNcMetaData(PutNonConformanceMetadataViewModel nonconformanceMetadataViewModel, int workItemId, int userId, int tenantId)
		{
			var metaData = await _context.NonConformanceMetadatas.FirstOrDefaultAsync(t => t.WorkItemId == workItemId);
			var nonConformance = await _context.WorkItemMasters.FirstOrDefaultAsync(nc => nc.WorkItemId == workItemId);
			var userDetails = await _workItemBusiness.GetPreviewWorkItem(workItemId, tenantId);

			if (metaData != null)
			{
				var existingTokens = _context.WorkItemWorkItemTokens.Where(ps => ps.WorkItemId == workItemId);
				_context.WorkItemWorkItemTokens.RemoveRange(existingTokens);
				await _context.SaveChangesAsync();

				var tokens = new List<WorkItemWorkItemToken>();

				if (nonConformance.SourceId == (int)IMSModules.InternalAudit)
				{
					var newTokens = new List<WorkItemWorkItemToken>
						{
							new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = 30 },
						};

					if (nonConformance.CategoryId == (int)IMSModules.MajorNonConformance)
					{
						newTokens.Add(new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = 2 });
					}
					else if (nonConformance.CategoryId == (int)IMSModules.MinorNonConformance)
					{
						newTokens.Add(new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = 4 });
					}

					tokens.AddRange(newTokens);
					metaData.StartDate = nonConformance.CreatedOn;
				}
				else
				{
					var tokenIds = nonconformanceMetadataViewModel.Tokens;
					var newTokens = tokenIds.Select(token => new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = token });
					tokens.AddRange(newTokens);
					metaData.StartDate = nonconformanceMetadataViewModel.DateOfNc;
				}

				await _context.WorkItemWorkItemTokens.AddRangeAsync(tokens);
				await _context.SaveChangesAsync();

				metaData.WorkItemId = workItemId;

				metaData.ImmediateAction = nonconformanceMetadataViewModel.ImmediateAction;
				metaData.IsApproved = true;
				metaData.UpdatedOn = DateTime.UtcNow;
				metaData.UpdatedBy = userId;
				metaData.ApprovedOn = DateTime.UtcNow;
				metaData.ApprovedBy = userId;
				await _context.SaveChangesAsync();

				var postCommentView = new PostCommentView
				{
					SourceId = (int)IMSModules.NonConformity,
					SourceItemId = workItemId,
					ParentCommentId = 0,
					ContentType = 1,
					CommentContent = nonconformanceMetadataViewModel.Comments
				};
				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
				await _context.SaveChangesAsync();

				var notificationMessage = new NotificationMessage
				{
					SourceIdUserId = userId,
					SourceIdUser = userDetails.UpdatedBy,
					BroadcastLevel = NotificationBroadcastLevel.Tenant,
					EventType = NotificationEventType.BusinessMaster,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Edit,
					Module = IMSControllerCategory.NonConformance,
					ItemId = workItemId,
					Description = nonconformanceMetadataViewModel.ImmediateAction,
					Title = nonconformanceMetadataViewModel.ImmediateAction,
					Date = metaData.UpdatedOn
				};
				await _messageService.SendNotificationMessage(notificationMessage);
			}
			else
			{
				metaData = new NonConformanceMetadata
				{
					WorkItemId = workItemId,

					ImmediateAction = nonconformanceMetadataViewModel.ImmediateAction,
					IsApproved = true,
					UpdatedOn = DateTime.UtcNow,
					UpdatedBy = userId,
					ApprovedOn = DateTime.UtcNow,
					ApprovedBy = userId
				};

				await _context.NonConformanceMetadatas.AddAsync(metaData);
				await _context.SaveChangesAsync();

				var workItemTokens = new List<WorkItemWorkItemToken>();

				if (nonConformance.SourceId == (int)IMSModules.InternalAudit)
				{
					if (nonConformance.CategoryId == (int)IMSModules.MajorNonConformance)
					{
						workItemTokens.Add(new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = 30 });
						workItemTokens.Add(new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = 2 });
					}
					else if (nonConformance.CategoryId == (int)IMSModules.MinorNonConformance)
					{
						workItemTokens.Add(new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = 30 });
						workItemTokens.Add(new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = 4 });
					}
					metaData.StartDate = nonConformance.CreatedOn;
					await _context.SaveChangesAsync();
				}
				else
				{
					var tokenIds = nonconformanceMetadataViewModel.Tokens;
					workItemTokens.AddRange(tokenIds.Select(token => new WorkItemWorkItemToken { WorkItemId = workItemId, TokenId = token }));
					metaData.StartDate = nonconformanceMetadataViewModel.DateOfNc;
					await _context.SaveChangesAsync();
				}

				await _context.WorkItemWorkItemTokens.AddRangeAsync(workItemTokens);
				await _context.SaveChangesAsync();

				var postCommentView = new PostCommentView
				{
					SourceId = (int)IMSModules.NonConformity,
					SourceItemId = workItemId,
					ParentCommentId = 0,
					ContentType = 1,
					CommentContent = nonconformanceMetadataViewModel.Comments
				};

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
				await _context.SaveChangesAsync();
			}

			nonConformance.AssignedToUserId = nonconformanceMetadataViewModel.AssignToUserId;
			nonConformance.StatusMasterDataId = (int)IMSItemStatus.Assigned;
			await _context.SaveChangesAsync();
			var userDetail = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetail.FirstName} {userDetail.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.NonConformance,
				ItemId = workItemId,
				Description = nonconformanceMetadataViewModel.ImmediateAction,
				Title = nonconformanceMetadataViewModel.ImmediateAction,
				Date = metaData.UpdatedOn
			});
		}
	}
}