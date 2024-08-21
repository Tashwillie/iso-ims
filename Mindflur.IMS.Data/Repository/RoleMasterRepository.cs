using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class RoleMasterRepository : BaseRepository<RoleMaster>, IRoleMasterRepository
    {
        private readonly ICacheService _cacheService;

        public RoleMasterRepository(IMSDEVContext dbContext, ILogger<RoleMaster> logger, ICacheService cacheService) : base(dbContext, logger)
        {
            _cacheService = cacheService;
        }

        public async Task<IList<RoleMaster>> GetRoles()
        {
            if (!_cacheService.TryGet(CacheKeysConstants.GetRoles(), out IList<RoleMaster> cachedItem))
            {
                cachedItem = await (from role in _context.RoleMasters
                                    select role).ToListAsync();

                _cacheService.Set(CacheKeysConstants.GetRoles(), cachedItem);
            }
            return await Task.FromResult(cachedItem);
        }

        public async Task<RoleMaster> GetRoleByName(int tenant, int roleId)
        {
            var query = (from role in _context.RoleMasters
                         join kcrole in _context.KCRoleToRoleMappings on role.RoleId equals kcrole.RoleId
                         where kcrole.TenantId == tenant && role.RoleId == roleId
                         select role)
                         .AsQueryable<RoleMaster>();

            return await query?.FirstOrDefaultAsync();
        }

        public async Task<IList<RoleMaster>> GetRoleList()
        {
            var roles = (from role in _context.RoleMasters
                         where role.RoleName != "super-admin-role"
                         select role
                       ).AsQueryable<RoleMaster>();
            return await roles.ToListAsync();
        }
    }
}