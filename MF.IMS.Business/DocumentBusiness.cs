using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.Word;
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
using Mindflur.IMS.Data.Repository;
using Stripe;

namespace Mindflur.IMS.Business
{
	public class DocumentBusiness : IDocumentBusiness
	{
		private readonly IDocumentRepository _documentRepository;
		private readonly IDocumentTagsRepository _documentTagsRepository;
		private readonly IDocumentUserRepository _documentUserRepository;
		private readonly IDocumentRoleRepository _documentRoleRepository;
		private readonly IActivityLogRepository _activityLogRepository;
		private readonly ICommentBusiness _commentBusiness;
		private readonly ICommentRepository _commentRepository;
		private readonly IEmailService _emailService;
		private readonly IUserRepository _userRepository;

		public DocumentBusiness(IDocumentRepository documentRepository, IDocumentTagsRepository documentTagsRepository, IActivityLogRepository activityLogRepository, ICommentBusiness commentBusiness, ICommentRepository commentRepository, IEmailService emailService, IUserRepository userRepository, IDocumentUserRepository documentUserRepository, IDocumentRoleRepository documentRoleRepository)
		{
			_documentRepository = documentRepository;
			_documentTagsRepository = documentTagsRepository;
			_documentUserRepository = documentUserRepository;
            _documentRoleRepository = documentRoleRepository;
            _activityLogRepository = activityLogRepository;
			_commentBusiness = commentBusiness;
			_commentRepository = commentRepository;
			_emailService = emailService;
			_userRepository = userRepository;
		}

		public async Task<PaginatedItems<DocumentGridView>> GetDocumentList(GetDocumentListRequest getListRequest)
		{
			return await _documentRepository.GetDocumentList(getListRequest);
		}

		public async Task<DocumentPreview> GetDocumentsById(int documentId, int tenantId, int userId)
		{
			var document = await _documentRepository.GetDocumentById(documentId, tenantId);
			
			if (document == null)
			{
				document = new DocumentPreview();
				return document;
            }
            
			else
			{
				DocumentPreview documentGridView = new DocumentPreview();    
				 documentGridView.DocumentId = document.DocumentId;
				documentGridView.TenantId = tenantId;
				documentGridView.DocumentNumber = document.DocumentNumber;
				documentGridView.Title = document.Title;
				documentGridView.Purpose = document.Purpose;
				documentGridView.HtmlContent = document.HtmlContent;
				documentGridView.DocumentCategoryId = document.DocumentCategoryId;
				documentGridView.DocumentCategory = document.DocumentCategory;
				documentGridView.DocumentPriority = document.DocumentPriority;
				documentGridView.DocumentPriorityId = document.DocumentPriorityId;
				documentGridView.DocumentStatusId = document.DocumentStatusId;
				documentGridView.DocumentStatus = document.DocumentStatus;
				documentGridView.DocumentReviewFrequencyMasterDataId = document.DocumentReviewFrequencyMasterDataId;
				documentGridView.DocumentReviewFrequencyMasterData = document.DocumentReviewFrequencyMasterData;
				documentGridView.NextReviewDate = document.NextReviewDate;
				documentGridView.IsPublish = document.IsPublish;
				documentGridView.ApprovedId = document.ApprovedId;
				documentGridView.ApprovedBy = document.ApprovedBy;
				documentGridView.CreatedBy = document.CreatedBy;
				documentGridView.CreatedById = document.CreatedById;
				documentGridView.CreatedOn = document.CreatedOn;
				documentGridView.UpdatedBy = document.UpdatedBy;
				documentGridView.UpdatedById = document.UpdatedById;
				documentGridView.UpdatedOn = document.UpdatedOn;

				var documentTags = await _documentTagsRepository.GetDocumentTags(documentId);
				IList<TagView> tags = new List<TagView>();

				foreach (TagDataView projectTag in documentTags)
				{
					tags.Add(new TagView() { TagId = projectTag.TagId, TagName = projectTag.TagName });
				}

				

				var ncComment = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.DocumentManagement, documentId);

				IList<CommentsView> comments = new List<CommentsView>();
				foreach (var comment in ncComment)
				{
					comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
				}

				documentGridView.DocumentTags = tags;
				

                documentGridView.Comments = comments;

				return documentGridView;
			}
		}

