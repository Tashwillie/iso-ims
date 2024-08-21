using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindflur.IMS.Data.Repository
{
    public class DocumentUserRepository : BaseRepository<DocumentUsers>, IDocumentUserRepository
    {
        public DocumentUserRepository(IMSDEVContext context, ILogger<DocumentUsers> logger) : base(context, logger)
        {
                
        }
        public async Task<IList<DocumentUserDataView>> GetDocumentUsers(int documentId)
        {
            return await (from documentusers in _context.DocumentUsers
                          join um in _context.UserMasters on documentusers.UserId equals um.UserId
                          where documentusers.DocumentId == documentId

                          select new DocumentUserDataView
                          {
                              DocumentUserId = documentusers.UserId,
                              DocumentUserName = $"{um.FirstName} {um.LastName}",
                          })
                          .OrderByDescending(md => md.DocumentUserId)
                          .ToListAsync();
        }
        public async Task<DocumentUserDataView> AuthorizedUsers(int documentId, int userId)
        {
            var users =   (from documentusers in _context.DocumentUsers
                          join um in _context.UserMasters on documentusers.UserId equals um.UserId
                          where documentusers.DocumentId == documentId && documentusers.UserId == userId

                          select new DocumentUserDataView
                          {
                              DocumentUserId = documentusers.UserId,
                              DocumentUserName = $"{um.FirstName} {um.LastName}",
                          }).AsQueryable();
            return users.FirstOrDefault();

        }
    }
}
