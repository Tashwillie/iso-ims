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

namespace Mindflur.IMS.Business
{
	public class MinutesBusiness : IMinutesBusiness
    {
        private readonly IMinutesRepository _minutesRepository;
        private readonly IMeetingPlanRepository _meetingPlanRepository;
        private readonly IWorkItemRepository _workItemRepository;
        private readonly IWorkItemWorkItemTokenRepository _workWorkItemTokenRepository;
        private readonly IMessageService _messageService;
        private readonly ITaskMasterRepository _taskMasterRepository;
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMeetingParticipantsRepository _meetingParticipantsRepository;
        private readonly IEmailService _emailService;

        public MinutesBusiness(IMinutesRepository minutesRepository, IMeetingPlanRepository meetingPlanRepository, IWorkItemRepository workItemRepository, 
            IWorkItemWorkItemTokenRepository workItemWorkItemTokenRepository, IMessageService messageService, ITaskMasterRepository taskMasterRepository,
            IActivityLogRepository activityLogRepository, IUserRepository userRepository,IMeetingParticipantsRepository meetingParticipantsRepository,IEmailService emailService)
        {
            _minutesRepository = minutesRepository;
            _meetingPlanRepository = meetingPlanRepository;
            _workItemRepository = workItemRepository;
            _workWorkItemTokenRepository = workItemWorkItemTokenRepository;
            _messageService = messageService;
            _taskMasterRepository = taskMasterRepository;
            _emailService = emailService;
            _activityLogRepository = activityLogRepository;
            _userRepository = userRepository;
            _meetingParticipantsRepository=meetingParticipantsRepository;
        }

        public async Task<PaginatedItems<MinutesOfMeetingListView>> GetMinutesList(GetMinutesList getMinutesList)
        {
            return await _minutesRepository.GetMinutesList(getMinutesList);
        }

        public async Task AddMinutesForMeeting(PostMinutesOfMeeting mm, int userId, int tenantId)
        {
            var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
            var meeting = await _meetingPlanRepository.GetByIdAsync(mm.MeetingId);
            if (meeting.ActualStart == null)
            {
                throw new BadRequestException(string.Format(ConstantsBusiness.AddingMinutesNotAllowedErrorMessage));
            }
            else
            {
                WorkItemMaster workItem = new WorkItemMaster();
                workItem.TenantId = tenantId;
                workItem.Title = mm.Title;
                workItem.Description = mm.Description;
                workItem.AssignedToUserId = mm.AssignTo;
                workItem.ResponsibleUserId = mm.ResponsibleUser;
                workItem.WorkItemTypeId = (int)IMSModules.TaskMaster;
                workItem.DueDate = mm.Deadline;
                workItem.SourceId = (int)IMSModules.ManagementReview;
                workItem.StatusMasterDataId = (int)IMSItemStatus.New;
                workItem.CreatedBy = userId;
                workItem.CreatedOn = DateTime.UtcNow;

                await _workItemRepository.AddAsync(workItem);
                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                    EventType = NotificationEventType.BusinessMaster,
                    BroadcastLevel = NotificationBroadcastLevel.Global,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Create,
                    Module = IMSControllerCategory.Tasks,
                    ItemId = workItem.WorkItemId,
                    Description = workItem.Description,
                    Title = workItem.Title,
                    Date = workItem.CreatedOn
                });
                if (mm.Priority != 0)
                {
                    WorkItemWorkItemToken token = new WorkItemWorkItemToken();
                    token.WorkItemId = workItem.WorkItemId;
                    token.TokenId = mm.Priority;
                    await _workWorkItemTokenRepository.AddAsync(token);
                }

                MinutesOfMeeting mrm = new MinutesOfMeeting();
                mrm.MeetingId = mm.MeetingId;
                mrm.AgendaId = mm.AgendaId;
                mrm.TaskId = workItem.WorkItemId;
                mrm.CreatedOn = DateTime.UtcNow;
                mrm.CreatedBy = userId;
				await _minutesRepository.AddAsync(mrm);			

				workItem.SourceItemId = mrm.Id;
                await _workItemRepository.UpdateAsync(workItem);

