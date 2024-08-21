using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
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
using Stripe;

namespace Mindflur.IMS.Data.Repository
{
    public class MinutesRepository : BaseRepository<MinutesOfMeeting>, IMinutesRepository
    {
        private readonly IEmailService _emailService;
        private readonly IMessageService _messageService;

        public MinutesRepository(IMSDEVContext dbContext, ILogger<MinutesOfMeeting> logger, IEmailService emailService, IMessageService messageService) : base(dbContext, logger)
        {
            _emailService = emailService;
            _messageService = messageService;
        }

        public async Task<PaginatedItems<MinutesOfMeetingListView>> GetMinutesList(GetMinutesList getMinutesList)
        {
            var query = (from mm in _context.MinutesOfMeetings
                         join meetingPlan in _context.MeetingPlans on mm.MeetingId equals meetingPlan.Id
                         
                         join workItem in _context.WorkItemMasters on mm.TaskId equals workItem.WorkItemId
                         join us in _context.UserMasters on workItem.AssignedToUserId equals us.UserId into users
                         from subuser in users.DefaultIfEmpty()
                         join ru in _context.UserMasters on workItem.ResponsibleUserId equals ru.UserId into ruser
                         from subruser in ruser.DefaultIfEmpty()
                         join tm in _context.TenanttMasters on meetingPlan.TenantId equals tm.TenantId
                         where mm.MeetingId == getMinutesList.MeetingId && meetingPlan.TenantId == getMinutesList.TenantId

                         select new MinutesOfMeetingListView
                         {
                             Id = workItem.WorkItemId,
                             AssignTo = $"{subuser.FirstName} {subuser.LastName}",
                             ResponsiblePerson = $"{subruser.FirstName} {subruser.LastName}",
                             Action = workItem.Title,
                             DeadLine = workItem.DueDate,
                             AgendaId = mm.AgendaId
                         }).AsQueryable();

            if (getMinutesList.AgendaId != 0)
            {
                query = query.Where(minute => minute.AgendaId == getMinutesList.AgendaId);
            }
            var filteredData = DataExtensions.OrderBy(query, getMinutesList.ListRequests.SortColumn, getMinutesList.ListRequests.Sort == "asc")
                              .Skip(getMinutesList.ListRequests.PerPage * (getMinutesList.ListRequests.Page - 1))
                              .Take(getMinutesList.ListRequests.PerPage);

            var totalItems = await query.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getMinutesList.ListRequests.PerPage);

            var model = new PaginatedItems<MinutesOfMeetingListView>(getMinutesList.ListRequests.Page, getMinutesList.ListRequests.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);


        }

        public async Task<IList<MinutesOfMeetingView>> GetMinutes(int minutesId, int tenantId)
        {
            var preview = await (from mm in _context.MinutesOfMeetings
                                 join am in _context.AgendaMasters on mm.AgendaId equals am.AgendaId
                               
                                 join wm in _context.WorkItemMasters on mm.TaskId equals wm.WorkItemId
                                 join us in _context.UserMasters on wm.AssignedToUserId equals us.UserId into us
                                  from subuser in us.DefaultIfEmpty()
                                 join mp in _context.MeetingPlans on mm.MeetingId equals mp.Id
                                 join tm in _context.TenanttMasters on mp.TenantId equals tm.TenantId
                                 where mm.MeetingId == minutesId && mp.TenantId == tenantId
                                 select new MinutesOfMeetingView
                                 {
                                     Id = mm.Id,
                                     AgendaId = am.AgendaId,
                                     Agenda = am.Title,
                                     Action = wm.Title,
                                     AssignTo = $"{subuser.FirstName} {subuser.LastName}",
                                     DeadLine = wm.DueDate,
                                 }).ToListAsync();
            return await Task.FromResult(preview);
        }
        public async Task<MinutesOfMeetingPreview> GetMinutesById(int minutesId)
        {
            var preview =  (from mm in _context.MinutesOfMeetings
                                
                                 join wm in _context.WorkItemMasters on mm.TaskId equals wm.WorkItemId
                                 join task in _context.TaskMasters on wm.WorkItemId equals task.WorkItemId into task
                                 from subTask in task.DefaultIfEmpty()
                                 join us in _context.UserMasters on wm.AssignedToUserId equals us.UserId
                                 
                                 where mm.Id == minutesId
                                 select new MinutesOfMeetingPreview
                                 {
                                     Id = mm.Id,
                                     Title = wm.Title,
                                     Description = wm.Description,
                                     AssignTo = wm.AssignedToUserId,
                                     FullName = $"{us.FirstName} {us.LastName}",
                                     Deadline = wm.DueDate,
                                     IsAcknowledge = subTask.IsAcknowledge,
                                     
                                     EffortRemainingPoints = subTask.RemainingEffortHours,

                                 }).AsQueryable();
            return preview.FirstOrDefault();
        }

