using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class DocumentChangeRequestRepository : BaseRepository<DocumentChangeRequestMaster>, IDocumentChangeRequestRepositroy
    {
        
        public DocumentChangeRequestRepository(IMSDEVContext dbContext, ILogger<DocumentChangeRequestMaster> logger) : base(dbContext, logger)
        {
           
        }

        public async Task<PaginatedItems<DocumentChangeRequestGridView>> GetAllDocumentList(GetDocumentChangeRequest getListRequest)
        {
            var document = (from request in _context.DocumentChangeRequestMasters
                            join doc in _context.Documents on request.DocumentId equals doc.DocumentId
                            join docUser in _context.UserMasters on doc.CreatedBy equals docUser.UserId

                            join status in _context.MasterData on request.ChangeRequestStatusMasterDataId equals status.Id into status
                            from subdocumentStatus in status.DefaultIfEmpty()

                            join category in _context.MasterData on request.ChangeRequestTypeMasterDataId equals category.Id into category
                            from subdocumentCategory in category.DefaultIfEmpty()
                            join versionType in _context.MasterData on request.VersionTypeId equals versionType.Id into versionType
                            from subVersionType in versionType.DefaultIfEmpty()

                            join tm in _context.TenanttMasters on request.TenantId equals tm.TenantId
                            
                            select new DocumentChangeRequestGridView()
                            {
                                ChangeRequestId = request.ChangeRequestId,
                                TenantId= request.TenantId,
                                DocumentId = request.DocumentId,
                                Document=doc.Title,
                                DocumentOwned = $"{docUser.FirstName} {docUser.LastName}",
                                ReferenceCode = request.ReferenceCode.HasValue ? request.ReferenceCode : 0,
                                Reason = request.Reason,
                                Consequences = request.Consequences,
                                ChangeRequestStatusMasterDataId = request.ChangeRequestStatusMasterDataId,
                                ChangeRequestStatusMasterData = subdocumentStatus.Items,
                                ChangeRequestTypeMasterDataId = request.ChangeRequestTypeMasterDataId,
                                ChangeRequestTypeMasterData = subdocumentCategory.Items,
                                VersionTypeId=request.VersionTypeId,
                                VersionType=subVersionType.Items,

                            }).OrderByDescending(data=>data.ChangeRequestId).AsQueryable();

            if (getListRequest.TenantId > 0)
            {
                document=document.Where(log=>log.TenantId==getListRequest.TenantId);
            }
            if(getListRequest.StatusId > 0)
            {
                document = document.Where(log => log.ChangeRequestStatusMasterDataId == getListRequest.StatusId);
            }
            var filteredData = DataExtensions.OrderBy(document, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
                               .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                               .Take(getListRequest.ListRequests.PerPage);

            var totalItems = await document.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<DocumentChangeRequestGridView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }

        public async Task<DocumentChangeRequestPreView> GetDocumentRequestDetails(int ChangeRequestId, int tenantId)
        {
            var rawData = (from request in _context.DocumentChangeRequestMasters
                           join doc in _context.Documents on request.DocumentId equals doc.DocumentId

                           join status in _context.MasterData on request.ChangeRequestStatusMasterDataId equals status.Id into status
                           from subdocumentStatus in status.DefaultIfEmpty()

                           join category in _context.MasterData on request.ChangeRequestTypeMasterDataId equals category.Id into category
                           from subdocumentCategory in category.DefaultIfEmpty()
						   join versionType in _context.MasterData on request.VersionTypeId equals versionType.Id into versionType
						   from subVersionType in versionType.DefaultIfEmpty()

						   where request.ChangeRequestId == ChangeRequestId && request.TenantId == tenantId 
						   select new DocumentChangeRequestPreView()
                           {
                               ChangeRequestId = request.ChangeRequestId,   
                               TenantId = tenantId,
                               DocumentId = request.DocumentId,
                               Document= doc.Title,
                               ReferenceCode = request.ReferenceCode.HasValue ? request.ReferenceCode:0,
                               Reason = request.Reason,
                               Description = request.Description,
                               Consequences = request.Consequences,
                               ChangeRequestStatusMasterDataId = request.ChangeRequestStatusMasterDataId,
                               ChangeRequestStatusMasterData = subdocumentStatus.Items,
                               ChangeRequestTypeMasterDataId = request.ChangeRequestTypeMasterDataId,
                               ChangeRequestTypeMasterData = subdocumentCategory.Items,
							   VersionTypeId = request.VersionTypeId,
							   VersionType = subVersionType.Items,
						   }).AsQueryable();
            return rawData.FirstOrDefault();
        }

        public async Task<DocumentChangeRequestEmail> GetDocumentRequestEmail(int ChangeRequestId, int tenantId)
        {
            var rawData = (from request in _context.DocumentChangeRequestMasters
                           join doc in _context.Documents on request.DocumentId equals doc.DocumentId
                           join user in _context.UserMasters on  request.RequestedBy equals user.UserId
                           where request.ChangeRequestId == ChangeRequestId && request.TenantId == tenantId
                           select new DocumentChangeRequestEmail()
                           {
                               ChangeRequestId = request.ChangeRequestId,
                               DocumentId = request.DocumentId,
                               DocumentTitle = doc.Title,
                               Reason = request.Reason,
                               Consequences = request.Consequences,
                               RequestedById = request.RequestedBy,
                               RequestedBy = $"{user.FirstName}  {user.LastName}",
                               RequestedOn = request.RequestedOn
                               
                           }).AsQueryable();

            return rawData.FirstOrDefault();
        }


        public async Task<DocumentChangeRequestReportDeatils> GetDocumentChangeRequestReports(int documentId, int tenantId)
        {
            var rawData = (from request in _context.DocumentChangeRequestMasters
                           join doc in _context.Documents on request.DocumentId equals doc.DocumentId
                           join user in _context.UserMasters on request.RequestedBy equals user.UserId into user
                           from subuser in user.DefaultIfEmpty()
                           join user1 in _context.UserMasters on request.ApprovedBy equals user1.UserId into user1
                           from subuser1 in user1.DefaultIfEmpty()                          
                           join  md in _context.MasterData on request.VersionTypeId equals md.Id into md
                           from subVersionType in md.DefaultIfEmpty()
                           where request.ChangeRequestId== documentId && request.TenantId==tenantId
                           select new DocumentChangeRequestReportDeatils
                           {
                               DocumentId = request.DocumentId,
                               DocumentTitle = doc.Title,
                               Reason = request.Reason,  
                               Description = request.Description,
                               Consequences = request.Consequences,
                               RequestedOn = request.RequestedOn,
                               RequestedBy = $"{subuser.FirstName} {subuser.LastName}",
                               ApprovedBy = $"{subuser1.FirstName} {subuser1.LastName}",
                               ApprovedOn= request.ApprovedOn,
                               VersionType=subVersionType.Items,
                               DocumentNumber = doc.DocumentNumber


                           }).AsQueryable();
            return rawData.FirstOrDefault();


        }

    }
}
