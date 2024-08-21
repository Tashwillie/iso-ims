using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;

namespace Mindflur.IMS.Business
{
    public class RiskTreatmentBusiness : IRiskTreatmentBusiness
    {
        private readonly IRiskTreatmentRepository _riskTreatmentRepository;
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IMessageService _messageService;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;

        public RiskTreatmentBusiness(IRiskTreatmentRepository riskTreatmentRepository, IActivityLogRepository activityLogRepository, IMessageService messageService, ICommentRepository commentRepository, IUserRepository userRepository)
        {
            _riskTreatmentRepository = riskTreatmentRepository;
            _activityLogRepository = activityLogRepository;
            _messageService = messageService;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
        }

        public async Task<PaginatedItems<RiskTreatmentView>> GetRiskTreatment(GetListRequest getRiskTreatment)
        {
            return await _riskTreatmentRepository.GetRiskTreatment(getRiskTreatment);
        }

        public async Task<RiskTreatmentPreviewView> GetRiskTreatmentPreview(int riskTreatmentId)
        {
            return await _riskTreatmentRepository.GetRiskTreatmentPreview(riskTreatmentId);
        }

        public async Task AddRiskTreatment(RiskTreatmentPostView riskTreatment, int userId, int tenantId)
        {
            var userDetails = await _userRepository.GetUserDetail(userId, tenantId);

            RiskTreatment risk = new RiskTreatment();
            risk.RiskId = riskTreatment.RiskId;
            risk.TreatmentOptionMasterDataId = riskTreatment.RiskTreatmentOption;
            risk.RiskRatingMasterDataId = riskTreatment.RiskRating;
            risk.ConsequenceMasterDataId = riskTreatment.ResidualRiskConsequence;
            risk.DueDate = riskTreatment.CompletionDate;
            risk.CurrentStatusMasterDataId = (int)IMSRiskTreatmentCurrentStatus.Unknown;
            risk.ReviewedOn = riskTreatment.LastReview;
            risk.OpportunityId = riskTreatment.Opportunities;
            risk.ProbabilityMasterDataId = riskTreatment.ResidualRiskProbability;
            risk.ResponsibleUserId = riskTreatment.ResponsiblePerson;
            risk.AssignedToUserId = riskTreatment.ResponsiblePerson;
            risk.MitigationPlan = riskTreatment.RiskTreatmentMitigationPlan;
            risk.TotalRiskScore = riskTreatment.TotalRiskScore;
            risk.CreatedOn = DateTime.UtcNow;
            risk.CreatedBy = userId;
            await _riskTreatmentRepository.AddAsync(risk);

            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = userId,
                SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                BroadcastLevel = NotificationBroadcastLevel.Global,
                EventType = NotificationEventType.BusinessMaster,
                TenantId = tenantId,
                Action = IMSControllerActionCategory.Create,
                Module = IMSControllerCategory.RiskManagementTreatment,
                ItemId = risk.Id,
                Description = risk.MitigationPlan,
                Title = "Risk Treatment",
                Date = risk.CreatedOn
            });

            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = risk.ResponsibleUserId,
                SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                BroadcastLevel = NotificationBroadcastLevel.Global,
                EventType = NotificationEventType.BusinessMaster,
                TenantId = tenantId,
                Action = IMSControllerActionCategory.Create,
                Module = IMSControllerCategory.RiskManagementTreatment,
                ItemId = risk.Id,
                Description = risk.MitigationPlan,
                Title = "Risk Treatment",
                Date = risk.CreatedOn
            });

            ActivityLog activityLog = new ActivityLog();
            activityLog.TenantId = tenantId;
            activityLog.ControllerId = (int)IMSControllerCategory.RiskManagementTreatment;
            activityLog.EntityId = risk.Id;
            activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
            activityLog.Description = "Risk Treatment Has Been created";
            activityLog.Details = System.Text.Json.JsonSerializer.Serialize(riskTreatment);
            activityLog.Status = true;
            activityLog.CreatedBy = userId;
            activityLog.CreatedOn = DateTime.UtcNow;
            await _activityLogRepository.AddAsync(activityLog);
        }

        public async Task<RiskTreatment> GetRiskTreatmentById(int riskTreatmentId)
        {
            var rawData = await _riskTreatmentRepository.GetByIdAsync(riskTreatmentId);
            return rawData == null ? throw new NotFoundException(string.Format(ConstantsBusiness.RiskTreatmentNotFoundErrorMessage), riskTreatmentId) : rawData;
        }

        public async Task UpdateRiskTraetment(RiskTreamentPutViewModel riskTreatmentPut, int riskTreatmentId, int userId, int tenantId)
        {
            var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
            var riskTreatment = await _riskTreatmentRepository.GetByIdAsync(riskTreatmentId);
            if (riskTreatment == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.RiskTreatmentNotFoundErrorMessage), riskTreatmentId);
            }
            else
            {
                riskTreatment.TreatmentOptionMasterDataId = riskTreatmentPut.RiskTreatmentOption;
                riskTreatment.MitigationPlan = riskTreatmentPut.RiskTreatmentMitigationPlan;
                riskTreatment.DueDate = riskTreatmentPut.CompletionDate;
                riskTreatment.OpportunityId = riskTreatmentPut.Opportunities;
                riskTreatment.CurrentStatusMasterDataId = riskTreatmentPut.CurrentStatus;
                riskTreatment.ConsequenceMasterDataId = riskTreatmentPut.ResidualRiskConsequence;
                riskTreatment.ReviewedOn = riskTreatmentPut.LastReview;
                riskTreatment.ProbabilityMasterDataId = riskTreatmentPut.ResidualRiskProbability;
                riskTreatment.RiskRatingMasterDataId = riskTreatmentPut.RiskRating;
                riskTreatment.ResponsibleUserId = riskTreatmentPut.ResponsiblePerson;
                riskTreatment.AssignedToUserId = riskTreatmentPut.ResponsiblePerson;
                riskTreatment.TotalRiskScore = riskTreatmentPut.TotalRiskScore;
                riskTreatment.UpdatedBy = userId;
                riskTreatment.UpdatedOn = DateTime.UtcNow;
                await _riskTreatmentRepository.UpdateAsync(riskTreatment);

                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                    BroadcastLevel = NotificationBroadcastLevel.Global,
                    EventType = NotificationEventType.BusinessMaster,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Create,
                    Module = IMSControllerCategory.RiskManagementTreatment,
                    ItemId = riskTreatment.Id,
                    Description = riskTreatment.MitigationPlan,
                    Title = "Risk Treatment",
                    Date = riskTreatment.UpdatedOn
                });
                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = riskTreatment.ResponsibleUserId,
                    SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                    BroadcastLevel = NotificationBroadcastLevel.Global,
                    EventType = NotificationEventType.BusinessMaster,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Create,
                    Module = IMSControllerCategory.RiskManagementTreatment,
                    ItemId = riskTreatment.Id,
                    Description = riskTreatment.MitigationPlan,
                    Title = "Risk Treatment",
                    Date = riskTreatment.UpdatedOn
                });

                ActivityLog activityLog = new ActivityLog();
                activityLog.TenantId = tenantId;
                activityLog.ControllerId = (int)IMSControllerCategory.RiskManagementTreatment;
                activityLog.EntityId = riskTreatment.Id;
                activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
                activityLog.Description = "Risk Treatment Has Been Updated";
                activityLog.Details = System.Text.Json.JsonSerializer.Serialize(riskTreatmentPut);
                activityLog.Status = true;
                activityLog.CreatedBy = userId;
                activityLog.CreatedOn = DateTime.UtcNow;
                await _activityLogRepository.AddAsync(activityLog);
            }
        }
        public async Task ReviewRiskTreatment(CommentsForReviewViewModel comment, int tenantId, int id, int userId)
        {
            var treatment = await _riskTreatmentRepository.GetByIdAsync(id);
            if (treatment.Id == id)
            {
                treatment.CurrentStatusMasterDataId = (int)IMSRiskTreatmentCurrentStatus.Monitoring;
                treatment.AssignedToUserId = treatment.CreatedBy;
                await _riskTreatmentRepository.UpdateAsync(treatment);

                var comments = new Comment();
                comments.CommentContent = comment.Comments;
                comments.SourceId = (int)IMSModules.RiskManagement;
                comments.SourceItemId = id;
                comments.ParentCommentId = comments.CommentId;
                comments.ContentType = 1;
                comments.CreatedBy = userId;
                comments.CreatedOn = DateTime.UtcNow;
                await _commentRepository.AddAsync(comments);
            }
            else
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.RiskTreatmentNotFoundErrorMessage), id);
            }
        }
        public async Task ApproveRiskTreatment(CommentsForReviewViewModel comment, int id, int userId)
        {
            var treatment = await _riskTreatmentRepository.GetByIdAsync(id);
            if (treatment.Id == id)
            {
                // need to talk and implement   

            }
            else
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.RiskTreatmentNotFoundErrorMessage), id);
            }
        }

        public async Task DeleteRiskTreatment(int riskTreatmentId, int userId, int tenantId)
        {
            var riskTreatment = await _riskTreatmentRepository.GetByIdAsync(riskTreatmentId);
            if (riskTreatment == null)
                throw new NotFoundException(string.Format(ConstantsBusiness.RiskTreatmentNotFoundErrorMessage), riskTreatmentId);
            await _riskTreatmentRepository.DeleteAsync(riskTreatment);

            ActivityLog activityLog = new ActivityLog();
            activityLog.TenantId = tenantId;
            activityLog.ControllerId = (int)IMSControllerCategory.RiskManagementTreatment;
            activityLog.EntityId = riskTreatment.Id;
            activityLog.ModuleAction = (int)IMSControllerActionCategory.Delete;
            activityLog.Description = "Risk Treatment Has Been Deleted";
            activityLog.Details = System.Text.Json.JsonSerializer.Serialize(riskTreatment);
            activityLog.Status = true;
            activityLog.CreatedBy = userId;
            activityLog.CreatedOn = DateTime.UtcNow;
            await _activityLogRepository.AddAsync(activityLog);
        }

        public async Task<PaginatedItems<GetRiskTreatmentViewByInherentRisk>> GetRiskTreatmentByInherentRiskId(GetCAList getTasksByProjectId)
        {
            return await _riskTreatmentRepository.GetRiskTreatmentByInherentRiskId(getTasksByProjectId);
        }
    }
}