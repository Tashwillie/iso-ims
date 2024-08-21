using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class ProcessMasterBusiness : IProcessMasterBusiness
    {
        private readonly IProcessMasterRepository _processMasterRepository;
        public ProcessMasterBusiness(IProcessMasterRepository processMasterRepository)
        {
            _processMasterRepository = processMasterRepository;
        }

        public async Task<PaginatedItems<ProcessMasterView>> GetProcessMasterList(ProcessMasterList processMasterList)
        {
            return await _processMasterRepository.GetProcessMasterList(processMasterList);
        }
        public async Task AddProcessMaster(PostProcessMaster postProcessMaster, int tenantId, int userId)
        {
            ProcessMaster process = new ProcessMaster();

            process.ParentProcessId = postProcessMaster.ParentProcessId;
            process.TenantId = tenantId;
            process.ProcessText= postProcessMaster.ProcessText;
            process.DepartmentId = postProcessMaster.DepartmentId;
            process.OwnedBy= postProcessMaster.OwnedBy;
            process.ProcessGroupMasterDataId = postProcessMaster.ProcessGroupMasterDataId;
            process.ProcessCategoryMasterDataId = postProcessMaster.ProcessCategoryMasterDataId;
            process.StatusMasterDataId = (int)IMSItemStatus.New;
            process.CreatedBy = userId;
            process.CreatedOn = DateTime.UtcNow;      

            await _processMasterRepository.AddAsync(process);

        }

        public async Task<IList<ParentProcessDropdown>> GetParentProcessId(int tenantId)
        {
            return await _processMasterRepository.GetParentProcessId(tenantId);
        }


        public async Task<GetProcessMaster> GetProcessMastertDetails(int tenantId, int processId)
        {
           return await _processMasterRepository.GetProcessMastertDetails(tenantId, processId);
        }

        public async Task UpdatedProcessMaster(PutProcessMaster putProcessMaster, int processId, int tenantId, int userId)
        {
            var rawData = await _processMasterRepository.GetByIdAsync(processId);
            if (rawData == null)
            {
                throw new NotFoundException("ProcessId ", processId);
            }

            rawData.ParentProcessId = putProcessMaster.ParentProcessId;
            rawData.TenantId = tenantId;
            rawData.ProcessText= putProcessMaster.ProcessText;
            rawData.OwnedBy= putProcessMaster.OwnedBy;
            rawData.DepartmentId= putProcessMaster.DepartmentId;    
            rawData.ProcessGroupMasterDataId = putProcessMaster.ProcessGroupMasterDataId;
            rawData.ProcessCategoryMasterDataId = putProcessMaster.ProcessCategoryMasterDataId;
            rawData.StatusMasterDataId = putProcessMaster.StatusMasterDataId;           
            rawData.UpdatedBy = userId;
            rawData.UpdatedOn = DateTime.UtcNow;

            await _processMasterRepository.UpdateAsync(rawData);
        }

        public async Task DeleteProcessMaster(int processId, int tenantId)
        {
            var data = await _processMasterRepository.GetByIdAsync(processId);
            if (data == null)
            {
                throw new NotFoundException("Process", processId);

            }
            await _processMasterRepository.DeleteAsync(data);
        }
    }
}
