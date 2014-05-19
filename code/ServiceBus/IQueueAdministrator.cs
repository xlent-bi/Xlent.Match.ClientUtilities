namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueAdministrator
    {
        long GetLength();
        void Delete();
    }
}