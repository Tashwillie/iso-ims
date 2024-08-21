using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public class DocumentChangeRequestBusiness : IDocumentChangeRequestBusiness
    {

        private readonly IDocumentChangeRequestRepositroy _documentChangeRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ICommentBusiness _commentBusiness;
        private readonly IEmailService _emailService;
        private readonly ICommentRepository _commentRepository;


        public DocumentChangeRequestBusiness(IDocumentChangeRequestRepositroy documentChangeRequestRepository, IUserRepository userRepository, IDocumentRepository documentRepository, IEmailService emailService, ICommentBusiness commentBusiness, ICommentRepository commentRepository)
        {
            _documentChangeRequestRepository = documentChangeRequestRepository;
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _emailService = emailService;
            _commentBusiness = commentBusiness;
            _commentRepository = commentRepository;
        }

        public async Task<PaginatedItems<DocumentChangeRequestGridView>> GetAllDocumentList(GetDocumentChangeRequest getListRequest)
        {
            return await _documentChangeRequestRepository.GetAllDocumentList(getListRequest);
        }
        public async Task<DocumentChangeRequestReportDeatils> GetDocumentChangeRequestReports(int documentId, int tenantId)
        {
            return await _documentChangeRequestRepository.GetDocumentChangeRequestReports(documentId, tenantId);
        }

        public async Task AddDcoumentRequest(PostDocumetChangeRequestView documetRequestView, int tenantId, int userId)
        {
            DocumentChangeRequestMaster document = new DocumentChangeRequestMaster();


            document.DocumentId = documetRequestView.DocumentId;   
            document.TenantId = tenantId;
            document.Reason = documetRequestView.Reason;
            document.Description = documetRequestView.Description;  
            document.Consequences = documetRequestView.Consequences;
            document.ChangeRequestStatusMasterDataId = (int)IMSDocumentStatus.New;
            document.ChangeRequestTypeMasterDataId = documetRequestView.ChangeRequestTypeMasterDataId;
            document.VersionTypeId = documetRequestView.VersionTypeId;
            document.RequestedBy = userId;
            document.RequestedOn = DateTime.UtcNow;
            await _documentChangeRequestRepository.AddAsync(document);

            var documents = await _documentChangeRequestRepository.GetDocumentRequestEmail(document.ChangeRequestId, tenantId);
            var change = await _documentRepository.GetDocumentById(document.DocumentId, tenantId);

            var user = await _userRepository.GetUserByUserId(change.CreatedById);

           
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#AUDITORS_NAME#", user.FullName);
                keyValuePairs.Add("#CHANGE_REQUEST_ID#", documents.ChangeRequestId.ToString());
                keyValuePairs.Add("#DOCUMENT_ID#", documents.DocumentId.ToString());
                keyValuePairs.Add("#DOCUMENT_TITLE#", documents.DocumentTitle);
                keyValuePairs.Add("#REASON#", documents.Reason);
                keyValuePairs.Add("#CONSQUENCES#", documents.Consequences);
                keyValuePairs.Add("#REQUESTED_BY#", documents.RequestedBy.ToString());
                keyValuePairs.Add("#REQUESTED_ON#", documents.RequestedOn.ToString());
                await _emailService.SendEmail(user.EmailAddress, user.FirstName, "DocumentChangeRequest.html", $"Document Change Request Added > {documents.DocumentId} - {documents.DocumentTitle} ", keyValuePairs);
            

        }

        public async Task<DocumentChangeRequestPreView> GetDocumentRequestDetails(int ChangeRequestId, int tenantId)
        {
           var document =  await _documentChangeRequestRepository.GetDocumentRequestDetails (ChangeRequestId, tenantId);
            if (document == null)
            {
                document = new DocumentChangeRequestPreView();
                return document;
            }
            else
            {
                DocumentChangeRequestPreView documentChangeRequestPreView = new DocumentChangeRequestPreView();
                documentChangeRequestPreView.ChangeRequestId = document.ChangeRequestId;
                documentChangeRequestPreView.TenantId = document.TenantId;
                documentChangeRequestPreView.DocumentId = document.DocumentId;
                documentChangeRequestPreView.Document = document.Document;
                documentChangeRequestPreView.ReferenceCode = document.ReferenceCode;
                documentChangeRequestPreView.Reason = document.Reason;
                documentChangeRequestPreView.Description = document.Description;
                documentChangeRequestPreView.Consequences = document.Consequences;
                documentChangeRequestPreView.ChangeRequestStatusMasterDataId = document.ChangeRequestStatusMasterDataId;
                documentChangeRequestPreView.ChangeRequestStatusMasterData = document.ChangeRequestStatusMasterData;
                documentChangeRequestPreView.VersionTypeId = document.VersionTypeId;
                documentChangeRequestPreView.VersionType = document.VersionType;
                documentChangeRequestPreView.ChangeRequestTypeMasterDataId = document.ChangeRequestTypeMasterDataId;
                documentChangeRequestPreView.ChangeRequestTypeMasterData = document.ChangeRequestTypeMasterData;


                var Comments = await _commentRepository.GetCommentsBySourceIdAndSourceItemId((int)IMSModules.DocumentChangeRequest, ChangeRequestId);
                IList<CommentsView> comments = new List<CommentsView>();
                foreach (var comment in Comments)
                {
                    comments.Add(new CommentsView() { CommentId = comment.CommentId, CommentContent = comment.CommentContent, ParentCommentId = comment.ParentCommentId, CreatedBy = comment.CreatedBy, CreatedOn = comment.CreatedOn });
                }
                documentChangeRequestPreView.Comments = comments;

                return documentChangeRequestPreView;
            }
        }

        public async Task<DocumentChangeRequestEmail> GetDocumentRequestEmail(int ChangeRequestId, int tenantId)
        {
            return await _documentChangeRequestRepository.GetDocumentRequestEmail(ChangeRequestId, tenantId);
        }

        public async Task UpdateDocumentRequest(PutDocumentChangeRequestView documentRequestPutView, int documentId, int tenantId,int userId)
        {
            var rawData = await _documentChangeRequestRepository.GetByIdAsync(documentId);
            if (rawData == null)
            {
                throw new NotFoundException("Document ", documentId);
            }
            else if( rawData.RequestedBy == userId)
            {
                rawData.DocumentId = documentRequestPutView.DocumentId;
                rawData.TenantId = tenantId;
                rawData.Reason = documentRequestPutView.Reason;
                rawData.Description = documentRequestPutView.Description;
                rawData.Consequences = documentRequestPutView.Consequences;
                rawData.ChangeRequestStatusMasterDataId = (int)IMSDocumentStatus.New;
                rawData.ChangeRequestTypeMasterDataId = documentRequestPutView.ChangeRequestTypeMasterDataId;
                rawData.VersionTypeId = documentRequestPutView.VersionTypeId;
                rawData.RequestedOn = DateTime.UtcNow;
                rawData.RequestedBy = userId;

                await _documentChangeRequestRepository.UpdateAsync(rawData);

                var documents = await _documentChangeRequestRepository.GetDocumentRequestEmail(rawData.ChangeRequestId, tenantId);
                var change = await _documentRepository.GetDocumentById(rawData.DocumentId, tenantId);

                var user = await _userRepository.GetUserByUserId(change.CreatedById);
                                
                    IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    keyValuePairs.Add("#AUDITORS_NAME#", user.FullName);
                    keyValuePairs.Add("#CHANGE_REQUEST_ID#", documents.ChangeRequestId.ToString());
                    keyValuePairs.Add("#DOCUMENT_ID#", documents.DocumentId.ToString());
                    keyValuePairs.Add("#DOCUMENT_TITLE#", documents.DocumentTitle);
                    keyValuePairs.Add("#REASON#", documents.Reason);
                    keyValuePairs.Add("#CONSQUENCES#", documents.Consequences);
                    keyValuePairs.Add("#REQUESTED_BY#", documents.RequestedBy.ToString());
                    keyValuePairs.Add("#REQUESTED_ON#", documents.RequestedOn.ToString());
                    await _emailService.SendEmail(user.EmailAddress, user.FirstName, "DocumentChangeRequestEdit.html", $"Document Change Request Edit > {documents.DocumentId} - {documents.DocumentTitle} ", keyValuePairs);
                
            }
            else
            {
                var requested = await _userRepository.GetUserByUserId(rawData.RequestedBy);
                throw new Exception($" Cannot Update Change Request as it is Created By  {requested.FullName}");
            }
            
        }

        public async Task DeleteDocumentRequest(int documentId, int tenantId)
        {
            var data = await _documentChangeRequestRepository.GetByIdAsync(documentId);
            if (data == null)
            {
                throw new NotFoundException("Document", documentId);

            }
            var documents = await _documentChangeRequestRepository.GetDocumentRequestEmail(data.ChangeRequestId, tenantId);
            var change = await _documentRepository.GetDocumentById(data.DocumentId, tenantId);

            var user = await _userRepository.GetUserBytenantId(tenantId);
            var userList = user.Where(t => t.RoleId == (int)IMSRolesMaster.ISOChampion || t.RoleId == (int)IMSRolesMaster.Manager).ToList();

            foreach (var details in userList)
            {
                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("#AUDITORS_NAME#", details.FullName);
                keyValuePairs.Add("#CHANGE_REQUEST_ID#", documents.ChangeRequestId.ToString());
                keyValuePairs.Add("#DOCUMENT_ID#", documents.DocumentId.ToString());
                keyValuePairs.Add("#DOCUMENT_TITLE#", documents.DocumentTitle);
                keyValuePairs.Add("#REASON#", documents.Reason);
                keyValuePairs.Add("#CONSQUENCES#", documents.Consequences);
                keyValuePairs.Add("#REQUESTED_BY#", documents.RequestedBy.ToString());
                keyValuePairs.Add("#REQUESTED_ON#", documents.RequestedOn.ToString());
                await _emailService.SendEmail(details.EmailAddress, details.FirstName, "DocumentChangeRequestDeleted.html", $"Document Change Request Deleted > {documents.DocumentId} - {documents.DocumentTitle} ", keyValuePairs);
            }
            
            await _documentChangeRequestRepository.DeleteAsync(data);

            

        }

        public async Task ApproveDocument(int id, int userId, int tenantId, CommentsForDocument document)
        {
            var doc = await _documentChangeRequestRepository.GetByIdAsync(id);
            var change = await _documentRepository.GetDocumentById(doc.DocumentId, tenantId);
            if (doc == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.DocumentNotFoundErrorMessage), id);
            }
            else if( change.CreatedById == userId)
            {
                doc.ApprovedOn = DateTime.UtcNow;
                doc.ApprovedBy = userId;
                doc.ChangeRequestStatusMasterDataId = (int)IMSDocumentStatus.Approved;
                await _documentChangeRequestRepository.UpdateAsync(doc);
                var postCommentView = new PostCommentView();
                postCommentView.SourceId = (int)IMSModules.DocumentChangeRequest;
                postCommentView.SourceItemId = id;
                postCommentView.ParentCommentId = 0;
                postCommentView.ContentType = 1;
                postCommentView.CommentContent = document.Comments;

                await _commentBusiness.AddComment(postCommentView, userId, tenantId);

                // need to implement Email
            }
            else
            {
                var requested = await _userRepository.GetUserByUserId(change.CreatedById);
                throw new Exception($" Cannot Approve Change Request as Document Belongs to   {requested.FullName}");
            }
        }

        public async Task RejectDocument(int id, int userId, int tenantId, CommentsForDocument document)
        {
            var doc = await _documentChangeRequestRepository.GetByIdAsync(id);
            var change = await _documentRepository.GetDocumentById(doc.DocumentId, tenantId);
            if (doc == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.DocumentNotFoundErrorMessage), id);
            }
            else if (change.CreatedById == userId)
            {
                doc.RejectedBy = userId;
                doc.RejectedOn = DateTime.Now;
                doc.ChangeRequestStatusMasterDataId = (int)IMSDocumentStatus.Rejected;
                await _documentChangeRequestRepository.UpdateAsync(doc);
                var postCommentView = new PostCommentView();
                postCommentView.SourceId = (int)IMSModules.DocumentManagement;
                postCommentView.SourceItemId = id;
                postCommentView.ParentCommentId = 0;
                postCommentView.ContentType = 1;
                postCommentView.CommentContent = document.Comments;

                await _commentBusiness.AddComment(postCommentView, userId, tenantId);
            }
            else
            {
                var requested = await _userRepository.GetUserByUserId(change.CreatedById);
                throw new Exception($" Cannot Reject Change Request as Document Belongs to   {requested.FullName}");
            }
        }
    }

}
