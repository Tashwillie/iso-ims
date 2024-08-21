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
using Mindflur.IMS.Data.Models.Custom;
using Mindflur.IMS.Data.Repository;

namespace Mindflur.IMS.Business
{
	public class InternalAuditBusiness : IInternalAuditBusiness
	{
		public readonly IChecklistQuestionRepository _checklistQuestionRepository;
		public readonly IAuditChecklistRepository _auditChecklistRepository;
		public readonly IAuditFindingRepository _auditFindingRepository;
		public readonly IAuditProgramRepository _auditProgramRepository;

		public readonly IProgramStandardsRepository _programStandardsRepository;
		private readonly IEmailService _emailService;

		private readonly IActivityLogRepository _activityLogRepository;
		private readonly IParticipantsRepository _participantsRepository;
		private readonly IParticipantsBusiness _participantsBusiness;
		private readonly IMessageService _messageService;
		private readonly ICheckListMasterBusiness _checkListMasterBusiness;
		private readonly IUserRepository _userRepository;

		public InternalAuditBusiness(IAuditFindingRepository auditFindingRepository, IAuditChecklistRepository auditchecklistRepository, IAuditProgramRepository auditProgramRepository,
			IProgramStandardsRepository programStandardsRepository, IEmailService emailService, IActivityLogRepository activityLogRepository, IParticipantsRepository participantsRepository,
			IParticipantsBusiness participantsBusiness, IMessageService messageService, IChecklistQuestionRepository checklistQuestionRepository, ICheckListMasterBusiness checkListMasterBusiness, IUserRepository userRepository)
		{
			_auditFindingRepository = auditFindingRepository;
			_auditChecklistRepository = auditchecklistRepository;

			_auditProgramRepository = auditProgramRepository;

			_programStandardsRepository = programStandardsRepository;
			_emailService = emailService;

			_activityLogRepository = activityLogRepository;
			_participantsRepository = participantsRepository;
			_participantsBusiness = participantsBusiness;
			_messageService = messageService;
			_checklistQuestionRepository = checklistQuestionRepository;
			_checkListMasterBusiness = checkListMasterBusiness;
			_userRepository = userRepository;
		}

		public async Task<PaginatedItems<AuditProgramGridView>> GetAuditProgramList(GetAuditProgramListRequest getListRequest)
		{
			return await _auditProgramRepository.GetAuditProgramList(getListRequest);
		}

		public async Task<IList<AuditProgramSchedule>> GetScheduledProgamByID(int auditProgramId)
		{
			return await _auditProgramRepository.GetScheduledProgamByID(auditProgramId);
		}

		public async Task<IList<ManagementProgramView>> GetMeetingsForAuditReport()
		{
			return await _auditProgramRepository.GetMeetingsForAuditReport();
		}

		public async Task AddAuditProgram(PostAuditProgram auditProgram, int tenantId, int userId)
		{
			AuditProgram ap = new AuditProgram();
			ap.TenantId = tenantId;
			ap.Title = auditProgram.Title;
			ap.MasterDataCategoryId = auditProgram.Category;
			ap.FromDate = auditProgram.FromDate;
			ap.DueDate = auditProgram.DueDate;
			ap.CreatedBy = userId;
			ap.CreatedOn = DateTime.UtcNow;
			ap.IsPublish = false;
			ap.Status = (int)IMSItemStatus.Draft;
			ap.Scope = auditProgram.Scope;
			ap.Objectives = auditProgram.Objectives;
			await _auditProgramRepository.AddAsync(ap);

			ProgramStandard standards = new ProgramStandard();
			standards.AuditProgramId = ap.Id;
			foreach (int a in auditProgram.Standards)
			{
				standards.Id = 0;
				standards.MasterDataStandardId = a;
				await _programStandardsRepository.AddAsync(standards);
			}
			IList<AuditChecklist> checklistMapping = new List<AuditChecklist>();

			var checklists = await _checklistQuestionRepository.ListAllAsync();
			foreach (var checklist in checklists)
			{
				AuditChecklist ac = new AuditChecklist();
				ac.AuditProgramId = ap.Id;
				ac.ChecklistMasterId = checklist.Id;
				ac.Reviewed = false;
				//ac.MasterDataClassificationId = 181; ToDo: Chnage it for charts
				await _auditChecklistRepository.AddAsync(ac);
			}
			Participant addParticipant = new Participant(); //GetUserBytenantId

			addParticipant.ModuleId = 1;
			addParticipant.UserId = userId;
			addParticipant.CreatedOn = DateTime.UtcNow;
			addParticipant.ModuleEntityId = ap.Id;
			addParticipant.RoleId = (int)IMSRoles.AuditLeader;
			addParticipant.CreatedBy = userId;
			await _participantsRepository.AddAsync(addParticipant);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);

