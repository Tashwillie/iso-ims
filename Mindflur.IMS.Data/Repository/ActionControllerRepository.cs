using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ActionControllerRepository : BaseRepository<ContollerActionMaster>, IActionControllerRepository
    {
        

        public ActionControllerRepository(IMSDEVContext context, ILogger<ContollerActionMaster> logger) : base(context, logger)
        {
            
        }

        public async Task<IList<AllowedActionsDataview>> GetActionList()
        {
            var actions = await (from ac in _context.ControllerActionMasters
                                 select new AllowedActionsDataview
                                 {
                                     Id = ac.ActionId,
                                     Text = ac.ControllerAction
                                 }).ToListAsync();
            return actions;
        }
    }
}