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

namespace Mindflur.IMS.Business
{
	public class AuditClauseBusiness : IAuditClauseBusiness
	{
		private readonly IAuditableItemClauseRepository _auditableItemClauseRepository;
		private readonly IAuditItemClauseRepository _auditItemClauseRepository;
		private readonly IClauseMasterRepository _clauseMasterRepository;
		private readonly IMessageService _messageService;
		private readonly IActivityLogRepository _activityLogRepository;
		private readonly IAuditItemsRepository _auditItemsRepository;
		private readonly ICommentRepository _commentRepository;
		private readonly IUserRepository _userRepository;
		private readonly IEmailService _emailService;
		private readonly IDepartmentMasterRepository _departmentMasterRepository;

		public AuditClauseBusiness(IAuditableItemClauseRepository auditableItemClauseRepository, IAuditItemClauseRepository auditItemClauseRepository,
			IClauseMasterRepository clauseMasterRepository, IMessageService messageService, IActivityLogRepository activityLogRepository, IAuditItemsRepository auditItemsRepository
, ICommentRepository commentRepository, IUserRepository userRepository, IEmailService emailService, IDepartmentMasterRepository departmentMasterRepository)

		{
			_auditableItemClauseRepository = auditableItemClauseRepository;
			_auditItemClauseRepository = auditItemClauseRepository;
			_clauseMasterRepository = clauseMasterRepository;
			_messageService = messageService;
			_activityLogRepository = activityLogRepository;
			_auditItemsRepository = auditItemsRepository;
			_commentRepository = commentRepository;
			_userRepository = userRepository;
			_emailService = emailService;
			_departmentMasterRepository= departmentMasterRepository;
		}

		public async Task<IList<AuditableItemClause>> GetAuditableItemClause()
		{
			return await _auditableItemClauseRepository.GetAuditableItemClause();
		}

		public async Task<AuditableItemClause> AddAuditableItemClause(PostAuditClause ac)
		{
			AuditableItemClause aic = new AuditableItemClause();
			aic.AuditableItemId = ac.AuditItemId;
			aic.MasterDataStandardId = ac.MasterDataStandardId;
			// aic.Comment = ac.Comment;
			await _auditableItemClauseRepository.AddAsync(aic);

			AuditItemClause c = new AuditItemClause();
			c.AuditableItemClauseId = aic.AuditableItemClauseId;
			foreach (int a in ac.Clauses)
			{
				c.Id = 0;
				c.ClauseMasterId = a;
				await _auditItemClauseRepository.AddAsync(c);
			}
			return aic;
		}

		public async Task<AuditableItemClause> UpdateAuditableItemClause(int Id, PutAuditClauseViewModel putAuditClause)
		{
			return await _auditableItemClauseRepository.UpdateAuditableItemClause(Id, putAuditClause);
		}

		public async Task<AuditableItemClause> GetAuditableItemClauseById(int clauseId)
		{
			var clause = await _auditableItemClauseRepository.GetByIdAsync(clauseId);
			return clause == null ? throw new NotFoundException(string.Format(ConstantsBusiness.ClauseIdNotFoundErrorMessage), clauseId) : clause;
		}

