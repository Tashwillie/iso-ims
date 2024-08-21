using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;
using NUnit.Framework.Internal.Execution;

namespace Mindflur.IMS.Business
{
	public class AuditFindingBusiness: IAuditFindingBusiness
	{
		private readonly IAuditFindingRepository _auditFindingRepository;
		private readonly IWorkItemBusiness _workItemBusiness;
		private readonly IMessageService _messageService;
		private readonly IActivityLogRepository _activityLogRepository;
		private readonly IUserRepository _userRepository;

		public AuditFindingBusiness(IAuditFindingRepository auditFindingRepository,IMessageService messageService,IActivityLogRepository activityLogRepository,IWorkItemBusiness workItemBusiness, IUserRepository userRepository)
		{
			_auditFindingRepository = auditFindingRepository;
			_activityLogRepository= activityLogRepository;		
			_messageService = messageService;
			_workItemBusiness= workItemBusiness;
			_userRepository= userRepository;
		
		}
		public async Task<PaginatedItems<AuditFindingView>> GetAuditFindings(GetAuditFindingListRequest getListRequest)
		{
			return await _auditFindingRepository.GetAuditFindings(getListRequest);
		}

		public async Task AddAuditFinding(PostAuditFindingView auditFinding, int userId, int tenantId,string path)
		{
			AuditFinding auditFindings = new AuditFinding();
			auditFindings.AuditProgramId = auditFinding.AuditProgramId;
			auditFindings.AuditableItemId = auditFinding.AuditableItemId;
			auditFindings.Title = auditFinding.Title;
			auditFindings.MasterDataFindingCategoryId = auditFinding.MasterDataFindingCategoryId;
			auditFindings.MasterDataFindingStatusId = (int)IMSItemStatus.New;
			auditFindings.Department = auditFinding.Department;
			auditFindings.Description = auditFinding.Description;
			auditFindings.CreatedBy = userId;
			auditFindings.CreatedOn = DateTime.UtcNow;
			await _auditFindingRepository.AddAsync(auditFindings);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.InternalAuditFinding,
				ItemId = auditFindings.Id,
				Description = auditFindings.Description,
				Title = auditFindings.Title,
				Date = auditFindings.CreatedOn
			});

			ActivityLog activityLog = new ActivityLog();
			activityLog.TenantId = tenantId;
			activityLog.ControllerId = (int)IMSControllerCategory.InternalAuditFinding;
			activityLog.EntityId = auditFindings.Id;
			activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
			activityLog.Description = "Audit Finding Has Been Created";
			activityLog.Details = System.Text.Json.JsonSerializer.Serialize(auditFinding);
			activityLog.Status = true;
			activityLog.CreatedBy = userId;
			activityLog.CreatedOn = DateTime.UtcNow;
			await _activityLogRepository.AddAsync(activityLog);
			int[]? standardId = { 2, 3 };
			WorkItemPostView workItemPostView = new WorkItemPostView();
			workItemPostView.SourceId = (int)IMSModules.AuditFinding;
			workItemPostView.SourceItemId = auditFindings.Id;
			
			workItemPostView.Title = auditFindings.Title;
			workItemPostView.DepartmentId = auditFindings.Department;
			workItemPostView.Description = auditFindings.Description;
			workItemPostView.ResponsibleUserId = auditFindings.CreatedBy;
			workItemPostView.DueDate = auditFindings.CreatedOn;
			workItemPostView.StandardId = standardId;
			