        public async Task<MinutesTaks> GetTaskMinutesDetails(int taskId)
        {
            return await (from mp in _context.MeetingPlans
                          join mm in _context.MinutesOfMeetings on mp.Id equals mm.MeetingId                        
                          join wm in _context.WorkItemMasters on mm.TaskId equals wm.WorkItemId
                          join us in _context.UserMasters on wm.AssignedToUserId equals us.UserId
                          join md in _context.MasterData on wm.StatusMasterDataId equals md.Id                         
                          where wm.WorkItemId == taskId
                          select new MinutesTaks
                          {
                              Name = us.FirstName,
                              TaskId = wm.WorkItemId,
                              TaskTitle = wm.Title,
                              MeetingId = mm.MeetingId,
                              MeetingTitle = mp.Title,
                              Description = wm.Description,
                              EmailAddress = us.EmailId,
                              Status = md.Items,
                             

                          }).FirstOrDefaultAsync();
        }

        public MinutesOfMeeting getminutesByTaskId(int taskId)
        {
            var minutes = _context.MinutesOfMeetings.FirstOrDefault(T => T.TaskId == taskId);
            return minutes;

        }
        public TaskComplition getTaskCompletionByTaskId(int taskId)
        {
            return _context.TaskComplitions.FirstOrDefault(T => T.TaskId == taskId);
        }

        public async Task<TaskMetaDataMinutes> GetTaskMetaDataForMinutes(int workItemId)
        {
            var rawData = (from task in _context.TaskMasters
                           join workItem in _context.WorkItemMasters on task.WorkItemId equals workItem.WorkItemId
                           where workItem.WorkItemId == workItemId
                           select new TaskMetaDataMinutes()
                           {
                               IsAcknowledge = task.IsAcknowledge,
                           }).AsQueryable();
            return rawData.FirstOrDefault();

		}

		public async Task<TaskDetails> GetMinutesDetailtsToEditMinutes(int taskId, int tenantId)
		{
			var minutesDetails = (from ts in _context.WorkItemMasters
                                  join task in _context.TaskMasters on ts.WorkItemId equals task.WorkItemId into task 
                                  from subTask in task.DefaultIfEmpty()
                                  join work in _context.WorkItemWorkItemTokens on ts.WorkItemId equals work.WorkItemId into work
                                  from subToken in work.DefaultIfEmpty()
                                  join workItem in _context.Tokens on subToken.TokenId equals workItem.TokenId into workItem
                                  from token in workItem.DefaultIfEmpty()
								  join user in _context.UserMasters on ts.AssignedToUserId equals user.UserId into users
								  from subusers in users.DefaultIfEmpty()	
                                  join ruser in _context.UserMasters on ts.ResponsibleUserId equals ruser.UserId into rusers
                                  from subrUser in rusers.DefaultIfEmpty()
                                  join tm in _context.TenanttMasters on ts.TenantId equals tm.TenantId
								  where ts.WorkItemId == taskId && ts.TenantId == tenantId && ts.WorkItemTypeId==231
								  select new TaskDetails
								  {
									  TaskId = ts.WorkItemId,
									  Title = ts.Title,
									  Description = ts.Description,
                                      ResponsibleUserId = ts.ResponsibleUserId,
                                      ResponsibleUser =$"{subrUser.FirstName} {subrUser.LastName}",
									  AssignedToId = ts.AssignedToUserId,
									  AssignedTo = $"{subusers.FirstName} {subusers.LastName}",
									  Deadline = ts.DueDate,
									  IsAcknowledge = subTask.IsAcknowledge, 
									  PriorityTypeId = subToken.TokenId,
									  PriorityType = token.TokenName,
									  EffortRemainingPointsId = subTask.RemainingEffortHours.HasValue ? subTask.RemainingEffortHours: 0,
                                      EffortPoints = subTask.EstimateEffortHours.HasValue ? subTask.EstimateEffortHours : 0
								  }).AsQueryable();
			return minutesDetails.FirstOrDefault();
		}


