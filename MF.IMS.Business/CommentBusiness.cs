using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class CommentBusiness : ICommentBusiness
    {
        private readonly ICommentRepository _commentRepository;
        public CommentBusiness(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<IList<GetCommentView>> GetCommentsBySourceIdAndSourceItemId(int sourceId, int sourceItemId)
        {
            return await _commentRepository.GetCommentsBySourceIdAndSourceItemId(sourceId, sourceItemId);

        }
        public async Task AddComment(PostCommentView postCommentView, int userId, int tenantId)
        {
            Comment comment = new Comment();

            comment.SourceId = postCommentView.SourceId;
            comment.SourceItemId = postCommentView.SourceItemId;
            comment.CommentContent = postCommentView.CommentContent;
            comment.ParentCommentId = postCommentView.ParentCommentId;
            comment.ContentType = postCommentView.ContentType;
            comment.CreatedBy = userId;
            comment.CreatedOn = DateTime.UtcNow;
            await _commentRepository.AddAsync(comment);
        }


    }
}
