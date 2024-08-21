using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class RoleMappingRepository : BaseRepository<KCRoleToRoleMapping>, IRoleMappingRepository
    {

        public RoleMappingRepository(IMSDEVContext dbContext, ILogger<KCRoleToRoleMapping> logger) : base(dbContext, logger)
        {
        }

        public async Task<KCRoleToRoleMapping> CreateRoleForTenant(int tenantId, Guid kcRoleid)
        {
            var clientrole = new KCRoleToRoleMapping();
            clientrole.RoleId = 35; //Hack Hardcoded for tenant admin user
            clientrole.TenantId = tenantId;
            clientrole.KCRoleId = kcRoleid;

            await _context.KCRoleToRoleMappings.AddAsync(clientrole);
            await _context.SaveChangesAsync();
            return clientrole;
        }
    }
}