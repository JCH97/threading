using System;
using System.Threading;

namespace NLpCountdown
{
    public class LpCountdown
    {
        Semaphore semaphore, mutex;

        public int InitialCount { get; private set; }
        public int CurrentCount { get; private set; }
        public int WaitingThreads { get; private set; }


        public bool IsSet { get { return CurrentCount == 0; } }


        public LpCountdown(int initial)
        {
            this.semaphore = new Semaphore(0, int.MaxValue);
            this.mutex = new Semaphore(1, 1);
            InitialCount = CurrentCount = initial;
            WaitingThreads = 0;
        }

        public void AddCount()
        {
            this.mutex.WaitOne();
            CurrentCount++;
            this.mutex.Release();
        }

        public void AddCount(int count)
        {
            this.mutex.WaitOne();
            CurrentCount += count;
            this.mutex.Release();
        }

        public void Reset()
        {
            this.mutex = new Semaphore(1, 1);
            this.semaphore = new Semaphore(0, int.MaxValue);
            CurrentCount = InitialCount;
            WaitingThreads = 0;
        }

        public void Reset(int count)
        {
            this.mutex = new Semaphore(1, 1);
            this.semaphore = new Semaphore(0, int.MaxValue);
            InitialCount = CurrentCount = count;
            WaitingThreads = 0;
        }

        public void Signal()
        {
            this.mutex.WaitOne();

            if (CurrentCount > 0)
                CurrentCount--;

            if (CurrentCount == 0)
            {
                this.semaphore.Release(WaitingThreads);
                WaitingThreads = 0;
            }

            this.mutex.Release();
        }

        public void Signal(int count)
        {
            this.mutex.WaitOne();

            if (CurrentCount > count)
                CurrentCount -= count;

            if (CurrentCount == 0)
            {
                this.semaphore.Release(WaitingThreads);
                WaitingThreads = 0;
            }

            this.mutex.Release();
        }

        public void Wait()
        {
            this.mutex.WaitOne();
            if (CurrentCount > 0)
            {
                WaitingThreads++;
                this.mutex.Release();
                this.semaphore.WaitOne();
            }
            else
                this.mutex.Release();
        }
    }
}