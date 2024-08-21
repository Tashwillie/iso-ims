using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ProjectMemberRepository : BaseRepository<ProjectMember>, IProjectMemberRepository
    {
        

        public ProjectMemberRepository(IMSDEVContext dbContext, ILogger<ProjectMember> logger) : base(dbContext, logger)
        {
           
        }

        public async Task<IList<ProjectMember>> GetAllMembers()
        {
            var projectMember = await _context.ProjectMembers.ToListAsync();
            return projectMember;
        }
    }
}