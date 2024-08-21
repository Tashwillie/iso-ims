using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class MeetingPlanRepository : BaseRepository<MeetingPlan>, IMeetingPlanRepository
    {
     


      

        public MeetingPlanRepository(IMSDEVContext dbContext, ILogger<MeetingPlan> logger) : base(dbContext, logger)
        {
           
        }

        public async Task<PaginatedItems<MeetingListView>> GetMeetingPlans(GetMeetingListRequest getListRequest)
        {
            var rawData = (from mp in _context.MeetingPlans
                           join tm in _context.TenanttMasters on mp.TenantId equals tm.TenantId
                           where getListRequest.TenantId == mp.TenantId
                           select new MeetingListView
                           {
                               Id = mp.Id,
                               Title = mp.Title,
                               TenantId = mp.TenantId,
                               Location = mp.Location,
                               StartDate = mp.StartDate,
                               EndDate = mp.EndDate,
                               MeetingType = mp.MeetingType,
                               IsPublished = mp.IsPublished,
                               CreatedBy = mp.CreatedBy,
                           }).AsQueryable();
            if (getListRequest.ForUserId > 0)
                rawData = rawData.Where(log => log.CreatedBy == getListRequest.ForUserId);

            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
                           .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                           .Take(getListRequest.ListRequests.PerPage);
            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<MeetingListView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
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

        public async Task<MeetingPlanPreview> GetMeetingPlanById(int id, int tenantId)
        {
            var rawData = (from meeting in _context.MeetingPlans
                           join pmrm in _context.MeetingPlans on meeting.PreviousMeetingId equals pmrm.Id into mrm
                           from subpmrm in mrm.DefaultIfEmpty()
                           join tm in _context.TenanttMasters on meeting.TenantId equals tm.TenantId
                           join ap in _context.AuditPrograms on meeting.AuditProgramId equals ap.Id into audits
                           from subaudit in audits.DefaultIfEmpty()
                           join md in _context.MasterData on meeting.Status equals md.Id into status
                           from substatus in status.DefaultIfEmpty()
                           where meeting.Id == id && meeting.TenantId == tenantId
                           select new MeetingPlanPreview
                           {
                               Id = meeting.Id,
                               TenantId = meeting.TenantId,
                               Title = meeting.Title,
                               Location = meeting.Location,
                               StartDate = meeting.StartDate,
                               EndDate = meeting.EndDate,
                               MeetingType = meeting.MeetingType,
                               AuditId = meeting.AuditProgramId,
                               Audit = subaudit.Title,
                               PreviousMeetingId = subpmrm.Id,
                               PreviousMeeting = subpmrm.Title,

                               IsPublished = meeting.IsPublished,
                               ApprovedOn = meeting.ApprovedOn,
                               ApprovedBy = meeting.ApprovedBy,
                               Status = meeting.Status,
                               StatusType = substatus.Items,
                               CreatedBy = meeting.CreatedBy,
                               CreatedOn = meeting.CreatedOn,
                               UpdatedBy = meeting.UpdatedBy,
                               UpdatedOn = meeting.UpdatedOn,
                               ActualStart = meeting.ActualStart,
                               ActualEnd = meeting.ActualEnd,
                           }).AsQueryable();
            return rawData.FirstOrDefault();
        }

        public async Task<IList<MrmDropdownList>> GetMRMDropDownList(int tenantId, int meetingId)
        {
            var meetingList = await (from meeting in _context.MeetingPlans
                                    where meeting.Id != meetingId && meeting.TenantId == tenantId
                                    select new MrmDropdownList
                                    {
                                        MeetingId = meeting.Id,
                                        Meeting = meeting.Title,
                                        EndDate = meeting.EndDate,
                                    }).OrderByDescending(x => x.EndDate).ToListAsync();
            return meetingList;

        }

        public async Task<MeetingPlan> UpdateMeetingPlan(int meetingId, PutMeeting meetingPlan, int tenantId, int userId)
        {
            var meetingPlans = await _context.MeetingPlans.FindAsync(meetingId);
            if (meetingPlans == null)
            {
                throw new NotFoundException(string.Format(RepositoryConstant.MeetingNotFoundErrorMessage), meetingId);
            }
            else if (meetingPlans.Id == meetingId && meetingPlans.TenantId == tenantId)
            {            
                
                meetingPlans.Title = meetingPlan.Title;
                meetingPlans.Location = meetingPlan.Location;
                meetingPlans.StartDate = meetingPlan.StartDate;
                meetingPlans.EndDate = meetingPlan.EndDate;
                meetingPlans.MeetingType = meetingPlan.MeetingType;
                meetingPlans.PreviousMeetingId = meetingPlan.PreviousMeetingId;
                meetingPlans.AuditProgramId = meetingPlan.InternalAuditId;
                meetingPlans.IsPublished = false;
                meetingPlans.Status = (int)IMSItemStatus.Draft;
                meetingPlans.UpdatedBy = userId;
                meetingPlans.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                ActivityLog activityLog = new ActivityLog();
                activityLog.TenantId = tenantId;
                activityLog.ControllerId = (int)IMSControllerCategory.MRM;
                activityLog.EntityId = meetingId;
                activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
                activityLog.Description = "MRM  Has Been Updated";
                activityLog.Details = System.Text.Json.JsonSerializer.Serialize(meetingPlan);
                activityLog.Status = true;
                activityLog.CreatedBy = userId;
                activityLog.CreatedOn = DateTime.UtcNow;
                await _context.ActivityLogs.AddAsync(activityLog);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new BadRequestException(string.Format(RepositoryConstant.MeetingNotCreatedErrorMessage));
            }

            return meetingPlans;
        }
    }
}