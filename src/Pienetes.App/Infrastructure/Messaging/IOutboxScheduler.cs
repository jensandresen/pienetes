namespace Pienetes.App.Infrastructure.Messaging
{
    public interface IOutboxScheduler
    {
        void ScheduleNow();
    }
}