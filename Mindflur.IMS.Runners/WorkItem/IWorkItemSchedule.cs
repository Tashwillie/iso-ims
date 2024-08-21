namespace Mindflur.IMS.Runners.WorkItem
{
    public interface IWorkItemSchedule
    {
        Task NighlyRemider();
        Task OverDueReminder();
    }
}
