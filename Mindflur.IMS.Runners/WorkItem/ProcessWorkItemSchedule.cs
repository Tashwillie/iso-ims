using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Service;

namespace Mindflur.IMS.Runners.WorkItem
{
    public class ProcessWorkItemSchedule : IWorkItemSchedule
    {
        private readonly ITaskMasterBusiness _taskMasterBusiness;
        private readonly ILogger<ProcessWorkItemSchedule> _logger;

        private readonly IEmailService _emailService;
        private readonly IWorkItemBusiness _workItemBusiness;

        public ProcessWorkItemSchedule(ILogger<ProcessWorkItemSchedule> logger, ITaskMasterBusiness taskMasterBusiness, IEmailService emailService, IWorkItemBusiness workItemBusiness)
        {
            _logger = logger;
            _taskMasterBusiness = taskMasterBusiness;
            _emailService = emailService;
            _workItemBusiness = workItemBusiness;
        }

        public async Task NighlyRemider()
        {
            _logger.LogInformation("Started NighlyRemider");

            await ProcessTaskNightlyRemider();
            await ProcessNonconformanceNightlyRemider();
            await ProcessCorrectiveActionNightlyRemider();
            await ProcessRiskNightlyRemider();
            await ProcessOpportunityNightlyRemider();
            await ProcessObservationNightlyRemider();
            await ProcessIncidentNightlyRemider();

            _logger.LogInformation("Processed NighlyRemider");
        }

        public async Task OverDueReminder()
        {
            _logger.LogInformation("Started OverDueReminder");
            await ProcessTaskOverDueRemider();
            await ProcessNonconformanceOverDueRemider();
            await ProcessCorrectiveActionOverDueRemineder();

            await ProcessOpportunityOverDueRemider();
            await ProcessObservationOverDueRemider();
            await ProcessIncidentOverDueRemider();
            _logger.LogInformation("Processed OverDueReminder");
        }

