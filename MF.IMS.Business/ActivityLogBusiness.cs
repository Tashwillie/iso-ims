using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class ActivityLogBusiness : IActivityLogBusiness
    {

        private readonly IActivityLogRepository _activityLogRepository;
        public ActivityLogBusiness(IActivityLogRepository activityLogRepository)
        {
            _activityLogRepository = activityLogRepository;
        }

        public async Task<ActivityLog> GetActivityLogById(int activityLogId)
        {
            var getlogs = await _activityLogRepository.GetByIdAsync(activityLogId);
            return getlogs;
        }

        public async Task<PaginatedItems<ActivityLogListView>> GetActivityLogs(ListActivityRequestModel activityListView)
        {
            return await _activityLogRepository.GetActivityLogs(activityListView);
        }
        public async Task<IList<ControllerMaster>> ControllerList()
        {
            return await _activityLogRepository.ControllerList();
        }

        public async Task<IList<ContollerActionMaster>> ActionList()
        {
            return await _activityLogRepository.ActionList();
        }

        public async Task<IList<ActivityLogEntityListView>> EntitiyList(int controllerId, int actionId, int tenantId)
        {
            return await _activityLogRepository.EntitiyList(controllerId, actionId, tenantId);
        }
    }
}
