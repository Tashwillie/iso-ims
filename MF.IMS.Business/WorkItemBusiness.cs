using AutoMapper;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.DomainModel;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using NUnit.Framework.Internal.Execution;
using Stripe;

namespace Mindflur.IMS.Business
{
	public class WorkItemBusiness : IWorkItemBusiness
	{
		private readonly IWorkItemRepository _workItemRepository;
		private readonly IMapper _mapper;
		private readonly IWorkItemStandardRepository _workItemStandardRepository;
		private readonly IAuditableItemBusiness _auditableItemBusiness;
		private readonly ITaskMasterRepository _taskMasterRepository;
		private readonly IMessageService _messageService;
		private readonly IUserRepository _userRepository;
		private readonly IEmailService _emailService;
		private readonly IWorkItemWorkItemTokenRepository _workItemTokenRepository;

		public WorkItemBusiness(IWorkItemRepository workItemRepository, IWorkItemStandardRepository workItemStandardRepository, IMapper mapper, IAuditableItemBusiness auditableItemBusiness, IMessageService messageService, IUserRepository userRepository, IEmailService emailService , ITaskMasterRepository taskMasterRepository, IWorkItemWorkItemTokenRepository workItemWorkItemTokenRepository)
		{
			_workItemRepository = workItemRepository;
			_workItemStandardRepository = workItemStandardRepository;
			_mapper = mapper;
			_auditableItemBusiness = auditableItemBusiness;
			_messageService = messageService;
			_userRepository = userRepository;
			_emailService = emailService;
			_taskMasterRepository = taskMasterRepository;
			_workItemTokenRepository = workItemWorkItemTokenRepository;
		}

		private WorkItemDomainModel TransformWorkitemDetail(WorkItemDomainModel workItemDomainModel)
		{
			/*
             * select * from MasterData where MasterDataGroupId=28 (workitem type category)
            223 Minor Non Conformance
            224 Major Non Conformance
            225 Opportunity
            226 Observation
            227 Risk
            */

			//Hack: categoryId is set for minor/major non conformance then set workitemtype category to non-conformance
			if (workItemDomainModel.CategoryId == 223 || workItemDomainModel.CategoryId == 224)
			{
				//Non-Conformance minor/major then set workitemtype category to non-conformance
				workItemDomainModel.WorkItemTypeId = 214;
				//ToDo: Create non-conformance metadata and add tag for nc type
			}
			else if (workItemDomainModel.CategoryId == 225)
			{
				//Opportunity
				workItemDomainModel.WorkItemTypeId = 217;
			}
			else if (workItemDomainModel.CategoryId == 226)
			{
				//Observation
				workItemDomainModel.WorkItemTypeId = 218;
			}
			else if (workItemDomainModel.CategoryId == 227)
			{
				//Risk
				workItemDomainModel.WorkItemTypeId = 216;
			}
			else
			{
				//when category not supplied then we need to asssume it would be as sourceId
				workItemDomainModel.WorkItemTypeId = workItemDomainModel.SourceId;
			}

			return workItemDomainModel;
		}

		private int GetSourceId(string path)
		{
			/*
             * select * from MasterData where MasterDataGroupId=29(Components)
             *
             * src\utility\constants.js
            export const WORKITEM_CATEGORY = {
                        INTERNAL_AUDIT: 228,
                        NON_CONFORMANCE: 214,
                        CORRECTIVE_ACTION: 215,
                        OBSERVATION_CATEGORY: 218,
                        OPPORTUNITY_CATEGORY: 217,
                        RISK_CATEGORY: 216,
                        INCIDENT_CATEGORY: 219,
                        PROJECT_CATEGORY: 220
                    }
            */

			if (path.Contains("internal-audit"))
				return 228;
			else if (path.Contains("non-conformance"))
				return 214;
			else if (path.Contains("corrective-action"))
				return 215;
			else if (path.Contains("observation"))
				return 218;
			else if (path.Contains("opportunity"))
				return 217;
			else if (path.Contains("risk"))
				return 216;
			else if (path.Contains("incident"))
				return 219;
			else if (path.Contains("project"))
				return 220;
			return 0;
		}

