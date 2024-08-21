using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;

namespace Mindflur.IMS.Business
{
	public class NonConformanceBusiness : INonConformanceBusiness
	{
		private readonly IChartBusiness _chartBusiness;

		private readonly IMessageService _messageService;
		private readonly ICommentRepository _commentRepository;
		private readonly INonConformanceMetaDataRepository _nonConformanceMetaDataRepository;
		private readonly ICommentBusiness _commentBusiness;
		private readonly IWorkItemRepository _workItemRepository;
		private readonly IUserRepository _userRepository;

		public NonConformanceBusiness(IChartBusiness chartBusiness, ICommentBusiness commentBusiness, IMessageService messageService, ICommentRepository commentRepository, INonConformanceMetaDataRepository nonConformanceMetaDataRepository, IWorkItemRepository workItemRepository, IUserRepository userRepository)
		{
			_chartBusiness = chartBusiness;
			_messageService = messageService;
			_commentRepository = commentRepository;
			_nonConformanceMetaDataRepository = nonConformanceMetaDataRepository;
			_commentBusiness = commentBusiness;
			_workItemRepository = workItemRepository;
			_userRepository = userRepository;
		}

		public async Task<List<NonConformanceListByMeetingId>> GetAllNcByMeetingId(int meetingId)
		{
			return await _nonConformanceMetaDataRepository.GetAllNcByMeetingId(meetingId);
		}

		public async Task<List<ObservationOpportunitiesListByMeetingId>> GetAllObservationsByMeetingId(int meetingId)
		{
			return await _nonConformanceMetaDataRepository.GetAllObservationsByMeetingId(meetingId);
		}

		public async Task<DonutsChartView> GetChartNonConformanceDonuts(int categoryId, int tenantId)
		{
			return await _chartBusiness.GetChartNonConformanceDonuts(categoryId, tenantId);
		}

