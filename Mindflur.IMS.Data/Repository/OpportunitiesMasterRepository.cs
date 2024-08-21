using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class OpportunitiesMasterRepository : BaseRepository<OpportunitiesMaster>, IOpportunitiesMasterRepository
	{
		

		public OpportunitiesMasterRepository(IMSDEVContext dbContext, ILogger<OpportunitiesMaster> logger) : base(dbContext, logger)
		{
			
		}

		public async Task<IList<OpportunitiesDropdown>> GetDropDown(int tenantId)
		{
			var opportunities = await (from op in _context.WorkItemMasters
									   where op.TenantId == tenantId && op.WorkItemTypeId==(int)IMSModules.Opportunity
									   select new OpportunitiesDropdown
									   {
										   Id = op.WorkItemId,
										   Title = op.Title,
									   }).ToListAsync();
			return await Task.FromResult(opportunities);
		}

		public async Task<BackTrace> GetOpportunitiesById(int moduleEntitiyId)
		{
			var rawdata = (from om in _context.OpportunitiesMasters
						   join um in _context.UserMasters on om.CreatedBy equals um.UserId
						   where moduleEntitiyId == om.Id && om.Source == (int)IMSModules.InternalAudit
						   select new BackTrace
						   {
							   ModuleId = (int)IMSControllerCategory.Opportunities,
							   ModuleName = "Opportunities",
							   ModuleItemId = om.Id,
							   Title = " ",
							   Content = om.OpportunitesDescription,
							   CreatedOn = om.CreatedOn,
							   CreatedBy = $"{um.FirstName} {um.LastName}",
							   OrderNumber = 1
						   }).AsQueryable();
			return rawdata.FirstOrDefault();
		}
	}
}