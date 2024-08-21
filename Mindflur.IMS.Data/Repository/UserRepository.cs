using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.DomainModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class UserRepository : BaseRepository<UserMaster>, IUserRepository
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public UserRepository(IMSDEVContext dbContext, ILogger<UserMaster> logger, IMapper mapper, ICacheService cacheService) : base(dbContext, logger)
        {
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task ValidateUser(AddNewUserViewModel addNewUserViewModel, int tenantId)
        {
            var rawData = await _context.UserMasters.Where(t => (t.EmailId == addNewUserViewModel.EmailAddress && t.DeletedOn == null && t.TenantId == tenantId) || (t.KCUsername == addNewUserViewModel.EmailAddress && t.DeletedOn == null && t.TenantId == tenantId)).FirstOrDefaultAsync();

            if (rawData != null)
            {
                throw new ArgumentException(string.Format(RepositoryConstant.AddEmailAddressErrorMessage, addNewUserViewModel.EmailAddress));
            }
        }

        public async Task<UsersView> GetUserDetails(int UsersId, int tenantId)
        {
            _logger.Log(LogLevel.Trace, "start fetch User detail");

            var users = await FetchUsersFromCache(tenantId);

            var user = users.FirstOrDefault(user => user.UserId == user.UserId && user.TenantId == tenantId);

            return _mapper.Map<UsersView>(user);
        }

        public async Task<IList<UserMasterView>> GetUserByDepartmentId(int departmentId, int tenantId)
        {
            var rawData = await (from us in _context.UserMasters
                                 join Mdep in _context.DepartmentMasters on us.DepartmentId equals Mdep.DepartmentId into Mdep
                                 from subDept in Mdep.DefaultIfEmpty()
                                 join tm in _context.TenanttMasters on us.TenantId equals tm.TenantId
                                 where subDept.DepartmentId == departmentId && us.TenantId == tenantId && us.DeletedBy == null
                                 orderby us.FirstName ascending
                                 select new UserMasterView()
                                 {
                                     UserId = us.UserId,
                                     FirstName = us.FirstName,
                                     LastName = us.LastName,
                                     FullName = string.Format("{0} {1}", us.FirstName, us.LastName),
                                     Username = us.KCUsername,
                                     EmailId = us.EmailId,
                                     Department = subDept.DepartmentName,
                                     TenantName = tm.ShortCode
                                 }).ToListAsync();

            return await Task.FromResult(rawData);
        }

        public async Task<OrganisationUserPreview> GetUserDetail(int? usersId, int tenantId)
        {
            var rawData = (from user in _context.UserMasters

                           join rm1 in _context.RoleMasters on user.RoleId equals rm1.RoleId into rm1
                           from subRole in rm1.DefaultIfEmpty()
                           join rm3 in _context.MasterData on user.Status equals rm3.Id into rm3
                           from subStatus in rm3.DefaultIfEmpty()
                           join md in _context.DepartmentMasters on user.DepartmentId equals md.DepartmentId into md
                           from subDept in md.DefaultIfEmpty()
                           join tm in _context.TenanttMasters on user.TenantId equals tm.TenantId
                           where user.UserId == usersId && user.TenantId == tenantId && user.DeletedOn == null
                           select new OrganisationUserPreview
                           {
                               UserId = user.UserId,
                               FirstName = $"{user.FirstName}",
                               LastName = $"{user.LastName}",
                               Username = user.KCUsername,
                               EmailId = user.EmailId,
                               UserRole = subRole.RoleName,
                               UserRoleId = user.RoleId,

                               Status = subStatus.Items,
                               StatusId = user.Status,

                               Department = subDept.DepartmentName,
                               DepartmentId = user.DepartmentId,

                               TenantId = user.TenantId,
                               Tenant = tm.ShortCode
                           }).AsQueryable();
            var userDetail = rawData.FirstOrDefault();

            var points = _context.UserPoints.Where(p => p.UserId == userDetail.UserId).Sum(p => p.Points);

            userDetail.UserPoints = points;

            return userDetail;
        }

        public async Task<PaginatedItems<UsersView>> GetAllUsersByTenantId(GetUserRequest getListRequest)
        {
            var users = await FetchUsersFromCache(getListRequest.TenantId);
            var rawData = (from us in _context.UserMasters

                           join md in _context.DepartmentMasters on us.DepartmentId equals md.DepartmentId into md
                           from subDept in md.DefaultIfEmpty()
                           join rm1 in _context.RoleMasters on us.RoleId equals rm1.RoleId into rm1
                           from subRole in rm1.DefaultIfEmpty()
                           join tm in _context.TenanttMasters on us.TenantId equals tm.TenantId
                           join rm3 in _context.MasterData on us.Status equals rm3.Id into rm3
                           from subStatus in rm3.DefaultIfEmpty()
                           where us.TenantId == getListRequest.TenantId && us.DeletedBy == null
                           orderby us.FirstName ascending
                           select new UsersView
                           {
                               UserId = us.UserId,
                               FullName = string.Format("{0} {1}", us.FirstName, us.LastName),
                               DepartmentId = us.DepartmentId,
                               Department = subDept.DepartmentName,
                               EmailAddress = us.EmailId,
                               RoleName = subRole.RoleName,
                               RoleId = us.RoleId,
                               Status = subStatus.Items,
                               StatusId = us.Status,
                           }).AsQueryable();

            if (getListRequest.DepartmentId > 0)
                rawData = rawData.Where(log => log.DepartmentId == getListRequest.DepartmentId);
            if (getListRequest.RoleId > 0)
                rawData = rawData.Where(log => log.RoleId == getListRequest.RoleId);
            if (getListRequest.StatusId > 0)
                rawData = rawData.Where(log => log.StatusId == getListRequest.StatusId);

            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
            .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                              .Take(getListRequest.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<UsersView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }

        public async Task<IList<UsersView>> GetAllUsers(int tenantId)
        {
            var users = await FetchUsersFromCache(tenantId);

            var selectedUsers = users.Where(user => user.TenantId == tenantId).ToList();

            return _mapper.Map<IList<UsersView>>(selectedUsers);
        }

        public async Task<PaginatedItems<UserProjectListView>> GetTasksByUserId(GetTaskListByUserId getTasksByUserID)
        {
            string searchString = string.Empty;

            var rawData = (from org in _context.TaskMasters
                           join wm in _context.WorkItemMasters on org.WorkItemId equals wm.WorkItemId
                           join md in _context.MasterData on wm.SourceId equals md.Id

                           join md1 in _context.MasterData on wm.StatusMasterDataId equals md1.Id
                           join user in _context.UserMasters on wm.AssignedToUserId equals user.UserId
                           join tm in _context.TenanttMasters on user.TenantId equals tm.TenantId
                           where getTasksByUserID.ActionId == wm.AssignedToUserId && getTasksByUserID.TenantId == user.TenantId
                           orderby tm.CreatedOn descending
                           select new UserProjectListView
                           {
                               TaskId = org.TaskId,
                               TaskName = wm.Title,
                               Task_Type = md.Items,
                               Effortspoint = org.EstimateEffortHours,

                               Status = md1.Items,
                           }).AsQueryable();
            var filteredData = DataExtensions.OrderBy(rawData, getTasksByUserID.ListRequests.SortColumn, getTasksByUserID.ListRequests.Sort == "asc")
                             .Skip(getTasksByUserID.ListRequests.PerPage * (getTasksByUserID.ListRequests.Page - 1))
                             .Take(getTasksByUserID.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getTasksByUserID.ListRequests.PerPage);
            var model = new PaginatedItems<UserProjectListView>(getTasksByUserID.ListRequests.Page, getTasksByUserID.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }

        public async Task<UserMaster> AddUsertoUsermasterForParticipants(string emailAddress)
        {
            UserMaster um = new UserMaster();
            um.TenantId = 35;
            um.FirstName = emailAddress;
            um.LastName = " ";
            um.KCUsername = emailAddress;

            um.EmailId = emailAddress;
            //um.DepartmentId = 6; //make nullable

            um.RoleId = 31;

            um.Status = 145;

            return await AddAsync(um);
        }

        public async Task<UserMaster> AddUserForTenant(int tenantId, int roleId, string emailId, string firstName, string lastname)
        {
            var tenantUser = new UserMaster();
            tenantUser.TenantId = tenantId;
            tenantUser.RoleId = roleId;
            tenantUser.FirstName = firstName;
            tenantUser.LastName = lastname;
            tenantUser.EmailId = emailId;
            tenantUser.DepartmentId = 6;
            tenantUser.Status = 145;
            tenantUser.CreatedOn = DateTime.UtcNow;
            tenantUser.CreatedBy = 1;
            return await AddAsync(tenantUser);
        }

        public async Task<UsersDetailView> GetUserDetails(int tenantId, string username)
        {
            var users = await FetchUsersFromCache(tenantId);

            var userDetail = users.FirstOrDefault(user => user.TenantId == tenantId && user.Username == username);

            return _mapper.Map<UsersDetailView>(userDetail);
        }

        public async Task<PaginatedItems<MeetingListView>> GetManagementReviewList(GetTaskListByUserId getTaskListByUserId)
        {
            var rawData = (from meeting in _context.MeetingPlans
                           join pt in _context.Participants on meeting.Id equals pt.ModuleEntityId
                           join tm in _context.TenanttMasters on meeting.TenantId equals tm.TenantId
                           where pt.UserId == getTaskListByUserId.ActionId && meeting.TenantId == getTaskListByUserId.TenantId
                           orderby tm.CreatedOn descending
                           select new MeetingListView()
                           {
                               Id = meeting.Id,
                               TenantId = meeting.TenantId,
                               Title = meeting.Title,
                               Location = meeting.Location,
                               StartDate = meeting.StartDate,
                               EndDate = meeting.EndDate,
                               MeetingType = meeting.MeetingType,
                               IsPublished = meeting.IsPublished,
                               CreatedBy = meeting.CreatedBy,
                           })
                           .AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, getTaskListByUserId.ListRequests.SortColumn, getTaskListByUserId.ListRequests.Sort == "asc")
                             .Skip(getTaskListByUserId.ListRequests.PerPage * (getTaskListByUserId.ListRequests.Page - 1))
                             .Take(getTaskListByUserId.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getTaskListByUserId.ListRequests.PerPage);
            var model = new PaginatedItems<MeetingListView>(getTaskListByUserId.ListRequests.Page, getTaskListByUserId.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }

        /// <summary>
        /// Common user store
        /// Fetch users from cache
        /// If cache does not exist then fetch from database
        /// </summary>
        /// <returns></returns>
        private async Task<IList<UserDomainModel>> FetchUsersFromCache(int tenantId)
        {
            if (!_cacheService.TryGet(CacheKeysConstants.GetUsersByTenant(tenantId), out IList<UserDomainModel> cachedItem))
            {
                cachedItem = await (
                from user in _context.UserMasters
                join role in _context.RoleMasters on user.RoleId equals role.RoleId into role
                from subRole in role.DefaultIfEmpty()
                join tenant in _context.TenanttMasters on user.TenantId equals tenant.TenantId
                join status in _context.MasterData on user.Status equals status.Id into status
                from SubStatus in status.DefaultIfEmpty()
                join department in _context.DepartmentMasters on user.DepartmentId equals department.DepartmentId into department
                from subDept in department.DefaultIfEmpty()
                where user.KCUserId != null && user.DeletedBy == null
                orderby user.UserId
                select new UserDomainModel
                {
                    TenantId = tenant.TenantId,
                    UserId = user.UserId,
                    Username = user.KCUsername.Trim().ToLower(),
                    RoleName = subRole.RoleName,
                    EmailAddress = user.EmailId,
                    Department = subDept.DepartmentName,
                    Status = SubStatus.Items,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = string.Format("{0} {1}", user.FirstName, user.LastName),
                    RoleId = user.RoleId
                    //DeletedBy = user.DeletedBy.HasValue ? user.DeletedBy.Value : 0
                }).ToListAsync();
                _cacheService.Set(CacheKeysConstants.GetUsersByTenant(tenantId), cachedItem);
            }
            return cachedItem;
        }
        public async Task<IList<UserListByTenantId>>GetUserBytenantId(int tenantId)
        {
		  var users = await (
		   from user in _context.UserMasters
			join role in _context.RoleMasters on user.RoleId equals role.RoleId into role
			from subRole in role.DefaultIfEmpty()
			join tenant in _context.TenanttMasters on user.TenantId equals tenant.TenantId
			join status in _context.MasterData on user.Status equals status.Id into status
			from SubStatus in status.DefaultIfEmpty()
			join department in _context.DepartmentMasters on user.DepartmentId equals department.DepartmentId into department
			from subDept in department.DefaultIfEmpty()
			where user.TenantId==tenantId && user.KCUserId != null && user.DeletedBy == null && user.RoleId== (int)IMSRolesMaster.ISOChampion || user.RoleId== (int)IMSRolesMaster.Auditor || user.RoleId== (int)IMSRolesMaster.Manager || user.RoleId== (int)IMSRolesMaster.Participants
		   orderby user.UserId
			select new UserListByTenantId
			{
				TenantId = tenant.TenantId,
				UserId = user.UserId,
				Username = user.KCUsername.Trim().ToLower(),
				RoleName = subRole.RoleName,
				EmailAddress = user.EmailId,
				Department = subDept.DepartmentName,
                DepartmentId=subDept.DepartmentId,
				Status = SubStatus.Items,
				FirstName = user.FirstName,
				LastName = user.LastName,
				FullName = string.Format("{0} {1}", user.FirstName, user.LastName),
				RoleId = user.RoleId
				
			}).ToListAsync();
            return await Task.FromResult(users);
		}


        public async Task <UserListByUserId> GetUserByUserId(int? userId)
        {
            var users =  (from user in _context.UserMasters
             join role in _context.RoleMasters on user.RoleId equals role.RoleId into role
             from subRole in role.DefaultIfEmpty()
             join tenant in _context.TenanttMasters on user.TenantId equals tenant.TenantId
             join status in _context.MasterData on user.Status equals status.Id into status
             from SubStatus in status.DefaultIfEmpty()
             join department in _context.DepartmentMasters on user.DepartmentId equals department.DepartmentId into department
             from subDept in department.DefaultIfEmpty()
             where user.UserId == userId 
             select new UserListByUserId()
             {
                 TenantId = tenant.TenantId,
                 UserId = user.UserId,
                 Username = user.KCUsername.Trim().ToLower(),
                 RoleName = subRole.RoleName,
                 EmailAddress = user.EmailId,
                 Department = subDept.DepartmentName,
                 DepartmentId = subDept.DepartmentId,
                 Status = SubStatus.Items,
                 FirstName = user.FirstName,
                 LastName = user.LastName,
                 FullName = string.Format("{0} {1}", user.FirstName, user.LastName),
                 RoleId = user.RoleId

             }).AsQueryable();
            return users.FirstOrDefault();
        }
    }
}