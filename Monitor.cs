using System.Collections.Generic;
using System;
using System.Threading;

namespace NLpMonitor
{
    public static class LpMonitor
    {
        static Dictionary<object, ParticularMonitor> warehouse = new Dictionary<object, ParticularMonitor>();
        static Semaphore mutex = new Semaphore(1, 1);

        public static void Enter(object obj)
        {
            mutex.WaitOne();

            if (!warehouse.ContainsKey(obj))
                warehouse.Add(obj, new ParticularMonitor());

            mutex.Release();

            warehouse[obj].Enter();
        }

        public static void Exit(object obj)
        {
            if (!warehouse.ContainsKey(obj) || Thread.CurrentThread.ManagedThreadId != warehouse[obj].ActiveNowThread)
                throw new SynchronizationLockException();
            warehouse[obj].Exit();
        }

        public static bool IsEntered(object obj)
        {
            if (!warehouse.ContainsKey(obj))
                throw new SynchronizationLockException();

            return warehouse[obj].ActiveNowThread == Thread.CurrentThread.ManagedThreadId;
        }

        public static bool Wait(object obj)
        {
            if (!warehouse.ContainsKey(obj) || Thread.CurrentThread.ManagedThreadId != warehouse[obj].ActiveNowThread)
                throw new SynchronizationLockException();
            return warehouse[obj].Wait();
        }

        public static void Pulse(object obj)
        {
            if (!warehouse.ContainsKey(obj))
                throw new SynchronizationLockException();
            warehouse[obj].Pulse();
        }

        public static void PulseAll(object obj)
        {
            if (!warehouse.ContainsKey(obj))
                throw new SynchronizationLockException();
            warehouse[obj].PulseAll();
        }
    }

    class ParticularMonitor
    {
        Semaphore semaphore, mutex, waitThreads;

        public int ActiveNowThread { get; private set; }
        public int WaitingThreads { get; private set; }

        public ParticularMonitor()
        {
            mutex = new Semaphore(1, 1);
            semaphore = new Semaphore(1, 1);
            waitThreads = new Semaphore(1, 1 << 30);

            WaitingThreads = 0;
            ActiveNowThread = -1;
        }

        public void Enter()
        {
            semaphore.WaitOne();
            ActiveNowThread = Thread.CurrentThread.ManagedThreadId;
        }

        public void Exit()
        {
            semaphore.Release();
            ActiveNowThread = -1;
        }

        public bool Wait()
        {
            Exit();
            mutex.WaitOne();
            WaitingThreads++;
            mutex.Release();
            waitThreads.WaitOne();
            Enter();

            return true;
        }

        public void Pulse()
        {
            mutex.WaitOne();

            if (WaitingThreads > 0)
                WaitingThreads--;

            mutex.Release();

            waitThreads.Release();
        }

        public void PulseAll()
        {
            waitThreads.Release(WaitingThreads);

            mutex.WaitOne();

            WaitingThreads = 0;

            mutex.Release();
        }

    }

}