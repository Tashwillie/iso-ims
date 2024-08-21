using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class TaskMasterRepository : BaseRepository<TaskMetaData>, ITaskMasterRepository
	{
		private readonly IConfiguration _configuration;
		private readonly ICommentBusiness _commentBusiness;
		private readonly IWorkItemRepository _workItemRepository;

		public TaskMasterRepository(IMSDEVContext dbContext, ILogger<TaskMetaData> logger, IConfiguration configuration, IWorkItemRepository workItemRepository, ICommentBusiness commentBusiness, IEmailService emailService, ICommentRepository commentRepository, IUserPointsBusiness userPointsBusiness) : base(dbContext, logger)
		{
			_commentBusiness = commentBusiness;
			_workItemRepository = workItemRepository;
			_configuration = configuration;
		}

		public async Task<IList<TaskMasterVew>> OverDueTaskRemains()
		{
			var rawData = await (from ts in _context.TaskMasters
								 join wm in _context.WorkItemMasters on ts.WorkItemId equals wm.WorkItemId
								 join us in _context.UserMasters on wm.ResponsibleUserId equals us.UserId
								 join md in _context.MasterData on wm.StatusMasterDataId equals md.Id
								 where wm.DueDate == DateTime.UtcNow.Date.AddDays(2) && wm.StatusMasterDataId == (int)IMSItemStatus.Open && wm.TenantId != 12
								 select new TaskMasterVew
								 {
									 Name = us.FirstName,
									 TaskId = ts.TaskId,
									 Title = wm.Title,
									 Description = wm.Description,
									 EmailAddress = us.EmailId,
									 Status = md.Items,
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<IList<TaskMasterVew>> NightlyRemiderForTask()
		{
			var rawData = await (from ts in _context.TaskMasters
								 join wm in _context.WorkItemMasters on ts.WorkItemId equals wm.WorkItemId
								 join us in _context.UserMasters on wm.ResponsibleUserId equals us.UserId
								 join md in _context.MasterData on wm.StatusMasterDataId equals md.Id
								 //join md1 in _context.MasterData on ts.Priority equals md1.Id
								 where wm.DueDate == DateTime.UtcNow.Date.AddDays(2) && wm.StatusMasterDataId == (int)IMSItemStatus.Open && wm.TenantId != 12
								 select new TaskMasterVew
								 {
									 Name = us.FirstName,
									 TaskId = ts.TaskId,
									 Title = wm.Title,
									 Description = wm.Description,
									 EmailAddress = us.EmailId,
									 Status = md.Items,
									 //Priority = md1.Items
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}

		public async Task<BackTrace> GetCorrectiveActionByTaskId(int moduleEntitiyId)
		{
			var rawdata = (from tm in _context.TaskMasters
						   join catm in _context.CorrectiveActionTaskMasterMappings on tm.TaskId equals catm.TaskId
						   join ca in _context.CorrectiveActions on catm.CorrectiveActionId equals ca.Id
						   join um in _context.UserMasters on ca.CreatedBy equals um.UserId
						   where moduleEntitiyId == catm.TaskId
						   select new BackTrace
						   {
							   ModuleId = (int)IMSControllerCategory.NonConformanceCorrectiveAction,
							   ModuleName = "Corective Action",
							   ModuleItemId = ca.Id,
							   Title = ca.Title,
							   Content = ca.Description,
							   CreatedOn = ca.CreatedOn,
							   CreatedBy = $"{um.FirstName} {um.LastName}",
							   OrderNumber = 2
						   }).AsQueryable();
			return rawdata.FirstOrDefault();
		}

		public async Task<BackTrace> GetNonConfirmityByTaskId(int moduleEntitiyId)
		{
			var rawdata = (from tm in _context.TaskMasters
						   join catm in _context.CorrectiveActionTaskMasterMappings on tm.TaskId equals catm.TaskId
						   join ca in _context.CorrectiveActions on catm.CorrectiveActionId equals ca.Id
						   join nc in _context.NonConformities on ca.ModuleItemId equals nc.Id
						   join um in _context.UserMasters on nc.CreatedBy equals um.UserId

						   where moduleEntitiyId == catm.TaskId && ca.ModuleId == (int)IMSCorrectiveActionModuleId.NonConformity
						   select new BackTrace
						   {
							   ModuleId = (int)IMSControllerCategory.NonConformance,
							   ModuleName = "NonConformance",
							   ModuleItemId = nc.Id,
							   Title = nc.Title,
							   Content = nc.Description,
							   CreatedOn = nc.CreatedOn,
							   CreatedBy = $"{um.FirstName} {um.LastName}",
							   OrderNumber = 3
						   }).AsQueryable();
			return rawdata.FirstOrDefault();
		}

		public async Task<BackTrace> GetFindingByTaskId(int moduleEntitiyId)
		{
			var rawdata = (from tm in _context.TaskMasters
						   join catm in _context.CorrectiveActionTaskMasterMappings on tm.TaskId equals catm.TaskId
						   join ca in _context.CorrectiveActions on catm.CorrectiveActionId equals ca.Id
						   join nc in _context.NonConformities on ca.ModuleItemId equals nc.Id
						   join af in _context.AuditFindings on nc.SourceId equals af.Id
						   join um in _context.UserMasters on af.CreatedBy equals um.UserId

						   where moduleEntitiyId == catm.TaskId && ca.ModuleId == (int)IMSCorrectiveActionModuleId.NonConformity
						   select new BackTrace
						   {
							   ModuleId = (int)IMSControllerCategory.InternalAuditFinding,
							   ModuleName = "InternalAuditFinding",
							   ModuleItemId = af.Id,
							   Title = af.Title,
							   Content = af.Description,
							   CreatedOn = af.CreatedOn,
							   CreatedBy = $"{um.FirstName} {um.LastName}",
							   OrderNumber = 4
						   }).AsQueryable();
			return rawdata.FirstOrDefault();
		}

		public async Task<BackTrace> GetAuditCheckListByTaskId(int moduleEntitiyId)
		{
			var rawdata = (from tm in _context.TaskMasters
						   join catm in _context.CorrectiveActionTaskMasterMappings on tm.TaskId equals catm.TaskId
						   join ca in _context.CorrectiveActions on catm.CorrectiveActionId equals ca.Id
						   join nc in _context.NonConformities on ca.ModuleItemId equals nc.Id
						   join af in _context.AuditFindings on nc.SourceId equals af.Id
						   join acf in _context.AuditChecklistFindings on af.Id equals acf.AuditFindingId
						   join ac in _context.AuditChecklists on acf.AuditChecklistId equals ac.Id
						   join cm in _context.ChecklistMasters on ac.ChecklistMasterId equals cm.Id
						   join um in _context.UserMasters on acf.CreatedBy equals um.UserId

						   where moduleEntitiyId == catm.TaskId && ca.ModuleId == (int)IMSCorrectiveActionModuleId.NonConformity
						   select new BackTrace
						   {
							   ModuleId = (int)IMSControllerCategory.AuditCheckList,
							   ModuleName = "AuditChecklist",
							   ModuleItemId = ac.Id,
							   Title = cm.Questions,
							   Content = ac.Comments,
							   CreatedOn = acf.CreatedOn,
							   CreatedBy = $"{um.FirstName} {um.LastName}",
							   OrderNumber = 5
						   }).AsQueryable();
			return rawdata.FirstOrDefault();
		}

		public async Task<TaskMetaData> AddTask(PostViewTask newTask, int tenantId, int workItemId, int userId)
		{
			var metaData = await _context.TaskMasters.Where(t => t.WorkItemId == workItemId).FirstOrDefaultAsync();

			if (metaData != null)
			{
				metaData.RemainingEffortHours = newTask.RemainingEffortHours;
				metaData.Reviewer = newTask.Reviewer;
				await _context.SaveChangesAsync();
				var taskMetada = await _workItemRepository.GetByIdAsync(workItemId);
				if (newTask.Status == 0)
				{
					taskMetada.StatusMasterDataId = taskMetada.StatusMasterDataId;
				}
				else
				{
					taskMetada.StatusMasterDataId = newTask.Status;
				}

				await _context.SaveChangesAsync();

				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.TaskMaster;
				postCommentView.SourceItemId = workItemId;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = newTask.Comments;
				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new NotFoundException("Effort Hours and Priority not set", workItemId);
			}

			return metaData;
		}

		public async Task<GetTaskMetaDataview> GetAllTaskMaster(int workItemId)
		{
			var rawData = (from task in _context.TaskMasters
						   join user in _context.UserMasters on task.Reviewer equals user.UserId into user
						   from subUser in user.DefaultIfEmpty()
						   join workItem in _context.WorkItemMasters on task.WorkItemId equals workItem.WorkItemId
						   join master in _context.MasterData on workItem.StatusMasterDataId equals master.Id
						   where task.WorkItemId == workItemId
						   select new GetTaskMetaDataview()
						   {
							   TaskId = task.TaskId,
							   Reviewer = $"{subUser.FirstName} {subUser.LastName}",
							   ReviewerId = task.Reviewer,
							   StatusId = workItem.StatusMasterDataId,
							   Status = master.Items,
							   WorkItemId = task.WorkItemId,
							   EstimatedEffortHours = task.EstimateEffortHours,
							   RemainingEffortHours = task.RemainingEffortHours,
						   }).AsQueryable();
			return rawData.FirstOrDefault();
		}

		public async Task<IList<TagView>> GetTaskTags(int workItemId)
		{
			var data = await (from Task in _context.TaskMasters
							  join tag in _context.TaskTags on Task.WorkItemId equals tag.TaskId
							  join master in _context.MasterData on tag.MasterDataTaskTagId equals master.Id
							  where Task.WorkItemId == workItemId
							  select new TagView()
							  {
								  TagId = tag.MasterDataTaskTagId,
								  TagName = master.Items
							  }).ToListAsync();
			return await Task.FromResult(data);
		}

		public async Task<TaskMetaData> GetTaskByWorkItemId(int workItemId)
		{
			var metaData = await _context.TaskMasters.Where(t => t.WorkItemId == workItemId).FirstOrDefaultAsync();
			return metaData;
		}

		public async Task<WorkItemWorkItemToken> EditTokenForTask(int priority, int workItemId)
		{
			var existingToken = _context.WorkItemWorkItemTokens.Where(ps => ps.WorkItemId == workItemId).ToList();
			_context.WorkItemWorkItemTokens.RemoveRange(existingToken);
			await _context.SaveChangesAsync();

			var tokens = new WorkItemWorkItemToken();

			tokens.WorkItemId = workItemId;
			tokens.TokenId = priority;
			await _context.AddAsync(tokens);
			await _context.SaveChangesAsync();
			return tokens;
		}

		public async Task<GetTasksDetailsForWorkItem> GetTasksDetailsForWorkItem(int workItemId)
		{
			var taskdetails = (from task in _context.TaskMasters
							   join work in _context.WorkItemMasters on task.WorkItemId equals work.WorkItemId
							   join workToken in _context.WorkItemWorkItemTokens on task.WorkItemId equals workToken.WorkItemId
							   join tokenMaster in _context.Tokens on workToken.TokenId equals tokenMaster.TokenId
							   where task.WorkItemId == workItemId
							   select new GetTasksDetailsForWorkItem()
							   {
								   EstimateEffortsHours = task.EstimateEffortHours,
								   TaskPriorityId = tokenMaster.TokenId,
								   TaskPriority = tokenMaster.TokenName
							   }).AsQueryable();
			return taskdetails.FirstOrDefault();
		}

		public async Task<IEnumerable<ReviewerList>> GetReviewerLists(int WorkItemId)
		{
			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();
			return await conn.QueryAsync<ReviewerList>(
				@"with  cte
            (UserId, UserName)
            as (
                select UserId as Id,FirstName as UserName from WorkItemMaster as w
                join  UserMaster as um on w.ResponsibleUserId=um.UserId or w.CreatedBy=um.UserId
                where w.WorkItemId = @WorkItemId )select * from cte order by UserId", new { WorkItemId }

				);
		}

		public async Task<TaskMetaData> getTaskMetaDataByWorkItemId(int workItemId)
		{
			var data = await _context.TaskMasters.FirstOrDefaultAsync(t => t.WorkItemId == workItemId);

			return data;
		}

		public async Task<IList<GetCAListByIncidentId>> GetTaskListByPhaseId(int phaseId, int tenantId)
		{
			var rawData = await (from tasks in _context.WorkItemMasters
								 join phases in _context.WorkItemMasters on tasks.SourceItemId equals phases.WorkItemId
								 where tasks.SourceItemId == phaseId && tasks.TenantId == tenantId && tasks.WorkItemTypeId == (int)IMSModules.TaskMaster && phases.WorkItemTypeId == (int)IMSModules.ProjectPhase
								 select new GetCAListByIncidentId()
								 {
									 Id = tasks.WorkItemId,
									 StatusId = tasks.StatusMasterDataId
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}
		public async Task<IList<GetCAListByIncidentId>> GetPhaseListByProjectId(int projectId, int tenantId)
		{
			var rawData = await(from phases in _context.WorkItemMasters
								join projects in _context.WorkItemMasters on phases.SourceItemId equals projects.WorkItemId
								where phases.SourceItemId == projectId && phases.TenantId == tenantId && phases.WorkItemTypeId == (int)IMSModules.ProjectPhase && projects.WorkItemTypeId == (int)IMSModules.ProjectManagement
								select new GetCAListByIncidentId()
								{
									Id = phases.WorkItemId,
									StatusId = phases.StatusMasterDataId
								}).ToListAsync();
			return await Task.FromResult(rawData);
		}
	}
}