using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using NUnit.Framework.Internal.Execution;

namespace Mindflur.IMS.Business
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleBusiness _roleBusiness;
        private readonly IEmailService _emailService;
        private readonly ITenantMasterRepository _tenantMasterRepository;
        private readonly IKeyClockBusiness _keyClockBusiness;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IMessageService _messageService;

        public UserBusiness(IMessageService messageService, IUserRepository userRepository, IEmailService emailService, ITenantMasterRepository tenantMasterRepository, IKeyClockBusiness keyClockBusiness, IRoleBusiness roleBusiness, IRolePermissionRepository rolePermissionRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _tenantMasterRepository = tenantMasterRepository;
            _keyClockBusiness = keyClockBusiness;
            _roleBusiness = roleBusiness;
            _rolePermissionRepository = rolePermissionRepository;
            _messageService = messageService;
        }

        public async Task<PaginatedItems<UsersView>> GetAllUsersByTenantId(GetUserRequest getListRequest)
        {
            return await _userRepository.GetAllUsersByTenantId(getListRequest);
        }

        public async Task<PaginatedItems<MeetingListView>> GetManagementReviewList(GetTaskListByUserId getTaskListByUserId)
        {
            return await _userRepository.GetManagementReviewList(getTaskListByUserId);
        }

        public async Task<PaginatedItems<UserProjectListView>> GetTasksByUserId(GetTaskListByUserId getTasksByUserID)//Get Tasks BY UserID
        {
            return await _userRepository.GetTasksByUserId(getTasksByUserID);
        }

        public async Task<UpdatedUserViewModel> UpdateUser(UpdatedUserViewModel updateUserViewModel, TenanttMaster tenant, int userID, int tenantId, int userId)//Update View Model
        {
            var tenantName = tenantId.ToString();

            var usermasters = await _userRepository.GetByIdAsync(userID);

            var kcuserdetails = _keyClockBusiness.UpdateUser(tenantName, updateUserViewModel.EmailAddress, updateUserViewModel.EmailAddress, updateUserViewModel.FirstName, updateUserViewModel.LastName, usermasters.KCUserId);

            if (usermasters.TenantId == tenantId && usermasters.UserId == userID)
            {
                usermasters.FirstName = updateUserViewModel.FirstName;
                usermasters.LastName = updateUserViewModel.LastName;

                usermasters.KCUsername = updateUserViewModel.EmailAddress;

                usermasters.EmailId = updateUserViewModel.EmailAddress;

                usermasters.DepartmentId = updateUserViewModel.DepartmentId;
                usermasters.RoleId = updateUserViewModel.RoleId;

                usermasters.Status = updateUserViewModel.Status;
                usermasters.UpdatedOn = DateTime.UtcNow;
                usermasters.UpdatedBy = userId;
                await _userRepository.UpdateAsync(usermasters);

                var client = await _tenantMasterRepository.GetByIdAsync(tenantId);
                await _keyClockBusiness.UnAssignRole(tenantName, usermasters.KCUserId, client.ClientId);
                await _roleBusiness.UpsertRoleToUser(tenant, updateUserViewModel.RoleId, usermasters.KCUserId);
				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    EventType = NotificationEventType.TenantMaster,
                    BroadcastLevel = NotificationBroadcastLevel.None,
                    SourceIdUserId = userID,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					TenantId = tenantId,
                    Action = IMSControllerActionCategory.Edit,
                    Module = IMSControllerCategory.UserManagement,
                    ItemId = tenantId,
                    Description = usermasters.KCUsername + " has Updated ",
                    Title = usermasters.KCUsername,
					Date = usermasters.UpdatedOn
				});

                return updateUserViewModel;
            }
            else
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.InalidTenantOrUserIdNotFoundErrorMessage), userID);
            }
        }

        public async Task<AddNewUserViewModel> AddUser(AddNewUserViewModel addNewUserViewModel, TenanttMaster tenant, int userId)
        {
            //1. ToDo: IMS DB check if a user with same username/email address already exist, then do not proceed
            await _userRepository.ValidateUser(addNewUserViewModel, tenant.TenantId);

            //2. KeyClock - Create user
            var kcUser = await _keyClockBusiness.CreateUser(tenant.TenantId.ToString(), addNewUserViewModel.EmailAddress, addNewUserViewModel.EmailAddress, addNewUserViewModel.FirstName, addNewUserViewModel.LastName);

            UserMaster um = new UserMaster()
            {
                TenantId = tenant.TenantId,
                FirstName = addNewUserViewModel.FirstName,
                LastName = addNewUserViewModel.LastName,
                DepartmentId = addNewUserViewModel.DepartmentId,
                EmailId = addNewUserViewModel.EmailAddress,
                RoleId = addNewUserViewModel.UserRole,
                KCUsername = addNewUserViewModel.EmailAddress,
                KCUserId = kcUser.Id,

                Status = 145, //ToDo: Replace this with enum
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId
            };

            //Add user to ims
            await _userRepository.AddAsync(um);

            //3. roleBusiness to handle role add/update for a user
            await _roleBusiness.UpsertRoleToUser(tenant, addNewUserViewModel.UserRole, kcUser.Id);

            await _keyClockBusiness.AddUserIdToKeyClockUser(tenantName: tenant.TenantId.ToString(), keyClockUserId: kcUser.Id.ToString(), userId: um.UserId.ToString());

            //4. set password for user
            await _keyClockBusiness.CreateUserPassword(tenant.TenantId.ToString(), kcUser.Id, addNewUserViewModel.Password);

            //5. send invitation email
            await SendWelcomeEmail(um.UserId);

            await _rolePermissionRepository.AddDefultPermissionsToRole(um.RoleId, tenant.TenantId);

			
			await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                EventType = NotificationEventType.TenantMaster,
                BroadcastLevel = NotificationBroadcastLevel.None,
                SourceIdUserId = userId,				
				TenantId = tenant.TenantId,
                Action = IMSControllerActionCategory.Create,
                Module = IMSControllerCategory.UserManagement,
                ItemId = um.TenantId,
                Description = um.KCUsername + " is Created ",
                Title = um.KCUsername,
                Date = DateTime.Now,
            });

            return addNewUserViewModel;
        }

        public async Task DeleteUser(int userID, int tenantId)
        {
            var user = await _userRepository.GetByIdAsync(userID);
            if (user == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), userID);
            }

            //Todo
            //- remove user from KeyClock
            else if (user.TenantId == tenantId && user.UserId == userID)
            {
                await SendAccountDeactivatedEmail(user.UserId); //HACK: Email sedning should be after the user has been deleted
                if (userID >= 1 && userID <= 6) //HACK: Do nothing for developer accounts
                {
                    //do nothing for these users
                }
                else
                {
                    user.DeletedBy = userID;
                    user.DeletedOn = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                    await _keyClockBusiness.DeleteUser(user.TenantId.ToString(), user.KCUserId);
                    await _messageService.SendNotificationMessage(new NotificationMessage()
                    {
                        EventType = NotificationEventType.TenantMaster,
                        BroadcastLevel = NotificationBroadcastLevel.None,
                        SourceIdUserId = userID,
                        TenantId = tenantId,
                        Action = IMSControllerActionCategory.Delete,
                        Module = IMSControllerCategory.UserManagement,
                        ItemId = tenantId,
                        Description = user.KCUsername + " is Deleted ",
                        Title = user.KCUsername,
                        Date = DateTime.UtcNow,
                    });
                }
            }
            else
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.TenantNotFoundErrorMessage), tenantId);
            }
        }

        public async Task<UsersDetailView> GetByUsername(int tenantId, string username)
        {
            return await _userRepository.GetUserDetails(tenantId, username);
        }

        public async Task<IList<UserMasterView>> GetUserByDepartmentId(int departmentId, int tenantId)
        {
            return await _userRepository.GetUserByDepartmentId(departmentId, tenantId);
        }

        public async Task<UsersView> GetUserDetails(int UsersId, int tenantId)
        {
            return await _userRepository.GetUserDetails(UsersId, tenantId);
        }

        public async Task<OrganisationUserPreview> GetUserDetail(int usersId, int tenantId)
        {
            return await _userRepository.GetUserDetail(usersId, tenantId);
        }

        public async Task<IList<UsersView>> GetAllUsers(int tenantId)
        {
            return await _userRepository.GetAllUsers(tenantId);
        }

        public async Task SendWelcomeEmail(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            keyValuePairs.Add("#FULL_NAME#", user.FirstName + " " + user.LastName);

            await _emailService.SendEmail(user.EmailId, user.FirstName, "NewUserLoginMail.html", "This is for account activation", keyValuePairs);
        }

        public async Task SendAccountDeactivatedEmail(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            keyValuePairs.Add("#USER_NAME#", user.KCUsername);

            await _emailService.SendEmail(user.EmailId, user.FirstName, "AccountDeactivated.html", "Your account has ben deactivated", keyValuePairs);
        }

        public async Task ChangeUserPassword(int tenantId, int userId, ChangeUserPasswordView changeUserPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            await _keyClockBusiness.CreateUserPassword(tenantId.ToString(), user.KCUserId, changeUserPassword.NewPassword);
        }
    }
}