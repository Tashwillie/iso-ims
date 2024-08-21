using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
	public class OpportunitiesMasterBusiness : IOpportunitiesMasterBusiness
	{
		private readonly IOpportunitiesMasterRepository _opportuniesMasterRepository;

		public OpportunitiesMasterBusiness(IOpportunitiesMasterRepository opportuniesMasterRepository)
		{
			_opportuniesMasterRepository = opportuniesMasterRepository;
		}

		public async Task<IList<OpportunitiesDropdown>> GetDropDown(int tenantId)
		{
			return await _opportuniesMasterRepository.GetDropDown(tenantId);
		}

		public async Task<OpportunitiesMaster> GetOpportunitiesById(int id, int tenantId)
		{
			var opportunities = await _opportuniesMasterRepository.GetByIdAsync(id);
			if (opportunities == null)
			{
				throw new NotFoundException("Opportunities", id);
			}
			else
			{
				return opportunities.Id == id && opportunities.TenantId == tenantId
					? opportunities
					: throw new BadRequestException("either opportunitiesId or tenantId dosent match");
			}
		}
	}
}