		public async Task AddDocument(PostDocumentView postDocumentView, int tenantId, int userId)
		{

			Documents doc = new Documents();
			doc.TenantId = tenantId;
			doc.Title = postDocumentView.Title;
			doc.Purpose = postDocumentView.Purpose;
			doc.HtmlContent = postDocumentView.HtmlContent;
			doc.DocumentNumber = postDocumentView.DocumentNumber;
			doc.DocumentCategoryMasterDataId = postDocumentView.DocumentCategoryMasterDataId;
			doc.DocumentStatusMasterDataId = (int)IMSDocumentStatus.Draft;
			doc.DocumentPriorityMasterDataId = postDocumentView.DocumentPriorityMasterDataId;
			doc.ReviewFreqencyMasterDataId = postDocumentView.ReviewFrequencyMasterDataId;
			if (doc.ReviewFreqencyMasterDataId == (int)IMSReviewFrequencyMasterDataId.Yearly)
			{
				doc.NextReviewDate = DateTime.UtcNow.AddYears(1);
			}
			else if (doc.ReviewFreqencyMasterDataId == (int)IMSReviewFrequencyMasterDataId.HalfYearly)
			{
				doc.NextReviewDate = DateTime.UtcNow.AddMonths(6);
			}
			else if (doc.ReviewFreqencyMasterDataId == (int)IMSReviewFrequencyMasterDataId.Quarterly)
			{
				doc.NextReviewDate = DateTime.UtcNow.AddMonths(3);
			}
			else
			{
				doc.NextReviewDate = DateTime.UtcNow.AddMonths(1);
			}
			doc.CreatedBy = userId;
			doc.CreatedOn = DateTime.UtcNow;
			doc.IsPublish = false;
			await _documentRepository.AddAsync(doc);
			ActivityLog activityLog = new ActivityLog();
			activityLog.TenantId = doc.TenantId;
			activityLog.ControllerId = (int)IMSControllerCategory.DocumentManagement;
			activityLog.EntityId = doc.DocumentId;
			activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
			activityLog.Description = "Document has been Created";
			activityLog.Details = System.Text.Json.JsonSerializer.Serialize(doc);
			activityLog.Status = true;
			activityLog.CreatedBy = userId;
			activityLog.CreatedOn = DateTime.UtcNow;
			await _activityLogRepository.AddAsync(activityLog);
			DocumentTags documentTags = new DocumentTags();
			documentTags.DocumentId = doc.DocumentId;
			foreach (int a in postDocumentView.MasterDataDocumentTagId)
			{
				documentTags.DocumentTagId = 0;
				documentTags.MasterDataDocumentTagId = a;
				await _documentTagsRepository.AddAsync(documentTags);
			}
            
            var user = await _userRepository.GetUserByUserId(userId);
			var document = await _documentRepository.GetDocumentById(doc.DocumentId, tenantId);
			
			
				IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
				keyValuePairs.Add("#AUDITORS_NAME#", user.FullName);
				keyValuePairs.Add("#DOCUMENT_ID#", document.DocumentId.ToString());
				keyValuePairs.Add("#DOCUMENT_TITLE#", document.Title);
				keyValuePairs.Add("#DOCUMENT_PURPOSE#", document.Purpose);
				keyValuePairs.Add("#DOCUMENT_CATEGORY#", document.DocumentCategory.ToString());
				keyValuePairs.Add("#CREATED_ON#", document.CreatedOn.ToString());
				keyValuePairs.Add("#NEXT_REVIEW_DATE#", document.NextReviewDate.ToString());
				await _emailService.SendEmail(user.EmailAddress, user.FullName, "DocumentCreate.html", $"Document Create > {document.DocumentId} - {document.Title} ", keyValuePairs);
			
		}

		public async Task<Documents> UpdateDocument(PutDocumentView putDocument, int id, int tenantId, int userId)
		{
			return await _documentRepository.UpdateDocument(putDocument, id, tenantId, userId);
		}

