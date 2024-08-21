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
    public class ProcessMasterRepository : BaseRepository<ProcessMaster>, IProcessMasterRepository
    {
        public ProcessMasterRepository(IMSDEVContext dbContext, ILogger<ProcessMaster> logger) : base(dbContext, logger)
        {

        }

        public async Task<PaginatedItems<ProcessMasterView>> GetProcessMasterList(ProcessMasterList processMasterList)
        {
            var process = (from pro in _context.ProcessMasters
                           join department in _context.DepartmentMasters on pro.DepartmentId equals department.DepartmentId into department
                           from subdepartment in department.DefaultIfEmpty()

                           join user in _context.UserMasters on pro.OwnedBy equals user.UserId into user
                           from subuser in user.DefaultIfEmpty()     
                           where pro.TenantId==processMasterList.TenantId 
                           select new ProcessMasterView()
                           {
                               ProcessId = pro.ProcessId,
                               TenantId=pro.TenantId,
                               ParentProcessId = pro.ParentProcessId,
                               ProcessText = pro.ProcessText,
                               DepartmentId = subdepartment.DepartmentId,
                               Department = subdepartment.DepartmentName,
                               //ProcessCategoryMasterData = subcategory.Items,                         
                               // StatusMasterData = substatus.Items,                                
                               OwnedBy = $"{subuser.FirstName} {subuser.LastName}",
                             
                           }
                           ).OrderByDescending(process => process.ProcessId).AsQueryable();

            if (processMasterList.ProcessId > 0)
            {
                process = process.Where(log => log.ProcessId == processMasterList.ProcessId);
            }
            if (processMasterList.DepartmentId > 0)
            {
                process = process.Where(log => log.DepartmentId == processMasterList.DepartmentId);
            }

            var filteredData = DataExtensions.OrderBy(process, processMasterList.ListRequests.SortColumn, processMasterList.ListRequests.Sort == "asc")
                             .Skip(processMasterList.ListRequests.PerPage * (processMasterList.ListRequests.Page - 1))
                             .Take(processMasterList.ListRequests.PerPage);

            var totalItems = await process.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)processMasterList.ListRequests.PerPage);
            var model = new PaginatedItems<ProcessMasterView>(processMasterList.ListRequests.Page, processMasterList.ListRequests.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);
        }

        public async Task<IList<ParentProcessDropdown>> GetParentProcessId(int tenantId)
        {
            var process = await (from pro in _context.ProcessMasters
                                 //join department in _context.DepartmentMasters on pro.DepartmentId equals department.DepartmentId into department
                                 //from subdepartment in department.DefaultIfEmpty()
                                 where pro.TenantId == tenantId
                                 select new ParentProcessDropdown
                                 {
                                    ParentProcessId = pro.ParentProcessId,
                                    ProcessText = pro.ProcessText,
                                    ProcessId= pro.ProcessId,
                                 }
                                 ).ToListAsync();
            return await Task.FromResult(process);
        }

        public async Task<GetProcessMaster> GetProcessMastertDetails(int tenantId, int processId )
        {
            var rawData = (from pro in _context.ProcessMasters
                           join department in _context.DepartmentMasters on pro.DepartmentId equals department.DepartmentId into department
                           from subdepartment in department.DefaultIfEmpty()

                           join category in _context.MasterData on pro.ProcessCategoryMasterDataId equals category.Id into category
                           from subcategory in category.DefaultIfEmpty()

                           join status in _context.MasterData on pro.StatusMasterDataId equals status.Id into status
                           from substatus in status.DefaultIfEmpty()

                           join groups in _context.MasterData on pro.ProcessGroupMasterDataId equals groups.Id into groups
                           from subgroups in groups.DefaultIfEmpty()

                           join user in _context.UserMasters on pro.OwnedBy equals user.UserId into user
                           from subuser in user.DefaultIfEmpty()
                           join createdby in _context.UserMasters on pro.CreatedBy equals createdby.UserId into createdby
                           from subCreatedby in createdby.DefaultIfEmpty()
                           join updatedBy in _context.UserMasters on pro.UpdatedBy equals updatedBy.UserId into updatedBy
                           from subUpdatedBy in updatedBy.DefaultIfEmpty()

                           where pro.ProcessId == processId && pro.TenantId == tenantId
                           select new GetProcessMaster()
                           {
                               ProcessId = pro.ProcessId,
                               ParentProcessId = pro.ParentProcessId,
                               ProcessText = pro.ProcessText,
                               TenantId = pro.TenantId,
                               DepartmentId = pro.DepartmentId,
                               Department = subdepartment.DepartmentName,
                               OwnedById = pro.OwnedBy,
                               OwnedBy = $"{subuser.FirstName} {subuser.LastName}",
                               ProcessCategoryMasterDataId = pro.ProcessCategoryMasterDataId,
                               ProcessCategoryMasterData = subcategory.Items,
                               StatusMasterDataId = pro.StatusMasterDataId,
                               StatusMasterData = substatus.Items,
                               ProcessGroupMasterDataId = pro.ProcessGroupMasterDataId,
                               ProcessGroupMasterData = subgroups.Items,
                               CreatedBy = $"{subCreatedby.FirstName} {subCreatedby.LastName}",
                               CreatedById=pro.CreatedBy,
                               CreatedOn = pro.CreatedOn,
                               UpdatedBy = $"{subUpdatedBy.FirstName} {subUpdatedBy.LastName}",
                               UpdateById = pro.UpdatedBy,
                               UpdatedOn=pro.UpdatedOn,
                               


                           }).AsQueryable();
            return rawData.FirstOrDefault();
        }
    }
}
