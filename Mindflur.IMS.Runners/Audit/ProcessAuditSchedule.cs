using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Service;

namespace Mindflur.IMS.Runners.Audit
{
    public class ProcessAuditSchedule : IAuditSchedule
    {
        private readonly IInternalAuditBusiness _internalAuditBusiness;
        private readonly ILogger<ProcessAuditSchedule> _logger;
        private readonly IEmailService _emailService;

        public ProcessAuditSchedule(ILogger<ProcessAuditSchedule> logger, IInternalAuditBusiness internalAuditBusiness, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
            _internalAuditBusiness = internalAuditBusiness;
        }

        public async Task NighlyRemider()
        {
            _logger.LogInformation("Started NighlyRemider");

            await ProcessTaskNightlyRemiderForAuidtProgram();
            await ProcessTaskNightlyRemiderForAuidtor();

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
        }

        private async Task ProcessTaskNightlyRemiderForAuidtProgram()
        {
            var audits = await _internalAuditBusiness.NightlyRemiderMailAuditProgram(); //Get task details (detail should have everything i.e. required to be sent in a email)

            foreach (var details in audits)
            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#AUDITORS_NAME#", details.Name);
                keyValuePairs.Add("#AUDIT_PROGRAM_ID#", details.AuditProgramId.ToString());
                keyValuePairs.Add("#AUDIT_TITLE#", details.Title);
                keyValuePairs.Add("#AUDIT_CATEGORY#", details.Category);
                keyValuePairs.Add("#START_DATE#", details.StartDate.ToString());
                keyValuePairs.Add("#END_DATE#", details.EndDate.ToString());
                await _emailService.SendEmail(details.EmailAddress, details.Name, "AuditScheduleReminderMail.html", $"Reminder > {details.AuditProgramId} - Internal audit program has be scheduled  ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessTaskNightlyRemiderForAuidtor()
        {
            var audits = await _internalAuditBusiness.NightlyRemiderMailToAuditorsForAuditItem(); //Get task details (detail should have everything i.e. required to be sent in a email)

            foreach (var details in audits)
            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#AUDITORS_NAME#", details.Name);
                keyValuePairs.Add("#AUDIT_PROGRAM_ID#", details.AuditProgramId.ToString());
                keyValuePairs.Add("#AUDIT_TITLE#", details.Title);
                keyValuePairs.Add("#AUDIT_CATEGORY#", details.Category);
                keyValuePairs.Add("#START_DATE#", details.StartDate.ToString());
                keyValuePairs.Add("#END_DATE#", details.EndDate.ToString());
                await _emailService.SendEmail(details.EmailAddress, details.Name, "AuditScheduleReminderMailToAuditorForAuditItems.html",
                    $"Reminder > {details.AuditProgramId}  - There is an audit Item That need to be audit  ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }
    }
}