			var usersList = await _userRepository.GetUserBytenantId(tenantId);
			//var audit = await _auditProgramRepository.GetAuditEmailDetails(ap.Id, tenantId, 1);
			foreach (var details in usersList)
			{
				IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
				keyValuePairs.Add("#AUDITORS_NAME#", details.FirstName);
				keyValuePairs.Add("#AUDIT_PROGRAM_ID#", ap.Id.ToString());
				keyValuePairs.Add("#AUDIT_TITLE#", ap.Title);
				keyValuePairs.Add("#AUDIT_CATEGORY#", ap.MasterDataCategoryId.ToString());  //This is Hack for Now Have to update data after getting Template from Client (Manish :15thMay)
				keyValuePairs.Add("#START_DATE#", ap.FromDate.ToString());
				keyValuePairs.Add("#END_DATE#", ap.DueDate.ToString());
				await _emailService.SendEmail(details.EmailAddress, details.FirstName, "AuditScheduleMail.html", $"Audit Scheduled > {ap.Id} - {ap.Title} ", keyValuePairs);
			}

			await _messageService.SendNotificationMessage(new NotificationMessage() //Tenant
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.InernalAuditSchedule,
				ItemId = ap.Id,
				Description = ap.Title,
				Title = ap.Title,
				Date = DateTime.UtcNow
			});

