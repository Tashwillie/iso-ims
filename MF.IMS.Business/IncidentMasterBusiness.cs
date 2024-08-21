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
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
	public class IncidentMasterBusiness : IIncidentMasterBusiness
	{
		private readonly IIncidentMasterRepository _incidentMasterRepository;

		private readonly IWorkItemRepository _workItemRepository;
		private readonly IMessageService _messageService;
		private readonly ICommentBusiness _commentBusiness;
		private readonly IUserRepository _userRepository;
		private readonly ICommentRepository _commentRepository;

		public IncidentMasterBusiness(ICommentBusiness commentBusiness, IMessageService messageService, IIncidentMasterRepository incidentMasterRepository, IWorkItemRepository workItemRepository, IUserRepository userRepository, ICommentRepository commentRepository)
		{
			_incidentMasterRepository = incidentMasterRepository;

			_workItemRepository = workItemRepository;
			_messageService = messageService;
			_commentBusiness = commentBusiness;
			_userRepository = userRepository;
			_commentRepository = commentRepository;
		}

		public async Task<IncidentMetaData> UpdateIncidentDescription(IncidentMasterDescriptionPutView incidentMaster, int userId, int incidentId, int tenantId)
		{
			var rawData = await _incidentMasterRepository.UpdateIncidentDescription(incidentMaster, userId, incidentId, tenantId);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()

			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.User,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.IncidentManagement,
				ItemId = incidentId,
				Description = rawData.ClassificationDescription,
				Title = rawData.ClassificationDescription,
				Date = rawData.UpdatedOn
			});
			return rawData;
		}

		public async Task<GetIncidentMetaDataView> GetIncidentMetaData(int workItemId, int tenantId)
		{
			var incidentdetails = await _incidentMasterRepository.GetIncidentMetaData(workItemId, tenantId);
			if (incidentdetails == null)
			{
				incidentdetails = new GetIncidentMetaDataView();
				return incidentdetails;
			}

			var preview = new GetIncidentMetaDataView();
			preview.IncidentId = incidentdetails.IncidentId;
			preview.WorkItemId = incidentdetails.WorkItemId;
			preview.WorkItemText = incidentdetails.WorkItemText;
			preview.EmployeeId = incidentdetails.EmployeeId;
			preview.EmployeeName = incidentdetails.EmployeeName;
			preview.DateOfIncident = incidentdetails.DateOfIncident;
			preview.Occupation = incidentdetails.Occupation;
			preview.OccupationName = incidentdetails.OccupationName;
			preview.AllowedToBeClosed = incidentdetails.AllowedToBeClosed;
			preview.WorkResumed = incidentdetails.WorkResumed;
			preview.WearedPpe = incidentdetails.WearedPpe;
			preview.DescriptionOfInjury = incidentdetails.DescriptionOfInjury;
			preview.DescriptionOfHowInjuryOccured = incidentdetails.DescriptionOfHowInjuryOccured;
			preview.DescriptionOfMedicalTreatment = incidentdetails.DescriptionOfMedicalTreatment;
			preview.ClassificationDescription = incidentdetails.ClassificationDescription;
			preview.UpdatedById = incidentdetails.UpdatedById;
			preview.UpdatedBy = incidentdetails.UpdatedBy;
			preview.UpdatedOn = incidentdetails.UpdatedOn;
			preview.AssignedUserId = incidentdetails.AssignedUserId;
			preview.AssignedUser = incidentdetails.AssignedUser;
			preview.IsApproved = incidentdetails.IsApproved;
			preview.ApprovedById = incidentdetails.ApprovedById;
			preview.ApprovedBy = incidentdetails.ApprovedBy;
			preview.ApprovedOn = incidentdetails.ApprovedOn;
			preview.ReviewedById = incidentdetails.ReviewedById;
			preview.ReviewedBy = incidentdetails.ReviewedBy;
			preview.ReviewedOn = incidentdetails.ReviewedOn;

			var incidentToken = await _workItemRepository.GetAllTokens(workItemId);
			IList<TokensView> getTokens = new List<TokensView>();
			foreach (var token in incidentToken)
			{
				getTokens.Add(new TokensView() { TokenId = token.TokenId, Token = token.TokenName, ParentTokenId = token.ParentTokenId, ParentTokenName = token.ParentTokenName });
			}
			var correctiveActionComments = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.NonConformity, workItemId);

			IList<CommentsView> comments = new List<CommentsView>();
			foreach (var comment in correctiveActionComments)
			{
				comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
			}
			preview.Comments = comments;
			preview.Tokens = getTokens;
			return preview;
		}

		public async Task UpadteIncidentMetaData(PutIncidentMasterView putIncidentMasterView, int workItemId, int UserId, int tenantId)
		{
			await _incidentMasterRepository.UpadteIncidentMetaData(putIncidentMasterView, workItemId, UserId, tenantId);
			var userDetails = await _userRepository.GetUserDetail(UserId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = UserId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.User,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.IncidentManagement,
				ItemId = workItemId,
				Description = putIncidentMasterView.ClassificationDescription,
				Title = putIncidentMasterView.ClassificationDescription,
				Date = DateTime.UtcNow
			});
		}

		public async Task AddIncidentComment(Incidentcomments incidentcomments, int tenantId, int workItemId, int UserId)
		{
			var IncidentMetadata = await _workItemRepository.GetByIdAsync(workItemId);

			IncidentMetadata.UpdatedBy = UserId;
			IncidentMetadata.UpdatedOn = DateTime.UtcNow;

			await _workItemRepository.UpdateAsync(IncidentMetadata);
			var postCommentView = new PostCommentView();
			postCommentView.SourceId = (int)IMSModules.IncidentManagement;
			postCommentView.SourceItemId = workItemId;
			postCommentView.ParentCommentId = 0;
			postCommentView.ContentType = 1;
			postCommentView.CommentContent = incidentcomments.Comments;
			await _commentBusiness.AddComment(postCommentView, UserId, tenantId);

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
				Module = IMSControllerCategory.IncidentManagement,
				ItemId = workItemId,
				Description = rawData.Description,
				Title = rawData.Title,
				Date = DateTime.UtcNow
			});
		}

		public async Task StartIncident(int userId, int incidentId, int tenantId)
		{
			var incident = await _workItemRepository.GetByIdAsync(incidentId);
			if (incident.StatusMasterDataId != (int)IMSItemStatus.Assigned)
			{
				throw new BadRequestException("Incident is Not Assigned Yet");
			}
			else
			{
				incident.StatusMasterDataId = (int)IMSItemStatus.Open;
				incident.UpdatedBy = userId;
				incident.UpdatedOn = DateTime.UtcNow;
				await _workItemRepository.UpdateAsync(incident);
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
				Module = IMSControllerCategory.IncidentManagement,
				ItemId = incidentId,
				Description = incident.Description,
				Title = incident.Title,
				Date = incident.UpdatedOn
			});
		}

		public async Task RejectMetadata(RejectNonconformanceMetadata rejectNonconformanceMetadata, int tenantId, int workItemId, int userId)
		{
			var incidentMetadata = await _incidentMasterRepository.GetIncidentByWorkItemId(workItemId);
			var incident = await _workItemRepository.GetByIdAsync(workItemId);
			if (incidentMetadata != null)
			{
				incidentMetadata.IsApproved = false;
				incidentMetadata.UpdatedOn = DateTime.UtcNow;
				incidentMetadata.UpdatedBy = userId;
				await _incidentMasterRepository.UpdateAsync(incidentMetadata);
				incident.StatusMasterDataId = (int)IMSItemStatus.Rejected;
				await _workItemRepository.UpdateAsync(incident);
			}
			else if (incident != null)
			{
				incident.StatusMasterDataId = (int)IMSItemStatus.Rejected;
				await _workItemRepository.UpdateAsync(incident);
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
				Module = IMSControllerCategory.IncidentManagement,
				ItemId = workItemId,
				Description = incident.Description,
				Title = incident.Title,
				Date = incident.UpdatedOn
			});
		}

		public async Task ReviewMetadata(ReviewNonconformanceMetadata reviewNonconformanceMetadata, int tenantId, int workItemId, int userId)
		{
			var incidentMetadata = await _incidentMasterRepository.GetIncidentByWorkItemId(workItemId);
			var incident = await _workItemRepository.GetByIdAsync(workItemId);
			if (incidentMetadata != null)
			{
				incidentMetadata.ReviewedBy = userId;
				incidentMetadata.ReviewedOn = DateTime.UtcNow;
				await _incidentMasterRepository.UpdateAsync(incidentMetadata);
				incident.StatusMasterDataId = (int)IMSItemStatus.InReview;
				await _workItemRepository.UpdateAsync(incident);
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
				Description = incident.Description,
				Title = incident.Title,
				Date = incident.UpdatedOn
			});
		}

		public async Task CloseMetadata(CloseNonConformanceMetaData closeNonConformanceMetaData, int tenantId, int workItemId, int userId)
		{
			var correctiveActions=await _incidentMasterRepository.GetCaListByIncidentId(tenantId,workItemId);
			var incidentMetadata = await _incidentMasterRepository.GetIncidentByWorkItemId(workItemId);
			var incident = await _workItemRepository.GetByIdAsync(workItemId);
			var correctiveAction=correctiveActions.Where(t=>t.StatusId!=(int)IMSItemStatus.Closed).ToList();

			if (incident.StatusMasterDataId != (int)IMSItemStatus.InReview)
			{
				throw new BadRequestException("Nc is Not InReview Yet");
			} else if(correctiveAction.Count != 0)
			{
				throw new BadRequestException("First Close all the Corrective Action related to Incident Id : "+workItemId);
			}
			else
			{
				incidentMetadata.UpdatedBy = userId;
				incidentMetadata.UpdatedOn = DateTime.UtcNow;
				await _incidentMasterRepository.UpdateAsync(incidentMetadata);
				incident.StatusMasterDataId = (int)IMSItemStatus.Closed; ;
				await _workItemRepository.UpdateAsync(incident);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.NonConformity;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = closeNonConformanceMetaData.Comments;
				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}

			var userDetail = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetail.FirstName} {userDetail.LastName}",
				BroadcastLevel = NotificationBroadcastLevel.Tenant,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Close,
				Module = IMSControllerCategory.IncidentManagement,
				ItemId = workItemId,
				Description = incident.Description,
				Title = incident.Title,
				Date = incident.UpdatedOn
			});
		}
	}
}