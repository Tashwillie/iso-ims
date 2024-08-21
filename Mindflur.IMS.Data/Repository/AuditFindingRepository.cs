using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class AuditFindingRepository : BaseRepository<AuditFinding>, IAuditFindingRepository
    {
        
        public AuditFindingRepository(IMSDEVContext dbContext, ILogger<AuditFinding> logger) : base(dbContext, logger)
        {
            
        }
        public async Task<PaginatedItems<AuditFindingView>> GetAuditFindings(GetAuditFindingListRequest getListRequest)
        {
            string searchString = string.Empty;

            var rawData = (from af in _context.AuditFindings

                           join md in _context.MasterData on af.MasterDataFindingCategoryId equals md.Id
                           join ai in _context.AuditableItems on af.AuditableItemId equals ai.Id
                           join mdt in _context.MasterData on af.MasterDataFindingStatusId equals mdt.Id
                           join dep in _context.DepartmentMasters on ai.DepartmentId equals dep.DepartmentId 
                           select new AuditFindingView()
                           {
                               Id = af.Id,
                               Department = dep.DepartmentName,
                               FindingCategory = md.Items,
                               Status = mdt.Items,
                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
                              .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                              .Take(getListRequest.ListRequests.PerPage);
            var data = await filteredData.ToListAsync();

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);

            var model = new PaginatedItems<AuditFindingView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, data);

            return await Task.FromResult(model);
        }
        public async Task<PaginatedItems<AuditFindingView>> GetAuditFindingByAuditId(AuditFindingViewForAudit auditFindingListView)
        {
            string searchString = string.Empty;

            var rawData = (from af in _context.AuditFindings
                           join ap in _context.AuditPrograms on af.AuditProgramId equals ap.Id
                           join md in _context.MasterData on af.MasterDataFindingCategoryId equals md.Id
                           join ai in _context.AuditableItems on af.AuditableItemId equals ai.Id
                           join mdt in _context.MasterData on af.MasterDataFindingStatusId equals mdt.Id
                           join tm in _context.TenanttMasters on ap.TenantId equals tm.TenantId
                           join dep in _context.DepartmentMasters on ai.DepartmentId equals dep.DepartmentId
                           where af.AuditProgramId == auditFindingListView.AuditProgramId && ap.TenantId == auditFindingListView.TenantId
                           select new AuditFindingView()
                           {
                               Id = af.Id,
                               Department = dep.DepartmentName,
                               FindingCategory = md.Items,
                               Status = mdt.Items,
                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, auditFindingListView.ListRequests.SortColumn, auditFindingListView.ListRequests.Sort == "asc")
                              .Skip(auditFindingListView.ListRequests.PerPage * (auditFindingListView.ListRequests.Page - 1))
                              .Take(auditFindingListView.ListRequests.PerPage);
            var data = await filteredData.ToListAsync();

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)auditFindingListView.ListRequests.PerPage);

            var model = new PaginatedItems<AuditFindingView>(auditFindingListView.ListRequests.Page, auditFindingListView.ListRequests.PerPage, totalPages, data);

            return await Task.FromResult(model);
        }
        public async Task<AuditFindingPreview> GetAuditFindingPreview(int auditFindingId, int tenantId)
        {
            var data = (from finding in _context.AuditFindings
                        join user in _context.UserMasters on finding.CreatedBy equals user.UserId
                        join md in _context.MasterData on finding.MasterDataFindingStatusId equals md.Id
                        join md1 in _context.MasterData on finding.MasterDataFindingCategoryId equals md1.Id
                        join items in _context.AuditableItems on finding.AuditableItemId equals items.Id
                        join program in _context.AuditPrograms on finding.AuditProgramId equals program.Id
                        join tm in _context.TenanttMasters on program.TenantId equals tm.TenantId
                        join dp in _context.DepartmentMasters on finding.Department equals dp.DepartmentId
                        where finding.Id == auditFindingId && program.TenantId == tenantId
                        select new AuditFindingPreview()
                        {
                            Id = finding.Id,
                            AuditableItemId = finding.AuditableItemId,
                            //AuditableItemName = items.AuditableItems,
                            AuditProgramId = finding.AuditProgramId,
                            AuditProgramName = program.Title,
                            Title = finding.Title,
                            Description = finding.Description,
                            MasterDataFindingCategoryId = finding.MasterDataFindingCategoryId,
                            MasterDataCategory = md1.Items,
                            MasterDataFindingStatusId = finding.MasterDataFindingStatusId,
                            MasterDataFindingStatus = md.Items,
                            DepartmentId = finding.Department,
                            Department = dp.DepartmentName,
                            CreatedById = finding.CreatedBy,
                            CreatedBy = $"{user.FirstName}{user.LastName}"


                        }).AsQueryable();
            return data.FirstOrDefault();
        }
        public async Task<BackTrace> GetAuditFindingByRiskTreatmentId(int moduleEntitiyId)
        {
            var rawdata = (from af in _context.AuditFindings
                           join risk in _context.Risks on af.Id equals risk.WorkItemId
                           join treatment in _context.RiskTreatments on risk.Id equals treatment.RiskId
                           join um in _context.UserMasters on af.CreatedBy equals um.UserId
                           where moduleEntitiyId == treatment.Id && risk.WorkItemId == 90
                           select new BackTrace
                           {
                               ModuleId = (int)IMSControllerCategory.InternalAuditFinding,
                               ModuleName = "AuditFinding",
                               ModuleItemId = af.Id,
                               Title = af.Title,
                               Content = af.Description,
                               CreatedOn = af.CreatedOn,
                               CreatedBy = $"{um.FirstName} {um.LastName}",
                               OrderNumber = 3
                           }).AsQueryable();
            return rawdata.FirstOrDefault();
        }

        public async Task<BackTrace> GetAuditFindingByObservationId(int moduleEntitiyId)
        {
            var rawdata = (from af in _context.AuditFindings
                           join ob in _context.ObservationMasters on af.Id equals ob.SourceId
                           join um in _context.UserMasters on af.CreatedBy equals um.UserId
                           where moduleEntitiyId == ob.Id && ob.Source == 90
                           select new BackTrace
                           {
                               ModuleId = (int)IMSControllerCategory.InternalAuditFinding,
                               ModuleName = "AuditFinding",
                               ModuleItemId = af.Id,
                               Title = af.Title,
                               Content = af.Description,
                               CreatedOn = af.CreatedOn,
                               CreatedBy = $"{um.FirstName} {um.LastName}",
                               OrderNumber = 2
                           }).AsQueryable();
            return rawdata.FirstOrDefault();
        }
        public async Task<BackTrace> GetAuditFindingByOpportunitiesId(int moduleEntitiyId)
        {
            var rawdata = (from af in _context.AuditFindings
                           join om in _context.OpportunitiesMasters on af.Id equals om.SourceId
                           join um in _context.UserMasters on af.CreatedBy equals um.UserId
                           where moduleEntitiyId == om.Id && om.Source == 90
                           select new BackTrace
                           {
                               ModuleId = (int)IMSControllerCategory.InternalAuditFinding,
                               ModuleName = "AuditFinding",
                               ModuleItemId = af.Id,
                               Title = af.Title,
                               Content = af.Description,
                               CreatedOn = af.CreatedOn,
                               CreatedBy = $"{um.FirstName} {um.LastName}",
                               OrderNumber = 2
                           }).AsQueryable();
            return rawdata.FirstOrDefault();
        }


        public async Task<IList<AuditFindingList>> GetAllFindings( int auditId, int tenantId)
        {
            var rawData =  await  (from ap in _context.AuditPrograms
                                   join Ai in _context.AuditableItems on ap.Id equals Ai.AuditProgramId
                                  join Af in _context.AuditFindingsMappings on Ai.Id equals Af.AuditableItemId
                                   join wi in _context.WorkItemMasters on Af.WorkItemId equals wi.WorkItemId

                                   where ap.Id ==auditId
                                   select new AuditFindingList
                                   {
				                    FindingId = wi.WorkItemId,

									StatusId = wi.StatusMasterDataId

								   }).ToListAsync();
			return await Task.FromResult(rawData);


		}
    }
}
