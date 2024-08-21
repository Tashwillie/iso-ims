using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class DocumentRepository : BaseRepository<Documents>, IDocumentRepository
    {
		private readonly IEmailService _emailService;
		private readonly IUserRepository _userRepository;
	

		public DocumentRepository(IMSDEVContext dbContext, ILogger<Documents> logger, IEmailService emailService, IUserRepository userRepository) : base(dbContext, logger)
        {
			
            _emailService = emailService;
            _userRepository = userRepository;

		}

        public async Task<PaginatedItems<DocumentGridView>> GetDocumentList(GetDocumentListRequest getListRequest)
        {

            var document = (from doc in _context.Documents
                            join documentCategory in _context.MasterData on doc.DocumentCategoryMasterDataId equals documentCategory.Id into documentCategories
                            from subdocumentCategory in documentCategories.DefaultIfEmpty()

                            join documentPriority in _context.MasterData on doc.DocumentPriorityMasterDataId equals documentPriority.Id into documentPriorities
                            from subdocumentPriority in documentPriorities.DefaultIfEmpty()

                            join documentStatus in _context.MasterData on doc.DocumentStatusMasterDataId equals documentStatus.Id into documentStatuses
                            from subdocumentStatus in documentStatuses.DefaultIfEmpty()

                            join documentReviewFreqency in _context.MasterData on doc.ReviewFreqencyMasterDataId equals documentReviewFreqency.Id into documentReviewFreqencies
                            from subdocumentReviewFreqency in documentReviewFreqencies.DefaultIfEmpty()

                            join user in _context.UserMasters on doc.ApprovedBy equals user.UserId into users
                            from subusers in users.DefaultIfEmpty()
                            join createdBy in _context.UserMasters on doc.CreatedBy equals createdBy.UserId into createdBy
                            from SubCreateBy in createdBy.DefaultIfEmpty()
                            join tm in _context.TenanttMasters on doc.TenantId equals tm.TenantId
                            where doc.TenantId == getListRequest.TenantId && doc.DeletedOn == null
                            select new DocumentGridView()
                            {
                                DocumentId = doc.DocumentId,
                                Title = doc.Title,
                                Purpose = doc.Purpose,
                                DocumentNumber = doc.DocumentNumber,
                                DocumentCategory = subdocumentCategory.Items,
                                DocumentCategoryId = subdocumentCategory.Id,
                                DocumentStatus = subdocumentStatus.Items,
                                DocumentStatusId = doc.DocumentStatusMasterDataId,
                                DocumentPriority = subdocumentPriority.Items,
                                DocumentPriorityId = doc.DocumentPriorityMasterDataId.HasValue ? doc.DocumentPriorityMasterDataId : 0,
                                ReviewFreqency = subdocumentReviewFreqency.Items,
                                ReviewFreqencyId = doc.ReviewFreqencyMasterDataId.HasValue ? doc.ReviewFreqencyMasterDataId : 0,
                                CreatedBy = $"{SubCreateBy.FirstName} {SubCreateBy.LastName}",
                                CreatedById = doc.CreatedBy,
                                NextReviewDate=doc.NextReviewDate,
                                CreatedOn= doc.CreatedOn,

                            }).OrderByDescending(doc => doc.DocumentId).AsQueryable();
            if (getListRequest.DocumentCategoryId > 0)
            {
                document=document.Where(log=>log.DocumentCategoryId==getListRequest.DocumentCategoryId);
            }


            var filteredData = DataExtensions.OrderBy(document, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
                              .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                              .Take(getListRequest.ListRequests.PerPage);

            var totalItems = await document.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<DocumentGridView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages,  filteredData);
            return await Task.FromResult(model);
        }

        public async Task<DocumentPreview> GetDocumentById(int documentId, int tenantId)
        {

            var document = (from doc in _context.Documents
                            join md in _context.MasterData on doc.DocumentCategoryMasterDataId equals md.Id into md
                            from subCategory in md.DefaultIfEmpty()
                            join status in _context.MasterData on doc.DocumentStatusMasterDataId equals status.Id into status
                            from subStatus in status.DefaultIfEmpty()
                            join priority in _context.MasterData on doc.DocumentPriorityMasterDataId equals priority.Id into priority
                            from subPriority in priority.DefaultIfEmpty()
                            join frequency in _context.MasterData on doc.ReviewFreqencyMasterDataId equals frequency.Id into frequency
                            from subFrequency in frequency.DefaultIfEmpty()
                            join tm in _context.TenanttMasters on doc.TenantId equals tm.TenantId
                            join user in _context.UserMasters on doc.ApprovedBy equals user.UserId into users
                            from subusers in users.DefaultIfEmpty()
							join createdBy in _context.UserMasters on doc.CreatedBy equals createdBy.UserId into createdBy
							from subcreatedBy in createdBy.DefaultIfEmpty()
							join updatedBy in _context.UserMasters on doc.UpdatedBy equals updatedBy.UserId into updatedBy
							from subupdatedBy in updatedBy.DefaultIfEmpty()
							where documentId == doc.DocumentId && tenantId == doc.TenantId

                            select new DocumentPreview()
                            {
                                DocumentId = doc.DocumentId,
                                TenantId=doc.TenantId,
								Title = doc.Title,
                                Purpose=doc.Purpose,
								DocumentNumber = doc.DocumentNumber,
								HtmlContent = doc.HtmlContent,
								DocumentCategoryId = doc.DocumentCategoryMasterDataId ,
                                DocumentCategory = subCategory.Items,
                                DocumentPriorityId=doc.DocumentPriorityMasterDataId,
                                DocumentPriority= subPriority.Items,
                                DocumentStatusId = doc.DocumentStatusMasterDataId,
                                DocumentStatus = subStatus.Items,
                                DocumentReviewFrequencyMasterData= subFrequency.Items,
                                DocumentReviewFrequencyMasterDataId= doc.ReviewFreqencyMasterDataId,   
                                NextReviewDate=doc.NextReviewDate,
                                ApprovedId = doc.ApprovedBy,
                                ApprovedBy = $"{subusers.FirstName}{subusers.LastName}",
                                IsPublish=doc.IsPublish,
                                CreatedBy= $"{subcreatedBy.FirstName}{subcreatedBy.LastName}",
                                CreatedById=subcreatedBy.UserId,
                                UpdatedBy= $"{subupdatedBy.FirstName}{subupdatedBy.LastName}",
                                UpdatedById= subupdatedBy.UserId,
								UpdatedOn =doc.UpdatedOn,
                                CreatedOn=doc.CreatedOn

							}).AsQueryable();
            return document.FirstOrDefault();

        }


        public async Task<Documents> UpdateDocument(PutDocumentView putDocument, int id, int tenantId, int userId)
        {


            var doc = await _context.Documents.Where(d => d.DocumentId == id).FirstOrDefaultAsync();
            if (doc == null)
            {
                throw new NotFoundException(string.Format(RepositoryConstant.DocumentNotFoundErrorMessage), id);
            }
            else if (doc.TenantId == tenantId && doc.DocumentId == id  && doc.CreatedBy == userId)
            {
                var documentTags = _context.DocumentTags.Where(ds => ds.DocumentId == id).ToList();
                _context.DocumentTags.RemoveRange(documentTags);
                await _context.SaveChangesAsync();

               

                var tagsForDocumentTags = new List<DocumentTags>();


                foreach (int documentTag in putDocument.MasterDataDocumentTagId)
                {
                    var newDocumentTags = new DocumentTags
                    {
                        DocumentId = id,
                        MasterDataDocumentTagId = documentTag,

                    };

                    tagsForDocumentTags.Add(newDocumentTags);
                }
                await _context.DocumentTags.AddRangeAsync(tagsForDocumentTags);
                await _context.SaveChangesAsync();

               

                doc.TenantId = tenantId;                
				doc.Title = putDocument.Title;
				doc.Purpose = putDocument.Purpose;
				doc.HtmlContent = putDocument.HtmlContent;
                doc.DocumentNumber = putDocument.DocumentNumber;
				doc.DocumentCategoryMasterDataId = putDocument.DocumentCategoryMasterDataId;
				doc.DocumentStatusMasterDataId = (int)IMSDocumentStatus.Draft;
				doc.DocumentPriorityMasterDataId = putDocument.DocumentPriorityMasterDataId;
				doc.ReviewFreqencyMasterDataId = putDocument.ReviewFrequencyMasterDataId;
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
				doc.UpdatedOn = DateTime.UtcNow;
                doc.UpdatedBy = userId;
                await _context.SaveChangesAsync();
            }
            else
            {
                var createdUser = await _userRepository.GetUserByUserId(doc.CreatedBy);
                throw new Exception($"Cannot Edit As is was created By {createdUser.FullName}");
            }
            var user = await _userRepository.GetUserByUserId(userId);
            var documents = await GetDocumentById(id, tenantId);
			
			
				IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
				keyValuePairs.Add("#AUDITORS_NAME#", user.FullName);
				keyValuePairs.Add("#DOCUMENT_ID#", documents.DocumentId.ToString());
				keyValuePairs.Add("#DOCUMENT_TITLE#", documents.Title);
				keyValuePairs.Add("#DOCUMENT_PURPOSE#", documents.Purpose);
				keyValuePairs.Add("#DOCUMENT_CATEGORY#", documents.DocumentCategory.ToString());
				keyValuePairs.Add("#CREATED_ON#", documents.CreatedOn.ToString());
				keyValuePairs.Add("#NEXT_REVIEW_DATE#", documents.NextReviewDate.ToString());
				await _emailService.SendEmail(user.EmailAddress, user.FullName, "DocumentEdit.html", $"Document Edit > {documents.DocumentId} - {documents.Title} ", keyValuePairs);
			
			return doc;

        }

        public async Task<IList<DocumentDropDown>>getDatabyMasterGroupId(int tenantId )
        {
            var rawData = await (from dc in _context.Documents 
                                 
                                 where dc.TenantId == tenantId && dc.DeletedOn == null
                                 select new DocumentDropDown
                                 {
                                     DocumentId = dc.DocumentId,
                                     Document =dc.Title
                                    
                                 })
                                 .OrderByDescending(md=>md.DocumentId).ToListAsync();
            return await Task.FromResult(rawData);
        }

    }
}
