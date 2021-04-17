using System;
using System.Threading;

namespace NLpBarrier
{
    public class LpBarrier
    {
        private Semaphore mutex, semaphore;

        public int Participants { get; private set; }
        public int RemainingParticipants { get; private set; }
        public int CurrentPhase { get; private set; }

        public LpBarrier(int participants)
        {
            Participants = participants;
            RemainingParticipants = participants;
            CurrentPhase = 1;
            this.mutex = new Semaphore(1, 1);
            this.semaphore = new Semaphore(0, participants - 1);
        }

        public int AddParticipant()
        {
            this.mutex.WaitOne();
            Participants++;
            RemainingParticipants++;
            this.mutex.Release();
            return CurrentPhase;
        }

        public void RemoveParticipant()
        {
            this.mutex.WaitOne();
            if (Participants == 0)
                throw new InvalidOperationException();
            Participants--;
            RemainingParticipants--;
            this.mutex.Release();
        }

        public void SignalAndWait()
        {
            this.mutex.WaitOne();
            if (RemainingParticipants > 1)
            {
                RemainingParticipants--;
                this.mutex.Release();
                semaphore.WaitOne();
            }
            else
            {
                semaphore.Release(Participants - 1);
                CurrentPhase++;
                RemainingParticipants = Participants;
                this.mutex.Release();
            }
        }
    }
}