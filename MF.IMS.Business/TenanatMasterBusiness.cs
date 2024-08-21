using DocumentFormat.OpenXml.Spreadsheet;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class TenanatMasterBusiness : ITenantMasterBusiness
    {
        private readonly ITenantMasterRepository _tenantMasterRepository;
        private readonly IKeyClockBusiness _keyClockBusiness;
        
        private readonly IRoleMappingRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
       
        private readonly IFileRepositoryBusiness _fileRepositoryBusiness;
        private readonly IMessageService _messageService;
        public TenanatMasterBusiness(ITenantMasterRepository tenantMasterRepository, IKeyClockBusiness keyClockBusiness, IRoleMappingRepository roleRepository, IUserRepository userRepository,
             IRolePermissionRepository rolePermissionRepository, IFileRepositoryBusiness fileRepositoryBusiness,IMessageService messageService)
        {
            _tenantMasterRepository = tenantMasterRepository;
            _keyClockBusiness = keyClockBusiness;
            
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _rolePermissionRepository = rolePermissionRepository;
            
            _fileRepositoryBusiness = fileRepositoryBusiness;
            _messageService = messageService;

        }
        public async Task<IList<TenantView>> GetTenantsView()
        {
            return await _tenantMasterRepository.GetTenantsViewList();
        }
        public async Task<IList<TenanttMaster>> GetTenantsMaster()
        {
            return await _tenantMasterRepository.GetTenantMaster();
        }
        public async Task<PaginatedItems<TenantView>> GetAllTenantsView(GetListRequest getListRequest)
        {
            return await _tenantMasterRepository.GetAllTenants(getListRequest);
        }

        public async Task<TenantView> GetTenantViewById(int tenantId)
        {
            return await _tenantMasterRepository.GetTenantsById(tenantId);
        }

        public async Task DeleteTenant(string tenantId)
        {

            var data = await _tenantMasterRepository.GetTenantByClientName(tenantId);
            if (data.ClientName == tenantId)
            {
                await _tenantMasterRepository.DeleteAsync(data);
                await _keyClockBusiness.DeleteRealm(tenantId);
            }

        }

        public async Task AddTenant(CreateTenantViewModel tenantViewModel, int UserId)
        {
            //Perform tenant validation from database
            await _tenantMasterRepository.ValidateTenant(tenantViewModel);

            //Create tenant into database
            var tenant = await _tenantMasterRepository.CreateTenant(tenantViewModel, UserId);

			var tenantName = tenant.TenantId.ToString();

            //KeyClock Create Realm
            await _keyClockBusiness.CreateRealm(tenantName);
			
			var tenantKeys = await _keyClockBusiness.GetRealmKeys(tenantName);

            string clientName = tenantName;

            await _tenantMasterRepository.ValidateClient(clientName);

            //KeyClock Create tenant client
            await _keyClockBusiness.CreateClient(clientName, tenantName);
			
			var client = await _keyClockBusiness.GetClient(clientName, tenantName);

            var tenantMaster = await _tenantMasterRepository.GetByIdAsync(tenant.TenantId);

            tenantMaster.ClientName = clientName;
            tenantMaster.ClientId = client.Id;
            tenantMaster.Name = tenantViewModel.Name;
            tenantMaster.ShortCode = tenantViewModel.ShortCode;

            var keyClockTenantKey = tenantKeys.Keys.Where(k => k.algorithm == "RS256").FirstOrDefault();
            if (keyClockTenantKey != null)
            {
                tenantMaster.Kid = keyClockTenantKey.kid;
                tenantMaster.RSAPublicKey = keyClockTenantKey.publicKey;
            }

            //update tenant name, short-cde, client id, client-name 
            await _tenantMasterRepository.UpdateAsync(tenantMaster);

            //KeyClock Create Role
            var role = await _keyClockBusiness.CreateRole(tenant, "tenant-admin-role");

            Guid kcRole = new Guid(role.Id);
            var tenantRole = await _roleRepository.CreateRoleForTenant(tenant.TenantId, kcRole);
			

			//Create userId protocal-map/token
			await _keyClockBusiness.CreateUserIdTokenProtocalMap(tenant);

            //ToDo - Create roles to Keyclock to specific realm. Map only tenant-admin-role to tenant's user
            /* 0 - super-admin-role (Do not create for all the tenants)
             * 1 - tenant-admin-role
             * 2 - tenant-manager-role
             * 3 - tenant-user-role
             * 4 - tenant-auditor-role
             * 5 - tenant-external-role
             */

            //Update role id and name


            //KeyClock Create User
            KeyClockUser keyClockUser = await _keyClockBusiness.CreateUser(tenantName, tenantViewModel.Username, tenantViewModel.EmailAddress, tenantViewModel.FirstName, tenantViewModel.LastName);

            var tenantUser = await _userRepository.AddUserForTenant(tenant.TenantId, tenantRole.RoleId, tenantViewModel.EmailAddress, tenantViewModel.FirstName, tenantViewModel.LastName);

            var userMaster = await _userRepository.GetByIdAsync(tenantUser.UserId);

            userMaster.KCUserId = keyClockUser.Id;
            userMaster.KCUsername = keyClockUser.userName;

            await _userRepository.UpdateAsync(userMaster);

            await _keyClockBusiness.AddUserIdToKeyClockUser(tenantName: tenant.TenantId.ToString(), keyClockUserId: userMaster.KCUserId.ToString(), userId: userMaster.UserId.ToString());

            //ToDo - As soon as a user is created into KeyClock, add a new user to user's master on SQL server. Also, use the User.Id returned from keyclock to map user to keyclock

            //KeyClock Create password
            await _keyClockBusiness.CreateUserPassword(tenantName, keyClockUser.Id, tenantViewModel.Password);

            //Update user id and username
            await _keyClockBusiness.AddUserToRole(tenant, role, keyClockUser.Id);


            await _rolePermissionRepository.AddRolePermissionToTenantAdmin(tenant.TenantId);
            string containerName = tenant.TenantId.ToString().PadLeft(6, '0');
            await _fileRepositoryBusiness.CreateContainer(containerName);

			//Following notification must be sent at the last, as tenant has been created at this point.
			
			await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = UserId,
                EventType = NotificationEventType.TenantMaster,
                BroadcastLevel = NotificationBroadcastLevel.None,
                TenantId = tenant.TenantId,
                Action = IMSControllerActionCategory.Create,
                Module = IMSControllerCategory.TenantMaster,
                ItemId = tenant.TenantId,
                Description = tenant.ClientName,
                Title = tenant.ClientName,
                Date = tenant.CreatedOn
            });
        }

        public async Task<TenanttMaster> GetTenantByShortCode(string username)
        {
            return await _tenantMasterRepository.GetTenantByShortCode(username);
        }

        public async Task<TenanttMaster> GetTenantById(int tenantId)
        {
            return await _tenantMasterRepository.GetByIdAsync(tenantId);
        }
        public async Task UpdateTenant(updateTenantViewModel createTenantViewModel, int userId, int tenantId)
        {
            var tenant = await _tenantMasterRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.TenantNotFoundErrorMessage), tenantId);
            }
            else
            {
                tenant.Name = createTenantViewModel.Name;
                tenant.ShortCode = createTenantViewModel.ShortCode;
                tenant.UpdatedBy = userId;
                tenant.UpdatedOn = DateTime.UtcNow;
                await _tenantMasterRepository.UpdateAsync(tenant);

                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    EventType = NotificationEventType.TenantMaster,
                    BroadcastLevel = NotificationBroadcastLevel.None,
                    TenantId = tenant.TenantId,
                    Action = IMSControllerActionCategory.Edit,
                    Module = IMSControllerCategory.TenantMaster,
                    ItemId = tenant.TenantId,
                    Description = tenant.ClientName,
                    Title = tenant.ClientName,
                    Date = tenant.UpdatedOn
				});
            }
        }
    }
}
