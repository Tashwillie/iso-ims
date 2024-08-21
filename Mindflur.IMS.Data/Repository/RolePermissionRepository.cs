using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;
using System.Data;

namespace Mindflur.IMS.Data.Repository
{
    public class RolePermissionRepository : BaseRepository<RolePermission>, IRolePermissionRepository
    {
        private readonly IControllerMasterRepository _controllerMasterRepository;
        private readonly IActionControllerRepository _actionControllerRepository;
        public RolePermissionRepository(IMSDEVContext context, ILogger<RolePermission> logger, IControllerMasterRepository controllerMasterRepository, IActionControllerRepository actionControllerRepository) : base(context, logger)
        {
            _controllerMasterRepository = controllerMasterRepository;
            _actionControllerRepository = actionControllerRepository;
        }

        public async Task UpsertRolePermissions(int tenantId, int roleId, UpsertPermissionModel upsertPermissionModel)
        {
            var existingPermissionForRole = await _context.RolePermissions.Where(rp => rp.TanentId == tenantId && rp.RoleId == roleId).ToListAsync();

            //remove existing permission
            if (existingPermissionForRole.Any())
            {
                _context.RolePermissions.RemoveRange(existingPermissionForRole);
                await _context.SaveChangesAsync();
            }

            var newPermissions = new List<RolePermission>();
            //make permission to action
            foreach (var permission in upsertPermissionModel.Permissions)
            {
                foreach (int action in permission.Action)
                {
                    var rolePermission = new RolePermission
                    {
                        TanentId = tenantId,
                        RoleId = roleId,
                        ControllerId = permission.ControllerId,
                        ActionId = action
                    };
                    newPermissions.Add(rolePermission);
                }
            }

            //Save permission when it has many
            if (newPermissions.Any())
            {
                await _context.RolePermissions.AddRangeAsync(newPermissions);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PaginatedItems<PermissionListView>> GetPermissionList(GetPermissionList getpermissionList)
        {
            var query = (from rp in _context.RolePermissions

                         join tm in _context.TenanttMasters on rp.TanentId equals tm.TenantId
                         join rm in _context.RoleMasters on rp.RoleId equals rm.RoleId
                         join cm in _context.ControllerMasters on rp.ControllerId equals cm.ControllerId
                         join cam in _context.ControllerActionMasters on rp.ActionId equals cam.ActionId
                         where rp.TanentId == getpermissionList.TenantId && rp.RoleId == getpermissionList.RoleId
                         orderby cm.ControllerName ascending

                         select new PermissionListView
                         {
                             RolePermissionId = rp.RolePermissionId,
                             Roles = rm.RoleName,
                             Controller = cm.ControllerName,
                             Action = cam.ControllerAction

                         }).AsQueryable();


            var filteredData = DataExtensions.OrderBy(query, getpermissionList.ListRequests.SortColumn, getpermissionList.ListRequests.Sort == "asc")
                              .Skip(getpermissionList.ListRequests.PerPage * (getpermissionList.ListRequests.Page - 1))
                              .Take(getpermissionList.ListRequests.PerPage);

            var totalItems = await query.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getpermissionList.ListRequests.PerPage);

            var model = new PaginatedItems<PermissionListView>(getpermissionList.ListRequests.Page, getpermissionList.ListRequests.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);
        }

        public async Task<IList<AbilityViewModel>> GetUserAbilities(int tenantId, int roleId)
        {
            var permissions = await (from rp in _context.RolePermissions

                                     join tm in _context.TenanttMasters on rp.TanentId equals tm.TenantId
                                     join rm in _context.RoleMasters on rp.RoleId equals rm.RoleId
                                     join cm in _context.ControllerMasters on rp.ControllerId equals cm.ControllerId
                                     join cam in _context.ControllerActionMasters on rp.ActionId equals cam.ActionId

                                     where rp.TanentId == tenantId && rp.RoleId == roleId
                                     orderby cm.ControllerName ascending

                                     select new AbilityViewModel
                                     {
                                         Subject = cm.ControllerName,
                                         Action = cam.ControllerAction
                                     }).ToListAsync();
            return await Task.FromResult(permissions);
        }

        public async Task<IList<ControllerActions>> GetActionsForController(int tenantId, int roleId, int controllerId)
        {
            var permissions = await (from cm in _context.ControllerMasters
                                     join rp in _context.RolePermissions on cm.ControllerId equals rp.ControllerId
                                     join tm in _context.TenanttMasters on rp.TanentId equals tm.TenantId
                                     where rp.ControllerId == controllerId && rp.RoleId == roleId && rp.TanentId == tenantId
                                     orderby cm.ControllerName ascending
                                     select new ControllerActions
                                     {
                                         ActionsId = rp.ActionId,
                                         ControllerId = rp.ControllerId,
                                     }).ToListAsync();
            return await Task.FromResult(permissions);
        }


        public async Task AddRolePermissionToTenantAdmin(int tenantId)
        {
            IList<RolePermission> permissionsForTenantAdminRole = new List<RolePermission>();

            var controllermaster = await _controllerMasterRepository.ListAllAsync();
            var actionMaster = await _actionControllerRepository.ListAllAsync();
            foreach (var controller in controllermaster)
            {
                if (controller.ControllerName == "Tenants")
                    continue;

                foreach (var actions in actionMaster)
                {
                    RolePermission roles = new RolePermission();
                    roles.TanentId = tenantId;
                    roles.RoleId = 35;
                    roles.ControllerId = controller.ControllerId;
                    roles.ActionId = actions.ActionId;
                    permissionsForTenantAdminRole.Add(roles);

                }
            }
            await _context.RolePermissions.AddRangeAsync(permissionsForTenantAdminRole);
            await _context.SaveChangesAsync();
        }
        public async Task AddDefultPermissionsToRole(int roleId, int tenantId)
        {
            var existingPermissions = await _context.RolePermissions.Where(t => t.RoleId == roleId && t.TanentId == tenantId).ToListAsync();
            if (!existingPermissions.Any())
            {
                IList<RolePermission> permissionsForUser = new List<RolePermission>();

                var controllermaster = await _controllerMasterRepository.ListAllAsync();
                var actionMaster = await _actionControllerRepository.ListAllAsync();
                foreach (var controller in controllermaster)
                {
                    if (controller.ControllerName == "Dashboard")
                    {
                        foreach (var actions in actionMaster)
                        {
                            RolePermission roles = new RolePermission();
                            roles.TanentId = tenantId;
                            roles.RoleId = roleId;
                            roles.ControllerId = controller.ControllerId;
                            roles.ActionId = actions.ActionId;
                            permissionsForUser.Add(roles);

                        }
                    }


                }
                await _context.RolePermissions.AddRangeAsync(permissionsForUser);
                await _context.SaveChangesAsync();
            }






        }


    }
}
