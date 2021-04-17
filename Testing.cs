using System;
using System.Threading;
using NLpMonitor;
using NLpCountdown;
using NLpBarrier;

namespace Test
{
    class Testing
    {
        #region  Utils

        static object objMonitor = new object();

        static int xMontitor = 0;

        static LpCountdown countdown = new LpCountdown(5);

        static LpBarrier barrier = new LpBarrier(3);

        static Random rnd = new Random();

        static string[] tasks = { "A", "B", "C" };


        static Semaphore semaphore = new Semaphore(3, 3);

        #endregion

        static void Main(string[] args)
        {
            // uncommet a line for test. 👇👇👇

            // TestMonitor();
            // TestCountdown();
            // TestBarrier();
            // TestSemaphore();
        }

        #region Test Semaphore


        static void TestSemaphore()
        {
            for (int i = 0; i < 10; ++i)
            {
                Thread t = new Thread(IntoShop);
                t.Name = $"{i}";
                t.Start();
            }
        }

        static void IntoShop()
        {
            semaphore.WaitOne();
            System.Console.WriteLine($"Customer {Thread.CurrentThread.Name} entering the store");
            Thread.Sleep(rnd.Next(1000, 2000));
            System.Console.WriteLine($"Customer {Thread.CurrentThread.Name} going out to the store");
            semaphore.Release();
        }


        #endregion

        #region Test Barrier

        static void TestBarrier()
        {
            new Thread(ProcessTask).Start();
            new Thread(ProcessTask).Start();
            new Thread(ProcessTask).Start();
            new Thread(ProcessTask).Start();
        }

        static void ProcessTask()
        {
            for (int i = 0; i < 3; ++i)
            {
                System.Console.WriteLine($"Complete task {tasks[i]}");
                barrier.SignalAndWait();
            }
        }

        #endregion

        #region Test countdown

        static void TestCountdown()
        {
            System.Console.WriteLine("Train arrive to the station.");
            new Thread(SetPassangers).Start();
            countdown.Wait();
            System.Console.WriteLine("Train ready.");
        }

        static void SetPassangers()
        {
            for (int i = 0; i < 5; ++i)
            {
                new Thread(CheckTicket).Start(i);
                Thread.Sleep(rnd.Next(1000, 3000));
            }
        }

        static void CheckTicket(object number)
        {
            Thread.Sleep(1000);
            System.Console.WriteLine($"Ticket ok for passanger {number}");
            countdown.Signal();
        }


        #endregion

        #region Test Monitor

        static void TestMonitor()
        {
            System.Console.WriteLine($"Initial value x = {xMontitor}");

            Thread t1 = new Thread(Increment);
            Thread t2 = new Thread(Decrement);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            System.Console.WriteLine($"Final value x = {xMontitor}");
        }


        static void Increment()
        {
            for (int i = 0; i < (1 << 10); ++i)
            {
                LpMonitor.Enter(objMonitor);
                xMontitor += 1;
                LpMonitor.Exit(objMonitor);
            }
        }

        static void Decrement()
        {
            for (int i = 0; i < (1 << 10); ++i)
            {
                LpMonitor.Enter(objMonitor);
                xMontitor -= 1;
                LpMonitor.Exit(objMonitor);
            }
        }

        #endregion

    }
}