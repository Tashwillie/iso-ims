using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Service;

namespace Mindflur.IMS.Runners.Review
{
    public class ProcessManagementReviewSchedule : IManagementReviewSchedule
    {
        private readonly IManagementReviewBusiness _managementReviewBusiness;
        private readonly ILogger<ProcessManagementReviewSchedule> _logger;
        private readonly IEmailService _emailService;

        public ProcessManagementReviewSchedule(ILogger<ProcessManagementReviewSchedule> logger, IManagementReviewBusiness managementReviewBusiness, IEmailService emailService)
        {
            _emailService = emailService;
            _managementReviewBusiness = managementReviewBusiness;
            _logger = logger;
        }

        public async Task NighlyRemider()
        {
            _logger.LogInformation("Started NighlyRemider");

            await ProcessTaskNightlyRemider();

            _logger.LogInformation("Processed NighlyRemider");
        }

        public async Task OverDueReminder()
        {
            _logger.LogInformation("Started OverDueReminder");
            await ProcessTaskOverDueRemider();
            _logger.LogInformation("Processed OverDueReminder");
        }

        private async Task ProcessTaskOverDueRemider()
        {
            {
            }
        }

        private async Task ProcessTaskNightlyRemider()
        {
            var participants = await _managementReviewBusiness.NightlyReminderForMeeting(); //Get task details (detail should have everything i.e. required to be sent in a email)

            foreach (var participant in participants)
            {
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                keyValuePairs.Add("#PARTICIPANTS_NAME#", participant.Name);
                keyValuePairs.Add("#MEETING_TITLE#", participant.MeetingTitle);
                keyValuePairs.Add("#VENUE#", participant.Venue);
                keyValuePairs.Add("#START_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
                keyValuePairs.Add("#END_DATE#", participant.StartDate.ToString("dd/mm/yyyy"));
                keyValuePairs.Add("#MANAGEMENT_REVIEW_ID#", participant.MeetingID.ToString());
                await _emailService.SendEmail(participant.EmailAddress, participant.Name, "ManagementReviewScheduleMailReminder.html", $"MRM Meeting scheduled > {participant.MeetingID} -{participant.MeetingTitle} ", keyValuePairs);
            }
        }
    }
}