			if (auditFindings.MasterDataFindingCategoryId == (int)IMSMasterFindingCategory.Risk)
			{
				workItemPostView.WorkItemTypeId = (int)IMSModules.RiskManagement;
				await _workItemBusiness.AddWorkitem(workItemPostView, userId, tenantId,path);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.RiskManagement,
					ItemId = auditFindings.Id,
					Description = workItemPostView.Description,
					Title = workItemPostView.Title,
					Date = auditFindings.CreatedOn
				});
			}
			else if (auditFindings.MasterDataFindingCategoryId == (int)IMSMasterFindingCategory.MajorNC || auditFindings.MasterDataFindingCategoryId == (int)IMSMasterFindingCategory.MinorNC)
			{
				
				workItemPostView.WorkItemTypeId = (int)IMSModules.NonConformity;
				
				await _workItemBusiness.AddWorkitem(workItemPostView, userId, tenantId, path);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.NonConformance,
					ItemId = auditFindings.Id,
					Description = workItemPostView.Description,
					Title = workItemPostView.Title,
					Date = auditFindings.CreatedOn
				});
			}
			else if (auditFindings.MasterDataFindingCategoryId == (int)IMSMasterFindingCategory.Opportunity)
			{
				
				workItemPostView.WorkItemTypeId = (int)IMSModules.Opportunity;
				
				await _workItemBusiness.AddWorkitem(workItemPostView, userId, tenantId, path);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.Opportunities,
					ItemId = auditFindings.Id,
					Description = workItemPostView.Description,
					Title = workItemPostView.Title,
					Date = auditFindings.CreatedOn
				});
			}
			else
			{
			
				workItemPostView.WorkItemTypeId = (int)IMSModules.Observation;
				
				await _workItemBusiness.AddWorkitem(workItemPostView, userId, tenantId, path);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.Observations,
					ItemId = auditFindings.Id,
					Description = workItemPostView.Description,
					Title = workItemPostView.Title,
					Date = auditFindings.CreatedOn
				});
			}

			
		}

		public async Task<AuditFinding> GetAuditFindingById(int auditFindingId)
		{
			var findings = await _auditFindingRepository.GetByIdAsync(auditFindingId);
			return findings == null ? throw new NotFoundException(string.Format(ConstantsBusiness.FindingNotFoundErrorMessage), auditFindingId) : findings;
		}

		public async Task<AuditFindingPreview> GetAuditFindingPreview(int auditFindingId, int tenantId)
		{
			return await _auditFindingRepository.GetAuditFindingPreview(auditFindingId, tenantId);
		}

		public async Task UpdateAuditFinding(PutAuditFindingViewModel auditFinding, int Id, int userId, int tenantId)
		{
			var auditFindings = await _auditFindingRepository.GetByIdAsync(Id);
			if (auditFindings == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.FindingNotFoundErrorMessage), Id);
			}
			else
			{
				auditFindings.AuditProgramId = auditFinding.AuditProgramId;
				auditFindings.AuditableItemId = auditFinding.AuditableItemId;
				auditFindings.Title = auditFinding.Title;
				auditFindings.MasterDataFindingCategoryId = auditFinding.MasterDataFindingCategoryId;
				auditFindings.MasterDataFindingStatusId = auditFinding.MasterDataFindingStatusId;
				auditFindings.Description = auditFinding.Description;
				auditFindings.Department = auditFinding.Department;
				auditFindings.UpdatedBy = userId;
				auditFindings.UpdatedOn = DateTime.UtcNow;
				await _auditFindingRepository.UpdateAsync(auditFindings);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Edit,
					Module = IMSControllerCategory.InternalAuditFinding,
					ItemId = auditFindings.Id,
					Description = auditFindings.Description,
					Title = auditFindings.Title,
					Date = auditFindings.UpdatedOn
				});

				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = tenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.InternalAuditFinding;
				activityLog.EntityId = auditFindings.Id;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
				activityLog.Description = "Audit Finding Has Been Updated";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(auditFinding);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);
			}
		}

		public async Task DeleteAuditFindingById(int auditFindingId, int tenantId, int userId)
		{
			var auditFinding = await _auditFindingRepository.GetByIdAsync(auditFindingId);
			if (auditFinding == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.FindingNotFoundErrorMessage), auditFindingId);
			}
			await _auditFindingRepository.DeleteAsync(auditFinding);
			ActivityLog activityLog = new ActivityLog();
			activityLog.TenantId = tenantId;
			activityLog.ControllerId = (int)IMSControllerCategory.InternalAuditFinding;
			activityLog.EntityId = auditFinding.Id;
			activityLog.ModuleAction = (int)IMSControllerActionCategory.Delete;
			activityLog.Description = "Audit Finding Has Been Deleted";
			activityLog.Details = System.Text.Json.JsonSerializer.Serialize(auditFinding);
			activityLog.Status = true;
			activityLog.CreatedBy = userId;
			activityLog.CreatedOn = DateTime.UtcNow;
			await _activityLogRepository.AddAsync(activityLog);
		}
		public async Task<PaginatedItems<AuditFindingView>> GetAuditFindingByAuditId(AuditFindingViewForAudit auditFindingListView)
		{
			return await _auditFindingRepository.GetAuditFindingByAuditId(auditFindingListView);
		}

		public async Task<IList<AuditFindingList>> GetAllFindings(int tenantId, int auditId)
		{
			return await _auditFindingRepository.GetAllFindings(tenantId, auditId);
		}
	}
}