		public async Task DeleteDocument(int id, int userId, int tenantId)
		{
			var document = await _documentRepository.GetByIdAsync(id);
			if (document == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.DocumentNotFoundErrorMessage), id);
			}
			else if (document.TenantId == tenantId && document.DocumentId == id)
			{
				document.DocumentStatusMasterDataId = (int)IMSDocumentStatus.Deleted;

				document.DeletedBy = userId;
				document.DeletedOn = DateTime.UtcNow;
				await _documentRepository.UpdateAsync(document);
				ActivityLog activityLog = new ActivityLog();
				activityLog.TenantId = document.TenantId;
				activityLog.ControllerId = (int)IMSControllerCategory.DocumentManagement;
				activityLog.EntityId = document.DocumentId;
				activityLog.ModuleAction = (int)IMSControllerActionCategory.Delete;
				activityLog.Description = "Document has been Deleted";
				activityLog.Details = System.Text.Json.JsonSerializer.Serialize(document);
				activityLog.Status = true;
				activityLog.CreatedBy = userId;
				activityLog.CreatedOn = DateTime.UtcNow;
				await _activityLogRepository.AddAsync(activityLog);
				var user = await _userRepository.GetUserBytenantId(tenantId);
				var documents = await _documentRepository.GetDocumentById(id, tenantId);
				var userList = user.Where(t => t.RoleId == (int)IMSRolesMaster.ISOChampion||t.RoleId== document.ApprovedBy || t.RoleId == (int)IMSRolesMaster.Manager).ToList(); //// ApprovedBy  values are  NUll in database
				foreach (var details in userList)
				{
					IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
					keyValuePairs.Add("#AUDITORS_NAME#", details.FullName);
					keyValuePairs.Add("#DOCUMENT_ID#", documents.DocumentId.ToString());
					keyValuePairs.Add("#DOCUMENT_TITLE#", documents.Title);
					keyValuePairs.Add("#DOCUMENT_PURPOSE#", documents.Purpose);
					keyValuePairs.Add("#DOCUMENT_CATEGORY#", documents.DocumentCategory.ToString());
					keyValuePairs.Add("#CREATED_ON#", documents.CreatedOn.ToString());
					keyValuePairs.Add("#NEXT_REVIEW_DATE#", documents.NextReviewDate.ToString());
					await _emailService.SendEmail(details.EmailAddress, details.FullName, "DocumentDeleted.html", $"Document Delete > {documents.DocumentId} - {documents.Title} ", keyValuePairs);
				}
			}
		}

		public async Task ReviewDocument(int id, int userId, int tenantId, CommentsForDocument document)
		{
			var doc = await _documentRepository.GetByIdAsync(id);
			if (doc == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.DocumentNotFoundErrorMessage), id);
			}
			else
			{
				doc.DocumentStatusMasterDataId = (int)IMSDocumentStatus.InReview;
				await _documentRepository.UpdateAsync(doc);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.DocumentManagement;
				postCommentView.SourceItemId = id;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = document.Comments;

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
		}

		public async Task RejectDocument(int id, int userId, int tenantId, CommentsForDocument document)
		{
			var doc = await _documentRepository.GetByIdAsync(id);
			if (doc == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.DocumentNotFoundErrorMessage), id);
			}
			else
			{
				doc.DocumentStatusMasterDataId = (int)IMSDocumentStatus.Rejected;
				await _documentRepository.UpdateAsync(doc);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.DocumentManagement;
				postCommentView.SourceItemId = id;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = document.Comments;

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
		}

		public async Task ApproveDocument(int id, int userId, int tenantId, CommentsForDocument document)
		{
			var doc = await _documentRepository.GetByIdAsync(id);
			if (doc == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.DocumentNotFoundErrorMessage), id);
			}
			else
			{
				doc.ApprovedOn = DateTime.UtcNow;
				doc.ApprovedBy = userId;
				doc.DocumentStatusMasterDataId = (int)IMSDocumentStatus.Approved;
				await _documentRepository.UpdateAsync(doc);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.DocumentManagement;
				postCommentView.SourceItemId = id;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = document.Comments;

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
		}

		public async Task PublishDocument(int id, int userId, int tenantId, CommentsForDocument document)
		{
			var doc = await _documentRepository.GetByIdAsync(id);
			if (doc == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.DocumentNotFoundErrorMessage), id);
			}
			else
			{
				doc.IsPublish = true;
				doc.PublishedOn = DateTime.UtcNow;
				doc.DocumentStatusMasterDataId = (int)IMSDocumentStatus.Published;
				await _documentRepository.UpdateAsync(doc);
				var postCommentView = new PostCommentView();
				postCommentView.SourceId = (int)IMSModules.DocumentManagement;
				postCommentView.SourceItemId = id;
				postCommentView.ParentCommentId = 0;
				postCommentView.ContentType = 1;
				postCommentView.CommentContent = document.Comments;

				await _commentBusiness.AddComment(postCommentView, userId, tenantId);
			}
		}

		public async Task<IList<DocumentDropDown>> getDatabyMasterGroupId(int tenantId)
		{
			return await _documentRepository.getDatabyMasterGroupId(tenantId);
		}
	}
}