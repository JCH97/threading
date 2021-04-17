using System.Threading;
using NLpBarrier;
using NLpPhilosopher;
using NLpBarber;

namespace LP
{
    class Program
    {
        static LpBarrier barrier = new LpBarrier(3);
        static string[] productsHelp = { "Art 1", "Art 2", "Art 3" };

        static void Main(string[] args)
        {
            // TestBarrier();

            // Run philosopher = new Run();
            // philosopher.Main();

            LpBarber simulate = new LpBarber();
            simulate.Simulate();
        }

        static void TestBarrier()
        {
            new Thread(HelpPrint).Start();
            new Thread(HelpPrint).Start();
            new Thread(HelpPrint).Start();
        }

        static void HelpPrint()
        {
            for (int i = 0; i < Program.productsHelp.Length; ++i)
            {
                System.Console.WriteLine($"Product {Program.productsHelp[i]}");
                Thread.Sleep(1000);
                barrier.SignalAndWait();
            }
        }
    }
}
