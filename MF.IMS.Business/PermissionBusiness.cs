using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class PermissionBusiness : IPermissionBusiness
    {
        private readonly IPermissionRepositoy _permissionRepositoy;
        public PermissionBusiness(IPermissionRepositoy permissionRepositoy)
        {
            _permissionRepositoy = permissionRepositoy;
        }
        public async Task AddPermission(AddPermissions addPermission)
        {
            PermissionMaster permission = new PermissionMaster();
            permission.ControllerMasterId = addPermission.Controller;
            permission.ControllerActionMasterId = addPermission.Action;
            await _permissionRepositoy.AddAsync(permission);
        }
        public async Task UpdatePermission(int permissionId, UpdatePermissions updatePermission)
        {
            var permissions = await _permissionRepositoy.GetByIdAsync(permissionId);
            if (permissions == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.PermissionNotFoundErrorMessage), permissionId);
            }
            else
            {
                permissions.ControllerMasterId = updatePermission.Controller;
                permissions.ControllerActionMasterId = updatePermission.Action;
                await _permissionRepositoy.UpdateAsync(permissions);
            }
        }

        public async Task<PermissionViewModel> GetPermitionDetailsById(int permissionId)
        {
            var permission = await _permissionRepositoy.GetPermitionDetailsById(permissionId);

            return permission;
        }

        public async Task<PaginatedItems<PermissionMasterView>> GetPermissionList(GetListRequest getListRequest)
        {
            return await _permissionRepositoy.GetPermissionList(getListRequest);
        }

        public async Task DeletePermission(int permissionId)
        {
            var permission = await _permissionRepositoy.GetByIdAsync(permissionId);
            if (permission == null)
                throw new NotFoundException(string.Format(ConstantsBusiness.PermissionDetailsNotFoundErrorMessage), permissionId);
            await _permissionRepositoy.DeleteAsync(permission);
        }

    }
}
