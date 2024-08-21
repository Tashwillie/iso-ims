using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;

namespace Mindflur.IMS.Business
{
    public class SurveySupplierMappingBusiness : ISurveySupplierMappingBusiness
    {
        private readonly ISurveySupplierMappingRepository _surveySupplierMappingRepository;
        private readonly ISurveyMasterDatumRepository _surveyMasterDatumRepository;
        private readonly IMessageService _messageService;
		private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
		public SurveySupplierMappingBusiness(ISurveySupplierMappingRepository surveySupplierMappingRepository, ISurveyMasterDatumRepository surveyMasterDatumRepository, IMessageService messageService, IUserRepository userRepository, IEmailService emailService)
		{
			_surveySupplierMappingRepository = surveySupplierMappingRepository;
			_surveyMasterDatumRepository = surveyMasterDatumRepository;
			_messageService = messageService;
			_userRepository = userRepository;
            _emailService = emailService;
		}
		public async Task AddSurveyToSupplier(int tenantId, SupplierSurveyPostView supplierSurveyPostView, int userId, int SupplierId)
        {
            SurveyMasterDatum survey = new SurveyMasterDatum();
            survey.TenantId = tenantId;
            survey.Title = supplierSurveyPostView.Title;
            survey.Description = supplierSurveyPostView.Description;
            survey.StartDate = supplierSurveyPostView.StartDate;
            survey.EndTime = supplierSurveyPostView.EndTime;
            survey.MasterDataSurveyStatusId = (int)IMSItemStatus.Open;
            survey.CreatedBy = userId;
            survey.CreatedOn = DateTime.UtcNow;
            survey.AssignedToUserId = supplierSurveyPostView.AssignedToUserId;
            survey.ResponsiblePersonId = supplierSurveyPostView.ResponsiblePersonId;
            await _surveyMasterDatumRepository.AddAsync(survey);

            var usersListByTenantId = await _userRepository.GetAllUsers(tenantId);
            var assigneduserdetails = usersListByTenantId.Where(t => t.UserId == survey.AssignedToUserId).FirstOrDefault();
            var responsibleUserDetails = usersListByTenantId.Where(t => t.UserId == survey.ResponsiblePersonId).FirstOrDefault();
            var keyValuePairs = new Dictionary<string, string>
            {
                {"#Survey_Id#" , survey.SurveyId.ToString() },
                {"#TITLE#", survey.Title },
                {"#DESCRIPTION#", survey.Description },
                {"#SURVEYDATE#", $"{ survey.StartDate  } to {survey.EndTime} " }
            };
            keyValuePairs["#USER_NAME#"] = assigneduserdetails.FullName;
            await _emailService.SendEmail(assigneduserdetails.EmailAddress, assigneduserdetails.FullName, "SurveyEmailTemplate.html", $"Survey Created > {survey.SurveyId} - ", keyValuePairs);
            keyValuePairs["#USER_NAME#"] = responsibleUserDetails.FullName;
            await _emailService.SendEmail(responsibleUserDetails.EmailAddress, responsibleUserDetails.FullName, "SurveyEmailTemplateResponsible.html", $"Survey Created > {survey.SurveyId} - ", keyValuePairs);



            SurveySupplierMapping mapping = new SurveySupplierMapping();
            mapping.SupplierMasterId = SupplierId;
            mapping.SurveyMasterId = survey.SurveyId;
            await _surveySupplierMappingRepository.AddAsync(mapping);
            var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				Action = IMSControllerActionCategory.Create,
                Module = IMSControllerCategory.SurveyManagement,
                ItemId = survey.SurveyId,
                Description = survey?.Description,
                Title = survey?.Title,
                Date = survey.CreatedOn
			});
        }
        public async Task<PaginatedItems<SurveyMasterDataGridView>> GetAllSurveyForSupplier(GetListSurveySupplier getListRequest)
        {
            return await _surveySupplierMappingRepository.GetAllSurveyForSupplier(getListRequest);
        }
		public async Task<SupplierIdFromSurveyId> getSupplierId(int surveyId, int tenantId)
		{
			return await _surveySupplierMappingRepository.getSupplierId(surveyId, tenantId);
		}

	}
}
