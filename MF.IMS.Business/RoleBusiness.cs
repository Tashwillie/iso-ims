using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class RoleBusiness : IRoleBusiness
    {
        private readonly IKeyClockBusiness _keyClockBusiness;
        private readonly IRoleMasterRepository _roleMasterRepository;

        public RoleBusiness(IKeyClockBusiness keyClockBusiness, IRoleMasterRepository roleMasterRepository)
        {
            _keyClockBusiness = keyClockBusiness;
            _roleMasterRepository = roleMasterRepository;
        }

        public async Task UpsertRoleToUser(TenanttMaster tenant, int? roleId, string userId)
        {
            //Fetch if the role has a kc mapping
            var role = await _roleMasterRepository.GetRoleByName(tenant.TenantId, roleId.Value);

            if (role == null)
            {
                role = await _roleMasterRepository.GetByIdAsync(roleId.Value);
            }

            //create kc role mapping
            var kc_roles = await _keyClockBusiness.GetRoles(tenant);

            var kcRole = kc_roles.Where(kcrole => kcrole.Name == role.RoleName).FirstOrDefault();

            //Create a role
            if (kcRole == null)
            {
                kcRole = await _keyClockBusiness.CreateRole(tenant, role?.RoleName);
            }

            //ToDo - Get all user roles
            //ToDo - Remove roles which are not suppose to be assigned

            // KC - map role to user
            await _keyClockBusiness.AddUserToRole(tenant, kcRole, userId);
        }

        public async Task<IList<RoleMaster>> GetRoleList()
        {
            return await _roleMasterRepository.GetRoleList();
        }
    }
}