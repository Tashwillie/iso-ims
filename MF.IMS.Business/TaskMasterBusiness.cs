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

namespace Mindflur.IMS.Business
{
	public class TaskMasterBusiness : ITaskMasterBusiness
	{
		private readonly ITaskMasterRepository _taskMasterRepository;

		private readonly IWorkItemRepository _workItemRepository;
		private readonly IMessageService _messageService;
		private readonly ICommentBusiness _commentBusiness;
		private readonly IUserRepository _userRepository;
		private readonly ICommentRepository _commentRepository;

		public TaskMasterBusiness(ITaskMasterRepository taskMasterRepository, ICommentRepository commentRepository, IUserRepository userRepository, ICommentBusiness commentBusiness, IWorkItemRepository workItemRepository, IMessageService messageService)
		{
			_taskMasterRepository = taskMasterRepository;

			_workItemRepository = workItemRepository;
			_messageService = messageService;
			_commentBusiness = commentBusiness;
			_userRepository = userRepository;
			_commentRepository = commentRepository;
		}

		public async Task DeleteTasks(int taskId)
		{
			var task = await _taskMasterRepository.GetByIdAsync(taskId);
			if (task == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.TaskMasterNotFoundErrorMessage), taskId);
			}

			await _taskMasterRepository.DeleteAsync(task);
		}

		public Task<IList<TaskDetailDomain>> GetTasks()
		{
			IList<TaskDetailDomain> tasks = new List<TaskDetailDomain>();
			return Task.FromResult(tasks); //Replace this with actual implementation
		}

		public async Task<IList<TaskMasterVew>> OverDueRemiderForTask()
		{
			var overDues = await _taskMasterRepository.OverDueTaskRemains();
			return overDues;
		}

		public async Task<IList<TaskMasterVew>> NightlyRemiderForTask()
		{
			var reminders = await _taskMasterRepository.NightlyRemiderForTask();
			return reminders;
		}

		public async Task UpsertTaskMetaData(PostViewTask newTask, int tenantId, int sourceId, int userId)
		{
			await _taskMasterRepository.AddTask(newTask, tenantId, sourceId, userId);
		}

		public async Task<GetTaskMetaDataview> GetTaskMetaData(int tenantId, int workItemId)
		{
			var rawData = await _taskMasterRepository.GetAllTaskMaster(workItemId);
			if (rawData == null)
			{
				rawData = new GetTaskMetaDataview();
				return rawData;
			}
			var preview = new GetTaskMetaDataview();
			preview.TaskId = rawData.TaskId;
			preview.WorkItemId = rawData.WorkItemId;
			preview.EstimatedEffortHours = rawData.EstimatedEffortHours;
			preview.RemainingEffortHours = rawData.RemainingEffortHours;
			preview.Reviewer = rawData.Reviewer;
			preview.ReviewerId = rawData.ReviewerId;
			preview.StatusId = rawData.StatusId;
			preview.Status = rawData.Status;

			var phaseComment = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.TaskMaster, workItemId);

			IList<CommentsView> comments = new List<CommentsView>();
			foreach (var comment in phaseComment)
			{
				comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
			}
			preview.Comments = comments;

			return preview;
		}

		public async Task ApproveTaskMetaData(TaskMetaDataComments taskMetaDataComments, int workItemId, int tenantId, int userId)
		{
			var taskMetaData = await _taskMasterRepository.GetTaskByWorkItemId(workItemId);
			var workitem = await _workItemRepository.GetByIdAsync(workItemId);
			if (workitem.StatusMasterDataId == (int)IMSItemStatus.InReview)
			{
				taskMetaData.ApprovedOn = DateTime.UtcNow;
				taskMetaData.ApprovedBy = userId;
				workitem.StatusMasterDataId = (int)IMSItemStatus.Closed;
				await _workItemRepository.UpdateAsync(workitem);

				await _taskMasterRepository.UpdateAsync(taskMetaData);

				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.TaskMaster;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = taskMetaDataComments.Comments;

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
			else
			{
				throw new BadRequestException("Task Master is Not InReview Yet");
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
				Module = IMSControllerCategory.Tasks,
				ItemId = workItemId,
				Description = workitem.Description,
				Title = workitem.Title,
				Date = workitem.UpdatedOn
			});
		}

		public async Task RejectMetadata(RejectTaskMetadata comments, int tenantId, int workItemId, int userId)
		{
			var taskMetaData = await _taskMasterRepository.GetAllTaskMaster(workItemId);
			var workitem = await _workItemRepository.GetByIdAsync(workItemId);
			if (taskMetaData != null)
			{
				workitem.UpdatedOn = DateTime.UtcNow;
				workitem.UpdatedBy = userId;
				workitem.StatusMasterDataId = (int)IMSItemStatus.Rejected;
				await _workItemRepository.UpdateAsync(workitem);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.TaskMaster;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = comments.Comments;

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
				Module = IMSControllerCategory.Tasks,
				ItemId = workItemId,
				Description = workitem.Description,
				Title = workitem.Title,
				Date = workitem.UpdatedOn
			});
		}

		public async Task StartMetaData(int tenantId, int workItemId, int userId)
		{
			var taskMetaData = await _taskMasterRepository.getTaskMetaDataByWorkItemId(workItemId);
			var workitem = await _workItemRepository.GetByIdAsync(workItemId);

			if (workitem == null)
			{
				throw new NotFoundException("Task MetaData", workItemId);
			}
			else if (workitem.WorkItemId == workItemId)/*&& taskMetaData.TaskId == tenantId)*/
			{
				workitem.StatusMasterDataId = (int)IMSItemStatus.Open;
				taskMetaData.IsAcknowledge = true;
				await _workItemRepository.UpdateAsync(workitem);
				await _taskMasterRepository.UpdateAsync(taskMetaData);
			}

			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = workitem.CreatedBy,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Edit,
				Module = IMSControllerCategory.Tasks,
				ItemId = workItemId,
				Description = workitem.Description,
				Title = workitem.Title,
			});
		}

		public async Task<IList<ReviewerList>> GetReviewerLists(int WorkItemId)
		{
			IList<ReviewerList> tokenresponseViews = new List<ReviewerList>();
			var rawData = await _taskMasterRepository.GetReviewerLists(WorkItemId);
			foreach (var data in rawData)
			{
				tokenresponseViews.Add(new ReviewerList() { UserId = data.UserId, UserName = data.UserName });
			}
			return tokenresponseViews;
		}

		public async Task ClosePhases(int phaseId, int tenantId, int userId)
		{
			var projectPhase = await _workItemRepository.GetByIdAsync(phaseId);
			var taskList = await _taskMasterRepository.GetTaskListByPhaseId(phaseId, tenantId);
			var tasks = taskList.Where(t => t.StatusId != (int)IMSItemStatus.Closed).ToList();
			if (tasks.Count != 0)
			{
				throw new BadRequestException("First Close all the Tasks  related to Project Phase Id : " + phaseId);
			}
			else
			{
				projectPhase.UpdatedBy = userId;
				projectPhase.UpdatedOn = DateTime.UtcNow;
				projectPhase.StatusMasterDataId = (int)IMSItemStatus.Closed;
				await _workItemRepository.UpdateAsync(projectPhase);
			}
		}

		public async Task CloseProjects(int projectId, int tenantId, int userId)
		{
			var project = await _workItemRepository.GetByIdAsync(projectId);
			var phaseList = await _taskMasterRepository.GetPhaseListByProjectId(projectId, tenantId);
			var phases = phaseList.Where(t => t.StatusId != (int)IMSItemStatus.Closed).ToList();

			if (phases.Count != 0)
			{
				throw new BadRequestException("First Close all the Phases related to Project Id : " + projectId);
			}
			else
			{
				project.UpdatedBy = userId;
				project.UpdatedOn = DateTime.UtcNow;
				project.StatusMasterDataId = (int)IMSItemStatus.Closed;
				await _workItemRepository.UpdateAsync(project);
			}
		}
	}
}