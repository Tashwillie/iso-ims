using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.View;

namespace Mindflur.IMS.Business
{
    public class AuditableItemBusiness : IAuditableItemBusiness
    {

        private readonly IAuditItemsRepository _auditItemsRepository;
        public AuditableItemBusiness(IAuditItemsRepository auditItemsRepository)
        {
            _auditItemsRepository= auditItemsRepository;
        }
        public async Task<AuditItemStandardsViewModel> GetStandardsForAuditItems(int auditableItemId)
        {
            return await _auditItemsRepository.GetStandardsForAuditItems(auditableItemId);
        }
    }
}
