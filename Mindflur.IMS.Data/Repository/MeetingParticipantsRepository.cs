using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Models.Custom;

namespace Mindflur.IMS.Data.Repository
{
    public class MeetingParticipantsRepository : BaseRepository<MeetingParticipant>, IMeetingParticipantsRepository
    {
        

        public MeetingParticipantsRepository(IMSDEVContext dbContext, ILogger<MeetingParticipant> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<IList<MeetingParticpantView>> GetParticipants(int meetingId)
        {
            var participants = await (from mp in _context.MeetingParticipants
                                      join us in _context.UserMasters on mp.UserId equals us.UserId
                                      join md in _context.MasterData on mp.DepartmentId equals md.Id
                                      where mp.MeetingId == meetingId
                                      select new MeetingParticpantView
                                      {
                                          Id = mp.Id,
                                          Name = $"{us.FirstName} {us.LastName}",
                                          Department = md.Items,
                                          Email = us.EmailId,
                                      }).ToListAsync();
            return await Task.FromResult(participants);
        }

        async Task<IList<ManagementReviewParticipant>> IMeetingParticipantsRepository.GetParticipantsForMeetingInvitation(int meetingId, int tenantId, int moduleId)
        {
            var participants = await (from mp in _context.Participants
                                      join us in _context.UserMasters on mp.UserId equals us.UserId
                                      join md in _context.MeetingPlans on mp.ModuleEntityId equals md.Id
                                      join tm in _context.TenanttMasters on md.TenantId equals tm.TenantId
                                      where mp.ModuleEntityId == meetingId && md.TenantId == tenantId && mp.ModuleId == moduleId
                                      select new ManagementReviewParticipant
                                      {
                                          UserId = us.UserId,
                                          Name = us.FirstName,
                                          EmailAddress = us.EmailId,
                                          MeetingTitle = md.Title,
                                          MeetingID = mp.ModuleEntityId,
                                          StartDate = md.StartDate,
                                          EndDate = md.EndDate,
                                          Venue = md.Location,
                                      }).ToListAsync();
            var userId = participants.DistinctBy(user => user.UserId);

            return await Task.FromResult(participants);
        }
        async Task<IList<ManagementReviewParticipant>> IMeetingParticipantsRepository.NightlyReminderForMeeting()
        {
            var participants = await (from mp in _context.MeetingPlans
                                      join p in _context.Participants on mp.Id equals p.ModuleEntityId
                                      join us in _context.UserMasters on p.UserId equals us.UserId
                                      where mp.StartDate == DateTime.UtcNow.Date.AddDays(2) && mp.Status == (int)IMSItemStatus.Open && p.ModuleId == 2
                                      select new ManagementReviewParticipant
                                      {
                                          UserId = us.UserId,
                                          Name = us.FirstName,
                                          EmailAddress = us.EmailId,
                                          MeetingTitle = mp.Title,
                                          MeetingID = mp.Id,
                                          StartDate = mp.StartDate,
                                          EndDate = mp.EndDate,
                                          Venue = mp.Location,
                                      }).ToListAsync();


            return await Task.FromResult(participants);
        }
    }
}