using Microsoft.ServiceBus;
using Microsoft.WindowsAzure;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class BaseClass
    {
        public BaseClass(string connectionStringName)
        {
            ConnectionString = CloudConfigurationManager.GetSetting(connectionStringName);
            NamespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
        }

        public string ConnectionString { get; private set; }
        public NamespaceManager NamespaceManager { get; private set; }
    }
}