		public async Task DeleteAuditableItemsClause(int clauseId)
		{
			var clause = await _auditableItemClauseRepository.GetByIdAsync(clauseId);
			if (clause == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ClauseIdNotFoundErrorMessage), clauseId);
			}
			await _auditableItemClauseRepository.DeleteAsync(clause);
		}

		//AuditItemClause

		public async Task<IList<AuditItemClause>> GetAuditItemClauses()
		{
			return await _auditItemClauseRepository.GetAuditItemClauses();
		}

		public async Task<AuditItemClause> AddAuditItemClause(AuditItemClause clauses)
		{
			return await _auditItemClauseRepository.AddAsync(clauses);
		}

		public async Task<AuditItemClause> GetAuditItemClauseById(int clauseId)
		{
			var clause = await _auditItemClauseRepository.GetByIdAsync(clauseId);
			return clause == null ? throw new NotFoundException(string.Format(ConstantsBusiness.ClauseIdNotFoundErrorMessage), clauseId) : clause;
		}

		public async Task EditAuditItemClause(int clauseId, AuditItemClause clauses)
		{
			var rawdata = await _auditItemClauseRepository.GetByIdAsync(clauseId);
			if (rawdata == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ClauseIdNotFoundErrorMessage), rawdata);
			}
			else
			{
				rawdata.ClauseMasterId = clauses.ClauseMasterId;
				rawdata.AuditableItemClauseId = clauses.AuditableItemClauseId;
				await _auditItemClauseRepository.UpdateAsync(rawdata);
			}
		}

		public async Task DeleteAuditItemClause(int clausesId)
		{
			var clause = await _auditItemClauseRepository.GetByIdAsync(clausesId);
			if (clause == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ClausesNotFoundErrorMessage), clausesId);
			}
			await _auditItemClauseRepository.DeleteAsync(clause);
		}

		//Clause Master
		public async Task<IList<ClauseMaster>> GetAllClause()
		{
			return await _clauseMasterRepository.GetAllClause();
		}

		public async Task<ClauseMaster> AddClausesToMaster(ClauseMaster clauseMaster)
		{
			return await _clauseMasterRepository.AddAsync(clauseMaster);
		}

		public async Task<ClauseMaster> GetClauseMasterById(int clauseId)
		{
			var clauseMaster = await _clauseMasterRepository.GetByIdAsync(clauseId);
			return clauseMaster == null ? throw new NotFoundException(string.Format(ConstantsBusiness.ClauseMasterNotFoundErrorMessage), clauseId) : clauseMaster;
		}

		public async Task UpdateClauseMaster(int clauseId, ClauseMaster clauseMaster)
		{
			var clauses = await _clauseMasterRepository.GetByIdAsync(clauseId);
			if (clauses == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ClauseMasterNotFoundErrorMessage), clauseId);
			}
			else
			{
				clauses.ClauseNo = clauseMaster.ClauseNo;
				clauses.ClauseName = clauseMaster.ClauseName;
				await _clauseMasterRepository.UpdateAsync(clauses);
			}
		}

		public async Task DeleteClauseMaster(int clauseId)
		{
			var clauses = await _clauseMasterRepository.GetByIdAsync(clauseId);
			if (clauses == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ClauseMasterNotFoundErrorMessage), clauseId);
			}
			await _clauseMasterRepository.DeleteAsync(clauses);
		}

		public async Task AddAuditableItems(PostAuditableItemView auditableItem, int userId, int tenantId)
		{
			AuditableItem items = new AuditableItem();
			items.AuditProgramId = auditableItem.AuditProgramId;
			//items.Department = auditableItem.DepartmentName;
			items.Description = auditableItem.Description;
			//items.Type = auditableItem.Type;
			items.Status = (int)IMSItemStatus.New;
			items.StartDate = auditableItem.StartDate;
			items.EndDate = auditableItem.EndDate;
			items.AuditorName = auditableItem.AuditorName;
			items.DepartmentId = auditableItem.DepartmentId;
			items.CreatedBy = userId;
			items.CreatedOn = DateTime.UtcNow;
			await _auditItemsRepository.AddAsync(items);
			//Sending mail to ISO Champion and Manager :To Do have to send a single mail for Auditable Items

			var usersListByTenantId = await _userRepository.GetUserBytenantId(tenantId);
			var userListByDepartment = usersListByTenantId.Where(t => t.DepartmentId == items.DepartmentId).ToList();
			var isoChampionAndDepartmentManager = userListByDepartment.Where(t => t.RoleId == (int)IMSRolesMaster.ISOChampion || t.RoleId == (int)IMSRolesMaster.Manager).ToList();
			var departement = await _departmentMasterRepository.GetByIdAsync(items.DepartmentId);
			var keyValuePairs = new Dictionary<string, string>
			{
				{ "#AUDIT_ITEM_ID#", items.Id.ToString() } ,
				{ "#AUDIT_ITEM#", departement.DepartmentName } ,
				//{ "#AUDIT_ITEM_TYPE#", items.Type.ToString() }, //Hack For Now
				{ "#START_DATE#", items.StartDate.ToString() },
				{ "#END_DATE#", items.EndDate.ToString() }
			};

			foreach (var user in isoChampionAndDepartmentManager)
			{
				keyValuePairs["#AUDITORS_NAME#"] = user.FullName;
				await _emailService.SendEmail(user.EmailAddress, user.FullName, "AuditItemCreate.html", $"AuditItem Created > {items.Id} - {departement.DepartmentName} ", keyValuePairs);
			}
			AuditableItemClause aic = new AuditableItemClause();
			aic.AuditableItemId = items.Id;
			aic.MasterDataStandardId = auditableItem.MasterDataStandardId;
			// aic.Comment = auditableItem.Comment;
			await _auditableItemClauseRepository.AddAsync(aic);
			AuditItemClause c = new AuditItemClause();
			c.AuditableItemClauseId = aic.AuditableItemClauseId;
			foreach (int a in auditableItem.Clauses)
			{
				c.Id = 0;
				c.ClauseMasterId = a;
				await _auditItemClauseRepository.AddAsync(c);
			}
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.AuditItemClause,
				ItemId = items.Id,
				Description = items.Description,
				Title = departement.DepartmentName,
				Date = items.CreatedOn,
			});
			

			//For Auditor Who has been Assigned audit item
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = items.AuditorName,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.AuditableItem,
				ItemId = items.Id,
				Description = items.Description,
				Title = departement.DepartmentName,
				Date = items.CreatedOn,
			});

			ActivityLog activityLog = new ActivityLog();
			activityLog.TenantId = tenantId;
			activityLog.ControllerId = (int)IMSControllerCategory.AuditableItem;
			activityLog.EntityId = items.Id;
			activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
			activityLog.Description = "Audit Item Has Been Created ";
			activityLog.Details = System.Text.Json.JsonSerializer.Serialize(auditableItem);
			activityLog.Status = true;
			activityLog.CreatedBy = userId;
			activityLog.CreatedOn = DateTime.UtcNow;
			await _activityLogRepository.AddAsync(activityLog);
		}

		public async Task<PaginatedItems<AuditItemView>> GetAuditItems(AuditItemListView auditItemListView)
		{
			var auditableItemsPaginatedList = await _auditItemsRepository.GetAuditItems(auditItemListView);

			var auditableItemsList = auditableItemsPaginatedList.Data?.ToList();

			for (int i = 0; i < auditableItemsList.Count(); i++)
			{
				var clauseList = await _auditItemClauseRepository.GetClausesByAuditItemId(auditableItemsList[i].Id);

				if (clauseList != null)
				{
					auditableItemsList[i].Clauses = clauseList.Select(c => c.ClauseName).ToArray();
				}
			}

			auditableItemsPaginatedList.Data = auditableItemsList.AsEnumerable();

			return auditableItemsPaginatedList;
		}

		public async Task<AuditableItem> GetAuditableItemsById(int auditableItemId)
		{
			var auditItems = await _auditItemsRepository.GetByIdAsync(auditableItemId);
			return auditItems == null ? throw new NotFoundException(string.Format(ConstantsBusiness.AuditItemNotFound), auditableItemId) : auditItems;
		}

		public async Task<AuditItemPreviewWithclauses> GetAuditableItemsPreview(int auditableItemId)
		{
			var auditItems = await _auditItemsRepository.GetAuditableItemsPreview(auditableItemId);
			if (auditItems == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditItemNotFound), auditableItemId);
			}
			else
			{
				AuditItemPreviewWithclauses preview = new AuditItemPreviewWithclauses();

				preview.Id = auditItems.Id;
				preview.AuditProgramId = auditItems.AuditProgramId;
				//preview.AuditableItems = auditItems.AuditableItems;
				preview.Description = auditItems.Description;
				//preview.TypeId = auditItems.TypeId;
				//preview.Type = auditItems.Type;
				preview.StatusId = auditItems.StatusId;
				preview.Status = auditItems.Status;
				preview.StartDate = auditItems.StartDate;
				preview.EndDate = auditItems.EndDate;
				preview.ResponsibleUserId = auditItems.AuditorId;
				preview.ResponsibleUser = auditItems.AuditorName;
				preview.DepartmentId = auditItems.DepartmentId;
				preview.Department = auditItems.DepartmentName;
				preview.CreatedById = auditItems.CreatedById;
				preview.CreatedBy = auditItems.CreatedBy;
				preview.CreatedOn = auditItems.CreatedOn;
				preview.UpdatedById = auditItems.UpdatedById;
				preview.UpdatedBy = auditItems.UpdatedBy;
				preview.UpdatedOn = auditItems.UpdatedOn;
				var auditItemStandards = await _auditItemsRepository.GetAuditableItemStandrads(auditableItemId);
				preview.StandardId = auditItemStandards.MasterDataStandardId;
				preview.Standard = auditItemStandards.Standards;
				var auditItemClauses = await _auditItemsRepository.GetAuditableItemsClauses(auditableItemId);
				IList<ClausesView> clauses = new List<ClausesView>();
				foreach (ClausesDataView audititem in auditItemClauses)
				{
					clauses.Add(new ClausesView() { ClauseId = audititem.ClauseId, ClauseNo = audititem.ClauseNo });
				}
				preview.Clauses = clauses;

				var auditItemComments = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.AuditableItems, auditableItemId);
				IList<CommentsView> comments = new List<CommentsView>();
				foreach (var comment in auditItemComments)
				{
					comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
				}
				preview.Comments = comments;

				return preview;
			}
		}

		public async Task<IList<GetAuditDetailsForReport>> GetAuditItemsDetailsForPlan(int auditId)
		{
			return await _auditItemsRepository.GetAuditItemsDetailsForPlan(auditId);
		}

		public async Task DeleteAuditableItems(int auditableItemId, int tenantId, int userId)
		{
			var items = await _auditItemsRepository.GetByIdAsync(auditableItemId);
			
			if (items == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditTableItemsNotFound), auditableItemId);
			}
			var usersListByTenantId = await _userRepository.GetUserBytenantId(tenantId);
			var userListByDepartment = usersListByTenantId.Where(t => t.DepartmentId == items.DepartmentId).ToList();
			var isoChampionAndDepartmentManager = userListByDepartment.Where(t => t.RoleId == (int)IMSRolesMaster.ISOChampion || t.RoleId == (int)IMSRolesMaster.Manager).ToList();
            var departement = await _departmentMasterRepository.GetByIdAsync(items.DepartmentId);
            var keyValuePairs = new Dictionary<string, string>
			{
				{ "#AUDIT_ITEM_ID#", items.Id.ToString() },
				{ "#AUDIT_ITEM#", departement.DepartmentName },
				//{ "#AUDIT_ITEM_TYPE#", items.Type.ToString() }, //Hack For Now
				{ "#START_DATE#", items.StartDate.ToString() },
				{ "#END_DATE#", items.EndDate.ToString() }
			};

			foreach (var user in isoChampionAndDepartmentManager)
			{
				keyValuePairs["#AUDITORS_NAME#"] = user.FullName;
				await _emailService.SendEmail(user.EmailAddress, user.FullName, "AuditItemDelete.html", $"AuditItem Delete > {items.Id} - {departement.DepartmentName}", keyValuePairs);
			}
			await _auditItemsRepository.DeleteAsync(items);

			ActivityLog activityLog = new ActivityLog();
			activityLog.TenantId = tenantId;
			activityLog.ControllerId = (int)IMSControllerCategory.AuditableItem;
			activityLog.EntityId = items.Id;
			activityLog.ModuleAction = (int)IMSControllerActionCategory.Delete;
			activityLog.Description = "Audit Item Has Been Deleted ";
			activityLog.Details = System.Text.Json.JsonSerializer.Serialize(items);
			activityLog.Status = true;
			activityLog.CreatedBy = userId;
			activityLog.CreatedOn = DateTime.UtcNow;
			await _activityLogRepository.AddAsync(activityLog);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.AuditableItem,
				ItemId = items.Id,
				Description = items.Description,
				Title = items.Description,
				Date = activityLog.CreatedOn
			});
		}

		public async Task CloseAuditItem(CommentsForReviewViewModel commentsForReviewViewModel, int tenantId, int id, int userId)
		{
			var items = await _auditItemsRepository.GetByIdAsync(id);
			if (items != null)
			{
				items.Status = (int)IMSItemStatus.Closed;
				await _auditItemsRepository.UpdateAsync(items);

				Comment comments = new Comment();
				comments.CommentContent = commentsForReviewViewModel.Comments;
				comments.SourceId = (int)IMSModules.AuditableItems;
				comments.SourceItemId = id;
				comments.ParentCommentId = comments.CommentId;
				comments.ContentType = 1;
				comments.CreatedBy = userId;
				comments.CreatedOn = DateTime.UtcNow;
				await _commentRepository.AddAsync(comments);
			}
			else
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditItemNotFound), id);
			}
		}

		public async Task StartAuditItem(CommentsForReviewViewModel commentsForReviewViewModel, int tenantId, int id, int userId)
		{
			var items = await _auditItemsRepository.GetByIdAsync(id);
			if (items != null)
			{
				if (items.StartDate != DateTime.Today)
				{
					throw new BadRequestException(string.Format(ConstantsBusiness.AuditItemStartNotFound));
				}
				else
				{
					items.Status = (int)IMSItemStatus.Open;
					await _auditItemsRepository.UpdateAsync(items);

					Comment comments = new Comment();
					comments.CommentContent = commentsForReviewViewModel.Comments;
					comments.SourceId = (int)IMSModules.AuditableItems;
					comments.SourceItemId = id;
					comments.ParentCommentId = comments.CommentId;
					comments.ContentType = 1;
					comments.CreatedBy = userId;
					comments.CreatedOn = DateTime.UtcNow;
					await _commentRepository.AddAsync(comments);
					var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
					await _messageService.SendNotificationMessage(new NotificationMessage()
					{
						SourceIdUserId = userId,
						SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
						EventType = NotificationEventType.BusinessMaster,
						BroadcastLevel = NotificationBroadcastLevel.Global,
						TenantId = tenantId,
						Action = IMSControllerActionCategory.Create,
						Module = IMSControllerCategory.AuditableItem,
						ItemId = items.Id,
						Description = items.Description,
						Title = items.Description,
						Date = items.CreatedOn
					});
				}
			}
			else
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.AuditItemNotFound), id);
			}
		}

		public async Task<IList<AuditItemsView>> GetAuditItemsByProgram(int auditId)
		{
			return await _auditItemsRepository.GetAuditItemsByProgram(auditId);
		}

		public async Task<IList<ParticipantDropDownList>> GetParticipantList(int moduleEntityId, int moduleId)
		{
			return await _auditItemsRepository.GetParticipantList(moduleEntityId, moduleId);
		}

		public async Task<AuditableItem> EditAuditableItems(int auditableItemId, PutAuditableItemViewModel auditableItem, int userId, int tenantId)
		{
			var items = await _auditItemsRepository.UpdateAuditableItems(auditableItemId, auditableItem, userId, tenantId);
			
			//Sending mail to ISO Champion and Manager :To Do have to send a single mail for Auditable Items

			var usersListByTenantId = await _userRepository.GetUserBytenantId(tenantId);
			var userListByDepartment = usersListByTenantId.Where(t => t.DepartmentId == items.DepartmentId).ToList();
			var isoChampionAndDepartmentManager = userListByDepartment.Where(t => t.RoleId == (int)IMSRolesMaster.ISOChampion || t.RoleId == (int)IMSRolesMaster.Manager).ToList();
            var departement = await _departmentMasterRepository.GetByIdAsync(items.DepartmentId);
            var keyValuePairs = new Dictionary<string, string>
			{
				{ "#AUDIT_ITEM_ID#", items.Id.ToString() },
				{ "#AUDIT_ITEM#", departement.DepartmentName },
				//{ "#AUDIT_ITEM_TYPE#", items.Type.ToString() }, //Hack For Now
				{ "#START_DATE#", items.StartDate.ToString() },
				{ "#END_DATE#", items.EndDate.ToString() }
			};

			foreach (var user in isoChampionAndDepartmentManager)
			{
				keyValuePairs["#AUDITORS_NAME#"] = user.FullName;
				await _emailService.SendEmail(user.EmailAddress, user.FullName, "AuditItemEdit.html", $"AuditItem ReCreate > {items.Id} -{departement.DepartmentName} ", keyValuePairs);
			}
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);

			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				Action = IMSControllerActionCategory.Edit,
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Module = IMSControllerCategory.AuditableItem,
				ItemId = items.Id,
				Description = items.Description,
				Title = departement.DepartmentName,
				Date = items.UpdatedOn
			});

			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = items.AuditorName,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				Action = IMSControllerActionCategory.Create,
				EventType = NotificationEventType.BusinessMaster,
				BroadcastLevel = NotificationBroadcastLevel.Global,
				TenantId = tenantId,
				Module = IMSControllerCategory.AuditableItem,
				ItemId = items.Id,
				Description = items.Description,
				Title = departement.DepartmentName,
				Date = items.UpdatedOn
			});
			return items;
		}
	}
}