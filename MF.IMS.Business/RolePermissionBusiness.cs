using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;

namespace Mindflur.IMS.Business
{
    public class RolePermissionBusiness : IRolePermissionBusiness
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IActionControllerRepository _actionControllerRepository;
        private readonly IControllerMasterRepository _controllerMasterRepository;
        private readonly IRoleMasterRepository _roleMasterRepository;
        public RolePermissionBusiness(IRolePermissionRepository rolePermissionRepository, IActionControllerRepository actionControllerRepository, IControllerMasterRepository controllerMasterRepository, IRoleMasterRepository roleMasterRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _actionControllerRepository = actionControllerRepository;
            _controllerMasterRepository = controllerMasterRepository;
            _roleMasterRepository = roleMasterRepository;
        }
        public async Task UpsertRolePermissions(int tenantId, int roleId, UpsertPermissionModel putRolePermission)
        {
            await _rolePermissionRepository.UpsertRolePermissions(tenantId, roleId, putRolePermission);
        }
        public async Task<PaginatedItems<PermissionListView>> GetPermissionList(GetPermissionList getPermissionList)
        {
            return await _rolePermissionRepository.GetPermissionList(getPermissionList);
        }
        public async Task<IList<AbilityViewModel>> GetUserAbilities(int tenantId, int roleId, string userRoleName)
        {

            IList<AbilityViewModel> abilities = userRoleName == "super-admin-role"
                ? new List<AbilityViewModel>
                {
                    new AbilityViewModel()
                    {
                        Action = "manage",
                        Subject = "all"
                    }
                }
                : await _rolePermissionRepository.GetUserAbilities(tenantId, roleId);
            return await Task.FromResult(abilities);
        }
        public async Task<RolePermissionViewModel> GetPermissionList(int tenantId, int roleId)
        {
            var role = await _roleMasterRepository.GetByIdAsync(roleId);
            var actions = await _actionControllerRepository.GetActionList();
            var controllers = await _controllerMasterRepository.ListAllAsync();
            var rolePermissions = await _rolePermissionRepository.ListAllAsync();

            RolePermissionViewModel permission = new RolePermissionViewModel();
            permission.RoleId = role.RoleId;
            permission.Role = role.RoleName;
            IList<AllowedActions> allowedActions = new List<AllowedActions>();
            foreach (AllowedActionsDataview action in actions)
            {
                allowedActions.Add(new AllowedActions() { Id = action.Id, Text = action.Text });
            }
            permission.AllowedAction = allowedActions;
            IList<Permission> permissionsList = new List<Permission>();

            foreach (var controler in controllers)
            {
                var getactions = await _rolePermissionRepository.GetActionsForController(tenantId, roleId, controler.ControllerId);
                var rplist = getactions.Where(rp => rp.ControllerId == controler.ControllerId);
                var p = new Permission();
                p.Controller = controler.ControllerName;
                p.ControllerId = controler.ControllerId;
                IList<int> actionsList = new List<int>();
                foreach (var rpl in rplist)
                {
                    if (rpl.ActionsId.HasValue)
                        actionsList.Add(rpl.ActionsId.Value);
                };
                p.Action = actionsList.ToArray();

                permissionsList.Add(p);

            }
            permissionsList = permissionsList.OrderBy(p => p.Controller).ToList();

            permission.Permissions = permissionsList;

            return permission;
        }

    }
}
