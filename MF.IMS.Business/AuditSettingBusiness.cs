using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class AuditSettingBusiness : IAuditSettingBusiness
    {
        private readonly IAuditSettingrepository _auditSettingrepository;
        public AuditSettingBusiness(IAuditSettingrepository auditSettingrepository)
        {
            _auditSettingrepository = auditSettingrepository;
        }


        public async Task<GetAuditSettingView> getAuditSetting(int tenantId)
        {
            return await _auditSettingrepository.getAuditSetting(tenantId);
        }
        public async Task updateAuditSetting(int tenantId, PutAuditSettingView auditSettingView)
        {
            var auditSetting = await _auditSettingrepository.getAuditSettingById(tenantId);
            if (auditSetting == null)
            {
                auditSetting = new AuditSettings();
                auditSetting.TenantId = tenantId;
                auditSetting.CreateAuditPlanRoleId = auditSettingView.CreateAuditRoleId;
                auditSetting.PublishAuditPlanRoleId = auditSettingView.PublishAuditPlanRoleId;
                auditSetting.AddAuditScheduleRoleId = auditSettingView.AddAuditScheduleRoleId;
                auditSetting.CreateAuditRoleId = auditSettingView.CreateAuditRoleId;
                auditSetting.AddFindingsRoleId = auditSettingView.AddFindingsRoleId;
                auditSetting.AddCorrectiveActionRoleId = auditSettingView.AddCorrectiveActionRoleId;
                auditSetting.AcceptResolvedCorrectiveActionRoleId = auditSettingView.AcceptResolvedCorrectiveActionRoleId;
                auditSetting.AcceptResolvedFindingRoleId = auditSettingView.AcceptResolvedFindingRoleId;
                auditSetting.CreatedOn = DateTime.UtcNow;
                await _auditSettingrepository.AddAsync(auditSetting);
            }
            else
            {
                auditSetting.TenantId = tenantId;
                auditSetting.CreateAuditPlanRoleId = auditSettingView.CreateAuditRoleId;
                auditSetting.PublishAuditPlanRoleId = auditSettingView.PublishAuditPlanRoleId;
                auditSetting.AddAuditScheduleRoleId = auditSettingView.AddAuditScheduleRoleId;
                auditSetting.CreateAuditRoleId = auditSettingView.CreateAuditRoleId;
                auditSetting.AddFindingsRoleId = auditSettingView.AddFindingsRoleId;
                auditSetting.AddCorrectiveActionRoleId = auditSettingView.AddCorrectiveActionRoleId;
                auditSetting.AcceptResolvedCorrectiveActionRoleId = auditSettingView.AcceptResolvedCorrectiveActionRoleId;
                auditSetting.AcceptResolvedFindingRoleId = auditSettingView.AcceptResolvedFindingRoleId;
                auditSetting.CreatedOn = DateTime.UtcNow;
                await _auditSettingrepository.UpdateAsync(auditSetting);

            }

        }
    }
}