		public async Task AddFinding(FindingPostView findingPostView, int sourceItemId, int userId, int tenantId, string path)
		{
			var workitemDomainModel = _mapper.Map<WorkItemDomainModel>(findingPostView);

			workitemDomainModel.SourceItemId = sourceItemId; //Internal audit Id
			workitemDomainModel.CreatedBy = userId;
			workitemDomainModel.CreatedOn = DateTime.UtcNow;

			var itemStandard = await _auditableItemBusiness.GetStandardsForAuditItems(findingPostView.AuditableItemId);

			int[] result = { itemStandard.StandardsId };//Hack: Change to proper code

			workitemDomainModel.StandardId = result;

			workitemDomainModel = TransformWorkitemDetail(workitemDomainModel);

			workitemDomainModel.TenantId = tenantId;
			workitemDomainModel.SourceId = (int)IMSModules.InternalAudit;
			if (workitemDomainModel.CategoryId == (int)IMSMasterWorkItemCategory.MinorNonConformance)
			{
				workitemDomainModel.WorkItemTypeId = (int)IMSModules.NonConformity;
			}
			else if (workitemDomainModel.CategoryId == (int)IMSMasterWorkItemCategory.Observation)
			{
				workitemDomainModel.WorkItemTypeId = (int)IMSModules.Observation;
			}
			else if (workitemDomainModel.CategoryId == (int)IMSMasterWorkItemCategory.MajorNonConformance)
			{
				workitemDomainModel.WorkItemTypeId = (int)IMSModules.NonConformity;
			}
			else if (workitemDomainModel.CategoryId == (int)IMSMasterWorkItemCategory.Opportunity)
			{
				workitemDomainModel.WorkItemTypeId = (int)IMSModules.Opportunity;
			}
			else if (workitemDomainModel.CategoryId == (int)IMSMasterWorkItemCategory.Risk)
			{
				workitemDomainModel.WorkItemTypeId = (int)IMSModules.RiskManagement;
			}
			else
			{
				workitemDomainModel.WorkItemTypeId = (int)IMSModules.AuditFinding;
			}

			var workitem = await _workItemRepository.AddWorkItem(workitemDomainModel);

			await _workItemRepository.AddAuditableItem(findingPostView.AuditableItemId, findingPostView.FollowUp, workitem.WorkItemId, findingPostView.AuditChecklistId);

			Dictionary<int?, IMSControllerCategory> notificationCategories = new Dictionary<int?, IMSControllerCategory>
			{
				{ (int)IMSModules.NonConformity, IMSControllerCategory.NonConformance },
				{ (int)IMSModules.CorrectiveAction, IMSControllerCategory.CorrectiveAction },
				{ (int)IMSModules.Observation, IMSControllerCategory.Observations },
				{ (int)IMSModules.IncidentManagement, IMSControllerCategory.IncidentManagement },
				{ (int)IMSModules.RiskManagement, IMSControllerCategory.RiskManagement },
				{ (int)IMSModules.ProjectManagement, IMSControllerCategory.ProjectManagement },
				{ (int)IMSModules.Opportunity, IMSControllerCategory.Opportunities },
			};
			var responsibleUser = await _userRepository.GetUserDetail(workitemDomainModel.ResponsibleUserId, tenantId);
			var users = await _userRepository.GetUserBytenantId(tenantId);
			var emailUsers = users.Where(t => t.UserId == workitem.ResponsibleUserId || t.RoleId == (int)IMSRolesMaster.Manager).ToList();
			if (notificationCategories.TryGetValue(workitemDomainModel.WorkItemTypeId, out IMSControllerCategory category2))
			{
				foreach (var details in emailUsers)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();//AUDITABLE_ITEM_ID
					keyValuePairs.Add("#AUDITABLE_ITEM_ID#", findingPostView.AuditableItemId.ToString());
					keyValuePairs.Add("#RESPONSIBLE_PERSON#", details.FullName);
					keyValuePairs.Add("#AUDIT_FINDING#", workitem.Title);
					keyValuePairs.Add("#RESPONSIBLE_USER#", $"{responsibleUser.FirstName} {responsibleUser.LastName}");
					keyValuePairs.Add("#AUDIT_FINDING_TYPE#", category2.ToString());
					keyValuePairs.Add("#CREATED_ON#", workitemDomainModel.CreatedOn.ToString());
					keyValuePairs.Add("#DUEDATE#", workitemDomainModel.DueDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.FullName, "AuditFindingCreateEmailTemplate.html", $"Audit Finding Created >{workitem.WorkItemId}-  {workitem.Title} ", keyValuePairs);
				};
			}
			if (notificationCategories.TryGetValue(workitemDomainModel.WorkItemTypeId, out IMSControllerCategory category))
			{
				var notification = new NotificationMessage();

				notification.Title = findingPostView.Title;
				notification.BroadcastLevel = NotificationBroadcastLevel.Global;
				notification.EventType = NotificationEventType.BusinessMaster;
				notification.TenantId = tenantId;
				notification.Action = IMSControllerActionCategory.Create;
				notification.Module = category;
				notification.Description = findingPostView.Description;
				notification.ItemId = workitem.WorkItemId;
				notification.SourceIdUserId = userId;
				notification.Date = workitemDomainModel.CreatedOn;
				await _messageService.SendNotificationMessage(notification);
			}
		}

