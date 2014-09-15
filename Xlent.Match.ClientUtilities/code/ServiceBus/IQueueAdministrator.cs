using System.Threading.Tasks;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueAdministrator
    {
        Task DeleteAsync();
    }
}