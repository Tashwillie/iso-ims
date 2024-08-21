namespace Mindflur.IMS.Runners.Audit
{
    public interface IAuditSchedule
    {
        Task NighlyRemider();

        Task OverDueReminder();
    }
}