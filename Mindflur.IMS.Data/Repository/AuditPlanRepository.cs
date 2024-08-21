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
    public class AuditPlanRepository : BaseRepository<AuditPlan>, IAuditPlanRepository
    {
      
        public AuditPlanRepository(IMSDEVContext dbContext, ILogger<AuditPlan> logger) : base(dbContext, logger)
        {
            
        }
        public async Task<PaginatedItems<AuditPlanGridView>> GetAuditPlan(GetListRequest getListRequest)
        {
            var rawData = (from ap in _context.AuditPlans
                           join ai in _context.AuditableItems on ap.AuditProgramId equals ai.AuditProgramId
                           join us in _context.UserMasters on ai.AuditorName equals us.UserId
                           join acl in _context.AuditableItemClauses on ai.Id equals acl.AuditableItemId


                           select new AuditPlanGridView()
                           {
                               Date = ai.StartDate,
                               Time = ai.EndDate,
                               //AreaToBeAudited = ai.AuditableItems,
                               Auditor = $"{us.FirstName} {us.LastName}"
                           }).AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.SortColumn, getListRequest.Sort == "asc")
                              .Skip(getListRequest.PerPage * (getListRequest.Page - 1))
                              .Take(getListRequest.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.PerPage);

            var model = new PaginatedItems<AuditPlanGridView>(getListRequest.Page, getListRequest.PerPage, totalPages, filteredData);

            return await Task.FromResult(model);
        }

        public async Task<AuditPlan> GetAuditPlanByAuditId(int auditId, int tenantId)
        {
            var plan = (from ap in _context.AuditPlans
                        join apro in _context.AuditPrograms on ap.AuditProgramId equals apro.Id
                        join tm in _context.TenanttMasters on apro.TenantId equals tm.TenantId
                        where ap.AuditProgramId == auditId && apro.TenantId == tenantId
                        select new AuditPlan
                        {
                            Id = ap.Id,
                            AuditProgramId = ap.AuditProgramId,
                            Scope = ap.Scope,
                            Objectives = ap.Objectives
                        }).AsQueryable();
            return plan.FirstOrDefault();
        }


        public AuditPlan GetAuditPlanByAuditProgramId(int auditId)
        {
            return _context.AuditPlans.FirstOrDefault(ap => ap.AuditProgramId == auditId);
        }

    }
}
