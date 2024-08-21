using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class ProjectMemberBusiness : IProjectMemberBusiness
    {
        private readonly IProjectMemberRepository _projectMemberRepository;
        public ProjectMemberBusiness(IProjectMemberRepository projectMemberRepository)
        {
            _projectMemberRepository = projectMemberRepository;
        }
        public async Task<IList<ProjectMember>> GetAllMembers()
        {
            return await _projectMemberRepository.GetAllMembers();

        }
        public async Task AddProjectMember(ProjectMember projectMember)
        {
            await _projectMemberRepository.AddAsync(projectMember);
        }
        public async Task DeleteProjectMember(int projectMemberId)
        {
            var member = await _projectMemberRepository.GetByIdAsync(projectMemberId);
            if (member == null)
                throw new NotFoundException(string.Format(ConstantsBusiness.ProjectMemberNotFoundErrorMessage), projectMemberId);
            await _projectMemberRepository.DeleteAsync(member);
        }
        public async Task<ProjectMember> GetMembersById(int projectMemberId)
        {
            var member = await _projectMemberRepository.GetByIdAsync(projectMemberId);
            return member == null ? throw new NotFoundException(string.Format(ConstantsBusiness.ProjectMemberNotFoundErrorMessage), projectMemberId) : member;
        }
        public async Task UpdateProjectMember(PutProjectMember projectMember, int projectMemberId)
        {
            var member = await _projectMemberRepository.GetByIdAsync(projectMemberId);
            if (member == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.ProjectMemberNotFoundErrorMessage), projectMemberId);
            }
            else
            {

                member.ProjectId = projectMember.ProjectId;

                member.UserId = projectMember.UserId;
                member.Role = projectMember.Role;
                await _projectMemberRepository.UpdateAsync(member);

            }



        }
    }
}
