using AutoMapper;
using Mindflur.IMS.Application.DomainModel;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository.AutoMapper
{
	public class RepositoryProfile : Profile
	{
		public RepositoryProfile()
		{
			CreateMap<FindingPostView, WorkItemDomainModel>();
			CreateMap<WorkItemPostView, WorkItemDomainModel>();
			CreateMap<WorkItemPutView, WorkItemDomainModel>();
			CreateMap<WorkItemDomainModel, WorkItemMaster>().ReverseMap();
			CreateMap<WorkItemDomainModel, WorkItemPutView>();
			CreateMap<WorkItemDomainModel, AuditFindingsMapping>();

            CreateMap<UserDomainModel, UsersDetailView>();
            CreateMap<UserDomainModel, UsersView>();

            CreateMap<Documents, DocumentGridView>();
        }
	}
}