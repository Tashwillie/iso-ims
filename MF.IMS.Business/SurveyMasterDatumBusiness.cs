using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.DomainModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Business.Service;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;
using NUnit.Framework.Internal.Execution;
using Stripe;
using System;

namespace Mindflur.IMS.Business
{
    public class SurveyMasterDatumBusiness : ISurveyMasterDatumBusiness
    {
        private readonly ISurveyMasterDatumRepository _surveyMasterDatumRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ISurveyResponseRepsoitory _surveyResponseRepsoitory;
        private readonly ISurveyQuestionAnswerRepository _surveyQuestionAnswerRepository;
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IMessageService _messageService;
        private readonly IUserRepository _userRepository;
        private readonly ICommentBusiness _commentBusiness;
        private readonly ICommentRepository _commentRepository;
        private readonly IEmailService _emailService;
        public SurveyMasterDatumBusiness(ISurveyMasterDatumRepository surveyMasterDatumRepository, ISurveyQuestionRepository surveyQuestionRepository, ISurveyResponseRepsoitory surveyResponseRepsoitory, ISurveyQuestionAnswerRepository surveyQuestionAnswerRepository, IActivityLogRepository activityLogRepository, IMessageService messageService, IUserRepository userRepository, ICommentBusiness commentBusiness, ICommentRepository commentRepository, IEmailService emailService)
        {
            _surveyMasterDatumRepository = surveyMasterDatumRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _surveyResponseRepsoitory = surveyResponseRepsoitory;
            _surveyQuestionAnswerRepository = surveyQuestionAnswerRepository;
            _activityLogRepository = activityLogRepository;
            _messageService = messageService;
            _userRepository = userRepository;
            _commentBusiness = commentBusiness;
            _commentRepository = commentRepository;
            _emailService = emailService;
        }
        public async Task<PaginatedItems<SurveyMasterDataGridView>> getAllSurveyData(GetSurveyListRequest getListRequest)
        {
            return await _surveyMasterDatumRepository.getAllSurveyData(getListRequest);

        }
        public async Task<SurveyDataPreview> GetSurveyDataPreviewById(int tenantId, int Id)
        {
         var data = await _surveyMasterDatumRepository.GetSurveyDataPreviewById(tenantId, Id);
            if (data == null)
            {
                data = new SurveyDataPreview();
                return data;
            }
            var Preview = new SurveyDataPreview();
            Preview.SurveyId = data.SurveyId;
            Preview.Title = data.Title;
            Preview.Description = data.Description;
            Preview.StartDate = data.StartDate;
            Preview.EndTime = data.EndTime;
            Preview.MasterDataSurveyStatusId = data.MasterDataSurveyStatusId;
            Preview.MasterDataSurveyStatus = data.MasterDataSurveyStatus;
            Preview.AssignedPersonId = data.AssignedPersonId;
            Preview.AssignedPerson = data.AssignedPerson;
            Preview.ResponsiblePersonId = data.ResponsiblePersonId;
            Preview.ResponsiblePerson = data.ResponsiblePerson;
			var phaseComment = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.SurevyManagement, Id);

			IList<CommentsView> comments = new List<CommentsView>();
			foreach (var comment in phaseComment)
			{
				comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
			}
			Preview.Comments = comments;

