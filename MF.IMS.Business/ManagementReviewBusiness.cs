using Microsoft.AspNetCore.Http;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Custrom;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Models.Custom;

namespace Mindflur.IMS.Business
{
	public class ManagementReviewBusiness : IManagementReviewBusiness
	{
		private readonly IAgendaRepository _agendaRepository;
		private readonly IMinutesRepository _minutesRepository;
		private readonly IMeetingPlanRepository _meetingPlanRepository;
		private readonly IMeetingParticipantsRepository _meetingParticipantsRepository;
		private readonly IMeetingAttendanceRepository _attendanceRepository;
		private readonly IEmailService _emailService;
		private readonly IMeetingAgendaMappingRepository _meetingAgendaMappingRepository;
		private readonly IActivityLogRepository _activityLogRepository;
		private readonly IParticipantsRepository _participantsRepository;
		private readonly IParticipantsBusiness _participantsBusiness;
		private readonly IMessageService _messageService;
		private readonly IAuditProgramRepository _auditProgramRepository;
		public readonly IProgramStandardsRepository _programStandardsRepository;
		public readonly IAgendaSummaryMasterRepository _agendaSummaryMasterRepository;
		private readonly IFileRepositoryBusiness _fileRepositoryBusiness;
		private readonly IUserRepository _userRepository;

		public ManagementReviewBusiness(IAgendaRepository agendaRepository, IMinutesRepository minutesRepository, IMeetingPlanRepository meetingPlanRepository, 
			IMeetingParticipantsRepository meetingParticipantsRepository, IMeetingAttendanceRepository meetingAttendanceRepository, IEmailService emailService,
			IMeetingAgendaMappingRepository meetingAgendaMappingRepository, IActivityLogRepository activityLogRepository,
			IParticipantsRepository participantsRepository, IParticipantsBusiness participantsBusiness, IMessageService messageService, 
			IAuditProgramRepository auditProgramRepository,	IProgramStandardsRepository programStandardsRepository,
			IFileRepositoryBusiness fileRepositoryBusiness, IAgendaSummaryMasterRepository agendaSummaryMasterRepository, IUserRepository userRepository)
		{
			_agendaRepository = agendaRepository;
			_minutesRepository = minutesRepository;
			_meetingPlanRepository = meetingPlanRepository;
			_meetingParticipantsRepository = meetingParticipantsRepository;
			_attendanceRepository = meetingAttendanceRepository;
			_emailService = emailService;
			_meetingAgendaMappingRepository = meetingAgendaMappingRepository;
			_activityLogRepository = activityLogRepository;
			_participantsRepository = participantsRepository;
			_participantsBusiness = participantsBusiness;
			_messageService = messageService;
			_auditProgramRepository = auditProgramRepository;
			_programStandardsRepository = programStandardsRepository;
			_fileRepositoryBusiness = fileRepositoryBusiness;
			_agendaSummaryMasterRepository = agendaSummaryMasterRepository;
			_userRepository = userRepository;
		}

		public async Task<AgnedaPrview> PreviewAgendas(int Id)
		{
			return await _agendaRepository.PreviewAgendas(Id);
		}

