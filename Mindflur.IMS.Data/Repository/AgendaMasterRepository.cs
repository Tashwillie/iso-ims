using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class AgendaMasterRepository : BaseRepository<AgendaMaster>, IAgendaRepository
    {
        

        public AgendaMasterRepository(IMSDEVContext dbContext, ILogger<AgendaMaster> logger) : base(dbContext, logger)
        {
           
        }

        public async Task<PaginatedItems<AgendaMasterView>> GetAgendas(GetAllAgendaListView getAllAgendaRequest)
        {
            var rawdata = (from am in _context.AgendaMasters
                           where am.IsInputType == getAllAgendaRequest.Input && am.ParentAgendaId == getAllAgendaRequest.ParentId

                           select new AgendaMasterView
                           {
                               Id = am.AgendaId,
                               AgendaTitle = am.Title
                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawdata, getAllAgendaRequest.ListRequests.SortColumn, getAllAgendaRequest.ListRequests.Sort == "asc")
                              .Skip(getAllAgendaRequest.ListRequests.PerPage * (getAllAgendaRequest.ListRequests.Page - 1))
                              .Take(getAllAgendaRequest.ListRequests.PerPage);

            var totalItems = await rawdata.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getAllAgendaRequest.ListRequests.PerPage);

            var model = new PaginatedItems<AgendaMasterView>(getAllAgendaRequest.ListRequests.Page, getAllAgendaRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }

        public async Task<IList<AgendaMasterView>> GetAgendasInput(GetAgendasView getAgendas)
        {
            var agendas = await (from am in _context.AgendaMasters
                                 where am.IsInputType == getAgendas.IsInputType && am.ParentAgendaId == getAgendas.ParentId

                                 select new AgendaMasterView
                                 {
                                     Id = am.AgendaId,
                                     AgendaTitle = am.Title
                                 }).ToListAsync();
            return await Task.FromResult(agendas);
        }

        public async Task<PaginatedItems<MeetingAgendaView>> GetMeetingAgenda(GetAllMeetingAgendas getAllMeetingAgendas)
        {
            var rawdata = (from mam in _context.MeetingAgendaMappings
                           join mp in _context.MeetingPlans on mam.MeetingId equals mp.Id
                           join am in _context.AgendaMasters on mam.AgendaId equals am.AgendaId
                           join tm in _context.TenanttMasters on mp.TenantId equals tm.TenantId
                           where mam.MeetingId == getAllMeetingAgendas.MeetingId && mp.TenantId == getAllMeetingAgendas.TenantId
                           select new MeetingAgendaView
                           {
                               MappingId = mam.MappingId,
                               AgendaId = am.AgendaId,
                               Title = am.Title,
                               InputType = am.IsInputType == true ? "Input" : "Output",
                               IsSet = am.IsSet,
                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawdata, getAllMeetingAgendas.ListRequests.SortColumn, getAllMeetingAgendas.ListRequests.Sort == "asc")
                              .Skip(getAllMeetingAgendas.ListRequests.PerPage * (getAllMeetingAgendas.ListRequests.Page - 1))
                              .Take(getAllMeetingAgendas.ListRequests.PerPage);

            var totalItems = await rawdata.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getAllMeetingAgendas.ListRequests.PerPage);

            var model = new PaginatedItems<MeetingAgendaView>(getAllMeetingAgendas.ListRequests.Page, getAllMeetingAgendas.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }
        public async Task<IList<MRMInputAgenda>>GetAllAgendaInput(int meetingId)
            {
            var rawDAta=await(from mam in _context.MeetingAgendaMappings
							  join mp in _context.MeetingPlans on mam.MeetingId equals mp.Id
							  join am in _context.AgendaMasters on mam.AgendaId equals am.AgendaId
							  join tm in _context.TenanttMasters on mp.TenantId equals tm.TenantId
                              where mp.Id == meetingId && am.IsInputType==true
                              select new MRMInputAgenda()
                              {
                                  AgendaId=am.AgendaId,
                                  Title=am.Title
                              }


							  ).ToListAsync();
            return await Task.FromResult(rawDAta);
            }


        public async Task<IList<MRMInputAgenda>> GetAllAgendaOutputs(int meetingId)
        {
            var rawDAta = await (from mam in _context.MeetingAgendaMappings
                                 join mp in _context.MeetingPlans on mam.MeetingId equals mp.Id
                                 join am in _context.AgendaMasters on mam.AgendaId equals am.AgendaId
                                 join tm in _context.TenanttMasters on mp.TenantId equals tm.TenantId
                                 where mp.Id == meetingId && am.IsInputType == false
                                 select new MRMInputAgenda()
                                 {
                                     AgendaId = am.AgendaId,
                                     Title = am.Title
                                 }


                              ).ToListAsync();
            return await Task.FromResult(rawDAta);
        }

        public async Task<IList<AgendaMasterView>> GetAllAgendasByMeeting(GetAllAgendasByMeeting getAllAgendasByMeeting)
        {
           

            var rawdata = await (from mam in _context.MeetingAgendaMappings
                                 join mp in _context.MeetingPlans on mam.MeetingId equals mp.Id
                                 join am in _context.AgendaMasters on mam.AgendaId equals am.AgendaId
                                 join tm in _context.TenanttMasters on mp.TenantId equals tm.TenantId
                                 where  mp.Id == getAllAgendasByMeeting.MeetingId && mp.TenantId == getAllAgendasByMeeting.TenantId

                                 select new AgendaMasterView
                                 {
                                     Id = am.AgendaId,
                                     AgendaTitle = am.Title
                                 }).ToListAsync();
            return await Task.FromResult(rawdata);
        }
        public async Task<IList<AgendaForMeeting>> GetAgendaForMeeting()
        {
            var agenda = await (from am in _context.AgendaMasters
                                where am.IsSet == true
                                select new AgendaForMeeting
                                {
                                    AgendaId = am.AgendaId,
                                    Title = am.Title,
                                    ParentAgendaId = am.ParentAgendaId,
                                    IsSet = am.IsSet,
                                    IsInputType = am.IsInputType,

                                }).ToListAsync();
            return await Task.FromResult(agenda);
        }
        public async Task<AgnedaPrview> PreviewAgendas(int Id)
        {
            var rawData = (from agendas in _context.AgendaMasters
                           where agendas.AgendaId == Id
                           select new AgnedaPrview()
                           {
                               Id = agendas.AgendaId,
                               AgendaTitle = agendas.Title,
                               ParentAgendaId = agendas.ParentAgendaId,
                               IsSet = agendas.IsSet,
                               IsInputType = agendas.IsInputType == true ? "Input" : "Output",

                           }).AsQueryable();
            return rawData.FirstOrDefault();







        }
    }
}