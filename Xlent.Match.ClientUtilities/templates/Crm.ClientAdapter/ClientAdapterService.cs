using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Crm.ClientAdapter
{
    public partial class ClientAdapterService : ServiceBase
    {
        public ClientAdapterService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            List<Task> tasks = new List<Task>();

            var task = Subscriber.PersonsSubscriber.HandleRequests();
            tasks.Add(task);
            task = Subscriber.Customer.HandleRequests();
            tasks.Add(task);
            Task.WaitAll(tasks.ToArray());
        }

        protected override void OnStop()
        {
        }
    }
}
