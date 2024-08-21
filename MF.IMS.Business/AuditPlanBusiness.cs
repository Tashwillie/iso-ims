using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
	public class AuditPlanBusiness:IAuditPlanBusiness
	{
		private readonly IAuditPlanRepository _auditPlanRepository;
		private readonly IMessageService _messageService;
		private readonly IActivityLogRepository _activityLogRepository;
		private readonly IAuditProgramRepository _auditProgramRepository;
		private readonly IParticipantsRepository _participantsRepository;
		public readonly IAuditItemsRepository _auditItemsRepository;
		private readonly IUserRepository _userRepository;
		private readonly IEmailService _emailService;

		public AuditPlanBusiness(IAuditPlanRepository auditPlanRepository,IMessageService messageService,IActivityLogRepository activityLogRepository,
			IAuditProgramRepository auditProgramRepository,IAuditItemsRepository auditItemsRepository,IParticipantsRepository participantsRepository, IUserRepository userRepository,IEmailService emailService)
		{
			_auditPlanRepository = auditPlanRepository;
			_messageService = messageService;
			_activityLogRepository = activityLogRepository;
			_auditProgramRepository = auditProgramRepository;
			_participantsRepository = participantsRepository;
			_auditItemsRepository= auditItemsRepository;
			_userRepository = userRepository;
			_emailService = emailService;
		}
			public async Task<PaginatedItems<AuditPlanGridView>> GetAuditPlan(GetListRequest getListRequest)
			{
				return await _auditPlanRepository.GetAuditPlan(getListRequest);
			}

			public async Task<AuditPlan> AddAuditPlan(AuditPlan auditplan, int tenantId, int userId)
			{
				AuditPlan plan = new AuditPlan();
				plan.AuditProgramId = auditplan.AuditProgramId;
				plan.Objectives = auditplan.Objectives;
				plan.Scope = auditplan.Scope;
				await _auditPlanRepository.AddAsync(plan);
			
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Delete,
					Module = IMSControllerCategory.InternalAudit,
					ItemId = plan.Id,
					Description = plan.Objectives,
					Title = plan.Objectives,
					Date = DateTime.UtcNow
			});

				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = tenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.InernalAuditSchedule;
				activityLog.EntityId = plan.Id;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
				activityLog.Description = "Audit Plan Has Been Created ";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(auditplan);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);
				return plan;
			}

			public async Task<AuditPlan> GetAuditPlanById(int auditPlanId, int tenantId)
			{
				return await _auditPlanRepository.GetAuditPlanByAuditId(auditPlanId, tenantId);
			}

			public async Task EditAuditPlan(int auditId, AuditPlanView ap, int tenantId, int userId)
			{
				var auditPlan = _auditPlanRepository.GetAuditPlanByAuditProgramId(auditId);
				if (auditPlan != null)
				{
					await _auditPlanRepository.DeleteAsync(auditPlan);
				}
				var newAuditPlan = new AuditPlan();
				newAuditPlan.AuditProgramId = auditId;
				newAuditPlan.Objectives = ap.Objectives;
				newAuditPlan.Scope = ap.Scope;
				await _auditPlanRepository.AddAsync(newAuditPlan);

				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = tenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.InernalAuditSchedule;
				activityLog.EntityId = newAuditPlan.Id;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
				activityLog.Description = "Audit Paln Has Been Edited ";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(newAuditPlan);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Delete,
					Module = IMSControllerCategory.InternalAudit,
					ItemId = auditPlan.Id,
					Description = newAuditPlan.Objectives,
					Title = newAuditPlan.Objectives,
					Date = DateTime.UtcNow
			});
			}

			public async Task DeleteAuditPlan(int auditPlanId, int tenantId, int userId)
			{
				var auditPlan = await _auditPlanRepository.GetByIdAsync(auditPlanId);
				if (auditPlan == null)
				{
					throw new NotFoundException(string.Format(ConstantsBusiness.AuditPlanNotFound), auditPlanId);
				}
				await _auditPlanRepository.DeleteAsync(auditPlan);

				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = tenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.InernalAuditSchedule;
				activityLog.EntityId = auditPlan.Id;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Delete;
				activityLog.Description = "Audit Item Has Been deleted ";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(auditPlan);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);	
			await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Delete,
					Module = IMSControllerCategory.InternalAudit,
					ItemId = auditPlan.Id,
					Description = auditPlan.Objectives,
					Title = auditPlan.Objectives,
					Date = DateTime.UtcNow
			});
			}
		public async Task ApproveAudit(int tenantId, int auditId, int userId)
		{
			var audit = await _auditProgramRepository.GetByIdAsync(auditId);			
			if (audit == null)
			{
			}
			else
			{
				var participants = await _participantsRepository.GetAuditParticipantsList(1, auditId);
				var audititems = await _auditItemsRepository.GetAuditItemsByProgram(auditId);
				//var auditplan = await _auditPlanRepository.GetAuditPlanByAuditId(auditId, tenantId);
				if (participants.Count == 0)
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.AddParticipantsToAuditProgramErrorMessgae));
				}
				else if (audititems.Count == 0)
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.AddAuditableItemToAuditProgramErrorMessgae));
				}
				else if (audit.Scope == null && audit.Objectives == null)
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.AddAuditPlanToAuditProgramErrorMessage));
				}
				
				else if ((audit.FromDate > DateTime.UtcNow || audit.FromDate.ToString("yyyy-MM-dd") == DateTime.UtcNow.ToString("yyyy-MM-dd")) && audit.DueDate > audit.FromDate) //hack for Now (audit.FromDate > DateTime.UtcNow )
				{
					audit.ApprovedBy = userId;
					audit.ApprovedOn = DateTime.UtcNow;
					audit.Status = (int)IMSItemStatus.Open;
					await _auditProgramRepository.UpdateAsync(audit);

					var usersList = await _userRepository.GetUserBytenantId(tenantId);
					var userList = usersList.Where(t => t.RoleId == (int)IMSRolesMaster.Manager).ToList();
					
					foreach (var details in userList)
					{
						IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
						keyValuePairs.Add("#AUDITORS_NAME#", details.FullName);
						keyValuePairs.Add("#AUDIT_PROGRAM_ID#", auditId.ToString());
						keyValuePairs.Add("#AUDIT_TITLE#", audit.Title);
						keyValuePairs.Add("#AUDIT_CATEGORY#", audit.MasterDataCategoryId.ToString());//Hack For Now 
						keyValuePairs.Add("#START_DATE#", audit.FromDate.ToString());
						keyValuePairs.Add("#END_DATE#", audit.DueDate.ToString());
						await _emailService.SendEmail(details.EmailAddress, details.FullName, "AuditApproval.html", $"Audit Approved > {audit.Id} - {audit.Title} ", keyValuePairs);
					}
					var userDetails = await _userRepository.GetUserDetail(userId, tenantId);

					await _messageService.SendNotificationMessage(new NotificationMessage()
					{
						SourceIdUserId = userId,
						SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
						EventType = NotificationEventType.BusinessMaster,
						BroadcastLevel = NotificationBroadcastLevel.Global,
						TenantId = tenantId,
						Action = IMSControllerActionCategory.Edit,
						Module = IMSControllerCategory.InernalAuditSchedule,
						ItemId = audit.Id,
						Description = audit.Title,
						Title = audit.Title,
						Date = audit.ApprovedOn,
					});

					foreach (var users in participants)
					{
						await _messageService.SendNotificationMessage(new NotificationMessage()
						{
							SourceIdUserId = users.UserId,
							EventType = NotificationEventType.BusinessMaster,
							BroadcastLevel = NotificationBroadcastLevel.Global,
							TenantId = tenantId,
							Action = IMSControllerActionCategory.Create,
							Module = IMSControllerCategory.InernalAuditSchedule,
							ItemId = audit.Id,
							Description = audit.Title,
							Title = audit.Title,
						});
					}

				}
				else
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.ChangeAuditDateErrorMessage));
				}


			}
		}
	}

}

