using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class MeetingAgendaMappingRepository : BaseRepository<MeetingAgendaMapping>, IMeetingAgendaMappingRepository
    {
       

        public MeetingAgendaMappingRepository(IMSDEVContext dbContext, ILogger<MeetingAgendaMapping> logger) : base(dbContext, logger)
        {
            
        }
    }
}