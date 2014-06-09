using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class BaseClass
    {
        private static bool _hasBeenInitialized;
        private static readonly Object LockThisClass = new Object();
        private static RetryPolicy<ServiceBusTransientErrorDetectionStrategy> _retryPolicy;

        public BaseClass(string connectionStringName) 
        {
            ConnectionString = CloudConfigurationManager.GetSetting(connectionStringName);
            MessagingFactory = MessagingFactory.CreateFromConnectionString(ConnectionString);
            NamespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
            if (_hasBeenInitialized) return;

            lock (LockThisClass)
            {
                if (_hasBeenInitialized) return;
                _retryPolicy = new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(RetryStrategy.DefaultClientRetryCount);
                _hasBeenInitialized = true;
            }
        }

        public RetryPolicy<ServiceBusTransientErrorDetectionStrategy> RetryPolicy { get { return _retryPolicy; } }

        public MessagingFactory MessagingFactory { get; private set; }
        public string ConnectionString { get; private set; }
        public NamespaceManager NamespaceManager { get; private set; }

        protected void CreatePairedNamespaceManager(string pairedConnectionStringName)
        {
            var pairedConnectionString = CloudConfigurationManager.GetSetting(pairedConnectionStringName);
            var pairedNamespaceManager = NamespaceManager.CreateFromConnectionString(pairedConnectionString);
            var pairedMessagingFactory = MessagingFactory.CreateFromConnectionString(pairedConnectionString);
            var pairedNamespaceOptions = new SendAvailabilityPairedNamespaceOptions(pairedNamespaceManager,
                pairedMessagingFactory);
            
            MessagingFactory.PairNamespaceAsync(pairedNamespaceOptions).Wait();
        }
    }
}