		public async Task AddWorkitem(WorkItemPostView workitemPostView, int userId, int tenantId, string path)
		{
			var workitemDomainModel = _mapper.Map<WorkItemDomainModel>(workitemPostView);
			workitemDomainModel.ResponsibleUserId = workitemPostView.ResponsibleUserId.HasValue ? workitemPostView.ResponsibleUserId : userId;
			workitemDomainModel.AssignedToUserId = workitemPostView.AssignToUserId;
			workitemDomainModel.CreatedBy = userId;
			workitemDomainModel.CreatedOn = DateTime.UtcNow;
			workitemDomainModel.TenantId = tenantId;

			var workitemdID = await _workItemRepository.AddWorkItem(workitemDomainModel);
			if(workitemDomainModel.WorkItemTypeId == (int)IMSMasterWorkItemType.Task)
			{
				var taskMetaData = new TaskMetaData();
				taskMetaData.WorkItemId = workitemdID.WorkItemId;

				taskMetaData.EstimateEffortHours = workitemPostView.EstimateEffortHours;
				await _taskMasterRepository.AddAsync(taskMetaData);

				var newTokens = new WorkItemWorkItemToken();


				newTokens.WorkItemId = workitemdID.WorkItemId;
				newTokens.TokenId = workitemPostView.TaskPriority;
                
				await _workItemTokenRepository.AddAsync(newTokens);


            }
			
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			Dictionary<int?, IMSControllerCategory> notificationCategories = new Dictionary<int?, IMSControllerCategory>
			{
				{ (int)IMSModules.NonConformity, IMSControllerCategory.NonConformance },
				{ (int)IMSModules.CorrectiveAction, IMSControllerCategory.CorrectiveAction },
				{ (int)IMSModules.Observation, IMSControllerCategory.Observations },
				{ (int)IMSModules.IncidentManagement, IMSControllerCategory.IncidentManagement },
				{ (int)IMSModules.RiskManagement, IMSControllerCategory.RiskManagement },
				{ (int)IMSModules.ProjectManagement, IMSControllerCategory.ProjectManagement },
				{ (int)IMSModules.Opportunity, IMSControllerCategory.Opportunities },
			};
			var responsibleUser = await _userRepository.GetUserDetail(workitemDomainModel.ResponsibleUserId, tenantId);
			var users = await _userRepository.GetUserBytenantId(tenantId);
			var emailUsers = users.Where(t => t.UserId == workitemPostView.ResponsibleUserId || t.RoleId==(int)IMSRolesMaster.Manager).ToList();
			if (notificationCategories.TryGetValue(workitemDomainModel.WorkItemTypeId, out IMSControllerCategory category2))
			{
				foreach (var details in emailUsers)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
					keyValuePairs.Add("#RESPONSIBLE_PERSON#", details.FullName);					
					keyValuePairs.Add("#WORK_ITEM#", workitemPostView.Title);
					keyValuePairs.Add("#RESPONSIBLE_USER#", $"{responsibleUser.FirstName} {responsibleUser.LastName}" );
					keyValuePairs.Add("#WORK_ITEM_TYPE#", category2.ToString());
					keyValuePairs.Add("#CREATED_ON#", workitemDomainModel.CreatedOn.ToString());
					keyValuePairs.Add("#DUEDATE#", workitemDomainModel.DueDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.FullName, "WorkItemEmailTemplate.html", $"Work Item Created >  {workitemPostView.Title} ", keyValuePairs);
				};
			}

