using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class DocumentTagsRepository : BaseRepository<DocumentTags>, IDocumentTagsRepository
    {
        

        public DocumentTagsRepository(IMSDEVContext dbContext, ILogger<DocumentTags> logger) : base(dbContext, logger)
        {
           
        }

        public async Task<IList<TagDataView>> GetDocumentTags(int documentId)
        {
            return await (from documentTags in _context.DocumentTags
                          join md in _context.MasterData on documentTags.MasterDataDocumentTagId equals md.Id
                          where documentTags.DocumentId == documentId

                          select new TagDataView
                          {
                              TagId = md.Id,
                              TagName = md.Items,
                          })
                          .OrderByDescending(md => md.TagId)
                          .ToListAsync();
        }
    }
}