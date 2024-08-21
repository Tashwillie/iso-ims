using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class DepartmentMasterBusiness : IDepartmentMasterBusiness
    {
        private readonly IDepartmentMasterRepository _departmentMasterRepository;
        public DepartmentMasterBusiness(IDepartmentMasterRepository departmentMasterRepository)
        {
            _departmentMasterRepository = departmentMasterRepository;
        }
        public async Task<IList<DepartmentListView>> GetDepartmentByTenant(int tenantId)
        {
            return await _departmentMasterRepository.GetDepartmentByTenant(tenantId);
        }
        public async Task<PaginatedItems<DepartmentListView>> GetDepartmentList(DepartmentMasterList departmentList)
        {
            return await _departmentMasterRepository.GetDepartmentList(departmentList);
        }
        public async Task AddDepartment(int tenantId, PostDepartments departmentMaster)
        {
            DepartmentMaster dept = new DepartmentMaster();
            dept.DepartmentName = departmentMaster.DepartmentName;
            dept.TenantId = tenantId;
            await _departmentMasterRepository.AddAsync(dept);
        }

        public async Task UpdateDepartment(int tenantId, int departmentId, PostDepartments departmentMaster)
        {
            var dept = await _departmentMasterRepository.GetByIdAsync(departmentId);
            if (dept == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.DepartmentNotFoundErrorMessage), departmentId);
            }
            else if (dept.DepartmentId == departmentId && dept.TenantId == tenantId)
            {
                dept.DepartmentName = departmentMaster.DepartmentName;
                await _departmentMasterRepository.UpdateAsync(dept);
            }
            else
            {
                throw new BadRequestException(string.Format(ConstantsBusiness.DepartmentIdOrTenantNotMatch));
            }
        }
        public async Task<DepartmentMaster> GetDepartmentById(int tenantId, int departmentId)
        {
            var dept = await _departmentMasterRepository.GetByIdAsync(departmentId);
            if (dept == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.DepartmentNotFoundErrorMessage), departmentId);
            }
            else
            {
                return dept.DepartmentId == departmentId && dept.TenantId == tenantId
                    ? dept
                    : throw new BadRequestException(string.Format(ConstantsBusiness.DepartmentIdOrTenantNotMatch));
            }
        }
        public async Task DeleteDepartmentById(int tenantId, int departmentId)
        {
            var dept = await _departmentMasterRepository.GetByIdAsync(departmentId);
            if (dept == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.DepartmentNotFoundErrorMessage), departmentId);
            }
            else if (dept.DepartmentId == departmentId && dept.TenantId == tenantId)
            {
                await _departmentMasterRepository.DeleteAsync(dept);
            }
            else
            {
                throw new BadRequestException(string.Format(ConstantsBusiness.DepartmentIdOrTenantNotMatch));
            }

        }
    }
}