			var participants = await _participantsRepository.GetAuditParticipantsList(1, ap.Id);
			foreach (var users in participants)
			{
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = users.UserId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.InernalAuditSchedule,
					ItemId = ap.Id,
					Description = "Audit Has been Scheduled",
					Title = ap.Title,
					Date = DateTime.UtcNow
				});
			}

			ActivityLog activityLog = new ActivityLog();
			activityLog.TenantId = ap.TenantId;
			activityLog.ControllerId = (int)IMSControllerCategory.InternalAudit;
			activityLog.EntityId = ap.Id;
			activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
			activityLog.Description = "Internal Audit Has Been Created";
			activityLog.Details = System.Text.Json.JsonSerializer.Serialize(auditProgram);
			activityLog.Status = true;
			activityLog.CreatedBy = userId;
			activityLog.CreatedOn = DateTime.UtcNow;
			await _activityLogRepository.AddAsync(activityLog);
		}

		public async Task<AuditProgramDetailView> GetPreviewForAudit(int auditProgramId, int tenantId)
		{
			var audit = await _auditProgramRepository.GetAuditPreview(auditProgramId, tenantId);

			var auditChecklist = await _checkListMasterBusiness.GetAuditChecklistByAuditId(auditProgramId, tenantId);
			var reviewPercentage = new ReviewedPercentageValue();
			var compliancePerCentage = new CompliancePercentageValue();

			if (audit.Scope == null && audit.Objective == null)
			{
				audit.Scope = "Not Available";
				audit.Objective = "Not Available";
			}

			if (auditChecklist.Count == 0)
			{
				reviewPercentage.PercentageValue = 0;
				compliancePerCentage.PercentageValue = 0;
			}
			else
			{
				reviewPercentage.TotalValue = auditChecklist.Count(c => c.HasReviewed == c.HasReviewed);
				reviewPercentage.EvaluatedValue = auditChecklist.Count(c => c.HasReviewed == c.HasReviewed && c.HasReviewed == true);
				reviewPercentage.PercentageValue = ((reviewPercentage.EvaluatedValue * 100) / reviewPercentage.TotalValue);

				compliancePerCentage.TotalValue = auditChecklist.Count(c => c.hasCompliance == c.hasCompliance);
				compliancePerCentage.EvaluatedValue = auditChecklist.Count(c => c.hasCompliance == c.hasCompliance && c.hasCompliance == true);
				compliancePerCentage.PercentageValue = ((compliancePerCentage.EvaluatedValue * 100) / compliancePerCentage.TotalValue);
			}

			if (audit == null)
			{
				audit = new AuditProgramDetailView();
				return audit;
			}
			else
			{
				AuditProgramDetailView preView = new AuditProgramDetailView();
				preView.Id = audit.Id;
				preView.Title = audit.Title;
				preView.FromDate = audit.FromDate;
				preView.DueDate = audit.DueDate;
				preView.Category = audit.Category;
				preView.CategoryId = audit.CategoryId;
				preView.IsPublished = audit.IsPublished;
				preView.AuditProgramStatus = audit.AuditProgramStatus;
				preView.Scope = audit.Scope;
				preView.Objective = audit.Objective;
				preView.Status = audit.Status;
				preView.CompliancePercentageValue = compliancePerCentage.PercentageValue;
				preView.ReviewPercentageValue = reviewPercentage.PercentageValue;
				preView.ActualStart = audit.ActualStart;
				preView.ActualEnd = audit.ActualEnd;
				preView.ApprovedById = audit.ApprovedById;
				preView.ApprovedBy = audit.ApprovedBy;
				preView.ApprovedOn = audit.ApprovedOn;

				var standards = await _programStandardsRepository.GetStandards(auditProgramId);
				IList<StandardView> tags = new List<StandardView>();

				foreach (StandardDataView standardTag in standards)
				{
					tags.Add(new StandardView() { StandardId = standardTag.StandardId, StandardName = standardTag.StandardName });
				}
				preView.Standard = tags;

				preView.hasAuditPlan = audit.Scope != null && audit.Objective != null;
				var audititems = await _auditProgramRepository.GetAuditItemsByProgram(auditProgramId);
				preView.hasAuditableItem = audititems.Any();
				var participantts = await _participantsRepository.GetAuditParticipantsList(1, auditProgramId);
				preView.hasParticipants = participantts.Any();

				return preView;
			}
		}

		public async Task AddAuditPaticipants(AddParticipantViewModel addParticipant, int moduleId, int userId, int tenantId)
		{
			var audit = await _auditProgramRepository.GetByIdAsync(addParticipant.ModuleEntityId);
			if (audit.ActualStart == null)
			{
				await _participantsBusiness.AddPaticipants(addParticipant, moduleId, userId);
				var audit1 = await _auditProgramRepository.GetAuditEmailDetails(audit.Id, tenantId, 1);
				foreach (var details in audit1)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
					keyValuePairs.Add("#AUDITORS_NAME#", details.Name);
					keyValuePairs.Add("#AUDIT_PROGRAM_ID#", details.AuditProgramId.ToString());
					keyValuePairs.Add("#AUDIT_TITLE#", details.Title);
					keyValuePairs.Add("#AUDIT_CATEGORY#", details.Category);
					keyValuePairs.Add("#START_DATE#", details.StartDate.ToString());
					keyValuePairs.Add("#END_DATE#", details.EndDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.Name, "AddAuditParticipant.html", $"Audit Participant Added > {details.AuditProgramId} - {details.Title} ", keyValuePairs);
				}
				audit.IsPublish = false;
				await _auditProgramRepository.UpdateAsync(audit);
			}
			else
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.CannotAddParticipantsErrorMessage));
			}
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.InernalAuditSchedule,
				ItemId = audit.Id,
				Description = "AuditParticipants  has been Added",
				Title = audit.Title,
				Date = DateTime.UtcNow
			});
		}

		public async Task<AuditProgram> GetAuditProgramById(int auditId)
		{
			var audit = await _auditProgramRepository.GetByIdAsync(auditId);
			return audit == null ? throw new NotFoundException(string.Format(ConstantsBusiness.AuditProgramNotFoundErrorMessage), auditId) : audit;
		}

		public async Task<IList<AuditProgramDropDown>> GetAuditDropdownList(int tenantId)
		{
			return await _auditProgramRepository.GetAuditDropdownList(tenantId);
		}

		public async Task<AuditProgram> UpdateAuditProgram(int Id, PutAuditProgramViewModel putAuditProgram, int userId, int tenantId)
		{
			var audit = await _auditProgramRepository.UpdateAuditProgram(Id, putAuditProgram, userId, tenantId);

			var usersList = await _userRepository.GetUserBytenantId(tenantId);

			foreach (var details in usersList)
			{
				IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
				keyValuePairs.Add("#AUDITORS_NAME#", details.FirstName);
				keyValuePairs.Add("#AUDIT_PROGRAM_ID#", audit.Id.ToString());
				keyValuePairs.Add("#AUDIT_TITLE#", audit.Title);
				keyValuePairs.Add("#AUDIT_CATEGORY#", audit.MasterDataCategoryId.ToString());  //This is Hack for Now Have to update data after getting Template from Client (Manish :15thMay)
				keyValuePairs.Add("#START_DATE#", audit.FromDate.ToString());
				keyValuePairs.Add("#END_DATE#", audit.DueDate.ToString());
				await _emailService.SendEmail(details.EmailAddress, details.FirstName, "AuditScheduleMail.html", $"Audit ReScheduled > {audit.Id} - {audit.Title} ", keyValuePairs);
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
				Description = "Audit has been Updated",
				Title = audit.Title,
				Date = DateTime.UtcNow
			});
			return audit;
		}

		public async Task DeleteAuditProgram(int auditId, int tenantId, int userId)
		{
			var program = await _auditProgramRepository.GetByIdAsync(auditId);
			if (program.Id == auditId && program.TenantId == tenantId)
			{
				var audit = await _auditProgramRepository.GetAuditEmailDetails(program.Id, tenantId, 1);
				foreach (var details in audit)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
					keyValuePairs.Add("#AUDITORS_NAME#", details.Name);
					keyValuePairs.Add("#AUDIT_PROGRAM_ID#", details.AuditProgramId.ToString());
					keyValuePairs.Add("#AUDIT_TITLE#", details.Title);
					keyValuePairs.Add("#AUDIT_CATEGORY#", details.Category);
					keyValuePairs.Add("#START_DATE#", details.StartDate.ToString());
					keyValuePairs.Add("#END_DATE#", details.EndDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.Name, "AuditDelete.html", $"Audit Delete > {details.AuditProgramId} - {details.Title} ", keyValuePairs);
				}


				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Delete,
					Module = IMSControllerCategory.InernalAuditSchedule,
					ItemId = auditId,
					Description = "Audit has been Deleted",
					Title = program.Title,
					Date = DateTime.UtcNow
				});

				await _auditProgramRepository.DeleteAsync(program);

				ActivityLog activity = new ActivityLog();
				activity.TenantId = tenantId;
				activity.ControllerId = (int)IMSControllerCategory.InternalAudit;
				activity.EntityId = auditId;
				activity.ModuleAction = (int)IMSControllerActionCategory.Delete;
				activity.Description = "Internal Audit Has been deleted";
				activity.Details = System.Text.Json.JsonSerializer.Serialize(program);
				activity.Status = true;
				activity.CreatedBy = userId;
				activity.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activity);

			}
			else
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditProgramNotFoundErrorMessage), auditId);
			}


		}

		public async Task UpdateAsPublishForAudit(int auditId, int tenantId, int userId)
		{
			var auditProgram = await _auditProgramRepository.GetByIdAsync(auditId);
			if (auditProgram == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditProgramNotFoundErrorMessage), auditId);
			}
			else if (auditProgram.ApprovedBy == null)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.AuditNotApprovedErrorMessage));
			}
			else
			{
				auditProgram.IsPublish = true;
				auditProgram.PublishedOn = DateTime.UtcNow;
				await _auditProgramRepository.UpdateAsync(auditProgram);
				var usersList = await _userRepository.GetUserBytenantId(tenantId);
				var userList = usersList.Where(t => t.RoleId == (int)IMSRolesMaster.ISOChampion || t.RoleId == (int)IMSRolesMaster.Manager).ToList();
				var audit = await _auditProgramRepository.GetAuditEmailDetails(auditId, tenantId, 1);
				foreach (var details in userList)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
					keyValuePairs.Add("#AUDITORS_NAME#", details.FullName);
					keyValuePairs.Add("#AUDIT_PROGRAM_ID#", auditId.ToString());
					keyValuePairs.Add("#AUDIT_TITLE#", auditProgram.Title);
					keyValuePairs.Add("#AUDIT_CATEGORY#", auditProgram.MasterDataCategoryId.ToString());//Hack For Now 
					keyValuePairs.Add("#START_DATE#", auditProgram.FromDate.ToString());
					keyValuePairs.Add("#END_DATE#", auditProgram.DueDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.FullName, "AuditPublish.html", $"Audit Published > {auditProgram.Id} - {auditProgram.Title} ", keyValuePairs);
				}

				ActivityLog activity = new ActivityLog();
				activity.TenantId = tenantId;
				activity.ControllerId = (int)IMSControllerCategory.InternalAudit;
				activity.EntityId = auditId;
				activity.ModuleAction = (int)IMSControllerActionCategory.Publish;
				activity.Description = "Audit has been Published";
				activity.Details = System.Text.Json.JsonSerializer.Serialize(audit);
				activity.Status = true;
				activity.CreatedBy = userId;
				activity.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activity);
			}
