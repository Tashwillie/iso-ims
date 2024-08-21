using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Custrom;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class UserPointsBusiness : IUserPointsBusiness
    {
        private readonly IUserPointRepository _userPointRepository;
        public UserPointsBusiness(IUserPointRepository userPointRepository)
        {
            _userPointRepository = userPointRepository;
        }
        public async Task<IList<PointsForUserView>> GetPointsByUserId(int userId)
        {
            return await _userPointRepository.GetPointsByUserId(userId);
        }
        public async Task<IList<PointsForUserView>> GetPoints()
        {
            return await _userPointRepository.GetPoints();
        }

        public async Task<UserPoints> UpsertUserPoint(UpsertUserPointRequest upsertUserPoint)
        {
            int earnedPoint = upsertUserPoint.DueDate > DateTime.UtcNow ? 1 : -1;
            var userPoints = new UserPoints();
            userPoints.Points = earnedPoint;
            userPoints.UserId = upsertUserPoint.UserId;
            userPoints.ModuleId = upsertUserPoint.ModuleId;
            userPoints.ModuleItemId = upsertUserPoint.ModuleItemId;
            userPoints.CreatedOn = DateTime.UtcNow;



            return await _userPointRepository.AddAsync(userPoints);
        }


    }
}
