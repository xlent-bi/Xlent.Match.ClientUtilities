using System.Threading;
using System.Threading.Tasks;
using Xlent.Match.ClientUtilities;

namespace Console.SampleAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopEvent = new ManualResetEvent(false);
            var subscription = new AdapterSubscription("Ax");

            System.Console.WriteLine("Start processing requests...");

            Task.Run(() => ProcessRequests(subscription, stopEvent));

            System.Console.WriteLine("Press any key to stop processing messages...");
            System.Console.ReadKey();

            stopEvent.Set();
            Thread.Sleep(1000);
        }

        private static void ProcessRequests(AdapterSubscription subscription, ManualResetEvent stopEvent)
        {
            subscription.ProcessRequests(
                AdapterBusinessLogic.GetObjectData,
                AdapterBusinessLogic.UpdateObject,
                AdapterBusinessLogic.CreateObject,
                stopEvent);
        }
    }
}
