using Microsoft.Extensions.Options;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Reflection;
using System.Text.Json;

namespace Mindflur.IMS.Business.Service
{
	public class EmailService : IEmailService
	{
		private readonly IOptions<CoreSettings> _coreSettings;
		private readonly IMessageService _messageService;

		public EmailService(IOptions<CoreSettings> coreConfig, IMessageService messageService)
		{
			_coreSettings = coreConfig;
			_messageService = messageService;
		}

		public async Task SendEmail(string receiverEmail, string receiverName, string templateName, string emailSubject, IDictionary<string, string> sourceKeyValuePairs)
		{
			if (emailSubject == null)
			{
				emailSubject = "Generic subject lines, you need to set it.";
			}

			string rowTemplate = GetRowContent(templateName);

			string parsedHtmlEmailContent = TemplateParser(rowTemplate, sourceKeyValuePairs);

			await SendEmailUsingSendGrid(receiverEmail, receiverName, emailSubject, parsedHtmlEmailContent);
		}

		private async Task SendEmailUsingSendGrid(string receiverEmail, string receiverName, string subject, string emailContent)
		{
			var client = new SendGridClient(_coreSettings.Value.EmailServiceSetting.ClientId);
			var from = new EmailAddress(_coreSettings.Value.EmailServiceSetting.FromEmailAddress, _coreSettings.Value.EmailServiceSetting.FromEmailName);

			var to = new EmailAddress(receiverEmail, receiverName);
			var plainTextContent = "We do not support plain text email content";

			var messageData = new MessageData()
			{
				ToName = receiverName,
				ToAddress = receiverEmail,
				Title = subject,
				Body = emailContent
			};

			var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, emailContent);

			var response = await client.SendEmailAsync(msg);

			await response.Body.ReadAsStringAsync();

			await _messageService.SendEmailMessage(JsonSerializer.Serialize(messageData));
		}

		private string TemplateParser(string rowTemplate, IDictionary<string, string> sourceKeyValuePairs)
		{
			var keyValuePairs = GetGenericKeyValuePairs();

			//merge generic and incomming kvp
			foreach (var item in sourceKeyValuePairs)
			{
				keyValuePairs.Add(item.Key, item.Value);
			}

			var parsedContent = rowTemplate;

			//parse rowcontent with kvp
			foreach (var item in keyValuePairs)
			{
				parsedContent = parsedContent.Replace(item.Key, item.Value);
			}

			return parsedContent;
		}

		private IDictionary<string, string> GetGenericKeyValuePairs()
		{
			IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

			keyValuePairs.Add("#COMPANY_LOGO#", "Sandeep");
			keyValuePairs.Add("#COMPANY_NAME#", "Mindflur System LPP");
			keyValuePairs.Add("#EMAIL_SUPPORT#", "we@wwise.com");
			keyValuePairs.Add("#COMPANY_NAME_TEAM#", "WWISE Team");

			return keyValuePairs;
		}

		/// <summary>
		/// Ensures a directory contains correct Separator characters.
		/// For better understanding, go to https://docs.microsoft.com/en-us/dotnet/api/system.io.path.directoryseparatorchar
		/// </summary>
		public static string EnsureDirectory(string directory)
		{
			string sepChar = Path.DirectorySeparatorChar.ToString();
			string altChar = Path.AltDirectorySeparatorChar.ToString();

			if (!directory.EndsWith(sepChar) && !directory.EndsWith(altChar))
				directory += sepChar;

			return OperatingSystem.IsWindows() ? directory.Replace(altChar, sepChar) : directory.Replace("\\", sepChar);
		}

