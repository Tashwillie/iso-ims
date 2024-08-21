using Microsoft.Extensions.Logging;
using Mindflur.IMS.Runners.Audit;
using Mindflur.IMS.Runners.Review;
using Mindflur.IMS.Runners.WorkItem;

namespace Mindflur.IMS.Runners
{
    public class Reminder : IReminders
    {
        private readonly ILogger<Reminder> _logger;

        private readonly IWorkItemSchedule _workItemSchedule;
        private readonly IAuditSchedule _auditSchedule;
        private readonly IManagementReviewSchedule _managementReviewSchedule;
        public Reminder(ILogger<Reminder> logger, IWorkItemSchedule workItemSchedule, IAuditSchedule auditSchedule, IManagementReviewSchedule managementReviewSchedule)
        {
            _logger = logger;
            _workItemSchedule = workItemSchedule;
            _auditSchedule = auditSchedule;
            _managementReviewSchedule = managementReviewSchedule;
        }

        public async Task Start()
        {
            _logger.LogInformation("Started reminder!");

            await _workItemSchedule.NighlyRemider();

            //Send reminder to participants of an internal-audit, if the meeting is due next day (tomorrow)
            //Send reminder to task owner/assigned to as the task due date is approaching
            //Send reminder to participants of management review, if the meeting is due next day (tomorrow)
            await _auditSchedule.NighlyRemider();
            await _workItemSchedule.OverDueReminder();
            await _managementReviewSchedule.NighlyRemider();
            //Send overdue reminder to owner of internal-audit, if the meeting is not started on previous day
        }
    }
}