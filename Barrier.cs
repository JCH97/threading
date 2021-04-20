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

        private Action<LpBarrier> toDo;

        public LpBarrier(int participants, Action<LpBarrier> toDo = null)
        {
            Participants = participants;
            RemainingParticipants = Participants;
            CurrentPhase = 1;
            this.mutex = new Semaphore(1, 1);
            this.semaphore = new Semaphore(0, Participants - 1);
            this.toDo = null;
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
                try
                {
                    toDo?.Invoke(this);
                }
                catch (System.Exception e)
                {
                    CurrentPhase++;
                    semaphore.Release(Participants - 1);
                    RemainingParticipants = Participants;
                    this.mutex.Release();

                    throw e;
                }

                semaphore.Release(Participants - 1);
                CurrentPhase++;
                RemainingParticipants = Participants;
                this.mutex.Release();
            }
        }
    }
}