		private string GetRowContent(string templateName)
		{
			string rowHtmlContent = string.Empty;
			var currentExecutionPath = new DirectoryInfo(Assembly.GetAssembly(typeof(EmailService)).Location);
			var templateFolderPath = EnsureDirectory(currentExecutionPath.Parent.FullName + @"\EmailTemplates\");
			var filePath = templateFolderPath + templateName;
			try
			{
				rowHtmlContent = File.ReadAllText(filePath);
			}
			catch (Exception)
			{
				if (templateName == "NewUserLoginMail.html")
					rowHtmlContent = EmailTemplateConstant.Constant_NewUserLoginMail;
				else if (templateName == "AccountDeactivated.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AccountDeactivated;
				else if (templateName == "ManagementReviewScheduleMail.html")
					rowHtmlContent = EmailTemplateConstant.Constant_ManagementReviewScheduleMail;
				else if (templateName == "NC_CATaskEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_NC_CATaskEmailTemplate;
				else if (templateName == "TaskEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_TaskEmailTemplate;
				else if (templateName == "MinutesTaskEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_MinutesTaskEmailTemplate;
				else if (templateName == "CorrectiveActionEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_CorrectiveActionEmailTemplate;
				else if (templateName == "NonConfirmityEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_NonConfirmityEmailTemplate;
				else if (templateName == "AuditScheduleMail.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditEmailTemplate;
				else if (templateName == "ProjectTaskMail.html")
					rowHtmlContent = EmailTemplateConstant.Constant_ProjectTaskEmail;
				else if (templateName == "ProjectTaskEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_ProjectTaskEmailTemplate;
				else if (templateName == "RiskEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_RiskEmailTemplate;
				else if (templateName == "TaskEmailTemplateForOverDue.html")
					rowHtmlContent = EmailTemplateConstant.Constant_TaskEmailTemplateOverDue;
				else if (templateName == "TaskEmailTemplateForReminder.html")
					rowHtmlContent = EmailTemplateConstant.Constant_TaskEmailTemplateReminder;
				else if (templateName == "AuditScheduleReminderMail.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditEmailReminderTemplate;
				else if (templateName == "OpportunityEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_OpportunityEmailTemplate;
				else if (templateName == "ObservationEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_ObservationEmailTemplate;
				else if (templateName == "IncidentEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_IncidentEmailTemplate;
				else if (templateName == "AuditScheduleReminderMailToAuditorForAuditItems.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditDeleteEmailTemplate;
				else if (templateName == "AuditDelete.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditDeleteEmailTemplate;
				else if (templateName == "AuditPublish.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditPublishEmailTemplate;
				else if (templateName == "AuditApproval.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditApproveEmailTemplate;
				else if (templateName == "AuditStart.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditStartEmailTemplate;
				else if (templateName == "AuditComplete.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditEompleteEmailTemplate;
				else if (templateName == "DocumentCreate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_DocumentCreateEmailTemplate;
				else if (templateName == "AuditItemDelete.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditEompleteEmailTemplate;
				else if (templateName == "AuditItemEdit.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditEompleteEmailTemplate;
				else if (templateName == "AuditItemCreate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditEompleteEmailTemplate;
				else if (templateName == "AddAuditParticipant.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditEompleteEmailTemplate;
				else if (templateName == "AuditPlanUpdate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditEompleteEmailTemplate;
				else if (templateName == "WorkItemEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_WorkItemEmailTemplate;
				else if (templateName == "WorkItemUpdateEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_WorkItemUpdateEmailTemplate;
				else if (templateName == "WorkItemDeleteEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_WorkItemDeleteEmailTemplate;
                else if (templateName == "DocumentChangeRequest.html")
                    rowHtmlContent = EmailTemplateConstant.Constant_DocumentChangeRequestAddEmailTemplate;                
				else if (templateName == "DocumentDeleted.html")
					rowHtmlContent = EmailTemplateConstant.Constant_DocumentDeletedTemplate;
				else if (templateName == "DocumentEdit.html")
					rowHtmlContent = EmailTemplateConstant.Constant_DocumentEditTemplate;
				else if (templateName == "AuditFindingCreateEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_AuditFindingEmailTemplate;
                else if (templateName == "DocumentChangeRequestEdit.html")
                    rowHtmlContent = EmailTemplateConstant.Constant_DocumentChangeRequestEditEmailTemplate;
                else if (templateName == "DocumentChangeRequestDeleted.html")
                    rowHtmlContent = EmailTemplateConstant.Constant_DocumentChangeRequestDeletedEmailTemplate;
				else if (templateName == "MRMRescheduledEmail.html")
					rowHtmlContent = EmailTemplateConstant.Constant_MRMRescheduleEmailTemplate; 

				else if (templateName == "MRMDeleteEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_MRMDeleteEmailTemplate;

				else if (templateName == "MRMAgendaEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_MRMDeleteEmailTemplate;

				else if (templateName == "MRMParticipantAddedEmailTemplate.html") 
					rowHtmlContent = EmailTemplateConstant.Constant_MRMParticipantAddEmailTemplate;

				else if (templateName == "MinutesAddedEmailTemplate.html")
					rowHtmlContent = EmailTemplateConstant.Constant_MRMMinutesAddedEmailTemplate;
                else if (templateName == "SurveyEmailTemplate.html")
                    rowHtmlContent = EmailTemplateConstant.Constant_SurveyEmailTemplate;
                else if (templateName == "MinutesAddedEmailTemplate.html")
                    rowHtmlContent = EmailTemplateConstant.Constant_MRMMinutesAddedEmailTemplate;
                else if (templateName == "SurveyEmailTemplateResponsible.html")
                    rowHtmlContent = EmailTemplateConstant.Constant_SurveyEmailTemplateResponsile;
                else rowHtmlContent = templateName == "ManagementReviewScheduleMailReminder.html"
                    ? EmailTemplateConstant.Constant_ManagementReviewScheduleMailReminder
                    : $"Unable to collect file {filePath} and no static template found";
            }

			return rowHtmlContent;
		}
	}
}