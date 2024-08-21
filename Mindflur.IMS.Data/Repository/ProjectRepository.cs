using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class ProjectRepository : BaseRepository<ProjectMetaData>, IProjectRepository
	{
		
		private readonly IMessageService _messageService;
        private readonly IUserRepository _userRepository;

        public ProjectRepository(IMSDEVContext dbContext, ILogger<ProjectMetaData> logger , IMessageService messageService, IUserRepository userRepository) : base(dbContext, logger)
		{
			
			_messageService = messageService;
            _userRepository = userRepository;
        }

        public async Task<ProjectMetaData> UpdateProjectMetaData(PutProjectmanagementView putProject, int workItemId, int userId, int tenantId)
        {
            var updateProject = await _context.Projects.Where(t => t.WorkItemId == workItemId).FirstOrDefaultAsync();
            if (updateProject != null)
            {
                var existingProjectTags = _context.ProjectTags.Where(ps => ps.WorkItemId == workItemId).ToList();
                _context.ProjectTags.RemoveRange(existingProjectTags);
                await _context.SaveChangesAsync();

                var tagsForProjectTags = new List<ProjectTag>();

                foreach (int projectTag in putProject.MasterDataProjectTagId)
                {
                    var newProjectTags = new ProjectTag
                    {
                        WorkItemId = workItemId,
                        MasterDataProjectTagId = projectTag,
                    };
                    tagsForProjectTags.Add(newProjectTags);
                }

                await _context.ProjectTags.AddRangeAsync(tagsForProjectTags);
                await _context.SaveChangesAsync();                                                  //Project Tags Updated

                var existingProjectToken = _context.WorkItemWorkItemTokens.Where(ps => ps.WorkItemId == workItemId).ToList();
                _context.WorkItemWorkItemTokens.RemoveRange(existingProjectToken);
                await _context.SaveChangesAsync();

                var tokensForProject = new List<WorkItemWorkItemToken>();

                foreach (int projectToken in putProject.Tokens)
                {
                    var newProjectTokens = new WorkItemWorkItemToken
                    {
                        WorkItemId = workItemId,
                        TokenId = projectToken,
                    };
                    tokensForProject.Add(newProjectTokens);
                }

                await _context.WorkItemWorkItemTokens.AddRangeAsync(tokensForProject);
                await _context.SaveChangesAsync();                                                                  //Project Token Updated

                updateProject.StartDate = putProject.StartDate;
                updateProject.EndDate = putProject.EndDate;
                updateProject.Budget = putProject.Budget;

                updateProject.UpdatedBy = userId;
                updateProject.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();                                                                                  //Project Updated

                var rawData = await _context.WorkItemMasters.FindAsync(workItemId);
                var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
                    BroadcastLevel = NotificationBroadcastLevel.Global,
                    EventType = NotificationEventType.BusinessMaster,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Edit,
                    Module = IMSControllerCategory.ProjectManagement,
                    ItemId = workItemId,
                    //Description = rawData.Description,
                    //Title = rawData.Title,
                    Date = updateProject.UpdatedOn
                });
            }
            else
            {
                updateProject = new ProjectMetaData();

                updateProject.WorkItemId = workItemId;
                updateProject.StartDate = putProject.StartDate;
                updateProject.EndDate = putProject.EndDate;
                updateProject.Budget = putProject.Budget;
                updateProject.UpdatedOn = DateTime.UtcNow;
                updateProject.UpdatedBy = userId;

                await _context.Projects.AddAsync(updateProject);
                await _context.SaveChangesAsync();

                var workItem = new List<WorkItemWorkItemToken>();

                foreach (int a in putProject.Tokens)
                {
                    var newWorkItem = new WorkItemWorkItemToken
                    {
                        WorkItemId = workItemId,
                        TokenId = a,
                    };
                    workItem.Add(newWorkItem);
                }
                await _context.WorkItemWorkItemTokens.AddRangeAsync(workItem);
                await _context.SaveChangesAsync();

                var projectTags = new List<ProjectTag>();

                foreach (int a in putProject.MasterDataProjectTagId)
                {
                    var newProjectTags = new ProjectTag
                    {
                        WorkItemId = workItemId,
                        MasterDataProjectTagId = a,
                    };
                    projectTags.Add(newProjectTags);
                }
                await _context.ProjectTags.AddRangeAsync(projectTags);
                await _context.SaveChangesAsync();

                var rawData = await _context.WorkItemMasters.FindAsync(workItemId);

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
            return updateProject;
        }

        public async Task<ProjectDetailView> GetProjectMetaData(int workItemId, int tenantId)
        {
            var rawData = (from project in _context.Projects
                           join workItem in _context.WorkItemMasters on project.WorkItemId equals workItem.WorkItemId
                           where project.WorkItemId == workItemId && workItem.TenantId == tenantId
                           select new ProjectDetailView()
                           {
                               ProjectId = project.ProjectId,
                               WorkItemId = project.WorkItemId,
                               StartDate = project.StartDate,
                               EndDate = project.EndDate,
                               Budget = project.Budget,
                           }
                          ).AsQueryable();
            return rawData.FirstOrDefault();
        }
    }
}