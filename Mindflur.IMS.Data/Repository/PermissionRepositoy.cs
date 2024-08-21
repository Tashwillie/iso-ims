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
    public class PermissionRepositoy : BaseRepository<PermissionMaster>, IPermissionRepositoy
    {
       
        public PermissionRepositoy(IMSDEVContext dbContext, ILogger<PermissionMaster> logger) : base(dbContext, logger)
        {
            
        }
        public async Task<PaginatedItems<PermissionMasterView>> GetPermissionList(GetListRequest getListRequest)
        {
            string searchString = string.Empty;

            var rawData = (from pm in _context.PermissionMasters
                           join cm in _context.ControllerMasters on pm.ControllerMasterId equals cm.ControllerId
                           join cam in _context.ControllerActionMasters on pm.ControllerActionMasterId equals cam.ActionId
                           orderby cm.ControllerName ascending
                           select new PermissionMasterView()
                           {
                               PermissionId = pm.PermissionId,
                               Controller = cm.ControllerName,
                               Action = cam.ControllerAction

                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.SortColumn, getListRequest.Sort == "asc")
                              .Skip(getListRequest.PerPage * (getListRequest.Page - 1))
                              .Take(getListRequest.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.PerPage);

            var model = new PaginatedItems<PermissionMasterView>(getListRequest.Page, getListRequest.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }
        public async Task<PermissionViewModel> GetPermitionDetailsById(int permissionId)
        {
            var permission = (from pm in _context.PermissionMasters
                              join cm in _context.ControllerMasters on pm.ControllerMasterId equals cm.ControllerId
                              join cam in _context.ControllerActionMasters on pm.ControllerActionMasterId equals cam.ActionId
                              select new PermissionViewModel
                              {
                                  PermissionId = pm.PermissionId,
                                  ControllerId = pm.ControllerMasterId,
                                  ControllerName = cm.ControllerName,
                                  ActionId = pm.ControllerActionMasterId,
                                  ControllerAction = cam.ControllerAction,

                              }).AsQueryable();
            return permission.FirstOrDefault();
        }
    }
}
