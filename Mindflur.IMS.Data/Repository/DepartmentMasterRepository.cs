using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class DepartmentMasterRepository : BaseRepository<DepartmentMaster>, IDepartmentMasterRepository
    {
       
        public DepartmentMasterRepository(IMSDEVContext dbContext, ILogger<DepartmentMaster> logger) : base(dbContext, logger)
        {
           
        }

        public async Task<IList<DepartmentListView>> GetDepartmentByTenant(int tenantId)
        {
            var department = await (from dep in _context.DepartmentMasters
                                    join tm in _context.TenanttMasters on dep.TenantId equals tm.TenantId
                                    where dep.TenantId == tenantId
                                    select new DepartmentListView
                                    {
                                        DepartmentId = dep.DepartmentId,
                                        DepartmentName = dep.DepartmentName,
                                    }).ToListAsync();
            return await Task.FromResult(department);
        }
        public async Task<PaginatedItems<DepartmentListView>> GetDepartmentList(DepartmentMasterList departmentList)
        {
            string searchString = string.Empty;
            var rawData = (from dept in _context.DepartmentMasters
                           join tm in _context.TenanttMasters on dept.TenantId equals tm.TenantId
                           where dept.TenantId == departmentList.TenantId
                           select new DepartmentListView
                           {
                               DepartmentId = dept.DepartmentId,
                               DepartmentName = dept.DepartmentName,
                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, departmentList.ListRequests.SortColumn, departmentList.ListRequests.Sort == "asc")
                              .Skip(departmentList.ListRequests.PerPage * (departmentList.ListRequests.Page - 1))
                              .Take(departmentList.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)departmentList.ListRequests.PerPage);

            var model = new PaginatedItems<DepartmentListView>(departmentList.ListRequests.Page, departmentList.ListRequests.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);
        }
    }
}