			return Preview;





		}
        public async Task AddSurveyDatas(SurveyMasterPostView surveyMasterDatum, int UserId, int tenantId)
        {
            SurveyMasterDatum masterDatum = new SurveyMasterDatum();
            masterDatum.TenantId = tenantId;
            masterDatum.Title = surveyMasterDatum.Title;
            masterDatum.Description = surveyMasterDatum.Description;
            masterDatum.StartDate = surveyMasterDatum.StartDate;
            masterDatum.EndTime = surveyMasterDatum.EndTime;
            masterDatum.MasterDataSurveyStatusId = (int)IMSItemStatus.Open;
            masterDatum.CreatedBy = UserId;
            masterDatum.CreatedOn = DateTime.UtcNow;
            masterDatum.AssignedToUserId = surveyMasterDatum.AssignedToUserId;
            masterDatum.ResponsiblePersonId = surveyMasterDatum.ResponsiblePersonId;
            await _surveyMasterDatumRepository.AddAsync(masterDatum);


            var usersListByTenantId = await _userRepository.GetAllUsers(tenantId);
            var assigneduserdetails = usersListByTenantId.Where(t => t.UserId == surveyMasterDatum.AssignedToUserId).FirstOrDefault();
            var responsibleUserDetails = usersListByTenantId.Where(t => t.UserId == surveyMasterDatum.ResponsiblePersonId).FirstOrDefault();
            var keyValuePairs = new Dictionary<string, string>
            {
                {"#Survey_Id#" , masterDatum.SurveyId.ToString() },
                {"#TITLE#", masterDatum.Title },
                {"#DESCRIPTION#", masterDatum.Description },
                {"#SURVEYDATE#", $"{ masterDatum.StartDate  } to {masterDatum.EndTime} " }
            };
            keyValuePairs["#USER_NAME#"] = assigneduserdetails.FullName;
            await _emailService.SendEmail(assigneduserdetails.EmailAddress, assigneduserdetails.FullName, "SurveyEmailTemplate.html", $"Survey Created > {masterDatum.SurveyId} - ", keyValuePairs);
            keyValuePairs["#USER_NAME#"] = responsibleUserDetails.FullName;
            await _emailService.SendEmail(responsibleUserDetails.EmailAddress, responsibleUserDetails.FullName, "SurveyEmailTemplateResponsible.html", $"Survey Created > {masterDatum.SurveyId} - ", keyValuePairs);

            ActivityLog activityLog = new ActivityLog();
            activityLog.TenantId = masterDatum.TenantId;
            activityLog.ControllerId = (int)IMSControllerCategory.SurveyManagement;
            activityLog.EntityId = masterDatum.SurveyId;
            activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
            activityLog.Description = "Survey Has been Created";
            activityLog.Details = System.Text.Json.JsonSerializer.Serialize(masterDatum);
            activityLog.Status = true;
            activityLog.CreatedBy = UserId;
            activityLog.CreatedOn = DateTime.UtcNow;
            await _activityLogRepository.AddAsync(activityLog);
            var userDetails = await _userRepository.GetUserDetail(UserId, tenantId);
            await _messageService.SendNotificationMessage(new Application.ViewModel.Core.NotificationMessage()
            {
                SourceIdUserId = UserId,
                SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                BroadcastLevel = NotificationBroadcastLevel.Global,
                EventType = NotificationEventType.BusinessMaster,
                TenantId = tenantId,
                Action = IMSControllerActionCategory.Create,
                Module = IMSControllerCategory.SurveyManagement,
                ItemId = masterDatum.SurveyId,
                Description = masterDatum?.Description,
                Title = masterDatum?.Title,
                Date = masterDatum.CreatedOn
            });


        }
        public async Task UpdateSurveyDatas(SurveyMasterDataUpdateView masterDatum, int id, int UserId, int tenantId)
        {
            var surveyData = await _surveyMasterDatumRepository.GetByIdAsync(id);
            if (surveyData.SurveyId == id && surveyData.TenantId == tenantId)
            {
                surveyData.Title = masterDatum.Title;
                surveyData.Description = masterDatum.Description;
                surveyData.StartDate = masterDatum.StartDate;
                surveyData.EndTime = masterDatum.EndTime;
                surveyData.UpdatedBy = UserId;
                surveyData.UpdatedIon = DateTime.UtcNow;
                surveyData.MasterDataSurveyStatusId = masterDatum.MasterDataSurveyStatusId;
                surveyData.AssignedToUserId = masterDatum.AssignedToUserId;
                surveyData.ResponsiblePersonId = masterDatum.ResponsiblePersonId;
                await _surveyMasterDatumRepository.UpdateAsync(surveyData);


                var usersListByTenantId = await _userRepository.GetAllUsers(tenantId);
                var assigneduserdetails = usersListByTenantId.Where(t => t.UserId == masterDatum.AssignedToUserId).FirstOrDefault();
                var responsibleUserDetails = usersListByTenantId.Where(t => t.UserId == masterDatum.ResponsiblePersonId).FirstOrDefault();
                var keyValuePairs = new Dictionary<string, string>
            {
                {"#Survey_Id#" , surveyData.SurveyId.ToString() },
                {"#TITLE#", masterDatum.Title },
                {"#DESCRIPTION#", masterDatum.Description },
                {"#SURVEYDATE#", $"{ masterDatum.StartDate  } to {masterDatum.EndTime} " }
            };
                keyValuePairs["#USER_NAME#"] = assigneduserdetails.FullName;
                await _emailService.SendEmail(assigneduserdetails.EmailAddress, assigneduserdetails.FullName, "SurveyEmailTemplate.html", $"Survey Created > {surveyData.SurveyId} - ", keyValuePairs);
                keyValuePairs["#USER_NAME#"] = responsibleUserDetails.FullName;
                await _emailService.SendEmail(responsibleUserDetails.EmailAddress, responsibleUserDetails.FullName, "SurveyEmailTemplateResponsible.html", $"Survey Created > {surveyData.SurveyId} - ", keyValuePairs);

                ActivityLog activityLog = new ActivityLog();
                activityLog.TenantId = surveyData.TenantId;
                activityLog.ControllerId = (int)IMSControllerCategory.SurveyManagement;
                activityLog.EntityId = surveyData.SurveyId;
                activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
                activityLog.Description = "Survey Has been Created";
                activityLog.Details = System.Text.Json.JsonSerializer.Serialize(surveyData);
                activityLog.Status = true;
                activityLog.CreatedBy = UserId;
                activityLog.CreatedOn = DateTime.UtcNow;
                await _activityLogRepository.AddAsync(activityLog);
                var userDetails = await _userRepository.GetUserDetail(UserId, tenantId);
                await _messageService.SendNotificationMessage(new Application.ViewModel.Core.NotificationMessage()
                {
                    SourceIdUserId = UserId,
                    SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                    BroadcastLevel = NotificationBroadcastLevel.Global,
                    EventType = NotificationEventType.BusinessMaster,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Edit,
                    Module = IMSControllerCategory.SurveyManagement,
                    ItemId = surveyData.SurveyId,
                    Description = surveyData?.Description,
                    Title = surveyData?.Title,
                    Date = surveyData.UpdatedIon
                });
            }
            else
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), id);


            }

        }
        public async Task DeleteSurveyData(int id, int userId, int tenantId)
        {
            var surveyData = await _surveyMasterDatumRepository.GetByIdAsync(id);
            if(surveyData == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), id);
            }
            else
            {
                if (surveyData.ApprovedOn != null)
                {
                    throw new Exception(string.Format(ConstantsBusiness.SurveyErrorMessage));
                }

                else if (surveyData.SurveyId == id && surveyData.TenantId == tenantId)
                {
                    await _surveyMasterDatumRepository.DeleteAsync(surveyData);
                    ActivityLog activityLog = new ActivityLog();
                    activityLog.TenantId = surveyData.TenantId;
                    activityLog.ControllerId = (int)IMSControllerCategory.SurveyManagement;
                    activityLog.EntityId = surveyData.SurveyId;
                    activityLog.ModuleAction = (int)IMSControllerActionCategory.Delete;
                    activityLog.Description = "Survey Has been Deleted";
                    activityLog.Details = System.Text.Json.JsonSerializer.Serialize(surveyData);
                    activityLog.Status = true;
                    activityLog.CreatedBy = userId;
                    activityLog.CreatedOn = DateTime.UtcNow;
                    await _activityLogRepository.AddAsync(activityLog);
                }
            }
            


        }
        public async Task<SurveyMasterDatum> GetSurveyDataById(int id, int tenantId)
        {
            var surveyData = await _surveyMasterDatumRepository.GetByIdAsync(id);
            return surveyData.SurveyId == id && surveyData.TenantId == tenantId ? surveyData : throw new NotFoundException("Id Not found", id);
        }
        public async Task<PaginatedItems<QuestionDetailViewBySurveyId>> getQuestionBySurveyId(GetSurveyListRequest getListrequest, int id)
        {
            return await _surveyQuestionRepository.getQuestionBySurveyId(getListrequest, id);
        }
        public async Task<PaginatedItems<GetSurveyResponseViewBySurveyID>> getSurveyResponseBySurveyId(GetSurveyListRequest getListrequest, int id)
        {
            return await _surveyResponseRepsoitory.getSurveyResponseBySurveyId(getListrequest, id);
        }



        public async Task<IList<ResponseView>> getResponseBySurveyId(int surveyId, int tenantId)
        {
            IList<ResponseView> responseViews = new List<ResponseView>();
            var responseItems = await _surveyQuestionAnswerRepository.getSurveyResponseAsync(surveyId, tenantId);
            var questions = responseItems.DistinctBy(question => question.QuestionId);
            foreach (var question in questions)
            {
                var responseForQuestion = responseItems.Where(items => items.QuestionId == question.QuestionId).OrderBy(items => items.QuestionId);
                var reponseForItems = new List<string>();
                foreach (var responseItemForQuestion in responseForQuestion)
                {

                    reponseForItems.Add(responseItemForQuestion.Options);
                }

                var reponsecount = new List<int>();
                foreach (var responseCountForQuestion in responseForQuestion)
                {

                    reponsecount.Add(responseCountForQuestion.Response);

                }
                var reponseView = new ResponseView
                {
                    QuestionId = question.QuestionId,
                    QuestionName = question.QuestionText,
                    Options = reponseForItems,
                    ResponseCount = reponsecount

                };
                responseViews.Add(reponseView);
            }
            return responseViews;

        }

        public async Task SubmitSurveyMaster(ApproveSurveyMaster comment,int tenantId, int SurveyId, int userId)
        {
            var surveyMetadata = await _surveyMasterDatumRepository.GetByIdAsync(SurveyId);

            if (surveyMetadata != null)
            {
                surveyMetadata.UpdatedIon = DateTime.UtcNow;
                surveyMetadata.UpdatedBy = userId;
                surveyMetadata.MasterDataSurveyStatusId = (int)IMSItemStatus.InReview;
                surveyMetadata.OverAllComments = comment.Comments;

                await _surveyMasterDatumRepository.UpdateAsync(surveyMetadata);

                var usersListByTenantId = await _userRepository.GetAllUsers(tenantId);
               
                var responsibleUserDetails = usersListByTenantId.Where(t => t.UserId == surveyMetadata.ResponsiblePersonId).FirstOrDefault();
                var keyValuePairs = new Dictionary<string, string>
            {
                {"#Survey_Id#" , surveyMetadata.SurveyId.ToString() },
                {"#TITLE#", surveyMetadata.Title },
                {"#DESCRIPTION#", surveyMetadata.Description },
                {"#SURVEYDATE#", $"{ surveyMetadata.StartDate  } to {surveyMetadata.EndTime} " }
            };
               
                keyValuePairs["#USER_NAME#"] = responsibleUserDetails.FullName;
                await _emailService.SendEmail(responsibleUserDetails.EmailAddress, responsibleUserDetails.FullName, "SurveyEmailTemplateResponsible.html", $"Survey Submitted > {surveyMetadata.SurveyId} - ", keyValuePairs);


                var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                    BroadcastLevel = NotificationBroadcastLevel.Tenant,
                    EventType = NotificationEventType.BusinessMaster,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Edit,
                    Module = IMSControllerCategory.SurveyManagement,
                    ItemId = SurveyId,
                    Description = surveyMetadata.Description,
                    Title = surveyMetadata.Title,
                    Date = surveyMetadata.UpdatedIon
                }); ;
            }
            else
            {

                throw new NotFoundException("SurveyId", SurveyId);

            }
           

        }

        public async Task ApproveSurveyMaster(ApproveSurveyMaster comment, int tenantId, int SurveyId, int userId)
        {
            var surveyMetadata = await _surveyMasterDatumRepository.GetByIdAsync(SurveyId);

            if (surveyMetadata.MasterDataSurveyStatusId == (int)IMSItemStatus.InReview)
            {
                surveyMetadata.ApprovedOn = DateTime.UtcNow;
                surveyMetadata.ApprovedBy = userId;
                surveyMetadata.MasterDataSurveyStatusId = (int)IMSItemStatus.Closed;

                await _surveyMasterDatumRepository.UpdateAsync(surveyMetadata);
                var usersListByTenantId = await _userRepository.GetAllUsers(tenantId);
                var assigneduserdetails = usersListByTenantId.Where(t => t.UserId == surveyMetadata.AssignedToUserId).FirstOrDefault();
               
                var keyValuePairs = new Dictionary<string, string>
            {
                {"#Survey_Id#" , surveyMetadata.SurveyId.ToString() },
                {"#TITLE#", surveyMetadata.Title },
                {"#DESCRIPTION#", surveyMetadata.Description },
                {"#SURVEYDATE#", $"{ surveyMetadata.StartDate  } to {surveyMetadata.EndTime} " }
            };
                keyValuePairs["#USER_NAME#"] = assigneduserdetails.FullName;
                await _emailService.SendEmail(assigneduserdetails.EmailAddress, assigneduserdetails.FullName, "SurveyEmailTemplate.html", $"Survey Approved > {surveyMetadata.SurveyId} - ", keyValuePairs);
               


                var postCommentView = new PostCommentView();
                postCommentView.SourceId = (int)IMSModules.SurevyManagement;
                postCommentView.SourceItemId = SurveyId;
                postCommentView.ParentCommentId = 0;
                postCommentView.ContentType = 1;
                postCommentView.CommentContent = comment.Comments;
                await _commentBusiness.AddComment(postCommentView, userId, tenantId);
            }
            else
            {

                throw new Exception("Cannot approve Survey As it is not submitted");

            }
            var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = userId,
                SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                BroadcastLevel = NotificationBroadcastLevel.Tenant,
                EventType = NotificationEventType.BusinessMaster,
                TenantId = tenantId,
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.SurveyManagement,
                ItemId = SurveyId,
                Description = surveyMetadata.Description,
                Title = surveyMetadata.Title,
                Date = surveyMetadata.UpdatedIon
            }); ;

        }

        public async Task RejectSurveyMaster(ApproveSurveyMaster comment, int tenantId, int SurveyId, int userId)
        {
            var surveyMetadata = await _surveyMasterDatumRepository.GetByIdAsync(SurveyId);

            if (surveyMetadata.MasterDataSurveyStatusId == (int)IMSItemStatus.InReview)
            {
                surveyMetadata.UpdatedIon = DateTime.UtcNow;
                surveyMetadata.UpdatedBy = userId;
                surveyMetadata.MasterDataSurveyStatusId = (int)IMSItemStatus.Rejected;
                await _surveyMasterDatumRepository.UpdateAsync(surveyMetadata);
                var postCommentView = new PostCommentView();
                postCommentView.SourceId = (int)IMSModules.SurevyManagement;
                postCommentView.SourceItemId = SurveyId;
                postCommentView.ParentCommentId = 0;
                postCommentView.ContentType = 1;
                postCommentView.CommentContent = comment.Comments;
                await _commentBusiness.AddComment(postCommentView, userId, tenantId);

                var usersListByTenantId = await _userRepository.GetAllUsers(tenantId);
                var assigneduserdetails = usersListByTenantId.Where(t => t.UserId == surveyMetadata.AssignedToUserId).FirstOrDefault();

                var keyValuePairs = new Dictionary<string, string>
            {
                {"#Survey_Id#" , surveyMetadata.SurveyId.ToString() },
                {"#TITLE#", surveyMetadata.Title },
                {"#DESCRIPTION#", surveyMetadata.Description },
                {"#SURVEYDATE#", $"{ surveyMetadata.StartDate  } to {surveyMetadata.EndTime} " }
            };
                keyValuePairs["#USER_NAME#"] = assigneduserdetails.FullName;
                await _emailService.SendEmail(assigneduserdetails.EmailAddress, assigneduserdetails.FullName, "SurveyEmailTemplate.html", $"Survey Approved > {surveyMetadata.SurveyId} - ", keyValuePairs);
            }
            else
            {

                throw new Exception("Cannot reject Survey As it is not submitted");

            }
            var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = userId,
                SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                BroadcastLevel = NotificationBroadcastLevel.Tenant,
                EventType = NotificationEventType.BusinessMaster,
                TenantId = tenantId,
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.SurveyManagement,
                ItemId = SurveyId,
                Description = surveyMetadata.Description,
                Title = surveyMetadata.Title,
                Date = surveyMetadata.UpdatedIon
            }); ;

        }
       /* public async Task<SupplierSurveyReportDetails> GetSupplierReportDetails(int SurveyId, int tenantId)
        {
            return await _surveyMasterDatumRepository.GetSupplierReportDetails(SurveyId, tenantId);
        }*/
    }
   
 }


