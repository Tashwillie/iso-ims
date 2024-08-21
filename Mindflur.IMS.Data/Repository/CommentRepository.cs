using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
       
        public CommentRepository(IMSDEVContext dbContext, ILogger<Comment> logger) : base(dbContext, logger)
        {
            
        }
        public async Task<IList<CommentDataView>> GetIncidentComments(int incidentId)
        {
            return await (from comment in _context.Comments
                          join user in _context.UserMasters on comment.CreatedBy equals user.UserId into user
                          from subuser in user.DefaultIfEmpty()
                          where comment.SourceItemId == incidentId

                          select new CommentDataView
                          {
                              CommentId = comment.CommentId,
                              CommentContent = comment.CommentContent,
                              ParentCommentId = comment.ParentCommentId,
                              CreatedBy = $"{subuser.FirstName} {subuser.LastName}",
                              CreatedOn = comment.CreatedOn,
                          })
                          .OrderByDescending(md => md.CommentId)
                          .ToListAsync();
        }
        public async Task<IList<GetCommentView>> GetCommentsBySourceIdAndSourceItemId(int sourceId, int sourceItemId)
        {
            var model = await (from comment in _context.Comments
                               join user in _context.UserMasters on comment.CreatedBy equals user.UserId into user
                               from subuser in user.DefaultIfEmpty()
                               where comment.SourceId == sourceId && comment.SourceItemId == sourceItemId

                               select new GetCommentView
                               {
                                   CommentId = comment.CommentId,
                                   CommentContent = comment.CommentContent,
                                   ParentCommentId = comment.ParentCommentId,
                                   CreatedBy = $"{subuser.FirstName} {subuser.LastName}",
                                   CreatedOn = comment.CreatedOn,
                               })
                          .OrderByDescending(md => md.CommentId)
                          .ToListAsync();
            return await Task.FromResult(model);
        }

    }
}