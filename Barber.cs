using System.Threading;
using System;
using System.Collections.Generic;

namespace NLpBarber
{
    class LpBarber
    {
        private Queue<int> clients;
        private int currentClient;
        private Semaphore semaphore;
        private int maxClients;

        public LpBarber(int clients = 6)
        {
            this.maxClients = clients;
            this.clients = new Queue<int>();
            semaphore = new Semaphore(1, 1);
            currentClient = 0;
        }

        public void Simulate()
        {
            Thread arrival = new Thread(MakeClient);
            Thread barber = new Thread(Start);

            arrival.Start();
            barber.Start();

            arrival.Join();
            barber.Join();
        }


        private void MakeClient()
        {
            Random r = new Random();
            while (true)
            {
                if (r.Next(0, 10) % 2 == 0)
                {

                    if (clients.Count < maxClients)
                    {
                        currentClient++;
                        Console.WriteLine($"Client {currentClient} arrive");

                        semaphore.WaitOne();

                        clients.Enqueue(currentClient);

                        semaphore.Release();
                    }
                    else
                        Console.WriteLine($"Full barber");

                    Thread.Sleep(r.Next(1000, 3000));
                }
            }
        }
        private void Start()
        {
            Random rnd = new Random();
            while (true)
            {
                if (clients.Count > 0)
                {
                    semaphore.WaitOne();

                    int now = clients.Dequeue();

                    semaphore.Release();

                    Console.WriteLine($"Shaving to {now}");
                    Thread.Sleep(rnd.Next(1000, 5000));
                    Console.WriteLine($"Client {now} complete");
                }
                else
                {
                    Console.WriteLine("Sleeping");
                    Thread.Sleep(500);
                }
            }
        }
    }
}