; var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.InernalAuditSchedule,
				ItemId = auditId,
				Description = "Audit has been Published",
				Title = auditProgram.Title,
				Date = DateTime.UtcNow
			});
		}

		public async Task StartAudit(int tenantId, int auditId, int userId)
		{
			var audit = await _auditProgramRepository.GetByIdAsync(auditId);
			var startDate = audit.FromDate.ToString("yyyy-MM-dd");
			var currentDate=DateTime.UtcNow.ToString("yyyy-MM-dd");
			if (audit == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditProgramNotFoundErrorMessage), auditId);
			}

			else if(audit.IsPublish != true)
			{
				throw new NotFoundException("Audit is not Published Yet", auditId);

			}else if (audit.ApprovedBy == null)
			
				{
				throw new NotFoundException("Audit is Not Approved Yet", auditId);
				}
			else if (startDate != currentDate)
			{
				throw new NotFoundException("Audit Can not start before Start Date", auditId);
			}
			else
			{

				audit.ActualStart = DateTime.UtcNow;
				audit.Status = (int)IMSItemStatus.Open;
				await _auditProgramRepository.UpdateAsync(audit);
				var participants = await _participantsRepository.GetAuditParticipantsList(1, auditId);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);

				var auditDetails = await _auditProgramRepository.GetAuditEmailDetails(auditId, tenantId, 1);
				foreach (var details in auditDetails)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
					keyValuePairs.Add("#AUDITORS_NAME#", details.Name);
					keyValuePairs.Add("#AUDIT_PROGRAM_ID#", details.AuditProgramId.ToString());
					keyValuePairs.Add("#AUDIT_TITLE#", details.Title);
					keyValuePairs.Add("#AUDIT_CATEGORY#", details.Category);
					keyValuePairs.Add("#START_DATE#", details.StartDate.ToString());
					keyValuePairs.Add("#END_DATE#", details.EndDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.Name, "AuditStart.html", $"Audit Start > {details.AuditProgramId} - {details.Title} ", keyValuePairs);
				}
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.User,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Edit,
					Module = IMSControllerCategory.InernalAuditSchedule,
					ItemId = audit.Id,
					Description = "Audit Has been Started",
					Title = audit.Title,
					Date = DateTime.UtcNow
				});

				foreach (var users in participants)
				{
					await _messageService.SendNotificationMessage(new NotificationMessage()
					{
						SourceIdUserId = users.UserId,
						SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
						EventType = NotificationEventType.BusinessMaster,
						BroadcastLevel = NotificationBroadcastLevel.Global,
						TenantId = tenantId,
						Action = IMSControllerActionCategory.Edit,
						Module = IMSControllerCategory.InernalAuditSchedule,
						ItemId = audit.Id,
						Description = "Audit Has been Started",
						Title = audit.Title,
						Date = DateTime.UtcNow
					});
				}
			}
			
		}

		public async Task CompleteAudit(int tenantId, int auditId, int userId)
		{
			var audit = await _auditProgramRepository.GetByIdAsync(auditId);
			if (audit == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditProgramNotFoundErrorMessage), auditId);
			}
			else
			{
				var participants = await _participantsRepository.GetAuditParticipantsList(1, auditId);
				var participant = participants.Where(p => p.IsPresent == false || p.IsPresent == null).ToList();
				var auditchecklist = await _checkListMasterBusiness.GetAuditChecklistByAuditId(auditId, tenantId);
				var checkList = auditchecklist.Where(c => c.HasReviewed == false).ToList();
				var auditItems = await _auditProgramRepository.GetAuditItemsToCompleteAudit(auditId, tenantId);
				var finding = await _auditFindingRepository.GetAllFindings(auditId, tenantId);
				var findingStatus = false;
				foreach (var item in finding)
				{
					if (item.StatusId == (int)IMSItemStatus.Closed)
					{
						findingStatus = true;
					}
					else
					{
						findingStatus = false;
					}
				}
				if (finding.Count == 0)
				{
					throw new NotFoundException(string.Format(ConstantsBusiness.FindingNotFoundErrorMessage), auditId);
				}
				if (findingStatus == false)
				{
					throw new NotFoundException(string.Format(ConstantsBusiness.FindingNotClosedErrorMessage), auditId);
				}

				if (audit.IsPublish = false && audit.ApprovedBy == null && audit.ActualStart > DateTime.UtcNow)

				{
					throw new BadRequestException(string.Format(ConstantsBusiness.AuditPublishedOrApprovedOrStartedErrorMessage));
				}
				if (participant.Any())
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.AuditPrticipantsNotMarkedPresent));
				}
				else if (checkList.Count == 0 && auditItems.Count == 0)
				{
					audit.ActualEnd = DateTime.UtcNow;
					audit.Status = (int)IMSItemStatus.Closed;
					await _auditProgramRepository.UpdateAsync(audit);

					var auditDetails = await _auditProgramRepository.GetAuditEmailDetails(auditId, tenantId, 1);
					foreach (var details in auditDetails)
					{
						IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
						keyValuePairs.Add("#AUDITORS_NAME#", details.Name);
						keyValuePairs.Add("#AUDIT_PROGRAM_ID#", details.AuditProgramId.ToString());
						keyValuePairs.Add("#AUDIT_TITLE#", details.Title);
						keyValuePairs.Add("#AUDIT_CATEGORY#", details.Category);
						keyValuePairs.Add("#START_DATE#", details.StartDate.ToString());
						keyValuePairs.Add("#END_DATE#", details.EndDate.ToString());
						await _emailService.SendEmail(details.EmailAddress, details.Name, "AuditComplete.html", $"Audit Complete > {details.AuditProgramId} - {details.Title} ", keyValuePairs);
					}
					var userDetails = await _userRepository.GetUserDetail(userId, tenantId);

					await _messageService.SendNotificationMessage(new NotificationMessage()
					{
						SourceIdUserId = userId,
						SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
						EventType = NotificationEventType.BusinessMaster,
						BroadcastLevel = NotificationBroadcastLevel.Global,
						TenantId = tenantId,
						Action = IMSControllerActionCategory.Create,
						Module = IMSControllerCategory.InernalAuditSchedule,
						ItemId = audit.Id,
						Description = "Audit Has been Completed",
						Title = audit.Title,
						Date = DateTime.UtcNow
					});
					foreach (var users in participants)
					{
						await _messageService.SendNotificationMessage(new NotificationMessage()
						{
							SourceIdUserId = users.UserId,
							SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
							EventType = NotificationEventType.BusinessMaster,
							BroadcastLevel = NotificationBroadcastLevel.Global,
							TenantId = tenantId,
							Action = IMSControllerActionCategory.Create,
							Module = IMSControllerCategory.InernalAuditSchedule,
							ItemId = audit.Id,
							Description = "Audit Has been Completed",
							Title = audit.Title,
							Date = DateTime.UtcNow
						});
					}
				}
				else
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.ChecklistQuestionsErrorMessage));
				}
			}
		}

		public async Task AuditReport(int tenantId, int auditId)
		{
			var audit = await _auditProgramRepository.GetByIdAsync(auditId);
			if (audit == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditProgramNotFoundErrorMessage), auditId);
			}
			else if (audit.ApprovedBy == null && audit.IsPublish == false && audit.ActualStart == null)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.AuditPublishedOrApprovedOrStartedErrorMessage));
			}
			else
			{
				//ToDo: Need to discuss and implement this place for reports
				await _auditProgramRepository.UpdateAsync(audit);
			}

			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = audit.UpdatedBy,
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.InernalAuditSchedule,
				ItemId = auditId,
				Description = "Audit Has been Started",
				Title = audit.Title,
			});
		}

		public async Task<IList<GetTotalMajorNonconformances>> getTotalMajorNc(int auditId)
		{
			return await _auditProgramRepository.getTotalMajorNc(auditId);
		}

		public async Task<IList<GetNonConformanceLegend>> NonConformanceLegendForReport(int auditId)
		{
			return await _auditProgramRepository.NonConformanceLegendForReport(auditId);
		}

		public async Task<IList<GetNonConformances>> GetNonConformances(int auditId)
		{
			return await _auditProgramRepository.GetNonConformances(auditId);
		}

		public async Task<IList<GetObservations>> GetObservations(int auditId)
		{
			return await _auditProgramRepository.GetObservations(auditId);
		}

		public async Task<IList<GetTotalMinorNonconformances>> getTotalMinorNc(int auditId)
		{
			return await _auditProgramRepository.getTotalMinorNc(auditId);
		}

		public async Task<IList<GetTotalObservation>> getTotalObservations(int auditId)
		{
			return await _auditProgramRepository.getTotalObservation(auditId);
		}

		public async Task<AuditReportDetails> AuditProgramDetails(int auditId)
		{
			return await _auditProgramRepository.AuditProgramDetails(auditId);
		}

		public async Task<IList<AuditDepartmentForReport>> GetAuditDepartmentListForReport(int auditId)
		{
			return await _auditProgramRepository.GetAuditDepartmentListForReport(auditId);
		}

		public async Task<IList<AuditProgramReport>> GetAuditDetailsForReport(int auditId)
		{
			return await _auditProgramRepository.GetAuditDetailsForReport(auditId);
		}

		public async Task<IList<AuditReportNonConfirmities>> GetAuditNonConfirmityForReport(int auditId)
		{
			return await _auditProgramRepository.GetAuditNonCobfirmityForReport(auditId);
		}

		public async Task<IList<AuditReportOpportunitiesAndObservations>> GetAuditObservationOpportunityForReport(int auditId)
		{
			return await _auditProgramRepository.GetAuditObservationOpportunityForReport(auditId);
		}

		public async Task<IList<PreviousAuidtNonconformance>> GetPreviousAuditNonConformities(int auditId)
		{
			return await _auditProgramRepository.GetPreviousAuditNonConformities(auditId);
		}

		public async Task<AuditProgramCreatedOn> GetAuditCreatedDate(int auditId)
		{
			return await _auditProgramRepository.GetAuditCreatedDate(auditId);
		}

		public async Task<AuditProgramDetailsFormCreatedOn> GetAuditIdFromCreatedOnDate(DateTime createdOn)
		{
			return await _auditProgramRepository.GetAuditIdFromCreatedOnDate(createdOn);
		}

		public async Task<IList<AuditProgramEmail>> NightlyRemiderMailAuditProgram()
		{
			var reminders = await _auditProgramRepository.NightlyRemiderMailAuditProgram();
			return reminders;
		}

		public async Task<IList<AuditProgramEmail>> NightlyRemiderMailToAuditorsForAuditItem()
		{
			var reminders = await _auditProgramRepository.NightlyRemiderMailToAuditorForItems();
			return reminders;
		}

		public async Task<IList<AuditProgramObjective>> AuditProgramObjectives(int auditId)
		{
			return await _auditProgramRepository.AuditProgramObjectives(auditId);
		}

		public async Task getClauses(int parentClauseId, int auditId, int tenantId)
		{
		}

		public async Task<IList<AuditPlanByDepartmentList>> GetDepartmentListfromAudid(int auditId, int tenantId)
		{
			return await _auditProgramRepository.GetDepartmentListfromAuditId(auditId, tenantId);
		}
        public async Task<IList<StandardList>> GetStandardsLitFromAuditId(int auditId, int tenantId)
        {
			return await _auditProgramRepository.GetStandardsLitFromAuditId(auditId, tenantId);
        }
    }

}