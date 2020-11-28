using System;
using System.Threading.Tasks;

namespace SmtcWatcher.ConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var smtcWatcher = new SystemMediaWatcher();
            await smtcWatcher.InitializeAsync();

            while (true)
            {

            }
            smtcWatcher.Dispose();
        }
    }
}
