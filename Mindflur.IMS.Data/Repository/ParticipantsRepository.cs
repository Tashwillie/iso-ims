using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ParticipantsRepository : BaseRepository<Participant>, IParticipantsRepository
    {
       
        public ParticipantsRepository(IMSDEVContext dbContext, ILogger<Participant> logger) : base(dbContext, logger)
        {
           
        }

		public async Task ValidateParticipants(AddParticipantViewModel addParticipant,int moduleId)
		{
			var rawData = await _context.Participants.Where(t => t.UserId == addParticipant.UserId && t.ModuleEntityId == addParticipant.ModuleEntityId && t.ModuleId==moduleId && t.DeletedBy==null).FirstOrDefaultAsync();

			if (rawData != null)
			{
				throw new ArgumentException(string.Format(RepositoryConstant.AddParticipantErrorMessage, addParticipant.UserId));
			}
		}
		public async Task<PaginatedItems<ParticipantsListView>> GetAuditParticipantsList(GetParticipantsList getParticipantsList, int moduleId)
        {
            var query = (from pa in _context.Participants
                         join ap in _context.AuditPrograms on pa.ModuleEntityId equals ap.Id
                         join rm in _context.MasterData on pa.RoleId equals rm.Id
                         join um in _context.UserMasters on pa.UserId equals um.UserId
                         join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
                         where pa.ModuleEntityId == getParticipantsList.ModuleEntityId && pa.ModuleId == moduleId && pa.DeletedOn == null && ap.TenantId == getParticipantsList.TenantId

                         select new ParticipantsListView
                         {
                             ParticipantId = pa.ParticipantId,
                             Title = ap.Title,
                             ParticipantName = $"{um.FirstName} {um.LastName}",
                             Role = rm.Items,
                             Ispresent = pa.MarkPresent,
                             ParticipantUserId=pa.UserId,
                             Present=pa.IsPresent

                         }).AsQueryable();


            var filteredData = DataExtensions.OrderBy(query, getParticipantsList.ListRequests.SortColumn, getParticipantsList.ListRequests.Sort == "asc")
                              .Skip(getParticipantsList.ListRequests.PerPage * (getParticipantsList.ListRequests.Page - 1))
                              .Take(getParticipantsList.ListRequests.PerPage);

            var totalItems = await query.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getParticipantsList.ListRequests.PerPage);

            var model = new PaginatedItems<ParticipantsListView>(getParticipantsList.ListRequests.Page, getParticipantsList.ListRequests.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);

        }

        public async Task<PaginatedItems<ParticipantsListView>> GetMinutesParticipantsList(GetParticipantsList getParticipantsList, int moduleId)
        {
            var query = (from pa in _context.Participants
                         join mrm in _context.MeetingPlans on pa.ModuleEntityId equals mrm.Id
                         join rm in _context.MasterData on pa.RoleId equals rm.Id
                         join um in _context.UserMasters on pa.UserId equals um.UserId
                         where pa.ModuleEntityId == getParticipantsList.ModuleEntityId && pa.ModuleId == moduleId && pa.DeletedOn == null && mrm.TenantId == getParticipantsList.TenantId

                         select new ParticipantsListView
                         {
                             ParticipantId = pa.ParticipantId,
                             Title = mrm.Title,
                             ParticipantName = $"{um.FirstName} {um.LastName}",
                             Role = rm.Items,
                             Ispresent = pa.MarkPresent,
                             ParticipantUserId=pa.UserId,

                         }).AsQueryable();


            var filteredData = DataExtensions.OrderBy(query, getParticipantsList.ListRequests.SortColumn, getParticipantsList.ListRequests.Sort == "asc")
                              .Skip(getParticipantsList.ListRequests.PerPage * (getParticipantsList.ListRequests.Page - 1))
                              .Take(getParticipantsList.ListRequests.PerPage);

            var totalItems = await query.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getParticipantsList.ListRequests.PerPage);

            var model = new PaginatedItems<ParticipantsListView>(getParticipantsList.ListRequests.Page, getParticipantsList.ListRequests.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);

        }
        public async Task<IList<Participant>> GetAuditParticipantsList(int moduleId, int moduleEntityId)
        {
            var participantsList = await (from pa in _context.Participants
                                          join ap in _context.AuditPrograms on pa.ModuleEntityId equals ap.Id
                                          where pa.ModuleId == moduleId && pa.ModuleEntityId == moduleEntityId && pa.DeletedBy==null
                                          select new Participant
                                          {
                                              ParticipantId = pa.ParticipantId,
                                              ModuleId = pa.ModuleId,
                                              ModuleEntityId = pa.ModuleEntityId,
                                              RoleId = pa.RoleId,
                                              MarkPresent = pa.MarkPresent,
                                              IsPresent = pa.IsPresent, 
                                              UserId = pa.UserId,
                                          }).ToListAsync();
            return await Task.FromResult(participantsList);
        }
        public async Task<IList<Participant>> GetMeetingParticipantsList(int moduleId, int moduleEntityId)
        {
            var participantsList = await (from pa in _context.Participants
                                          join mp in _context.MeetingPlans on pa.ModuleEntityId equals mp.Id
                                          where pa.ModuleId == moduleId && pa.ModuleEntityId == moduleEntityId && pa.DeletedBy == null
                                          select new Participant
                                          {
                                              ParticipantId = pa.ParticipantId,
                                              ModuleId = pa.ModuleId,
                                              ModuleEntityId = pa.ModuleEntityId,
                                              RoleId = pa.RoleId,
                                              MarkPresent = pa.MarkPresent,
                                              IsPresent = pa.IsPresent,
                                              UserId = pa.UserId,
                                          }).ToListAsync();
            return await Task.FromResult(participantsList);
        }

    }
}
