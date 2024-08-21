using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class MasterDataRepository : BaseRepository<MasterDatum>, IMasterDataRepository
    {
       
        private readonly ICacheService _cacheService;

        public MasterDataRepository(IMSDEVContext dbContext, ICacheService cacheService, ILogger<MasterDatum> logger) : base(dbContext, logger)
        {
           
            _cacheService = cacheService;
        }

        public async Task<IList<SelectView>> GetMasterData(int tenantId)
        {

            if (!_cacheService.TryGet(CacheKeysConstants.GetMasterData(tenantId), out IList<SelectView> cachedItem))
            {
                cachedItem = await (from md in _context.MasterData
                                    where md.Active == true
                                    select new SelectView
                                    {
                                        Value = md.Id,
                                        Label = md.Items,
                                        ParentId = md.MasterDataGroupId
                                    })
                                    .OrderByDescending(md => md.Value)
                                    .ToListAsync();
                
                _cacheService.Set(CacheKeysConstants.GetMasterData(tenantId), cachedItem);
            }

            return cachedItem;
        }
        public async Task<IList<SelectView>> getDataByMasterGroupId(int tenantId, int masterDataGroupId)
        {
            if (!_cacheService.TryGet(CacheKeysConstants.GetMasterData(tenantId), out IList<SelectView> cachedItem))
            {
                cachedItem = await (from md in _context.MasterData
                                    where md.Active == true && md.MasterDataGroupId == masterDataGroupId
                                    select new SelectView
                                    {
                                        Value = md.Id,
                                        Label = md.Items,
                                        ParentId = md.MasterDataGroupId
                                    })
                .OrderByDescending(md => md.Value)
                              .ToListAsync();
                _cacheService.Set(CacheKeysConstants.GetMasterData(tenantId), cachedItem);
            }

            return cachedItem;
        }
        public async Task<PaginatedItems<MasterDataListView>> getMasterDataList(GetMasterDataList getMasterDataList)
        {
            var masterList = (from masterData in _context.MasterData
                              join md in _context.MasterDataGroups on masterData.MasterDataGroupId equals md.Id

                              select new MasterDataListView()
                              {
                                  Id = masterData.Id,
                                  Items = masterData.Items,
                                  Active = masterData.Active,
                                  ParentId = masterData.ParentId,
                                  MasterDataGroupId = masterData.MasterDataGroupId,
                                  MasterDataGroup = md.Name,
                                  OrderId = masterData.OrderId,

                              }).OrderByDescending(md => md.Items).AsQueryable();
            if (getMasterDataList.MasterDataGroupId > 0)
                masterList = masterList.Where(log => log.MasterDataGroupId == getMasterDataList.MasterDataGroupId);
            var filteredData = DataExtensions.OrderBy(masterList, getMasterDataList.ListRequests.SortColumn, getMasterDataList.ListRequests.Sort == "asc")
                             .Skip(getMasterDataList.ListRequests.PerPage * (getMasterDataList.ListRequests.Page - 1))
                             .Take(getMasterDataList.ListRequests.PerPage);

            var totalItems = await masterList.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getMasterDataList.ListRequests.PerPage);
            var model = new PaginatedItems<MasterDataListView>(getMasterDataList.ListRequests.Page, getMasterDataList.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }
        public async Task<MasterDataPreView> getPreviewData(int tenantId, int Id)
        {
            var rawData = (from masterData in _context.MasterData
                           join md in _context.MasterDataGroups on masterData.MasterDataGroupId equals md.Id
                           where masterData.Id == Id
                           select new MasterDataPreView()
                           {
                               Id = masterData.Id,
                               Items = masterData.Items,
                               MasterDataGroupId = masterData.MasterDataGroupId,
                               MasterDataGroup = md.Name,
                               ParentId = masterData.ParentId,
                               OrderId = masterData.OrderId,
                               Active = masterData.Active,
                           }).AsQueryable();
            return rawData.FirstOrDefault();
        }

    }
}
