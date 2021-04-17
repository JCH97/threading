using System.Collections.Generic;
using System;
using System.Threading;

namespace NLpPhilosopher
{
    class Fork
    {
        public int Number { get; private set; }

        public Fork(int number)
        {
            Number = number;
        }
    }

    class LpPhilosopher
    {
        Tuple<Fork, Fork> forks; //left - rigth
        string name;

        Random random;

        public LpPhilosopher(Fork fork1, Fork fork2, string name, Random rnd)
        {
            this.forks = new Tuple<Fork, Fork>(fork1, fork2);
            this.name = name;
            this.random = rnd;
        }

        public void Simulate()
        {
            while (true)
            {
                System.Console.WriteLine($"{this.name} is thinking");

                Thread.Sleep(random.Next(1000, 5000));

                System.Console.WriteLine($"{this.name} wants to eat");

                if (Monitor.TryEnter(this.forks.Item1, random.Next(3000)))
                {
                    System.Console.WriteLine($"{this.name} has the fork with id {this.forks.Item1.Number}");
                    Thread.Sleep(500);

                    if (Monitor.TryEnter(this.forks.Item2, random.Next(3000)))
                    {
                        System.Console.WriteLine($"{this.name} has the fork with id {this.forks.Item2.Number}");
                        System.Console.WriteLine($"{this.name} is eating with forks {this.forks.Item1.Number} and {this.forks.Item2.Number}");
                        Thread.Sleep(random.Next(3000, 5000));

                        Monitor.Exit(this.forks.Item2);
                        System.Console.WriteLine($"{this.name} put down the fork with id {this.forks.Item2.Number}");
                    }

                    Thread.Sleep(500);

                    Monitor.Exit(this.forks.Item1);
                    System.Console.WriteLine($"{this.name} put down the fork with id {this.forks.Item1.Number}");
                }
            }
        }

    }

    class Run
    {
        public void Main()
        {
            Random rnd = new Random();
            Fork[] forks = { new Fork(1), new Fork(2), new Fork(3), new Fork(4), new Fork(5) };

            LpPhilosopher[] philosophers = {
                new LpPhilosopher(forks[0], forks[1], "F1", rnd),
                new LpPhilosopher(forks[1], forks[2], "F2", rnd),
                new LpPhilosopher(forks[2], forks[3], "F3", rnd),
                new LpPhilosopher(forks[3], forks[4], "F4", rnd),
                new LpPhilosopher(forks[4], forks[0], "F5", rnd)
            };

            foreach (var item in philosophers)
                new Thread(item.Simulate).Start();
        }
    }
}