                TaskMetaData task = new TaskMetaData();
                task.WorkItemId = workItem.WorkItemId;
                task.EstimateEffortHours = mm.EstimatedEffortPoint;
                task.IsAcknowledge = mm.isAcknowledge;
                await _taskMasterRepository.AddAsync(task);
				
                var tasks =  await _minutesRepository.GetTaskMinutesDetails(mrm.TaskId);
				IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
				keyValuePairs.Add("#USER_NAME#", (string)tasks.Name);
				keyValuePairs.Add("#TASKID#", tasks.TaskId.ToString());
				keyValuePairs.Add("#TASKTITLE#", (string)tasks.TaskTitle);
				keyValuePairs.Add("#MEETINGID#", tasks.MeetingId.ToString());
				keyValuePairs.Add("#MEETINGTITLE#", (string)tasks.MeetingTitle);
				keyValuePairs.Add("#TASKDESCRIPTION#", (string)tasks.Description);
				keyValuePairs.Add("#STATUS#", (string)tasks.Status);
				//  keyValuePairs.Add("#PRIORITY#", tasks.Priority);
				await _emailService.SendEmail((string)tasks.EmailAddress, (string)tasks.Name, "MinutesAddedEmailTemplate.html", $"Added > Minutes Task - {tasks.TaskId} - {tasks.TaskTitle}", keyValuePairs);
				await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                    EventType = NotificationEventType.BusinessMaster,
                    BroadcastLevel = NotificationBroadcastLevel.Global,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Create,
                    Module = IMSControllerCategory.MRMMinutes,
                    ItemId = mrm.Id,
                    Description = workItem.Description,
                    Title = workItem.Title,
                    Date = workItem.CreatedOn
                });
            }
        }

        public async Task<IList<MinutesOfMeetingView>> GetMinutes(int minutesId, int tenantId)
        {
            return await _minutesRepository.GetMinutes(minutesId, tenantId);
        }

        public async Task UpdateMinutesofMeeting(int TaskId, PutMinutesOfMeeting mm, int userId, int tenantId)
        {
            await _minutesRepository.UpdateMinutesofMeeting(TaskId, mm, userId, tenantId);
        }

        public async Task<TaskDetails> GetMinutesDetailtsToEditMinutes(int taskId, int tenantId)
        {
            return await _minutesRepository.GetMinutesDetailtsToEditMinutes(taskId, tenantId);
        }

        public async Task<MinutesOfMeetingPreview> GetMinutesById(int minutesId)
        {
            return await _minutesRepository.GetMinutesById(minutesId);
        }

        public async Task DeleteMinutesById(int taskId, int userId, int tenantId)
        {
            var minutes = _minutesRepository.getminutesByTaskId(taskId);
            if (minutes == null)
            {
                throw new NotFoundException("Meeting", minutes.MeetingId);
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
				Module = IMSControllerCategory.MRMMinutes,
				ItemId = minutes.Id,
				Description = "Minutes has been Deleted",
				Title = minutes.Id + " is Deleted",
				Date = DateTime.UtcNow
			});
			await _minutesRepository.DeleteAsync(minutes);

            
            var task = await _workItemRepository.GetByIdAsync(taskId);
            await _workItemRepository.DeleteAsync(task);
            // var taskCompletion = _minutesRepository.getTaskCompletionByTaskId(taskId);
            //await _taskCompletionRepository.DeleteAsync(taskCompletion);

            ActivityLog activityLog = new ActivityLog();
            activityLog.TenantId = tenantId;
            activityLog.ControllerId = (int)IMSControllerCategory.MRMMinutes;
            activityLog.EntityId = taskId;
            activityLog.ModuleAction = (int)IMSControllerActionCategory.Delete;
            activityLog.Description = "MRM minutes Has Been deleted";
            activityLog.Details = System.Text.Json.JsonSerializer.Serialize(minutes);
            activityLog.Status = true;
            activityLog.CreatedBy = userId;
            activityLog.CreatedOn = DateTime.UtcNow;
            await _activityLogRepository.AddAsync(activityLog);
        }

        public async Task<IList<GetMinutesDetailsForReport>> GetMinutesForReports(int meetingId)
        {
            return await _minutesRepository.GetMinutesForReports(meetingId);
        }
    }
}