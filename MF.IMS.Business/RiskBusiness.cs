using DocumentFormat.OpenXml.Spreadsheet;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.DomainModel;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Repository;
using Comment = Mindflur.IMS.Data.Models.Comment;

namespace Mindflur.IMS.Business
{
    public class RiskBusiness : IRiskBusiness
    {
        private readonly IRiskRepository _riskRepository;
        private readonly IRiskTreatmentRepository _riskTreatmentRepository;
        private readonly IMessageService _messageService;
        private readonly ICommentBusiness _commentBusiness;
        private readonly ICommentRepository _commentRepository;
        private readonly IWorkItemRepository _workItemRepository;
        private readonly IUserRepository _userRepository;

        public RiskBusiness(IRiskTreatmentRepository riskTreatmentRepository,
            IMessageService messageService, ICommentBusiness commentBusiness, ICommentRepository commentRepository, IRiskRepository riskRepository, IWorkItemRepository workItemRepository, IUserRepository userRepository)
        {
            _riskTreatmentRepository = riskTreatmentRepository;
            _messageService = messageService;
            _commentRepository = commentRepository;
            _riskRepository = riskRepository;
            _workItemRepository = workItemRepository;
            _commentBusiness = commentBusiness;
            _userRepository = userRepository;
        }

        public async Task ReviewRisk(CommentsForReviewViewModel comment, int tenantId, int riskId, int userId)
        {
            var risk = await _riskRepository.GetByIdAsync(riskId);
            var treatement = await _riskTreatmentRepository.GetRiskTreatmentForInherant(riskId);
            if (risk.Id == riskId)
            {
                if (treatement.Any())
                {
                    await _riskRepository.UpdateAsync(risk);

                    var comments = new Comment();
                    comments.CommentContent = comment.Comments;
                    comments.SourceId = (int)IMSModules.RiskManagement;
                    comments.SourceItemId = riskId;
                    comments.ParentCommentId = comments.CommentId;
                    comments.ContentType = 1;
                    comments.CreatedBy = userId;
                    comments.CreatedOn = DateTime.UtcNow;
                    await _commentRepository.AddAsync(comments);
                }
                else
                {
                    throw new BadRequestException(string.Format(ConstantsBusiness.NoTreatmentForRiskErrorMessage));
                }
            }
            else
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.RiskNotFoundErrorMessage), riskId);
            }
        }

        public async Task UpdateRiskMetaData(PutRiskMetadataViewModel riskView, int workItemId, int userId, int tenantId)
        {
            await _riskRepository.UpdateRiskMetaData(riskView, workItemId, userId, tenantId);
            var rawData = await _workItemRepository.GetByIdAsync(workItemId);

            var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = userId,
                SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                BroadcastLevel = NotificationBroadcastLevel.User,
                EventType = NotificationEventType.BusinessMaster,
                TenantId = tenantId,
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.RiskManagement,
                ItemId = workItemId,
                Description = rawData.Description,
                Title = rawData.Title,
                Date = DateTime.UtcNow
            });
        }

        public async Task<GetRiskMetadataViewModel2> GetRiskMetaData(int tenantId, int workItemId)
        {
            var rawData = await _riskRepository.GetRiskMetaData(tenantId, workItemId);
            if (rawData == null)
            {
                rawData = new GetRiskMetadataViewModel2();
                return rawData;
            }
            var preview = new GetRiskMetadataViewModel2();
            preview.WorkItemId = rawData.WorkItemId;
            preview.CurrentControls = rawData.CurrentControls;
            preview.Id = rawData.Id;
            preview.TotalRiskScore = rawData.TotalRiskScore;
            preview.AssignedToId = rawData.AssignedToId;
            preview.AssignedTo = rawData.AssignedTo;
            preview.ApprovedById= rawData.ApprovedById;
            preview.ApprovedBy = rawData.ApprovedBy;
            preview.IsApproved = rawData.IsApproved;
            preview.AppovedOn = rawData.AppovedOn;
            preview.ReviewedById= rawData.ReviewedById;
            preview.ReviewedBy= rawData.ReviewedBy;
            preview.ReviewedOn= rawData.ReviewedOn;
            preview.UpdatedById= rawData.UpdatedById;
            preview.UpdatedBy= rawData.UpdatedBy;
            preview.UpdatedOn= rawData.UpdatedOn;
            var workItemTokens = await _workItemRepository.GetAllTokens(workItemId);
            var ncComment = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.RiskManagement, workItemId);

            IList<CommentsView> comments = new List<CommentsView>();
            foreach (var comment in ncComment)
            {
                comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
            }
            preview.Comments = comments;

            IList<TokensView> getTokens = new List<TokensView>();
            foreach (var token in workItemTokens)
            {
                getTokens.Add(new TokensView() { TokenId = token.TokenId, Token  = token.TokenName, ParentTokenId = token.ParentTokenId, ParentTokenName = token.ParentTokenName });
            }
            preview.tokens = getTokens;
            return preview;
        }


        public async Task ReviewMetadata(ReviewRiskMetadata reviewRiskMetadata, int tenantId, int workItemId, int userId)
        {
            var riskMetadata = await _riskRepository.GetRiskByWorkItemId(workItemId);
            var workitem = await _workItemRepository.GetByIdAsync(workItemId);
            if (riskMetadata != null)
            {
                riskMetadata.ReviewedBy = userId;
                riskMetadata.ReviewedOn = DateTime.UtcNow;
                await _riskRepository.UpdateAsync(riskMetadata);
                workitem.StatusMasterDataId = (int)IMSItemStatus.InReview;
                await _workItemRepository.UpdateAsync(workitem);
                var postCommentView = new PostCommentView();
                postCommentView.SourceId = (int)IMSModules.RiskManagement;
                postCommentView.SourceItemId = workItemId;
                postCommentView.ParentCommentId = 0;
                postCommentView.ContentType = 1;
                postCommentView.CommentContent = reviewRiskMetadata.Comments;

                await _commentBusiness.AddComment(postCommentView, userId, tenantId);
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
                Module = IMSControllerCategory.RiskManagement,
                ItemId = workItemId,
                Description = workitem.Description,
                Title = workitem.Title,
                Date = riskMetadata.ReviewedOn
            });
        }

        public async Task RejectMetadata(RejectRiskMetadata rejectRiskMetadata, int tenantId, int workItemId, int userId)
        {
            var riskMetadata = await _riskRepository.GetRiskByWorkItemId(workItemId);
            var workitem = await _workItemRepository.GetByIdAsync(workItemId);
            if (riskMetadata != null)
            {
                riskMetadata.IsApproved = false;
                riskMetadata.UpdatedOn = DateTime.UtcNow;
                riskMetadata.UpdatedBy = userId;
                await _riskRepository.UpdateAsync(riskMetadata);
                workitem.StatusMasterDataId = (int)IMSItemStatus.Rejected;
                await _workItemRepository.UpdateAsync(workitem);
                var postCommentView = new PostCommentView();
                postCommentView.SourceId = (int)IMSModules.RiskManagement;
                postCommentView.SourceItemId = workItemId;
                postCommentView.ParentCommentId = 0;
                postCommentView.ContentType = 1;
                postCommentView.CommentContent = rejectRiskMetadata.Comments;

                await _commentBusiness.AddComment(postCommentView, userId, tenantId);
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
                Module = IMSControllerCategory.RiskManagement,
                ItemId = workItemId,
                Description = workitem.Description,
                Title = workitem.Title,
                Date = riskMetadata.UpdatedOn
            });
        }

        public async Task CloseMetadata(CloseRiskMetaData closeRiskMetaData, int tenantId, int workItemId, int userId)
        {
            var riskMetadata = await _riskRepository.GetRiskByWorkItemId(workItemId);
            var workitem = await _workItemRepository.GetByIdAsync(workItemId);
            if (workitem.StatusMasterDataId == (int)IMSItemStatus.InReview)
            {

                riskMetadata.UpdatedBy = userId;
                riskMetadata.UpdatedOn = DateTime.UtcNow;
                await _riskRepository.UpdateAsync(riskMetadata);
                workitem.StatusMasterDataId = (int)IMSItemStatus.Closed; ;
                await _workItemRepository.UpdateAsync(workitem);
                var postCommentView = new PostCommentView();
                postCommentView.SourceId = (int)IMSModules.RiskManagement;
                postCommentView.SourceItemId = workItemId;
                postCommentView.ParentCommentId = 0;
                postCommentView.ContentType = 1;
                postCommentView.CommentContent = closeRiskMetaData.Comments;
                await _commentBusiness.AddComment(postCommentView, userId, tenantId);
             

            }
            else
            {
                   throw new BadRequestException("Risk is Not InReview Yet");
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
                Module = IMSControllerCategory.RiskManagement,
                ItemId = workItemId,
                Description = workitem.Description,
                Title = workitem.Title,
                Date = riskMetadata.UpdatedOn
            });
        }

		public async Task SubmitTreatmentMetadata(SubmitRiskMetaData submitRiskMetaData, int tenantId, int workItemId, int userId)
		{
			var riskTreatments = await _riskRepository.GetAallRiskTreament(tenantId, workItemId);
			var risks = await _workItemRepository.GetByIdAsync(workItemId);
			var rawData = true;
			foreach (var data in riskTreatments)
			{
				if (data.StatusId == (int)IMSItemStatus.Closed)
				{
					rawData = true;
				}
				else
				{
					throw new BadRequestException("Risk Treatments is Not Closed Yet");
				}
			}
			if (rawData == true)
			{
				risks.StatusMasterDataId = (int)IMSItemStatus.InReview;
				await _workItemRepository.UpdateAsync(risks);
			}
			var postCommentView = new PostCommentView();
			postCommentView.SourceId = (int)IMSModules.RiskManagement;
			postCommentView.SourceItemId = workItemId;
			postCommentView.ParentCommentId = 0;
			postCommentView.ContentType = 1;
			postCommentView.CommentContent = submitRiskMetaData.Comments;
			await _commentBusiness.AddComment(postCommentView, userId, tenantId);

		}
	}
      
}