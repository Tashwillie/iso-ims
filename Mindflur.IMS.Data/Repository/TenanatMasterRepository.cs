using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class TenanatMasterRepository : BaseRepository<TenanttMaster>, ITenantMasterRepository
    {

        private readonly ICacheService _cacheService;
        public TenanatMasterRepository(IMSDEVContext dbContext, ILogger<TenanttMaster> logger, ICacheService cacheService) : base(dbContext, logger)
        {
            _cacheService = cacheService;
        }
        public async Task<IList<TenanttMaster>> GetTenantMaster()
        {
            return await GetTenants();

        }
        public async Task<IList<TenantView>> GetTenantsViewList()
        {
            if (!_cacheService.TryGet(CacheKeysConstants.GetTenantsView(), out IList<TenantView> cachedItem))
            {
                cachedItem = await (from tm in _context.TenanttMasters
                                    select new TenantView()
                                    {
                                        TenantId = tm.TenantId,
                                        Name = tm.Name,
                                        ShortCode = tm.ShortCode,
                                        FirstName = "NA, TBD",
                                        LastName = "Na, TBD"
                                    }).ToListAsync();

                _cacheService.Set(CacheKeysConstants.GetTenantsView(), cachedItem);
            }
            return await Task.FromResult(cachedItem);
        }

        public async Task<TenantView> GetTenantsById(int tenantId)
        {

            var rawData = (from tm in _context.TenanttMasters
                           where tm.TenantId == tenantId
                           select new TenantView()
                           {
                               TenantId = tm.TenantId,

                               Name = tm.Name,
                               //AdminUsername = tm.AdminUserName,
                               //AdminRoleName = tm.AdminRoleName,
                               ShortCode = tm.ShortCode,
                               FirstName = "NA, TBD",
                               LastName = "Na, TBD",




                           }).AsQueryable();
            return rawData.FirstOrDefault();
        }

        public async Task<PaginatedItems<TenantView>> GetAllTenants(GetListRequest getListRequest)
        {
            var rawData = (from tm in _context.TenanttMasters

                           select new TenantView()
                           {
                               TenantId = tm.TenantId,

                               Name = tm.Name,
                               // AdminUsername = tm.AdminUserName,
                               // AdminRoleName = tm.AdminRoleName,
                               ShortCode = tm.ShortCode,
                               FirstName = "NA, TBD",
                               LastName = "Na, TBD",


                           }).AsQueryable();
            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.SortColumn, getListRequest.Sort == "asc")
                              .Skip(getListRequest.PerPage * (getListRequest.Page - 1))
                              .Take(getListRequest.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.PerPage);
            var model = new PaginatedItems<TenantView>(getListRequest.Page, getListRequest.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);




        }
        public async Task ValidateTenant(CreateTenantViewModel tenant)
        {
            var rawData = await _context.TenanttMasters.Where(t => t.Name == tenant.Name).FirstOrDefaultAsync();

            if (rawData != null)
            {
                throw new ArgumentException(string.Format(RepositoryConstant.TenantAlreadyExitsErrorMessage,tenant.Name));
            }
        }
        public async Task ValidateClient(string clientName)
        {
            var tenants = await GetTenants();

            var tenant = tenants.Where(tenant => tenant.ClientName == clientName).FirstOrDefault();

            if (tenant != null)
            {
                throw new ArgumentException(string.Format(RepositoryConstant.ClientNameAlreadyExitsErrorMessage),clientName);
            }
        }

        public async Task<TenanttMaster> GetTenantByClientName(string clientName) 
        {
            var tenants = await GetTenants();
            return tenants.FirstOrDefault(T => T.ClientName == clientName);
        }

        public async Task<TenanttMaster> GetTenantByShortCode(string shortCode)
        {
            var tenants = await GetTenants();
            return tenants.Where(T => T.ShortCode == shortCode).FirstOrDefault();
        }

        public async Task<TenanttMaster> getByRealmName(string tenantName)
        {
            var tenants = await GetTenants();
            return tenants.Where(T => T.Name == tenantName).FirstOrDefault();
        }

        public async Task<TenanttMaster> CreateTenant(CreateTenantViewModel tnt, int UserId)
        {
            var tenantMaster = new TenanttMaster();
            tenantMaster.Name = tnt.Name;
            tenantMaster.ShortCode = tnt.ShortCode;
            tenantMaster.CreatedOn = DateTime.UtcNow;
            tenantMaster.CreatedBy = UserId;

            var tenant = await _context.TenanttMasters.AddAsync(tenantMaster);
            await _context.SaveChangesAsync();

            return tenant.Entity;
        }

        private async Task<IList<TenanttMaster>> GetTenants() 
        {
            if (!_cacheService.TryGet(CacheKeysConstants.GetTenantsMaster(), out IList<TenanttMaster> cachedItem))
            {
                cachedItem = await _context.TenanttMasters.ToListAsync();
                _cacheService.Set(CacheKeysConstants.GetTenantsMaster(), cachedItem);
            }
            return await Task.FromResult(cachedItem);
       }
    }
}