		public async Task StartNonconformance(int userId, int nonConformanceId, int tenantId)
		{
			var nc = await _workItemRepository.GetByIdAsync(nonConformanceId);
			if (nc.StatusMasterDataId != (int)IMSItemStatus.Assigned)
			{
				throw new BadRequestException("Nc is Not Assigned Yet");
			}
			else
			{
				nc.StatusMasterDataId = (int)IMSItemStatus.Open;
				nc.UpdatedBy = userId;
				nc.UpdatedOn = DateTime.UtcNow;
				await _workItemRepository.UpdateAsync(nc);
			}

			var userDetail = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetail.FirstName} {userDetail.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Start,
				Module = IMSControllerCategory.NonConformance,
				ItemId = nonConformanceId,
				Description = nc.Description,
				Title = nc.Title,
				Date = nc.UpdatedOn
			});
		}

		public async Task UpsertMetadata(PutNonConformanceMetadataViewModel nonconformanceMetadataViewModel, int workItemId, int userId, int tenantId)
		{
			await _nonConformanceMetaDataRepository.UpdateNcMetaData(nonconformanceMetadataViewModel, workItemId, userId, tenantId);
		}

		public async Task<GetNonConformanceMetadataView> GetMetadata(int workItemId, int tenantId)
		{
			var ncMetaData = await _nonConformanceMetaDataRepository.GetMetadata(workItemId, tenantId);
			if (ncMetaData == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.NonConformityNotFoundErrorMessage), workItemId);
			}
			else
			{
				GetNonConformanceMetadataView preview = new GetNonConformanceMetadataView();

				preview.StartDate = ncMetaData.StartDate;
				preview.ImmediateAction = ncMetaData.ImmediateAction;
				preview.Documents = ncMetaData.Documents;
				preview.NcType = ncMetaData.NcType;
				preview.NcTypeId = ncMetaData.NcTypeId;
				preview.NcCategory = ncMetaData.NcCategory;
				preview.NcCategoryId = ncMetaData.NcCategoryId;
				preview.IsApproved = ncMetaData.IsApproved;
				preview.ReviewedById = ncMetaData.ReviewedById;
				preview.ReviewedBy = ncMetaData.ReviewedBy;
				preview.ReviewedOn = ncMetaData.ReviewedOn;
				preview.ApprovedById = ncMetaData.ApprovedById;
				preview.ApprovedBy = ncMetaData.ApprovedBy;
				preview.AppovedOn = ncMetaData.AppovedOn;
				preview.UpdatedById = ncMetaData.UpdatedById;
				preview.UpdatedBy = ncMetaData.UpdatedBy;
				preview.UpdatedOn = ncMetaData.UpdatedOn;

				var correctiveActionComments = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.NonConformity, workItemId);

				IList<CommentsView> comments = new List<CommentsView>();
				foreach (var comment in correctiveActionComments)
				{
					comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
				}
				preview.Comments = comments;

				return preview;
			}
		}

		public async Task RejectMetadata(RejectNonconformanceMetadata rejectNonconformanceMetadata, int tenantId, int workItemId, int userId)
		{
			var ncMetadata = await _nonConformanceMetaDataRepository.getNonConformanceMetaDataByWorkItemId(workItemId);
			var nonConformity = await _workItemRepository.GetByIdAsync(workItemId);
			if (ncMetadata != null)
			{
				ncMetadata.IsApproved = false;
				ncMetadata.UpdatedOn = DateTime.UtcNow;
				ncMetadata.UpdatedBy = userId;
				await _nonConformanceMetaDataRepository.UpdateAsync(ncMetadata);
				nonConformity.StatusMasterDataId = (int)IMSItemStatus.Rejected;
				await _workItemRepository.UpdateAsync(nonConformity);
			}
			else if (nonConformity != null)
			{
				nonConformity.StatusMasterDataId = (int)IMSItemStatus.Rejected;
				await _workItemRepository.UpdateAsync(nonConformity);
			}
			else
			{
				throw new BadRequestException("Non Conformance Not found");
			}

			var postCommentView = new PostCommentView();
			postCommentView.SourceId = (int)IMSModules.NonConformity;
			postCommentView.SourceItemId = workItemId;
			postCommentView.ParentCommentId = 0;
			postCommentView.ContentType = 1;
			postCommentView.CommentContent = rejectNonconformanceMetadata.Comments;

			await _commentBusiness.AddComment(postCommentView, userId, tenantId);

			var userDetail = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetail.FirstName} {userDetail.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Reject,
				Module = IMSControllerCategory.NonConformance,
				ItemId = workItemId,
				Description = nonConformity.Description,
				Title = nonConformity.Title,
				Date = nonConformity.UpdatedOn
			});
		}

		public async Task CloseMetadata(CloseNonConformanceMetaData closeNonConformanceMetaData, int tenantId, int workItemId, int userId)
		{
			var ncMetadata = await _nonConformanceMetaDataRepository.getNonConformanceMetaDataByWorkItemId(workItemId);
			var nonConformity = await _workItemRepository.GetByIdAsync(workItemId);
			var caList = await _nonConformanceMetaDataRepository.GetCaListByNcId(tenantId, workItemId);
			var correctiveAction = caList.Where(t => t.StatusId != (int)IMSItemStatus.Closed).ToList();
			if (nonConformity.StatusMasterDataId != (int)IMSItemStatus.InReview)
			{
				throw new BadRequestException("Nc is Not InReview Yet");
			}
			else if (correctiveAction.Count != 0)
			{
				throw new BadRequestException("First Close all the Corrective Action related to NonConformance Id : " + workItemId);
			}
			else
			{
				ncMetadata.UpdatedBy = userId;
				ncMetadata.UpdatedOn = DateTime.UtcNow;
				await _nonConformanceMetaDataRepository.UpdateAsync(ncMetadata);
				nonConformity.StatusMasterDataId = (int)IMSItemStatus.Closed; ;
				await _workItemRepository.UpdateAsync(nonConformity);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.NonConformity;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = closeNonConformanceMetaData.Comments;
				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
			var rawData = await _workItemRepository.GetByIdAsync(workItemId);

			var userDetail = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetail.FirstName} {userDetail.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Close,
				Module = IMSControllerCategory.NonConformance,
				ItemId = workItemId,
				Description = rawData.Description,
				Title = rawData.Title,
				Date = rawData.UpdatedOn
			});
		}

		public async Task ReviewMetadata(ReviewNonconformanceMetadata reviewNonconformanceMetadata, int tenantId, int workItemId, int userId)
		{
			var ncMetadata = await _nonConformanceMetaDataRepository.getNonConformanceMetaDataByWorkItemId(workItemId);
			var nonConformity = await _workItemRepository.GetByIdAsync(workItemId);
			if (ncMetadata != null)
			{
				ncMetadata.ReviewedBy = userId;
				ncMetadata.ReviewedOn = DateTime.UtcNow;
				await _nonConformanceMetaDataRepository.UpdateAsync(ncMetadata);
				nonConformity.StatusMasterDataId = (int)IMSItemStatus.InReview;
				await _workItemRepository.UpdateAsync(nonConformity);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.NonConformity;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = reviewNonconformanceMetadata.Comments;

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
			else
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.NonConformityNotFoundErrorMessage), workItemId);
			}
			var rawData = await _workItemRepository.GetByIdAsync(workItemId);
			var userDetails = await _workItemRepository.GetPreviewWorkItemById(workItemId, tenantId);
			var userDetail = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetail.FirstName} {userDetail.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.NonConformance,
				ItemId = workItemId,
				Description = rawData.Description,
				Title = rawData.Title,
				Date = rawData.UpdatedOn
			});
		}

		public async Task<GetNonConformanceMetadataViewWithTokens> GetNonConformanceMetadata(int workItemId, int tenantId)
		{
			var ncMetaData = await _nonConformanceMetaDataRepository.GetNonConformanceMetadata(workItemId, tenantId);
			if (ncMetaData == null)
			{
				ncMetaData = new GetNonConformanceMetadataViewWithTokens();
				return ncMetaData;
			}
			else
			{
				var preview = new GetNonConformanceMetadataViewWithTokens();
				preview.DateOfNc = ncMetaData.DateOfNc;
				preview.ImmediateAction = ncMetaData.ImmediateAction;
				preview.Documents = ncMetaData.Documents;

				preview.IsApproved = ncMetaData.IsApproved;
				preview.ReviewedById = ncMetaData.ReviewedById;
				preview.ReviewedBy = ncMetaData.ReviewedBy;
				preview.ReviewedOn = ncMetaData.ReviewedOn;
				preview.ApprovedById = ncMetaData.ApprovedById;
				preview.ApprovedBy = ncMetaData.ApprovedBy;
				preview.AssignToUserId = ncMetaData.AssignToUserId;
				preview.AssignToUser = ncMetaData.AssignToUser;
				preview.AppovedOn = ncMetaData.AppovedOn;
				preview.UpdatedById = ncMetaData.UpdatedById;
				preview.UpdatedBy = ncMetaData.UpdatedBy;
				preview.UpdatedOn = ncMetaData.UpdatedOn;
				var nCMetaDatatoken = await _workItemRepository.GetAllTokens(workItemId);
				var ncComment = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.NonConformity, workItemId);

				IList<CommentsView> comments = new List<CommentsView>();
				foreach (var comment in ncComment)
				{
					comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
				}
				preview.Comments = comments;

				IList<TokensView> getTokens = new List<TokensView>();
				foreach (var token in nCMetaDatatoken)
				{
					getTokens.Add(new TokensView() { TokenId = token.TokenId, Token = token.TokenName, ParentTokenId = token.ParentTokenId, ParentTokenName = token.ParentTokenName });
				}
				preview.Tokens = getTokens;

				return preview;
			}
		}

		public async Task<IList<SelectView>> GetNcCategoryDropdown(int tenantId)
		{
			return await _nonConformanceMetaDataRepository.GetNcCategoryDropdown(tenantId);
		}
	}
}