			if (notificationCategories.TryGetValue(workitemDomainModel.WorkItemTypeId, out IMSControllerCategory category))
			{
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					BroadcastLevel = NotificationBroadcastLevel.Global,
					EventType = NotificationEventType.BusinessMaster,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = category,
					ItemId = workitemDomainModel.SourceItemId,
					Description = workitemPostView.Description,
					Title = workitemPostView.Title,
					Date = workitemDomainModel.CreatedOn
				});
			}
			if (notificationCategories.TryGetValue(workitemDomainModel.WorkItemTypeId, out IMSControllerCategory category1))
			{
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = workitemPostView.ResponsibleUserId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					BroadcastLevel = NotificationBroadcastLevel.Global,
					EventType = NotificationEventType.BusinessMaster,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Create,
					Module = category1,
					ItemId = workitemDomainModel.SourceItemId,
					Description = workitemPostView.Description,
					Title = workitemPostView.Title,
					Date = workitemDomainModel.CreatedOn
				});
			}
			// send notification and email to assigned  select template as per the worktypeId, activity log
		}

		public async Task UpdateWorkItem(WorkItemPutView workItemPutView, int userId, int workItemId, int tenantId, string path)
		{
			var existingWorkItem = await _workItemRepository.GetByIdAsync(workItemId);
			var existingWorkDomainModel = _mapper.Map<WorkItemDomainModel>(existingWorkItem);

			_mapper.Map(workItemPutView, existingWorkDomainModel);

			existingWorkDomainModel.StatusId = workItemPutView.Status;
			existingWorkDomainModel.AssignedToUserId = workItemPutView.AssignedToUserId;
			existingWorkDomainModel.CategoryId=workItemPutView.CategoryId;
			existingWorkDomainModel.UpdatedBy = userId;
			existingWorkDomainModel.UpdatedOn = DateTime.UtcNow;

			_mapper.Map(existingWorkDomainModel, existingWorkItem);
			existingWorkItem.StatusMasterDataId = existingWorkDomainModel.StatusId;
			await _workItemRepository.UpdateWorkItem(existingWorkItem);
			await _workItemRepository.AddStandardToWorkItem(workItemPutView, workItemId);
			if(existingWorkItem.WorkItemTypeId == (int)IMSMasterWorkItemType.Task)
			{
				var existingTask = await _taskMasterRepository.GetTaskByWorkItemId(workItemId);
				existingTask.EstimateEffortHours = workItemPutView.EstimateEffortHours;
				await _taskMasterRepository.UpdateAsync(existingTask);

				await _taskMasterRepository.EditTokenForTask(workItemPutView.TaskPrority, workItemId);

            }

			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			Dictionary<int?, IMSControllerCategory> notificationCategories = new Dictionary<int?, IMSControllerCategory>
			{
				{ (int)IMSModules.NonConformity, IMSControllerCategory.NonConformance },
				{ (int)IMSModules.CorrectiveAction, IMSControllerCategory.CorrectiveAction },
				{ (int)IMSModules.Observation, IMSControllerCategory.Observations },
				{ (int)IMSModules.IncidentManagement, IMSControllerCategory.IncidentManagement },
				{ (int)IMSModules.RiskManagement, IMSControllerCategory.RiskManagement },
				{ (int)IMSModules.ProjectManagement, IMSControllerCategory.ProjectManagement },
				{ (int)IMSModules.Opportunity, IMSControllerCategory.Opportunities },
			};
			var responsibleUser = await _userRepository.GetUserDetail(existingWorkDomainModel.ResponsibleUserId, tenantId);
			var users = await _userRepository.GetUserBytenantId(tenantId);
			var emailUsers = users.Where(t => t.UserId == existingWorkDomainModel.ResponsibleUserId || t.RoleId == (int)IMSRolesMaster.Manager).ToList();
			if (notificationCategories.TryGetValue(existingWorkDomainModel.WorkItemTypeId, out IMSControllerCategory category2))
			{
				foreach (var details in emailUsers)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
					keyValuePairs.Add("#RESPONSIBLE_PERSON#", details.FullName);
					keyValuePairs.Add("#WORK_ITEM#", existingWorkDomainModel.Title);
					keyValuePairs.Add("#RESPONSIBLE_USER#", $"{responsibleUser.FirstName} {responsibleUser.LastName}");
					keyValuePairs.Add("#WORK_ITEM_TYPE#", category2.ToString());
					keyValuePairs.Add("#CREATED_ON#", existingWorkDomainModel.CreatedOn.ToString());
					keyValuePairs.Add("#DUEDATE#", existingWorkDomainModel.DueDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.FullName, "WorkItemUpdateEmailTemplate.html", $"Work Item Updated > {workItemId} - {existingWorkDomainModel.Title} ", keyValuePairs);
				};
			}
			if (notificationCategories.TryGetValue(existingWorkDomainModel.WorkItemTypeId, out IMSControllerCategory category))
			{
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					BroadcastLevel = NotificationBroadcastLevel.Global,
					EventType = NotificationEventType.BusinessMaster,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Edit,
					Module = category,
					ItemId = workItemId,
					Description = workItemPutView.Description,
					Title = workItemPutView.Title,
					Date = existingWorkDomainModel.UpdatedOn
				});
			}
			if (notificationCategories.TryGetValue(existingWorkDomainModel.WorkItemTypeId, out IMSControllerCategory category1))
			{
				await _messageService.SendNotificationMessage(new NotificationMessage()
				{
					SourceIdUserId = workItemPutView.ResponsibleUserId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					BroadcastLevel = NotificationBroadcastLevel.Global,
					EventType = NotificationEventType.BusinessMaster,
					TenantId = tenantId,
					Action = IMSControllerActionCategory.Edit,
					Module = category1,
					ItemId = workItemId,
					Description = workItemPutView.Description,
					Title = workItemPutView.Title,
					Date = existingWorkDomainModel.UpdatedOn
				});
			}

			// send notification to user and email to assigend to user , activitylog
		}

		public async Task<PaginatedItems<WorkItemGridView>> GetWorkItemList(GetWorkItemGridRequest getWorkItemList)
		{
			return await _workItemRepository.GetWorkItemList(getWorkItemList);
		}

		public async Task<PaginatedItems<WorkItemGridView>> GetPastWorkItemList(GetWorkItemGridRequest getWorkItemList)
		{
			return await _workItemRepository.GetPastWorkItemList(getWorkItemList);
		}

		public async Task<WorkItemPreview> GetPreviewWorkItem(int workItemId, int tenantId)
		{
			var workItem = await _workItemRepository.GetPreviewWorkItemById(workItemId, tenantId);
			var risk = await _workItemRepository.GetRiskBy(workItemId);
			var task = await _taskMasterRepository.GetTasksDetailsForWorkItem(workItemId);

			if (workItem == null)
			{
				workItem = new WorkItemPreview();
				return workItem;
			}
			else if (workItem.WorkItemId == workItemId && workItem.TenantId == tenantId)
			{
				WorkItemPreview workItemPreView = new WorkItemPreview();
				workItemPreView.WorkItemId = workItem.WorkItemId;
				workItemPreView.WorkitemTypeId = workItem.WorkitemTypeId;
				workItemPreView.AuditableItemId = workItem.AuditableItemId;
				//workItemPreView.AuditableItem = workItem.AuditableItem;
				workItemPreView.SourceId = workItem.SourceId;
				workItemPreView.TenantId = workItem.TenantId;
				workItemPreView.Source = workItem.Source;
				workItemPreView.SourceItemId = workItem.SourceItemId;
				workItemPreView.SourceItem = workItem.SourceItem;
				workItemPreView.Title = workItem.Title;
				workItemPreView.Description = workItem.Description;
				workItemPreView.CategoryId = workItem.CategoryId;
				workItemPreView.Category = workItem.Category;
				workItemPreView.DepartmentId = workItem.DepartmentId;
				workItemPreView.Department = workItem.Department;
				workItemPreView.StatusId = workItem.StatusId;
				workItemPreView.Status = workItem.Status;
				workItemPreView.AssignedToUserId = workItem.AssignedToUserId;
				workItemPreView.AssignedToUser = workItem.AssignedToUser;
				workItemPreView.ResponsibleUserId = workItem.ResponsibleUserId;
				workItemPreView.ResponsibleUser = workItem.ResponsibleUser;
				workItemPreView.DueDate = workItem.DueDate;
				workItemPreView.CreatedById = workItem.CreatedById;
				workItemPreView.CreatedBy = workItem.CreatedBy;
				workItemPreView.CreatedOn = workItem.CreatedOn;
				workItemPreView.UpdatedOn = workItem.UpdatedOn;
				workItemPreView.UpdatedById = workItem.UpdatedById;
				workItemPreView.UpdatedBy = workItem.UpdatedBy;
				workItemPreView.ParentWorkItemId = workItem.ParentWorkItemId;
				if(task == null)
				{
					workItemPreView.TaskEstimateHours = null;
					workItemPreView.TaskPriority = null;
					workItemPreView.TaskPriorityId = null;

				}
				else
				{
					workItemPreView.TaskEstimateHours = task.EstimateEffortsHours;
                    workItemPreView.TaskPriority = task.TaskPriority;
                    workItemPreView.TaskPriorityId = task.TaskPriorityId;
                }
				if (risk == null)
				{
					workItemPreView.RiskId = null;
					workItemPreView.RiskCreatedOn = null;
				}
				else
				{
					workItemPreView.RiskId = risk.Id;
					workItemPreView.RiskCreatedOn = risk.CreatedOn;
				}
				var standard = await _workItemStandardRepository.GetWorkItemStandards(workItemId);
				IList<StandardView> standards = new List<StandardView>();
				foreach (StandardDataView standardDataView in standard)
				{
					standards.Add(new StandardView() { StandardId = standardDataView.StandardId, StandardName = standardDataView.StandardName });
				}
				workItemPreView.Standards = standards;
				if (workItemPreView.Standards.Any())
				{
					var standardname = await _workItemStandardRepository.GetWorkWorkItemStandardsISO(workItemId);
					workItemPreView.SelectedStandardId = standardname.StandardNameId;
					workItemPreView.SelectedStandard = standardname.StandardName;
				}

				return workItemPreView;
			}
			else
			{
				throw new BadRequestException("WorkItem Not Found");
			}
		}

		public async Task DeleteWorkItem(int workItemId, int userId, int tenantId)
		{
			var workItem = await _workItemRepository.GetByIdAsync(workItemId);
			if (workItem == null)
			{
				throw new NotFoundException("WorkItem", workItemId);
			}
			else if (workItem.WorkItemId == workItemId && workItem.TenantId == tenantId)
			{
				Dictionary<int?, IMSControllerCategory> notificationCategories = new Dictionary<int?, IMSControllerCategory>
			{
				{ (int)IMSModules.NonConformity, IMSControllerCategory.NonConformance },
				{ (int)IMSModules.CorrectiveAction, IMSControllerCategory.CorrectiveAction },
				{ (int)IMSModules.Observation, IMSControllerCategory.Observations },
				{ (int)IMSModules.IncidentManagement, IMSControllerCategory.IncidentManagement },
				{ (int)IMSModules.RiskManagement, IMSControllerCategory.RiskManagement },
				{ (int)IMSModules.ProjectManagement, IMSControllerCategory.ProjectManagement },
				{ (int)IMSModules.Opportunity, IMSControllerCategory.Opportunities },
			};
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				if (notificationCategories.TryGetValue(workItem.WorkItemTypeId, out IMSControllerCategory category))
				{
					await _messageService.SendNotificationMessage(new NotificationMessage()
					{
						SourceIdUserId = workItem.CreatedBy,
						SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
						BroadcastLevel = NotificationBroadcastLevel.Global,
						EventType = NotificationEventType.BusinessMaster,
						TenantId = tenantId,
						Action = IMSControllerActionCategory.Delete,
						Module = category,
						ItemId = workItemId,
						Description = workItem.Description,
						Title = workItem.Title,
						Date = workItem.UpdatedOn
					});
				}
				var responsibleUser = await _userRepository.GetUserDetail(workItem.ResponsibleUserId, tenantId);
				var users = await _userRepository.GetUserBytenantId(tenantId);
				var emailUsers = users.Where(t => t.UserId == workItem.ResponsibleUserId || t.RoleId == (int)IMSRolesMaster.Manager).ToList();
				if (notificationCategories.TryGetValue(workItem.WorkItemTypeId, out IMSControllerCategory category2))
				{
					foreach (var details in emailUsers)
					{
						IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
						keyValuePairs.Add("#RESPONSIBLE_PERSON#", details.FullName);
						keyValuePairs.Add("#WORK_ITEM#", workItem.Title);
						keyValuePairs.Add("#RESPONSIBLE_USER#", $"{responsibleUser.FirstName} {responsibleUser.LastName}");
						keyValuePairs.Add("#WORK_ITEM_TYPE#", category2.ToString());
						keyValuePairs.Add("#CREATED_ON#", workItem.CreatedOn.ToString());
						keyValuePairs.Add("#DUEDATE#", workItem.DueDate.ToString());
						await _emailService.SendEmail(details.EmailAddress, details.FullName, "WorkItemDeleteEmailTemplate.html", $"Work Item Deleted > {workItem.WorkItemId}- {workItem.Title} ", keyValuePairs);
					};
				}
				await _workItemRepository.DeleteAsync(workItem);
			}
			else
			{
				throw new BadRequestException("WorkItem Not Found");
			}

			
		}

		public async Task<IList<GetNcEmailDetails>> OverDueRemiderForNc()
		{
			var nonConformance = await _workItemRepository.OverDueRemiderForNc();
			return nonConformance;
		}

		public async Task<IList<GetNcEmailDetails>> NightlyRemiderForNc()
		{
			var nonConformance = await _workItemRepository.NightlyRemiderForNc();
			return nonConformance;
		}

		public async Task<IList<GetCaEmailDetails>> OverDueRemiderForCA()
		{
			var correctiveAction = await _workItemRepository.OverDueRemiderForCA();
			return correctiveAction;
		}

		public async Task<IList<GetCaEmailDetails>> NightlyRemiderForCA()
		{
			var correctiveAction = await _workItemRepository.NightlyRemiderForCA();
			return correctiveAction;
		}

		public async Task<IList<GetRiskEmailDetails>> OverDueRemiderForRisk()
		{
			var risk = await _workItemRepository.OverDueRemiderForRisk();
			return risk;
		}

		public async Task<IList<GetRiskEmailDetails>> NightlyRemiderForRisk()
		{
			var risk = await _workItemRepository.NightlyRemiderForRisk();
			return risk;
		}

		public async Task<IList<EmailDetailsForOpportunity>> OverDueRemiderForOpportunity()
		{
			var opportunity = await _workItemRepository.OverDueRemiderForOpportunity();
			return opportunity;
		}

		public async Task<IList<EmailDetailsForOpportunity>> NightlyRemiderForOpportunity()
		{
			var opportunity = await _workItemRepository.NightlyRemiderForOpportunity();
			return opportunity;
		}

		public async Task<IList<EmailDetailsForObservation>> NightlyRemiderForObservation()
		{
			var observation = await _workItemRepository.NightlyRemiderForObservation();
			return observation;
		}

		public async Task<IList<EmailDetailsForObservation>> OverDueRemiderForObservation()
		{
			var observation = await _workItemRepository.OverDueRemiderForObservation();
			return observation;
		}

		public async Task<IList<EmailDetailsForIncident>> NightlyRemiderForIncident()
		{
			var incidents = await _workItemRepository.NightlyRemiderForIncident();
			return incidents;
		}

		public async Task<IList<EmailDetailsForIncident>> OverDueRemiderForIncident()
		{
			var incidents = await _workItemRepository.OverDueRemiderForIncident();
			return incidents;
		}

		public async Task<IList<WorkItemDropDownView>> GetNcDropDown(int tenantid)
		{
			return await _workItemRepository.GetNcDropDown(tenantid);
		}

		public async Task<IList<WorkItemDropDownView>> GetIncidentDropDown(int tenantid)
		{
			return await _workItemRepository.GetIncidentDropDown(tenantid);
		}

		public async Task AddNcToRisk(int tenantId, int workItemId, int userId, string path)
		{
			var nc = await _workItemRepository.GetByIdAsync(workItemId);
			WorkItemPostView work = new WorkItemPostView();
			work.SourceId = (int)IMSModules.NonConformity;
			work.SourceItemId = nc.WorkItemId;
			work.WorkItemTypeId = (int)IMSModules.RiskManagement;
			work.Title = nc.Title;
			work.Description = nc.Description;
			work.DepartmentId = nc.DepartmentId;
			work.ResponsibleUserId = nc.ResponsibleUserId;
			work.AssignToUserId = nc.AssignedToUserId;
			work.DueDate = nc.DueDate;

			await AddWorkitem(work, userId, tenantId, path);
		}
	}
}