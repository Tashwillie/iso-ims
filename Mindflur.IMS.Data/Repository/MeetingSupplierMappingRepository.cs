using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class MeetingSupplierMappingRepository : BaseRepository<MeetingSupplierMapping>, IMeetingSupplierMappingRepository
    {
       

        public MeetingSupplierMappingRepository(IMSDEVContext context, ILogger<MeetingSupplierMapping> logger) : base(context, logger)
        {
          
        }

        public async Task<IList<SupplierDataView>> MeetingPlanPreview(int meetingId, int tenantId)
        {
            return await (from mp in _context.MeetingPlans
                          join mps in _context.MeetingSupplierMappings on mp.Id equals mps.MeetingId
                          join sm in _context.SupplierMasters on mps.SupplierId equals sm.SupplierId
                          join tm in _context.TenanttMasters on mps.TenantId equals tm.TenantId
                          where mps.MeetingId == meetingId && mps.TenantId == tenantId
                          select new SupplierDataView
                          {
                              SupplierId = sm.SupplierId,
                              SupplierName = sm.SupplierName
                          }).OrderByDescending(md => md.SupplierId).ToListAsync();
        }
    }
}