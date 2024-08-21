namespace Mindflur.IMS.Runners.Review
{
    public interface IManagementReviewSchedule
    {
        Task NighlyRemider();

        Task OverDueReminder();
    }
}