        public async Task UpdateMinutesofMeeting(int taskId, PutMinutesOfMeeting mm, int userId, int tenantId)
        {
            var tasks = await _context.TaskMasters.Where(t => t.WorkItemId == taskId).FirstOrDefaultAsync();
            if (tasks == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.MeetingNotFoundMessage), taskId);
            }
            else
            {
                
                if(mm.IsAcknowledge == false)
                {
                    tasks.RemainingEffortHours = mm.EffortRemainingPoints;
                    tasks.IsAcknowledge = mm.IsAcknowledge;
                    await _context.SaveChangesAsync();
                } 
                
                

                var minutes = await _context.MinutesOfMeetings.Where(t => t.TaskId == taskId).FirstOrDefaultAsync();
                minutes.UpdatedOn = DateTime.UtcNow;
                minutes.UpdatedBy = userId;
               
                await _context.SaveChangesAsync();

                var workItem = await _context.WorkItemMasters.FindAsync(taskId);
                workItem.Title = mm.Title;
                workItem.Description = mm.Description;
                workItem.AssignedToUserId = mm.AssignTo;
                workItem.ResponsibleUserId = mm.ResponsibleUser;
                workItem.UpdatedOn = DateTime.UtcNow;
                workItem.UpdatedBy = userId;    
                await _context.SaveChangesAsync();
                
                if(mm.Priority != 0)
                {
                    var priority = await _context.WorkItemWorkItemTokens.Where(t => t.WorkItemId == taskId).FirstOrDefaultAsync();
                    priority.TokenId = mm.Priority;
                    await _context.SaveChangesAsync();
                }

                

                if (tasks.IsAcknowledge == false)
                {
                    tasks.RemainingEffortHours = mm.EffortRemainingPoints;
                    tasks.IsAcknowledge = mm.IsAcknowledge;
                    await _context.SaveChangesAsync();

                    var task = await GetTaskMinutesDetails(taskId);
                    IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    keyValuePairs.Add("#USER_NAME#", (string)task.Name);
                    keyValuePairs.Add("#TASKID#", task.TaskId.ToString());
                    keyValuePairs.Add("#TASKTITLE#", (string)task.TaskTitle);
                    keyValuePairs.Add("#MEETINGID#", task.MeetingId.ToString());
                    keyValuePairs.Add("#MEETINGTITLE#", (string)task.MeetingTitle);
                    keyValuePairs.Add("#TASKDESCRIPTION#", (string)task.Description);
                    keyValuePairs.Add("#STATUS#", (string)task.Status);
                    //  keyValuePairs.Add("#PRIORITY#", tasks.Priority);
                    await _emailService.SendEmail((string)task.EmailAddress, (string)task.Name, "MinutesTaskEmailTemplate.html", $"Updated > Minutes Task - {task.TaskId} - {task.TaskTitle}", keyValuePairs);
                }
                ActivityLog activityLog = new ActivityLog();
                activityLog.TenantId = tenantId;
                activityLog.ControllerId = (int)IMSControllerCategory.MRMMinutes;
                activityLog.EntityId = taskId;
                activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
                activityLog.Description = "MRM minutes Has Been updated";
                activityLog.Details = System.Text.Json.JsonSerializer.Serialize(mm);
                activityLog.Status = true;
                activityLog.CreatedBy = userId;
                activityLog.CreatedOn = DateTime.UtcNow;
                await _context.AddAsync(activityLog);
                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    EventType = NotificationEventType.BusinessMaster,
                    BroadcastLevel = NotificationBroadcastLevel.Global,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Create,
                    Module = IMSControllerCategory.MRMMinutes,
                    ItemId = tasks.TaskId,
                    Description = "Minutes has been Updated",
                    Title = tasks.WorkItemId + " is Updated",
                });

            }
        }

        public async Task<IList<GetMinutesDetailsForReport>>GetMinutesForReports(int meetingId)
        {
            var minutes =  await (from mp in _context.MeetingPlans
                          join mm in _context.MinutesOfMeetings on mp.Id equals mm.MeetingId
                          join wm in _context.WorkItemMasters on mm.TaskId equals wm.WorkItemId
                          join tm in _context.TaskMasters on wm.WorkItemId equals tm.WorkItemId
                          join user in _context.UserMasters on wm.AssignedToUserId equals user.UserId into users
                          from subusers in users.DefaultIfEmpty()
                          where mp.Id == meetingId && wm.SourceId == (int)IMSModules.ManagementReview 
                          select new GetMinutesDetailsForReport
                          {
                              AgendaId = mm.AgendaId,
                              Actions =  wm.Title,
                              Description = wm.Description,
                              ResponsiblePerson = $"{subusers.FirstName} {subusers.LastName}",
                              Deadline = wm.DueDate.ToString(),
                              Noted = tm.IsAcknowledge == true ? "Noted" : ""
                          }).ToListAsync();
            return await Task.FromResult(minutes);

        }
    }
}