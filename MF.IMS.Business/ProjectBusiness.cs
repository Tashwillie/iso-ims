using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;

namespace Mindflur.IMS.Business
{
	public class ProjectBusiness : IProjectsBusiness
	{
		private readonly IProjectRepository _projectRepository;
		private readonly IProjectTagRepository _projectTagRepository;
		
		private readonly IMessageService _messageService;
		private readonly IWorkItemRepository _workItemRepository;
       

        public ProjectBusiness(IProjectRepository projectRepository, IProjectTagRepository projectTagRepository, IMessageService messageService, IWorkItemRepository workItemRepository)
		{
			
			_projectTagRepository = projectTagRepository;
			_projectRepository = projectRepository;
			
			_messageService = messageService;
			_workItemRepository = workItemRepository;
          
        }

		public async Task<ProjectMetaData> UpdateProjectMetaData(PutProjectmanagementView putProject, int workItemId, int userId, int tenantId)
		{
			return await _projectRepository.UpdateProjectMetaData(putProject, workItemId, userId, tenantId);
			
			var rawData=await _workItemRepository.GetByIdAsync(workItemId);

            
            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = userId,
                BroadcastLevel = NotificationBroadcastLevel.Global,
				EventType = NotificationEventType.BusinessMaster,
				TenantId = tenantId,
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.ProjectManagement,
                ItemId = workItemId,
                Description = rawData.Description,
                Title = rawData.Title,
            });
        }

		public async Task<ProjectDetailView> GetProjectMetaData(int workItemId, int tenantId)
		{
			var project = await _projectRepository.GetProjectMetaData(workItemId, tenantId);
			if (project != null)
			{
				ProjectDetailView detailView = new ProjectDetailView();
				detailView.ProjectId = project.ProjectId;
				detailView.WorkItemId = project.WorkItemId;
				detailView.StartDate = project.StartDate;
				detailView.EndDate = project.EndDate;
				detailView.Budget = project.Budget;

				var projectsTags = await _projectTagRepository.GetProjectTags(workItemId);

				IList<TagView> tags = new List<TagView>();

				foreach (TagDataView projectTag in projectsTags)
				{
					tags.Add(new TagView() { TagId = projectTag.TagId, TagName = projectTag.TagName });
				}

				var nCMetaDatatoken = await _workItemRepository.GetAllTokens(workItemId);
				IList<TokensView> getTokens = new List<TokensView>();
				foreach (var token in nCMetaDatatoken)
				{
					getTokens.Add(new TokensView() { TokenId = token.TokenId, Token  = token.TokenName, ParentTokenId = token.ParentTokenId, ParentTokenName = token.ParentTokenName });
				}
				detailView.ProjectTags = tags;
				detailView.Tokens = getTokens;

				return detailView;
			}
			else
			{
				project=new ProjectDetailView();
				return project;
			}
		}

		

		
		
		
		
	}
}