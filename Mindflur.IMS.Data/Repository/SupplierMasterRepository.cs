using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class SupplierMasterRepository : BaseRepository<SupplierMaster>, ISupplierMasterRepository
    {
        public SupplierMasterRepository(IMSDEVContext dbContext, ILogger<SupplierMaster> logger) : base(dbContext, logger)
        {
        }
        public async Task<PaginatedItems<GetSupplierGridView>> getAllSupplierDetails(GetSupplierListRequest getListRequest)
        {
            var rawData = (from supplier in _context.SupplierMasters
                           join md in _context.MasterData on supplier.MasterDataSupplierStatusId equals md.Id
                           join tm in _context.TenanttMasters on supplier.TenantId equals tm.TenantId
                           join dep in _context.DepartmentMasters on supplier.DepartmentId equals dep.DepartmentId into dep
                           from department in dep.DefaultIfEmpty()
                           

                           where supplier.TenantId == getListRequest.TenantId
                           select new GetSupplierGridView()
                           {
                               SupplierId = supplier.SupplierId,
                               SupplierName = supplier.SupplierName,
                               EmailAddress = supplier.EmailAddress,
                               ContactPerson = supplier.ContactPerson,
                               ContactNumber = supplier.ContactNumber,
                               SupplierLocation = supplier.SupplierLocation,
                               DepartmentId= supplier.DepartmentId,
                               Department=department.DepartmentName,
                               MasterDataSupplierStatusId = md.Items,
                               CreatedBy = supplier.CreatedBy,


                           }).OrderByDescending(rawData => rawData.SupplierId).AsQueryable().AsQueryable();
            if (getListRequest.ForUserId > 0)
            {
                rawData = rawData.Where(log => log.CreatedBy == getListRequest.ForUserId);
            }
            if (getListRequest.DepartmentId > 0)
            {
                rawData = rawData.Where(log => log.CreatedBy == getListRequest.DepartmentId);
            }
            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc").Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1)).Take(getListRequest.ListRequests.PerPage);
            var totalItems = await rawData.LongCountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<GetSupplierGridView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }
        public async Task<SupplierPreview> getSupplierPreviewById(int Id, int tenantId)
        {

            var rawData = (from supplier in _context.SupplierMasters
                           join md in _context.MasterData on supplier.MasterDataSupplierStatusId equals md.Id
                           join tm in _context.TenanttMasters on supplier.TenantId equals tm.TenantId
                           join dep in _context.DepartmentMasters on supplier.DepartmentId equals dep.DepartmentId
                           where Id == supplier.SupplierId && tenantId == supplier.TenantId
                           select new SupplierPreview()
                           {
                               SupplierId = supplier.SupplierId,
                               SupplierName = supplier.SupplierName,
                               EmailAddress = supplier.EmailAddress,
                               ContactPerson = supplier.ContactPerson,
                               ContactNumber = supplier.ContactNumber,
                               SupplierLocation = supplier.SupplierLocation,
                               DepartmentId = supplier.DepartmentId,
                               Department = dep.DepartmentName,
                               MasterDataSupplierStatusId = md.Id,
                               MasterDataSupplierStatus = md.Items

                           }).AsQueryable();
            return rawData.FirstOrDefault();

        }
        public async Task<IList<SupplierDropDownView>> GetSupplierDropDown(int tenantId)
        {
            var rawData = await (from sp in _context.SupplierMasters
                                 where sp.TenantId == tenantId
                                 select new SupplierDropDownView
                                 {
                                     SupplierId = sp.SupplierId,
                                     SupplierName = sp.SupplierName
                                 }).ToListAsync();
            return rawData;
        }

     
    }
}
