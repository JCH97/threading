using System.Threading;
using NLpBarrier;
using NLpPhilosopher;
using NLpBarber;
using NLpCountdown;
using System;
using NLpMonitor;

namespace LP
{
    class Program
    {
        #region  Utils

        static object objMonitor = new object();

        static int xMontitor = 0;

        static LpCountdown countdown;

        static LpBarrier barrier;

        static Random rnd = new Random();

        static string[] tasks = { "A", "B", "C" };


        static Semaphore semaphore;

        #endregion

        static void Main(string[] args)
        {
            // uncommet a line for test. 👇👇👇

            // TestMonitor();
            // TestCountdown(5);
            // TestBarrier(2);
            // TestSemaphore();
        }

        #region Test Semaphore


        static void TestSemaphore()
        {
            semaphore = new Semaphore(3, 3);

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

        static void TestBarrier(int amount)
        {
            barrier = new LpBarrier(amount);
            for (int i = 0; i < amount; ++i)
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

        static void TestCountdown(int amount)
        {
            countdown = new LpCountdown(amount);

            System.Console.WriteLine("Train arrive to the station.");
            new Thread(SetPassangers).Start(amount);
            countdown.Wait();
            System.Console.WriteLine("Train ready.");
        }

        static void SetPassangers(object amount)
        {
            for (int i = 0; i < (int)amount; ++i)
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
