using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class AgendaSummaryMasterRepository : BaseRepository <AgendaSummaryMaster>, IAgendaSummaryMasterRepository
    {
        public AgendaSummaryMasterRepository(IMSDEVContext dbContext, ILogger<AgendaSummaryMaster> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<AgendaFilesRepositoryViewModel> GetFileId(int meetingId, int agendaId)
        {
            var rawdata =  (from ag in _context.AgendaSummaryMasters 
                                 join fr in _context.FileRepositories on ag.Id equals fr.SourceItemId
                                 where ag.AgendaId == agendaId && ag.MeetingId == meetingId && fr.SourceId == (int)IMSModules.AgendaSummary
                                 select new AgendaFilesRepositoryViewModel
                                 {
                                     FileId = fr.FileRepositoryId,
                                     Summary = ag.Summary,

                                 }).AsQueryable();
            return rawdata.FirstOrDefault();

        }
    }
}
