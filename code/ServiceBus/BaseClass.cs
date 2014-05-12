using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.WindowsAzure;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class BaseClass
    {
        public BaseClass(string connectionStringName)
        {
            ConnectionString = null;
#if true
            ConnectionString = CloudConfigurationManager.GetSetting(connectionStringName);
#else
            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (settings != null)
            {
                ConnectionString = settings.ConnectionString;
            }
#endif
            NamespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
        }

        public string ConnectionString { get; private set; }
        public NamespaceManager NamespaceManager { get; private set; }
    }
}