        private async Task ProcessTaskOverDueRemider()
        {
            var overdues = await _taskMasterBusiness.OverDueRemiderForTask(); //Get task details (detail should have everything i.e. required to be sent in a email)

            foreach (var tasks in overdues)
            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", tasks.Name);
                keyValuePairs.Add("#TASKID#", tasks.TaskId.ToString());
                keyValuePairs.Add("#TASKTITLE#", tasks.Title);
                keyValuePairs.Add("#TASKDESCRIPTION#", tasks.Description);
                keyValuePairs.Add("#STATUS#", tasks.Status);

                await _emailService.SendEmail(tasks.EmailAddress, tasks.Name, "TaskEmailTemplateForOverDue.html", $"Overdue > Task - {tasks.TaskId} - {tasks.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessNonconformanceOverDueRemider()
        {
            var nc = await _workItemBusiness.OverDueRemiderForNc(); //Get Nc details (detail should have everything i.e. required to be sent in a email)

            foreach (var nonConformance in nc)
            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", nonConformance.Name);
                keyValuePairs.Add("#NC_ID#", nonConformance.Id.ToString());
                keyValuePairs.Add("#NC_TITLE#", nonConformance.Title);
                keyValuePairs.Add("#NC_DESCRIPTION#", nonConformance.Description);
                keyValuePairs.Add("#NC_STATUS#", nonConformance.Status);
                keyValuePairs.Add("#NC_TYPE#", nonConformance.NCType);
                await _emailService.SendEmail(nonConformance.EmailAddress, nonConformance.Name, "NonConfirmityEmailTemplate.html", $"Overdue > NonConformance - {nonConformance.Id} - {nonConformance.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessCorrectiveActionOverDueRemineder()
        {
            var Ca = await _workItemBusiness.OverDueRemiderForCA();
            foreach (var correctiveAction in Ca)
            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", correctiveAction.Name);
                keyValuePairs.Add("#NC_ID#", correctiveAction.NcId.ToString());
                keyValuePairs.Add("#NC_TITLE#", correctiveAction.NcTitle);
                keyValuePairs.Add("#CA_DESCRIPTION#", correctiveAction.CaDescription);
                keyValuePairs.Add("#CORRECTIVE_ACTION_REQUIRED#", correctiveAction.CaTitle);

                await _emailService.SendEmail(correctiveAction.EmailAddress, correctiveAction.Name, "CorrectiveActionEmailTemplate.html", $"Overdue > CorrectiveAction - {correctiveAction.CaId} - {correctiveAction.CaTitle} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessTaskNightlyRemider()
        {
            var tasks = await _taskMasterBusiness.NightlyRemiderForTask(); //Get task details (detail should have everything i.e. required to be sent in a email)

            foreach (var task in tasks)
            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", task.Name);
                keyValuePairs.Add("#TASKID#", task.TaskId.ToString());
                keyValuePairs.Add("#TASKTITLE#", task.Title);
                keyValuePairs.Add("#TASKDESCRIPTION#", task.Description);
                keyValuePairs.Add("#STATUS#", task.Status);
                //  keyValuePairs.Add("#PRIORITY#", task.Priority);
                await _emailService.SendEmail(task.EmailAddress, task.Name, "TaskEmailTemplateForReminder.html", $"Reminder > Task - {task.TaskId} - {task.Title}  ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessNonconformanceNightlyRemider()
        {
            var Nc = await _workItemBusiness.NightlyRemiderForNc(); //Get Nc details (detail should have everything i.e. required to be sent in a email)

            foreach (var nonConformance in Nc)
            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", nonConformance.Name);
                keyValuePairs.Add("#NC_ID#", nonConformance.Id.ToString());
                keyValuePairs.Add("#NC_TITLE#", nonConformance.Title);
                keyValuePairs.Add("#NC_DESCRIPTION#", nonConformance.Description);
                keyValuePairs.Add("#NC_STATUS#", nonConformance.Status);
                keyValuePairs.Add("#NC_TYPE#", nonConformance.NCType);
                await _emailService.SendEmail(nonConformance.EmailAddress, nonConformance.Name, "NonConfirmityEmailTemplate.html", $"Reminder > Task - {nonConformance.Id} - {nonConformance.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessCorrectiveActionNightlyRemider()
        {
            var Ca = await _workItemBusiness.NightlyRemiderForCA();
            foreach (var correctiveAction in Ca)
            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", correctiveAction.Name);
                keyValuePairs.Add("#NC_ID#", correctiveAction.NcId.ToString());
                keyValuePairs.Add("#NC_TITLE#", correctiveAction.NcTitle);
                keyValuePairs.Add("#CA_DESCRIPTION#", correctiveAction.CaDescription);
                keyValuePairs.Add("#CORRECTIVE_ACTION_REQUIRED#", correctiveAction.CaTitle);

                await _emailService.SendEmail(correctiveAction.EmailAddress, correctiveAction.Name, "CorrectiveActionEmailTemplate.html", $"Reminder > Task - {correctiveAction.CaId} - {correctiveAction.CaTitle} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessRiskNightlyRemider()
        {
            var risk = await _workItemBusiness.NightlyRemiderForRisk();
            foreach (var allRisk in risk)

            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", allRisk.Name);
                keyValuePairs.Add("# #RISK_ID#", allRisk.RiskId.ToString());
                keyValuePairs.Add("#RISKDATE#", allRisk.RiskDate.ToString());
                keyValuePairs.Add("#DESCRIPTION#", allRisk.Description);

                keyValuePairs.Add("#RISKOWNER#", allRisk.RiskOwner);

                keyValuePairs.Add("#TOTALRISKSCORE#", allRisk.TotalScore.ToString());

                await _emailService.SendEmail(allRisk.EmailAddress, allRisk.Name, "RiskEmailTemplate.html", $"Reminder > Risk - {allRisk.RiskId}  ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessOpportunityNightlyRemider()
        {
            var rawdata = await _workItemBusiness.NightlyRemiderForOpportunity();
            foreach (var opportunities in rawdata)

            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", opportunities.Name);
                keyValuePairs.Add("# #OPPORTUNITY_ID#", opportunities.OpportunityId.ToString());
                keyValuePairs.Add("#DESCRIPTION#", opportunities.Description);
                keyValuePairs.Add("#Title#", opportunities.Title);
                keyValuePairs.Add("#DUEDATE#", opportunities.DueDate.ToString());
                keyValuePairs.Add("#DEPARTMENT#", opportunities.Department);
                await _emailService.SendEmail(opportunities.EmailAddress, opportunities.Name, "OpportunityEmailTemplate.html", $"Reminder > Opportunity - {opportunities.OpportunityId} - {opportunities.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessOpportunityOverDueRemider()
        {
            var rawdata = await _workItemBusiness.OverDueRemiderForOpportunity();
            foreach (var opportunities in rawdata)

            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", opportunities.Name);
                keyValuePairs.Add("# #OPPORTUNITY_ID#", opportunities.OpportunityId.ToString());
                keyValuePairs.Add("#DESCRIPTION#", opportunities.Description);
                keyValuePairs.Add("#Title#", opportunities.Title);
                keyValuePairs.Add("#DUEDATE#", opportunities.DueDate.ToString());
                keyValuePairs.Add("#DEPARTMENT#", opportunities.Department);
                await _emailService.SendEmail(opportunities.EmailAddress, opportunities.Name, "OpportunityEmailTemplate.html", $"Overdue > Opportunity - {opportunities.OpportunityId} - {opportunities.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessObservationNightlyRemider()
        {
            var rawdata = await _workItemBusiness.NightlyRemiderForObservation();
            foreach (var opportunities in rawdata)

            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", opportunities.Name);
                keyValuePairs.Add("# #OBSERVATION_ID#", opportunities.ObservationId.ToString());
                keyValuePairs.Add("#DESCRIPTION#", opportunities.Description);
                keyValuePairs.Add("#Title#", opportunities.Title);
                keyValuePairs.Add("#DUEDATE#", opportunities.DueDate.ToString());
                keyValuePairs.Add("#DEPARTMENT#", opportunities.Department);
                await _emailService.SendEmail(opportunities.EmailAddress, opportunities.Name, "ObservationEmailTemplate.html", $"Reminder > Observation - {opportunities.ObservationId} - {opportunities.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessObservationOverDueRemider()
        {
            var rawdata = await _workItemBusiness.OverDueRemiderForObservation();
            foreach (var opportunities in rawdata)

            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", opportunities.Name);
                keyValuePairs.Add("# #OBSERVATION_ID#", opportunities.ObservationId.ToString());
                keyValuePairs.Add("#DESCRIPTION#", opportunities.Description);
                keyValuePairs.Add("#Title#", opportunities.Title);
                keyValuePairs.Add("#DUEDATE#", opportunities.DueDate.ToString());
                keyValuePairs.Add("#DEPARTMENT#", opportunities.Department);
                await _emailService.SendEmail(opportunities.EmailAddress, opportunities.Name, "ObservationEmailTemplate.html", $"Overdue > Observation - {opportunities.ObservationId} - {opportunities.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessIncidentNightlyRemider()
        {
            var rawdata = await _workItemBusiness.NightlyRemiderForIncident();
            foreach (var opportunities in rawdata)

            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", opportunities.Name);
                keyValuePairs.Add("# #INCIDENT_ID#", opportunities.IncidentId.ToString());
                keyValuePairs.Add("#DESCRIPTION#", opportunities.Description);
                keyValuePairs.Add("#Title#", opportunities.Title);
                keyValuePairs.Add("#INCIDENTDATE#", opportunities.IncidentDate.ToString());
                keyValuePairs.Add("#DEPARTMENT#", opportunities.Department);
                keyValuePairs.Add("#INJURY_DESCRIPTION#", opportunities.InjuryDescription);
                keyValuePairs.Add("#HOWITOCCURED#", opportunities.HowItOccured);
                keyValuePairs.Add("#MEDICALTREATMENT#", opportunities.MedicalTreatment);
                keyValuePairs.Add("#CLASSIFICATION_DESCRIPTION#", opportunities.ClassificationDescription);
                await _emailService.SendEmail(opportunities.EmailAddress, opportunities.Name, "IncidentEmailTemplate.html", $"Reminder > Observation - {opportunities.IncidentId} - {opportunities.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }

        private async Task ProcessIncidentOverDueRemider()
        {
            var rawdata = await _workItemBusiness.OverDueRemiderForIncident();
            foreach (var opportunities in rawdata)

            {
                //Prepare email such as sender, subject, body
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#USER_NAME#", opportunities.Name);
                keyValuePairs.Add("# #INCIDENT_ID#", opportunities.IncidentId.ToString());
                keyValuePairs.Add("#DESCRIPTION#", opportunities.Description);
                keyValuePairs.Add("#Title#", opportunities.Title);
                keyValuePairs.Add("#INCIDENTDATE#", opportunities.IncidentDate.ToString());
                keyValuePairs.Add("#DEPARTMENT#", opportunities.Department);
                keyValuePairs.Add("#INJURY_DESCRIPTION#", opportunities.InjuryDescription);
                keyValuePairs.Add("#HOWITOCCURED#", opportunities.HowItOccured);
                keyValuePairs.Add("#MEDICALTREATMENT#", opportunities.MedicalTreatment);
                keyValuePairs.Add("#CLASSIFICATION_DESCRIPTION#", opportunities.ClassificationDescription);
                await _emailService.SendEmail(opportunities.EmailAddress, opportunities.Name, "IncidentEmailTemplate.html", $"Overdue > Incident - {opportunities.IncidentId} - {opportunities.Title} ", keyValuePairs);

                //_emailService.SendEmail()
            }
        }
    }
}