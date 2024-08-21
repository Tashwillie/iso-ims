using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class ControllerMasterRepository : BaseRepository<ControllerMaster>, IControllerMasterRepository
    {
        
        public ControllerMasterRepository(IMSDEVContext context, ILogger<ControllerMaster> logger) : base(context, logger)
        {
            
        }
        public async Task<IList<PermissionsDataview>> GetConrollerActionList(int tenantId, int roleId)
        {
            var list = await (from cm in _context.ControllerMasters
                              join rp in _context.RolePermissions on cm.ControllerId equals rp.ControllerId into permissions
                              from allPermissions in permissions.DefaultIfEmpty()
                              select new PermissionsDataview
                              {
                                  ControllerId = cm.ControllerId,
                                  Controller = cm.ControllerName,
                                  Action = allPermissions.ActionId,
                              }).ToListAsync();
            return list;
        }

        public async Task<PaginatedItems<ContollerListView>> GetControllerList(GetControllerListRequest getListRequest)
        {
            string searchString = string.Empty;

            var rawData = (from ap in _context.ControllerMasters

                           select new ContollerListView()
                           {
                               ControllerId = ap.ControllerId,
                               ControllerName = ap.ControllerName,

                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
                              .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                              .Take(getListRequest.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);

            var model = new PaginatedItems<ContollerListView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }

    }
}
