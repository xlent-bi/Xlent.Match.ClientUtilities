using System.Threading.Tasks;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueAdministrator
    {
        void Delete();
        Task DeleteAsync();
    }
}