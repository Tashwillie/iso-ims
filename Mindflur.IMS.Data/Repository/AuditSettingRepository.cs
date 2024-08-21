using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class AuditSettingRepository : BaseRepository<AuditSettings>, IAuditSettingrepository
    {
        
        public AuditSettingRepository(IMSDEVContext dbContext, ILogger<AuditSettings> logger) : base(dbContext, logger)
        {
            
        }



        public async Task<AuditSettings> getAuditSettingById(int Id)
        {
            var setting = await _context.AuditSettings.Where(rp => rp.TenantId == Id).ToListAsync();
            return setting.FirstOrDefault();

        }

        public async Task<GetAuditSettingView> getAuditSetting(int tenantId)
        {
            var auditSetting = (from audit in _context.AuditSettings
                                join role in _context.RoleMasters on audit.CreateAuditPlanRoleId equals role.RoleId
                                join role1 in _context.RoleMasters on audit.PublishAuditPlanRoleId equals role1.RoleId
                                join role2 in _context.RoleMasters on audit.AddAuditScheduleRoleId equals role2.RoleId
                                join role3 in _context.RoleMasters on audit.CreateAuditRoleId equals role3.RoleId
                                join role4 in _context.RoleMasters on audit.AddFindingsRoleId equals role4.RoleId
                                join role5 in _context.RoleMasters on audit.AddCorrectiveActionRoleId equals role5.RoleId
                                join role6 in _context.RoleMasters on audit.AcceptResolvedCorrectiveActionRoleId equals role6.RoleId
                                join role7 in _context.RoleMasters on audit.AcceptResolvedFindingRoleId equals role7.RoleId
                                where audit.TenantId == tenantId
                                select new GetAuditSettingView()


                                {
                                    AuditSettingId = audit.AuditSettingId,
                                    Tenant = tenantId,
                                    CreateAuditPlanRoleId = audit.CreateAuditPlanRoleId,
                                    CreateAuditPlanRole = role.RoleName,
                                    PublishAuditPlanRoleId = audit.PublishAuditPlanRoleId,
                                    PublishAuditPlanRole = role1.RoleName,
                                    AddAuditScheduleRoleId = audit.AddAuditScheduleRoleId,
                                    AddAuditScheduleRole = role2.RoleName,
                                    CreateAuditRoleId = audit.CreateAuditRoleId,
                                    CreateAuditRole = role3.RoleName,
                                    AddFindingsRoleId = audit.AddFindingsRoleId,
                                    AddFindingsRole = role4.RoleName,
                                    AddCorrectiveActionRoleId = audit.AddCorrectiveActionRoleId,
                                    AddCorrectiveActionRole = role5.RoleName,
                                    AcceptResolvedCorrectiveActionRoleId = audit.AcceptResolvedCorrectiveActionRoleId,
                                    AcceptResolvedCorrectiveActionRole = role6.RoleName,
                                    AcceptResolveFindingRoleId = audit.AcceptResolvedFindingRoleId,
                                    AcceptResolvedFindingRole = role7.RoleName,
                                }





                              ).AsQueryable();
            return auditSetting.FirstOrDefault();
        }



    }
}
