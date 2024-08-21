using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class MeetingAttendanceRepository : BaseRepository<MeetingAttendence>, IMeetingAttendanceRepository
    {
        private readonly ILogger _logger;

        public MeetingAttendanceRepository(IMSDEVContext dbContext, ILogger<MeetingAttendence> logger) : base(dbContext, logger)
        {
            _logger = logger;
        }

        public async Task<IList<MeetingAttendenceView>> GetAttendanceList(int minutesId)
        {
            _logger.Log(LogLevel.Trace, "start fetch CorrectiveAction detail");
            var attendence = await (from ma in _context.MeetingAttendences
                                    join mp in _context.MeetingPlans on ma.MeetingId equals mp.Id
                                    join md in _context.MasterData on ma.DepartmentId equals md.Id
                                    join us in _context.UserMasters on ma.ParticipantsId equals us.UserId
                                    where mp.Id == minutesId
                                    select new MeetingAttendenceView
                                    {
                                        Id = ma.Id,
                                        MeetingName = mp.Title,
                                        DepartmentName = md.Items,
                                        Participants = string.Format("{0} {1}", us.FirstName, us.LastName),
                                    }).ToListAsync();
            return await Task.FromResult(attendence);
        }
    }
}