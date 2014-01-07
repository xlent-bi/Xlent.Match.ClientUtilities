using System.Collections.Generic;
using System.Threading.Tasks;
namespace Crm.ClientAdapter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if true
            List<Task> tasks = new List<Task>();
            
            var task = PersonsSubscriber.HandleRequests();
            tasks.Add(task);
            task = CustomerSubscriber.HandleRequests();
            tasks.Add(task);
            Task.WaitAll(tasks.ToArray());
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new Service1() 
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
