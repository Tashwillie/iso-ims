using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
	public class CorrectiveActionBusiness : ICorrectiveActionBusiness
	{
		private readonly IMessageService _messageService;
		private readonly IWorkItemRepository _workItemRepository;
		private readonly ICommentRepository _commentRepository;
		private readonly ICommentBusiness _commentBusiness;
		private readonly ICorrectiveActionMetaDataRepository _correctiveActionMetaDataRepository;
		private readonly IUserRepository _userRepository;

		public CorrectiveActionBusiness(
			IMessageService messageService, ICommentRepository commentRepository,
			ICorrectiveActionMetaDataRepository correctiveActionMetaDataRepository, ICommentBusiness commentBusiness, IWorkItemRepository workItemRepository, IUserRepository userRepository)
		{
			_messageService = messageService;
			_commentRepository = commentRepository;
			_correctiveActionMetaDataRepository = correctiveActionMetaDataRepository;
			_commentBusiness = commentBusiness;
			_workItemRepository = workItemRepository;
			_userRepository = userRepository;
		}

		public async Task<PaginatedItems<GetTaskListByCorrectiveAction>> GetTaskListByCorrectiveAction(GetTaskLists getAllTaskByCA)
		{
			return await _correctiveActionMetaDataRepository.GetTaskListByCorrectiveAction(getAllTaskByCA);
		}

		public async Task UpsertMetadata(PutCorrectiveActionMetadataViewModel rca, int workItemId, int userId, int tenantId)
		{
			var rcaMetaData = await _correctiveActionMetaDataRepository.getCorrectiveActionMetaDataByWorkItemId(workItemId);
			//var assignedUser = await _workItemRepository.GetByIdAsync(workItemId);

			if (rcaMetaData != null)
			{
				rcaMetaData.WhyAnalysis1 = rca.WhyAnalysis1;
				rcaMetaData.WhyAnalysis2 = rca.WhyAnalysis2;
				rcaMetaData.WhyAnalysis3 = rca.WhyAnalysis3;
				rcaMetaData.WhyAnalysis4 = rca.WhyAnalysis4;
				rcaMetaData.WhyAnalysis5 = rca.WhyAnalysis5;
				rcaMetaData.RootCauseAnalysis = rca.RootCause;

				rcaMetaData.UpdatedBy = userId;
				rcaMetaData.UpdatedOn = DateTime.UtcNow;
				await _correctiveActionMetaDataRepository.UpdateAsync(rcaMetaData);
				var Data = await _workItemRepository.GetByIdAsync(workItemId);

				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					BroadcastLevel = NotificationBroadcastLevel.Tenant,
					EventType = NotificationEventType.BusinessMaster,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Edit,
					Module = IMSControllerCategory.CorrectiveAction,
					ItemId = workItemId,
					Description = Data.Description,
					Title = Data.Title,
					Date = rcaMetaData.UpdatedOn
				});
			}
			else
			{
				rcaMetaData = new CorrectiveActionMetadata();
				rcaMetaData.WorkItemId = workItemId;

				rcaMetaData.WhyAnalysis1 = rca.WhyAnalysis1;
				rcaMetaData.WhyAnalysis2 = rca.WhyAnalysis2;
				rcaMetaData.WhyAnalysis3 = rca.WhyAnalysis3;
				rcaMetaData.WhyAnalysis4 = rca.WhyAnalysis4;
				rcaMetaData.WhyAnalysis5 = rca.WhyAnalysis5;
				rcaMetaData.RootCauseAnalysis = rca.RootCause;

				rcaMetaData.UpdatedBy = userId;
				rcaMetaData.UpdatedOn = DateTime.UtcNow;
				await _correctiveActionMetaDataRepository.AddAsync(rcaMetaData);
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
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.CorrectiveAction,
				ItemId = workItemId,
				Description = rawData.Description,
				Title = rawData.Title,
				Date = rcaMetaData.UpdatedOn
			});
		}

		public async Task<GetCorrectiveActionMetadataView> GetMetadata(int workItemId, int tenanṭId)
		{
			var caMetaData = await _correctiveActionMetaDataRepository.GetMetadata(workItemId, tenanṭId);

			if (caMetaData == null)
			{
				caMetaData = new GetCorrectiveActionMetadataView();
				return caMetaData;
			}
			else
			{
				GetCorrectiveActionMetadataView preview = new GetCorrectiveActionMetadataView();

				preview.Id = caMetaData.Id;
				preview.WorkItemId = caMetaData.WorkItemId;
				preview.IsApproved = caMetaData.IsApproved;
				preview.WhyAnalysis1 = caMetaData.WhyAnalysis1;
				preview.WhyAnalysis2 = caMetaData.WhyAnalysis2;
				preview.WhyAnalysis3 = caMetaData.WhyAnalysis3;
				preview.WhyAnalysis4 = caMetaData.WhyAnalysis4;
				preview.WhyAnalysis5 = caMetaData.WhyAnalysis5;
				preview.RootCauseAnalysis = caMetaData.RootCauseAnalysis;
				preview.Documents = caMetaData.Documents;
				preview.RootCauseReviewedById = caMetaData.RootCauseReviewedById;
				preview.RootCauseReviewedBy = caMetaData.RootCauseReviewedBy;
				preview.RootCauseReviewedOn = caMetaData.RootCauseReviewedOn;
				preview.UpdatedBy = caMetaData.UpdatedBy;
				preview.UpdatedById = caMetaData.UpdatedById;
				preview.UpdatedOn = caMetaData.UpdatedOn;

				var correctiveActionComments = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.CorrectiveAction, workItemId);

				IList<CommentsView> comments = new List<CommentsView>();
				foreach (var comment in correctiveActionComments)
				{
					comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
				}
				preview.Comments = comments;

				return preview;
			}
		}

		public async Task ApproveMetaData(ApproveCorrectiveActionMetaDataView comment, int tenantId, int workItemId, int userId)
		{
			var rcaMetaData = await _correctiveActionMetaDataRepository.getCorrectiveActionMetaDataByWorkItemId(workItemId);
			var workItem = await _workItemRepository.GetByIdAsync(workItemId);
			if (rcaMetaData != null)
			{
				rcaMetaData.IsApproved = comment.IsApproved;
				if (rcaMetaData.IsApproved == true)
				{
					workItem.StatusMasterDataId = (int)IMSItemStatus.Approved;
				}
				else
				{
					workItem.StatusMasterDataId = (int)IMSItemStatus.Rejected;
				}
				await _workItemRepository.UpdateAsync(workItem);
				rcaMetaData.RootCauseAppovedOn = DateTime.UtcNow;
				rcaMetaData.RootCauseAppovedBy = userId;
				await _correctiveActionMetaDataRepository.UpdateAsync(rcaMetaData);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.CorrectiveAction;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = comment.Comments;

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
			else
			{
				throw new NotFoundException("RCA", workItemId);
			}
			//var rawData = await _workItemRepository.GetByIdAsync(workItemId);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.CorrectiveAction,
				ItemId = workItemId,
				Description = rcaMetaData.RootCauseAnalysis,
				Title = rcaMetaData.RootCauseAnalysis,
				Date = rcaMetaData.RootCauseAppovedOn
			});
		}

		public async Task AssignCorrectiveActionMetaData(AssignCorrectiveActionMetaDataView comment, int tenantId, int workItemId, int userId)
		{
			var correctiveAction = await _workItemRepository.GetByIdAsync(workItemId);

			if (correctiveAction != null)
			{
				correctiveAction.AssignedToUserId = comment.AssignToUserId;
				correctiveAction.StatusMasterDataId = (int)IMSItemStatus.Assigned;
				await _workItemRepository.UpdateAsync(correctiveAction);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.CorrectiveAction;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = comment.Comments;

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
			else
			{
				throw new NotFoundException("Corrective Action", workItemId);
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
				Module = IMSControllerCategory.CorrectiveAction,
				ItemId = workItemId,
				Description = correctiveAction.Description,
				Title = correctiveAction.Title,
				Date = DateTime.UtcNow
			});
		}

		public async Task OpenMetadata(int tenantId, int workItemId, int userId)
		{
			var ncMetadata = await _workItemRepository.GetByIdAsync(workItemId);

			if (ncMetadata.StatusMasterDataId != (int)IMSItemStatus.Assigned)
			{
				throw new BadRequestException("CA is Not Assigned Yet");
			}
			else
			{
				ncMetadata.UpdatedBy = userId;
				ncMetadata.UpdatedOn = DateTime.UtcNow;
				ncMetadata.StatusMasterDataId = (int)IMSItemStatus.Open;
				await _workItemRepository.UpdateAsync(ncMetadata);
			}
			var rawData = await _workItemRepository.GetByIdAsync(workItemId);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.CorrectiveAction,
				ItemId = workItemId,
				Description = rawData.Description,
				Title = rawData.Title,
				Date = ncMetadata.UpdatedOn
			});
		}

		public async Task CloseMetadata(CloseCorrectiveActionMetaData closeCorrectiveActionMetaData, int tenantId, int workItemId, int UserId)
		{
			var ncMetadata = await _workItemRepository.GetByIdAsync(workItemId);

			if (ncMetadata.StatusMasterDataId != (int)IMSItemStatus.InReview)
			{
				throw new BadRequestException("CA is Not InReview Yet");
			}
			else
			{
				ncMetadata.UpdatedBy = UserId;
				ncMetadata.UpdatedOn = DateTime.UtcNow;
				ncMetadata.StatusMasterDataId = (int)IMSItemStatus.Closed;
				await _workItemRepository.UpdateAsync(ncMetadata);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.CorrectiveAction;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = closeCorrectiveActionMetaData.Comments;
				await _commentBusiness.AddComment(postCommentView, UserId, tenantId);
			}

			var rawData = await _workItemRepository.GetByIdAsync(workItemId);
			var userDetails = await _userRepository.GetUserDetail(UserId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = UserId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.CorrectiveAction,
				ItemId = workItemId,
				Description = rawData.Description,
				Title = rawData.Title,
				Date = ncMetadata.UpdatedOn
			});
		}

		public async Task SubmitCAMetadata(int tenantId, int workItemId, int userId)
		{
			var ncMetadata = await _workItemRepository.GetByIdAsync(workItemId);

			if (ncMetadata.StatusMasterDataId != (int)IMSItemStatus.Approved)
			{
				throw new BadRequestException("CA is Not Approved Yet");
			}
			else
			{
				ncMetadata.UpdatedBy = userId;
				ncMetadata.UpdatedOn = DateTime.UtcNow;
				ncMetadata.StatusMasterDataId = (int)IMSItemStatus.InReview;
				await _workItemRepository.UpdateAsync(ncMetadata);
			}
			var rawData = await _workItemRepository.GetByIdAsync(workItemId);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.CorrectiveAction,
				ItemId = workItemId,
				Description = rawData.Description,
				Title = rawData.Title,
				Date = ncMetadata.UpdatedOn
			});
		}
	}
}