		public async Task DeleteAgenda(int agendaId)
		{
			var agenda = await _agendaRepository.GetByIdAsync(agendaId);
			if (agenda == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AgendaNotFoundErrorMessage), agendaId);
			}
			await _agendaRepository.DeleteAsync(agenda);
		}

		public async Task<PaginatedItems<AgendaMasterView>> GetAgendas(GetAllAgendaListView getAllAgendaRequest)
		{
			return await _agendaRepository.GetAgendas(getAllAgendaRequest);
		}

		public async Task<IList<AgendaMasterView>> GetAgendasInput(GetAgendasView getAgendas)
		{
			return await _agendaRepository.GetAgendasInput(getAgendas);
		}

		public async Task<AgendaMaster> GetAgenda(int agendaId)
		{
			var agenda = await _agendaRepository.GetByIdAsync(agendaId);

			return agenda == null ? throw new NotFoundException(string.Format(ConstantsBusiness.AgendaNotFoundErrorMessage), agendaId) : agenda;
		}

		public async Task<PaginatedItems<MeetingListView>> GetMeetingPlans(GetMeetingListRequest getListRequest)
		{
			return await _meetingPlanRepository.GetMeetingPlans(getListRequest);
		}

		public async Task<IList<MeetingParticpantView>> GetParticipants(int meetingId)
		{
			return await _meetingParticipantsRepository.GetParticipants(meetingId);
		}

		public async Task<IList<MRMInputAgenda>> GetAllAgendaInput(int meetingId)
		{
			return await _agendaRepository.GetAllAgendaInput(meetingId);
		}

		public async Task<IList<MRMInputAgenda>> GetAllAgendaOutputs(int meetingId)
		{
			return await _agendaRepository.GetAllAgendaOutputs(meetingId);
		}

		public async Task<PaginatedItems<MeetingAgendaView>> GetMeetingAgenda(GetAllMeetingAgendas getAllMeetingAgendas)
		{
			return await _agendaRepository.GetMeetingAgenda(getAllMeetingAgendas);
		}

		public async Task<IList<AgendaMasterView>> GetAllAgendasByMeeting(GetAllAgendasByMeeting getAllAgendasByMeeting)
		{
			return await _agendaRepository.GetAllAgendasByMeeting(getAllAgendasByMeeting);
		}

		public async Task<IList<MeetingAttendenceView>> GetAttendanceList(int minutesId)
		{
			return await _attendanceRepository.GetAttendanceList(minutesId);
		}

		//Post for adding new agena item to meeting
		public async Task AddAgendaToMeeting(PostAgendaToMeetingView postAgendaToMeetingView, int tenantId, int userId)
		{
			var useDetails = await _userRepository.GetUserDetail(userId, tenantId);
			var meeting = await _meetingPlanRepository.GetByIdAsync(postAgendaToMeetingView.MeetingId);
			if (meeting.ActualStart == null)
			{
				AgendaMaster am = new AgendaMaster();
				am.Title = postAgendaToMeetingView.Title;
				am.ParentAgendaId = 14;
				am.IsInputType = false;
				am.IsSet = false;
				await _agendaRepository.AddAsync(am);
				MeetingAgendaMapping agendaMapping = new MeetingAgendaMapping();
				agendaMapping.AgendaId = am.AgendaId;
				agendaMapping.MeetingId = postAgendaToMeetingView.MeetingId;				
				await _meetingAgendaMappingRepository.AddAsync(agendaMapping);
				var participants = await _meetingParticipantsRepository.GetParticipantsForMeetingInvitation(meeting.Id, tenantId, 2);

				foreach (var participant in participants)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

					keyValuePairs.Add("#PARTICIPANTS_NAME#", participant.Name); 
					keyValuePairs.Add("#MEETING_TITLE#", participant.MeetingTitle);
					keyValuePairs.Add("#AGENDA#", am.Title);
					keyValuePairs.Add("#VENUE#", participant.Venue);
					keyValuePairs.Add("#START_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
					keyValuePairs.Add("#END_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
					keyValuePairs.Add("#MANAGEMENT_REVIEW_ID#", participant.MeetingID.ToString());
					await _emailService.SendEmail(participant.EmailAddress, participant.Name, "MRMAgendaEmailTemplate.html", $"Agenda Added > {participant.MeetingID} - {participant.MeetingTitle}", keyValuePairs);
				}
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{useDetails.FirstName} {useDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.MRMAgenda,
					ItemId = am.AgendaId,
					Description = "New Agenda has been added to the meeting",
					Title = am.Title,
					Date = DateTime.UtcNow
				});
				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = tenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.MRMAgenda;
				activityLog.EntityId = am.AgendaId;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
				activityLog.Description = "MRM Agenda Has Been Created";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(postAgendaToMeetingView);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);
			}
			else
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.NewAgendaNotAllowedErrorMessage));
			}
		}

		public async Task AddNewMeeting(PostMeetingPlan postMeetingAgenda, int tenantId, int userId)
		{
			MeetingPlan mp = new MeetingPlan();

			mp.TenantId = tenantId;
			mp.Title = postMeetingAgenda.Title;
			mp.Location = postMeetingAgenda.Location;
			mp.StartDate = postMeetingAgenda.StartDate;
			mp.EndDate = postMeetingAgenda.EndDate;
			mp.MeetingType = postMeetingAgenda.MeetingType;
			mp.AuditProgramId = postMeetingAgenda.InternalAuditId;
			mp.IsPublished = false;
			mp.Status = (int)IMSItemStatus.Draft;
			mp.CreatedBy = userId;
			mp.CreatedOn = DateTime.UtcNow;
			await _meetingPlanRepository.AddAsync(mp);
			Participant addParticipant = new Participant();
			addParticipant.ModuleId = 2;
			addParticipant.UserId = userId;
			addParticipant.CreatedOn = DateTime.UtcNow;
			addParticipant.ModuleEntityId = mp.Id;
			addParticipant.RoleId = (int)IMSRoles.AuditLeader;
			addParticipant.CreatedBy = userId;
			await _participantsRepository.AddAsync(addParticipant);
			var participants = await _meetingParticipantsRepository.GetParticipantsForMeetingInvitation(mp.Id, tenantId, 2);
			foreach (var participant in participants)
			{
				IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
				keyValuePairs.Add("#PARTICIPANTS_NAME#", participant.Name);
				keyValuePairs.Add("#MEETING_TITLE#", participant.MeetingTitle);
				keyValuePairs.Add("#VENUE#", participant.Venue);
				keyValuePairs.Add("#START_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
				keyValuePairs.Add("#END_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
				keyValuePairs.Add("#MANAGEMENT_REVIEW_ID#", participant.MeetingID.ToString());
				await _emailService.SendEmail(participant.EmailAddress, participant.Name, "ManagementReviewScheduleMail.html", $"MRM Scheduled > {participant.MeetingID} - {participant.MeetingTitle}", keyValuePairs);
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
				Module = IMSControllerCategory.MRM_Participant,
				ItemId = mp.Id,
				Description = mp.Title,
				Title = mp.Title,
				Date = mp.CreatedOn,
			});

			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.MRM,
				ItemId = mp.Id,
				Description = mp.Title,
				Title = mp.Title,
				Date = DateTime.UtcNow
			});

			IList<MeetingAgendaMapping> agendaMapping = new List<MeetingAgendaMapping>();

			var agendas = await _agendaRepository.GetAgendaForMeeting();

			foreach (var agenda in agendas)
			{
				MeetingAgendaMapping m = new MeetingAgendaMapping();
				m.MeetingId = mp.Id;
				m.AgendaId = agenda.AgendaId;
				await _meetingAgendaMappingRepository.AddAsync(m);
			}

			ActivityLog activityLog = new ActivityLog();
			activityLog.TenantId = tenantId;
			activityLog.ControllerId = (int)IMSControllerCategory.MRMAgenda;
			activityLog.EntityId = mp.Id;
			activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
			activityLog.Description = "MRM  Has Been Created";
			activityLog.Details = System.Text.Json.JsonSerializer.Serialize(postMeetingAgenda);
			activityLog.Status = true;
			activityLog.CreatedBy = userId;
			activityLog.CreatedOn = DateTime.UtcNow;
			await _activityLogRepository.AddAsync(activityLog);
		}

		public async Task<MeetingPlanPreview> GetMeetingPlanById(int Id, int tenantId)
		{
			var meeting = await _meetingPlanRepository.GetMeetingPlanById(Id, tenantId);
			if (meeting == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.MeetingNotFoundErrorMessage), Id);
			}
			else
			{
				MeetingPlanPreview preview = new MeetingPlanPreview();
				preview.Id = meeting.Id;
				preview.TenantId = meeting.TenantId;
				preview.Title = meeting.Title;
				preview.Location = meeting.Location;
				preview.StartDate = meeting.StartDate;
				preview.EndDate = meeting.EndDate;
				preview.MeetingType = meeting.MeetingType;
				preview.AuditId = meeting.AuditId;
				preview.Audit = meeting.Audit;
				preview.PreviousMeetingId = meeting.PreviousMeetingId;
				preview.PreviousMeeting = meeting.PreviousMeeting;
				preview.IsPublished = meeting.IsPublished;
				preview.ApprovedOn = meeting.ApprovedOn;
				preview.ApprovedBy = meeting.ApprovedBy;
				preview.ActualStart = meeting.ActualStart;
				preview.ActualEnd = meeting.ActualEnd;
				preview.PublishedOn = meeting.PublishedOn;
				preview.IsPublished = meeting.IsPublished;
				preview.Status = meeting.Status;
				preview.StatusType = meeting.StatusType;
				preview.CreatedBy = meeting.CreatedBy;
				preview.CreatedOn = meeting.CreatedOn;
				preview.UpdatedOn = meeting.UpdatedOn;
				preview.UpdatedBy = meeting.UpdatedBy;

				/*var suppliers = await _meetingSupplierMappingRepository.MeetingPlanPreview(Id, tenantId);
                IList<SupplierView> tags = new List<SupplierView>();
                foreach (SupplierDataView supplier in suppliers)
                {
                    var rating = await _surveyQuestionAnswerRepository.GetSupplierRatingFromSurvay(supplier.SupplierId);
                    var surveyList = await _surveySupplierMappingRepository.GetSurveys(supplier.SupplierId);
                    IList<SurveyView> survey = new List<SurveyView>();
                    foreach (SurveyDataView surveyDataView in surveyList)
                    {
                        var questionslist = await _surveyQuestionAnswerRepository.GetQuestionAnswers(surveyDataView.SurveyId);
                        IList<QuestionsView> question = new List<QuestionsView>();
                        foreach (QuestionsDataView questionsDataView in questionslist)
                        {
                            question.Add(new QuestionsView { Questions = questionsDataView.Questions, Answers = questionsDataView.Answers });
                        }
                        survey.Add(new SurveyView { SurveyId = surveyDataView.SurveyId, SurveyTitle = surveyDataView.SurveyTitle, Questions = question });
                    }
                    tags.Add(new SupplierView { SupplierId = supplier.SupplierId, Supplier = supplier.SupplierName, SupplierRating = rating.SupplierRating, Survey = survey, });
                }
                preview.Suppliers = tags;*/ 
				
				/// DO Not Delete 

				return preview;
			}
		}

		public async Task<IList<MrmDropdownList>> GetMRMDropDownList(int tenantId, int meetingId)
		{
			var mrmList = await _meetingPlanRepository.GetMRMDropDownList(tenantId, meetingId);
			return mrmList;
		}

		public async Task<MeetingPlan> UpdateMeetingPlan(int meetingId, PutMeeting meetingPlan, int tenantId, int userId)
		{
			var meetingPlans = await _meetingPlanRepository.UpdateMeetingPlan(meetingId, meetingPlan, tenantId, userId);

			var participants = await _meetingParticipantsRepository.GetParticipantsForMeetingInvitation(meetingId, tenantId, 2);

			foreach (var participant in participants)
			{
				IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

				keyValuePairs.Add("#PARTICIPANTS_NAME#", participant.Name);
				keyValuePairs.Add("#MEETING_TITLE#", participant.MeetingTitle);
				keyValuePairs.Add("#VENUE#", participant.Venue);
				keyValuePairs.Add("#START_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
				keyValuePairs.Add("#END_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
				keyValuePairs.Add("#MANAGEMENT_REVIEW_ID#", participant.MeetingID.ToString());
				await _emailService.SendEmail(participant.EmailAddress, participant.Name, "MRMRescheduledEmail.html", $"MRM ReScheduled > {participant.MeetingID} - {participant.MeetingTitle}", keyValuePairs);
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
				Module = IMSControllerCategory.MRM,
				ItemId = meetingPlans.Id,
				Description = meetingPlans.Title,
				Title = meetingPlans.Title,
				Date = meetingPlans.UpdatedOn
			});
			return meetingPlans;
		}

		public async Task UpdateAsPubluishForMeeting(int mrmid, int tenantId, int userId)
		{
			var meetingPlans = await _meetingPlanRepository.GetByIdAsync(mrmid);
			if (meetingPlans == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.MeetingNotFoundErrorMessage), mrmid);
			}
			else if (meetingPlans.ApprovedBy == null)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.MeetingNotApprovedErrorMessage));
			}
			else
			{
				meetingPlans.IsPublished = true;
				meetingPlans.PublishedOn = DateTime.UtcNow;
				await _meetingPlanRepository.UpdateAsync(meetingPlans);

				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Publish,
					Module = IMSControllerCategory.MRM,
					ItemId = meetingPlans.Id,
					Description = meetingPlans.Title,
					Title = meetingPlans.Title,
					Date = meetingPlans.PublishedOn
				});
				var participants = await _meetingParticipantsRepository.GetParticipantsForMeetingInvitation(meetingPlans.Id, tenantId, 2);

				foreach (var participant in participants)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

					keyValuePairs.Add("#PARTICIPANTS_NAME#", participant.Name);
					keyValuePairs.Add("#MEETING_TITLE#", participant.MeetingTitle);
					keyValuePairs.Add("#VENUE#", participant.Venue);
					keyValuePairs.Add("#START_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
					keyValuePairs.Add("#END_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
					keyValuePairs.Add("#MANAGEMENT_REVIEW_ID#", participant.MeetingID.ToString());
					await _emailService.SendEmail(participant.EmailAddress, participant.Name, "ManagementReviewScheduleMail.html", $"MRM Scheduled > {participant.MeetingID} - {participant.MeetingTitle}", keyValuePairs);
				}
				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = tenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.MRM;
				activityLog.EntityId = mrmid;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Publish;
				activityLog.Description = "MRM  Has Been Published";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(meetingPlans);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);
			}
		}

		public async Task DeleteMeetingById(int meetingId, int tenantId, int userId)
		{
			var meeting = await _meetingPlanRepository.GetByIdAsync(meetingId);
			if (meeting == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.MeetingNotFoundMessage), meetingId);
			}
			else if (meeting.Id == meetingId && meeting.TenantId == tenantId)
			{
				var participants = await _meetingParticipantsRepository.GetParticipantsForMeetingInvitation(meetingId, tenantId, 2);

				foreach (var participant in participants)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

					keyValuePairs.Add("#PARTICIPANTS_NAME#", participant.Name);
					keyValuePairs.Add("#MEETING_TITLE#", participant.MeetingTitle);
					keyValuePairs.Add("#VENUE#", participant.Venue);
					keyValuePairs.Add("#START_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
					keyValuePairs.Add("#END_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
					keyValuePairs.Add("#MANAGEMENT_REVIEW_ID#", participant.MeetingID.ToString());
					await _emailService.SendEmail(participant.EmailAddress, participant.Name, "MRMDeleteEmailTemplate.html", $"MRM Deleted > {participant.MeetingID} - {participant.MeetingTitle}", keyValuePairs);
				}
				await _meetingPlanRepository.DeleteAsync(meeting);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Delete,
					Module = IMSControllerCategory.MRM,
					ItemId = meeting.Id,
					Description = meeting.Title,
					Title = meeting.Title,
					Date = DateTime.UtcNow
				});
				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = tenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.MRM;
				activityLog.EntityId = meetingId;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Delete;
				activityLog.Description = "MRM  Has Been Deleted";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(meeting);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);
			}
		}

		public async Task<IList<ManagementReviewParticipant>> NightlyReminderForMeeting()
		{
			var participants = await _meetingParticipantsRepository.NightlyReminderForMeeting();
			return participants;
		}

		//adding agendas
		public async Task AddAgendas(PostAgendaView postAgendaView, int userId, int tenantId)
		{
			AgendaMaster am = new AgendaMaster();
			am.Title = postAgendaView.Title;
			am.ParentAgendaId = postAgendaView.ParentId;
			am.IsInputType = postAgendaView.IsInputAgenda;
			am.IsSet = false;
			await _agendaRepository.AddAsync(am);

			ActivityLog activityLog = new ActivityLog();
			activityLog.TenantId = tenantId;
			activityLog.ControllerId = (int)IMSControllerCategory.MRMAgenda;
			activityLog.EntityId = am.AgendaId;
			activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
			activityLog.Description = "MRM Agenda  Has Been Updated";
			activityLog.Details = System.Text.Json.JsonSerializer.Serialize(postAgendaView);
			activityLog.Status = true;
			activityLog.CreatedBy = userId;
			activityLog.CreatedOn = DateTime.UtcNow;
			await _activityLogRepository.AddAsync(activityLog);
		}

		public async Task<AgendaMaster> UpdateAgendasById(int agendaId, PutAgendaView agenda, int userId, int tenantId)
		{
			var agendas = await _agendaRepository.GetByIdAsync(agendaId);
			if (agendas == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AgendaNotFoundErrorMessage), agendaId);
			}
			else
			{
				agendas.Title = agenda.Title;
				/*agendas.ParentAgendaId = 14;
                agendas.IsInputType = false;
                agendas.IsSet = false;*/
				await _agendaRepository.UpdateAsync(agendas);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()

				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.MRMAgenda,
					ItemId = agendas.AgendaId,
					Description = agendas.Title,
					Title = agendas.Title,
					Date = DateTime.UtcNow
				});

				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = tenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.MRMAgenda;
				activityLog.EntityId = agendaId;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
				activityLog.Description = "MRM Agenda  Has Been Updated";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(agenda);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);

				return agendas;
			}
		}

		// Adding Minutes to a meeting

		//add meeting Participants
		public async Task AddParticipantsToMeeting(MeetingParticipant meetingParticipant)
		{
			await _meetingParticipantsRepository.AddAsync(meetingParticipant);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = meetingParticipant.UserId,
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = 1,//Hack for NOw
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.MRM_Participant,
				ItemId = meetingParticipant.Id,
				Description = meetingParticipant.Participants,
				Title = meetingParticipant.Participants + " is Added",
			});
		}

		public async Task UpdateMeetingParticipants(int Id, MeetingParticipant meetingParticipant)
		{
			var participants = await _meetingParticipantsRepository.GetByIdAsync(Id);
			if (participants == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ParticipantsNotFoundErrorMessage), Id);
			}
			else
			{
				participants.MeetingId = meetingParticipant.MeetingId;
				participants.DepartmentId = meetingParticipant.DepartmentId;
				participants.UserId = meetingParticipant.UserId;
				participants.Participants = meetingParticipant.Participants;
				await _meetingParticipantsRepository.UpdateAsync(participants);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = participants.UserId,
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = 1,//Hack for NOw
					Action = IMSControllerActionCategory.Edit,
					Module = IMSControllerCategory.MRM_Participant,
					ItemId = participants.Id,
					Description = participants.Participants,
					Title = participants.Participants + " is Added",
				});
			}
		}

		public async Task DeleteMeetingParticipants(int meetingparticipantsId)
		{
			var participants = await _meetingParticipantsRepository.GetByIdAsync(meetingparticipantsId);
			if (participants == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ParticipantsNotFoundErrorMessage), meetingparticipantsId);
			}
			await _meetingParticipantsRepository.DeleteAsync(participants);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = participants.UserId,
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = 1,//Hack for NOw
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.MRM_Participant,
				ItemId = participants.Id,
				Description = participants.Participants,
				Title = participants.Participants + " is Added",
			});
		}

		//Add Attendence to the meeting

		public async Task AddAttendenceToMeeting(PostAttendenceView meetingAttendence)
		{
			MeetingAttendence minutes = new MeetingAttendence();
			minutes.MeetingId = meetingAttendence.MeetingId;
			minutes.ParticipantsId = meetingAttendence.ParticipantsId;
			minutes.DepartmentId = meetingAttendence.DepartmentId;
			await _attendanceRepository.AddAsync(minutes);
		}

		public async Task EditMeetingAttendenceById(int Id, MeetingAttendence meetingAttendence)
		{
			var attendence = await _attendanceRepository.GetByIdAsync(Id);
			if (attendence == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AttendenceNotFoundErrorMessage), Id);
			}
			else
			{
				attendence.MeetingId = meetingAttendence.MeetingId;
				attendence.DepartmentId = meetingAttendence.DepartmentId;
				attendence.ParticipantsId = meetingAttendence.ParticipantsId;
				await _attendanceRepository.UpdateAsync(attendence);
			}
		}

		public async Task DeleteAttendenceById(int attendenceId)
		{
			var attendence = await _attendanceRepository.GetByIdAsync(attendenceId);
			if (attendence == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AttendenceNotFoundErrorMessage), attendenceId);
			}
			await _attendanceRepository.DeleteAsync(attendence);
		}

		public async Task ApproveMeeting(int tenantId, int meetingId, int userId)
		{
			var meeting = await _meetingPlanRepository.GetByIdAsync(meetingId);
			if (meeting == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.MeetingPlanNotFoundErrorMessage), meetingId);
			}
			else
			{
				var participantts = await _participantsRepository.GetMeetingParticipantsList(2, meetingId);
				if (participantts.Count == 0)
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.AddParticipantsErrorMessage));
				}
				else if (meeting.EndDate > meeting.StartDate)
				{
					meeting.ApprovedOn = DateTime.UtcNow;
					meeting.ApprovedBy = userId;
					meeting.Status = 15;
					await _meetingPlanRepository.UpdateAsync(meeting);

					var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
					await _messageService.SendNotificationMessage(new NotificationMessage()
					{
						SourceIdUserId = userId,
						SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
						EventType = NotificationEventType.BusinessMaster,
						BroadcastLevel = NotificationBroadcastLevel.Global,
						TenantId = tenantId,
						Action = IMSControllerActionCategory.Edit,
						Module = IMSControllerCategory.MRM,
						ItemId = meeting.Id,
						Description = "Meeting Has been Approved",
						Title = meeting.Title,
						Date = meeting.ApprovedOn
					});
					foreach (var users in participantts)
					{
						await _messageService.SendNotificationMessage(new NotificationMessage()
						{
							SourceIdUserId = users.UserId,
							SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
							EventType = NotificationEventType.BusinessMaster,
							BroadcastLevel = NotificationBroadcastLevel.Global,
							TenantId = tenantId,
							Action = IMSControllerActionCategory.Approve,
							Module = IMSControllerCategory.MRM_Participant,
							ItemId = meeting.Id,
							Description = "Meeting Has been Approved",
							Title = meeting.Title,
							Date = meeting.ApprovedOn
						});
					}
				}
				else
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.ChangeDateOfMeetingErrorMessage));
				}
			}
		}

		public async Task StartMeeting(int tenantId, int meetingId, int userId)
		{
			var meeting = await _meetingPlanRepository.GetByIdAsync(meetingId);
			if (meeting == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.MeetingPlanNotFoundErrorMessage), meetingId);
			}
			else if (meeting.IsPublished == true && meeting.ApprovedBy != null)
			{
				meeting.ActualStart = DateTime.UtcNow;
				await _meetingPlanRepository.UpdateAsync(meeting);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Edit,
					Module = IMSControllerCategory.MRM,
					ItemId = meeting.Id,
					Description = "Meeting Has been Started",
					Title = meeting.Title,
					Date = meeting.ActualStart
				});

				var participantts = await _participantsRepository.GetMeetingParticipantsList(2, meetingId);
				foreach (var users in participantts)
				{
					await _messageService.SendNotificationMessage(new NotificationMessage()
					{
						SourceIdUserId = users.UserId,
						SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
						EventType = NotificationEventType.BusinessMaster,
						BroadcastLevel = NotificationBroadcastLevel.Global,
						TenantId = tenantId,
						Action = IMSControllerActionCategory.Create,
						Module = IMSControllerCategory.MRM_Participant,
						ItemId = meeting.Id,
						Description = "Meeting Has been Started",
						Title = meeting.Title,
						Date = meeting.ActualStart
					});
				}
			}
			else
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.MeetingNotApprovedErrorMessage));
			}
		}

		public async Task CompleteMeeting(int tenantId, int meetingId, int userId)
		{
			var meeting = await _meetingPlanRepository.GetByIdAsync(meetingId);
			var agendaInputList = await GetAllAgendaInput(meetingId);
			var participantts = await _participantsRepository.GetMeetingParticipantsList(2, meetingId);
			var participant = participantts.Where(p => p.IsPresent == false || p.IsPresent == null).ToList();

			var minutesList = await _minutesRepository.GetMinutes(meetingId, tenantId);
			if (meeting == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.MeetingPlanNotFoundErrorMessage), meetingId);
			}
			else if (agendaInputList.Count == 0)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.AgendaNotAddedToMeetingErrorMessage));
			}
			else if (minutesList.Count == 0)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.MinutesNotAddedToMeetingErrorMessage));
			}
			else if (participant.Any())
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.ParticipantNotMarkPresentErrorMessage));
			}
			else if (meeting.IsPublished = true && meeting.ActualStart < DateTime.UtcNow)
			{
				meeting.ActualEnd = DateTime.UtcNow;
				meeting.Status = 16;
				await _meetingPlanRepository.UpdateAsync(meeting);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.MRM,
					ItemId = meeting.Id,
					Description = "Meeting Has been Completed",
					Title = meeting.Title,
					Date = meeting.ActualEnd
				});

				foreach (var users in participantts)
				{
					await _messageService.SendNotificationMessage(new NotificationMessage()
					{
						SourceIdUserId = users.UserId,
						SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
						EventType = NotificationEventType.BusinessMaster,
						BroadcastLevel = NotificationBroadcastLevel.Global,
						TenantId = tenantId,
						Action = IMSControllerActionCategory.Edit,
						Module = IMSControllerCategory.MRM_Participant,
						ItemId = meeting.Id,
						Description = "Meeting has been Completed",
						Title = meeting.Title,
						Date = meeting.ActualEnd
					});
				}
			}
			else
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.MeetingNotApprovedErrorMessage));
			}
		}

		public async Task GenerateReport(int tenantId, int meetingId)
		{
			var meeting = await _meetingPlanRepository.GetByIdAsync(meetingId);
			if (meeting == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.MeetingPlanNotFoundErrorMessage), meetingId);
			}
			else if (meeting.ApprovedBy == null && meeting.ApprovedOn == null)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.MeetingNotApprovedAndReportNotGeneratedErrorMessage));
			}
			else if (meeting.IsPublished == null)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.MeetingPublishedAndReportNotGeneratedErrorMessage));
			}
			else if (meeting.ActualStart == null)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.MeetingNotStartedAndReportNotGeneratedErrorMessage));
			}
			else
			{
				//ToDo have to write code for Generating the Report.
				await _meetingPlanRepository.UpdateAsync(meeting);
			}
		}

		public async Task<AuditDetailsForMeeting> GetAuditDetailsForMeeting(int tenantId, int meetingId)
		{
			var meeting = await _meetingPlanRepository.GetByIdAsync(meetingId);

			var audit = await _auditProgramRepository.GetAuditPreview(meeting.AuditProgramId, tenantId);
			AuditDetailsForMeeting mrmAudit = new AuditDetailsForMeeting();
			mrmAudit.AuditId = audit.Id;
			mrmAudit.AuditTitle = audit.Title;
			mrmAudit.FromDate = audit.FromDate;
			mrmAudit.ToDate = audit.DueDate;
			mrmAudit.Status = audit.Status;
			var standards = await _programStandardsRepository.GetStandards(audit.Id);
			IList<StandardView> tags = new List<StandardView>();

			foreach (StandardDataView standardTag in standards)
			{
				tags.Add(new StandardView() { StandardId = standardTag.StandardId, StandardName = standardTag.StandardName });
			}
			mrmAudit.Standards = tags;

			return mrmAudit;
		}

		public async Task AddMeetingParticipants(AddParticipantViewModel addParticipant, int moduleId, int tenantId, int userId)
		{
			var meeting = await _meetingPlanRepository.GetByIdAsync(addParticipant.ModuleEntityId);
			if (meeting.ActualStart != null)
			{
				throw new BadRequestException(string.Format(ConstantsBusiness.NewParticipantNotAddedErrorMessage));
			}
			else
			{
				await _participantsBusiness.AddPaticipants(addParticipant, moduleId, userId);
				var participants = await _meetingParticipantsRepository.GetParticipantsForMeetingInvitation(meeting.Id, tenantId, 2);

				foreach (var participant in participants)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

					keyValuePairs.Add("#PARTICIPANTS_NAME#", participant.Name);
					keyValuePairs.Add("#MEETING_TITLE#", participant.MeetingTitle);
					keyValuePairs.Add("#VENUE#", participant.Venue);
					keyValuePairs.Add("#START_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
					keyValuePairs.Add("#END_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
					keyValuePairs.Add("#MANAGEMENT_REVIEW_ID#", participant.MeetingID.ToString());
					await _emailService.SendEmail(participant.EmailAddress, participant.Name, "MRMParticipantAddedEmailTemplate.html", $"Participant Added > {participant.MeetingID} - {participant.MeetingTitle}", keyValuePairs);
				}

				meeting.IsPublished = false;
				await _meetingPlanRepository.UpdateAsync(meeting);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);

				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					EventType = NotificationEventType.BusinessMaster,
					BroadcastLevel = NotificationBroadcastLevel.Global,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = IMSControllerCategory.MRM_Participant,
					ItemId = meeting.Id,
					Description = "Participant has been added",
					Title = meeting.Title,
					Date = meeting.CreatedOn
				});
			}
		}

		public async Task AgendaFileUpload(IFormFile files, int tenantId, int meetingId, int agendaId, AgendaSummaryPostView agendaSummaryPostView, int userId)
		{
			AgendaSummaryMaster summaryMaster = new AgendaSummaryMaster();
			summaryMaster.MeetingId = meetingId;
			summaryMaster.AgendaId = agendaId;
			summaryMaster.Summary = agendaSummaryPostView.Summary;
			await _agendaSummaryMasterRepository.AddAsync(summaryMaster);

			PostFileRepositoryView postFile = new PostFileRepositoryView();
			postFile.SourceItemId = summaryMaster.Id;
			postFile.SourceId = (int)IMSModules.AgendaSummary;
			postFile.Description = summaryMaster.Summary;
			postFile.IsPrivate = true;
			await _fileRepositoryBusiness.AddFile(files, tenantId, postFile, userId);
		}

		public async Task<FileDownloadDomain> GetFile(int tenantId, int meetingId, int agendaId)
		{
			var data = await _agendaSummaryMasterRepository.GetFileId(meetingId, agendaId);
			var files = await _fileRepositoryBusiness.DownloadFile(data.FileId, tenantId);
			return files;
		}

		public async Task DeleteFile(int tenantId, int meetingId, int agendaId)
		{
			var data = await _agendaSummaryMasterRepository.GetFileId(meetingId, agendaId);
			var participants = await _meetingParticipantsRepository.GetParticipantsForMeetingInvitation(meetingId, tenantId, 2);

			foreach (var participant in participants)
			{
				IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

				keyValuePairs.Add("#PARTICIPANTS_NAME#", participant.Name);
				keyValuePairs.Add("#MEETING_TITLE#", participant.MeetingTitle);
				keyValuePairs.Add("#VENUE#", participant.Venue);
				keyValuePairs.Add("#START_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
				keyValuePairs.Add("#END_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
				keyValuePairs.Add("#MANAGEMENT_REVIEW_ID#", participant.MeetingID.ToString());
				await _emailService.SendEmail(participant.EmailAddress, participant.Name, "ManagementReviewScheduleMail.html", $"Agenda  Deleted > {participant.MeetingID} - {participant.MeetingTitle}", keyValuePairs);
			}
			await _fileRepositoryBusiness.DeleteFile(data.FileId, tenantId);
		}
	}
}