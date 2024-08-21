using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ActivityLogRepository : BaseRepository<ActivityLog>, IActivityLogRepository
    {
      
        public ActivityLogRepository(IMSDEVContext dbContext, ILogger<ActivityLog> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<PaginatedItems<ActivityLogListView>> GetActivityLogs(ListActivityRequestModel listActivityRequestModel)
        {
            string searchString = string.Empty;
            var query = (from al in _context.ActivityLogs
                         join cm in _context.ControllerMasters on al.ControllerId equals cm.ControllerId
                         join cam in _context.ControllerActionMasters on al.ModuleAction equals cam.ActionId
                         where al.TenantId == listActivityRequestModel.TenatId
                         select new ActivityLogListView
                         {
                             ActivityLogId = al.ActivityLogId,
                             ControllerId = al.ControllerId,
                             ControllerName = cm.ControllerName,
                             ActionId = al.ModuleAction,
                             ControllerAction = cam.ControllerAction,
                             EntityId = al.EntityId
                         }).AsQueryable();
            if (listActivityRequestModel.ControllerId > 0)
                query = query.Where(log => log.ControllerId == listActivityRequestModel.ControllerId);

            if (listActivityRequestModel.ActionId > 0)
                query = query.Where(log => log.ActionId == listActivityRequestModel.ActionId);

            if (listActivityRequestModel.EntityId > 0)
                query = query.Where(log => log.EntityId == listActivityRequestModel.EntityId);

            var filteredData = DataExtensions.OrderBy(query, listActivityRequestModel.GridProperties.SortColumn, listActivityRequestModel.GridProperties.Sort == "asc")
                              .Skip(listActivityRequestModel.GridProperties.PerPage * (listActivityRequestModel.GridProperties.Page - 1))
                              .Take(listActivityRequestModel.GridProperties.PerPage);

            var totalItems = await query.LongCountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)listActivityRequestModel.GridProperties.PerPage);

            var model = new PaginatedItems<ActivityLogListView>(listActivityRequestModel.GridProperties.Page, listActivityRequestModel.GridProperties.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);


        }
        public async Task<IList<ControllerMaster>> ControllerList()
        {
            var controllerlist = await _context.ControllerMasters.ToListAsync();
            return await Task.FromResult(controllerlist);
        }

        public async Task<IList<ContollerActionMaster>> ActionList()
        {
            var actionList = await _context.ControllerActionMasters.ToListAsync();
            return await Task.FromResult(actionList);
        }
        public async Task<IList<ActivityLogEntityListView>> EntitiyList(int controllerId, int actionId, int tenantId)
        {
            var entityList = await (from al in _context.ActivityLogs
                                    where al.ControllerId == controllerId && al.ModuleAction == actionId && al.TenantId == tenantId
                                    select new ActivityLogEntityListView
                                    {
                                        EntityId = al.EntityId,
                                    }).ToListAsync();
            return await Task.FromResult(entityList);
        }
    }
}
