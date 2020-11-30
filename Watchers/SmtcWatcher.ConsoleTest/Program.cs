using System;
using System.Threading.Tasks;

namespace SmtcWatcher.ConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await TestSystemMediaWatcher();
            //TestSystemMediaWatcher();
            Console.ReadLine();
        }

        private static async Task TestSystemMediaWatcher()
        {
            var smtcWatcher = new SystemMediaWatcher();
            await smtcWatcher.InitializeAsync();

            while (true)
            {
            }
            smtcWatcher.Dispose();
        }

        private static void TestCyclicalSmtcWatcher()
        {
            var smtcWatcher = new CyclicalSmtcWatcher(TimeSpan.FromSeconds(3));
            while (true)
            {
            }
            smtcWatcher.Dispose();
        }
    }
}
