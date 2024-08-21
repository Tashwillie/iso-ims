using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class DocumentRoleRepository : BaseRepository<DocumentRoles>, IDocumentRoleRepository
    {
        public DocumentRoleRepository(IMSDEVContext context, ILogger<DocumentRoles> logger) : base(context, logger)
        {
                
        }
        public async Task<IList<DocumentRoleDataView>> GetDocumentRoles(int documentId)
        {
            return await (from documentroles in _context.DocumentRoles
                          join rm in _context.RoleMasters on documentroles.RolesId equals rm.RoleId
                          where documentroles.DocumentId == documentId

                          select new DocumentRoleDataView
                          {
                              DocumentRoleId = documentroles.RolesId,
                              DocumentRoleName = rm.RoleName,
                          })
                          .OrderByDescending(md => md.DocumentRoleId)
                          .ToListAsync();
        }
    }
}
