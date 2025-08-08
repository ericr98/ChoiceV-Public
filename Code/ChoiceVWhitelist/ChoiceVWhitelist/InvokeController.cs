using ChoiceVServer.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChoiceVServer {
    class InvokeController : ChoiceVScript {
        private static BackgroundWorker workerThread = new BackgroundWorker();
        private static EventWaitHandle workerSignal = new AutoResetEvent(false);
        private static ConcurrentDictionary<long, BasicTimedInvoke> AllTimedInvokes = new ConcurrentDictionary<long, BasicTimedInvoke>();
        private static DateTime nextTimedInvoke = DateTime.MaxValue;
        private static object LockObject = new object();

        public static DateTime NextTimedInvoke {
            get { return nextTimedInvoke; }
        }

        static InvokeController() {
            workerThread.WorkerSupportsCancellation = true;
            workerThread.DoWork += WorkerThread_DoWork;
        }

        public InvokeController() {
            //TODO CHANGE!
            EventController.MainShutdownDelegate += onMainShutdown;
            EventController.MainReadyDelegate += onStart;
        }

        private void onMainShutdown() {
            Logger.logInfo("TimedInvokemanager stopping");
            workerThread.CancelAsync();
            workerSignal.Set();
        }


        public static void onStart() {
            Logger.logInfo("InvokeController starting");
            workerThread.RunWorkerAsync();
        }


        private static void WorkerThread_DoWork(object sender, DoWorkEventArgs e) {
            while(!workerThread.CancellationPending) {
                try {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    TimeSpan nextTimedInvoke = UpdateTimedInvokes();
                    sw.Stop();
                    workerSignal.WaitOne(nextTimedInvoke);
                } catch(Exception ex) {
                    Logger.logError($"Exception in Invoke-Thread: {ex.Message}");
                    e.Cancel = true;
                    return;
                }
            }
            Logger.logTrace("InvokeController workerthread exit");
        }


        public static void StopCustomTimedInvokes() {
            Logger.logInfo("Stop non-System timed Invokes");
            foreach(var item in AllTimedInvokes.Values) {
                if(!item.IsSystemCallback)
                    item.IsEnabled = false;
            }

        }

        private static void RegisterTimedInvoke(BasicTimedInvoke timedInvoke) {

            if(AllTimedInvokes.Count > 50000)
                throw new IndexOutOfRangeException("Too many timed Invokes");
            AllTimedInvokes.Add(timedInvoke.InvokeId, timedInvoke);

            workerSignal.Set();
        }

        private static readonly TimeSpan toleranceTimespan = TimeSpan.FromMilliseconds(150);
        public static TimeSpan UpdateTimedInvokes() {
            lock(LockObject) {
                DateTime now = DateTime.Now;

                AllTimedInvokes.Values.Where(item => (item.IsEnabled && (((item.When <= now) || (item.When - now < toleranceTimespan))))).AsParallel().ForEach(c => c.Execute());

                Housekeeping();

                nextTimedInvoke = DateTime.MaxValue;

                foreach(var item in AllTimedInvokes.Values) {
                    if(item.IsEnabled && (item.When < nextTimedInvoke))
                        nextTimedInvoke = item.When;
                }
                if(nextTimedInvoke == DateTime.MaxValue) {
                    return TimeSpan.FromMilliseconds(-1);
                }
                now = DateTime.Now;
                return TimeSpan.FromMilliseconds(Math.Max(0.0, (NextTimedInvoke - now).TotalMilliseconds));
            }
        }

        public static void Housekeeping() {
            lock(LockObject) {
                List<BasicTimedInvoke> newList = new List<BasicTimedInvoke>();


                foreach(var item in AllTimedInvokes.Values) {
                    if(item.Done)
                        AllTimedInvokes.Remove(item.InvokeId);
                }

            }
        }

        public static IInvoke AddTimedInvoke(string name, Action<IInvoke> callback, TimeSpan delay, bool persistant) {
            return AddTimedInvoke(name, callback, delay, persistant, false);
        }

        public static IInvoke AddTimedInvoke(string name, Action<IInvoke> callback, TimeSpan delay, bool persistant, bool isSystem = false) {
            BasicTimedInvoke c = new BasicTimedInvoke();
            c.When = DateTime.Now + delay;
            c.Name = name;
            c.Callback = callback;
            c.Persistent = persistant;
            c.Delay = delay;
            c.IsSystemCallback = isSystem;
            Logger.logTrace($"New TimedInvoke {c.InvokeId} Name {c.Name} Delay {delay} Persitant {persistant} System {isSystem}");
            RegisterTimedInvoke(c);
            return c;
        }

        public static void RemoveTimedInvoke(IInvoke timedInvoke) {
            AllTimedInvokes.Remove(timedInvoke.InvokeId);
            workerSignal.Set();
        }
        public static void RemoveTimedInvoke(long timedInvokeID) {
            AllTimedInvokes.Remove(timedInvokeID);
            workerSignal.Set();
        }

    }

    public interface IInvoke {
        long InvokeId { get; }
        DateTime When { get; }
        TimeSpan Delay { get; set; }
        bool Done { get; set; }
        bool Persistent { get; }

        void EndSchedule();
        void Reschdedule();
    }

    class BasicTimedInvoke : IInvoke {
        private static long lastTimedInvokeID = 0;

        public string Name { get; set; }
        public DateTime When { get; set; }
        public TimeSpan Delay { get; set; }
        public bool Done { get; set; }
        public bool Persistent { get; set; }
        public Action<IInvoke> Callback { get; set; }
        public long InvokeId { get; }
        public bool IsSystemCallback { get; set; }
        public bool IsEnabled { get; internal set; } = true;

        private bool IsRescheduled = false;
        private bool IsRunning = false;
        public BasicTimedInvoke() {
            InvokeId = Interlocked.Add(ref lastTimedInvokeID, 1);
        }

        public void EndSchedule() {
            Persistent = false;
            if(!IsRunning)
                InvokeController.RemoveTimedInvoke(this);
        }

        public void Reschdedule() {
            IsRescheduled = true;
        }

        public virtual void Execute() {
            try {
                IsRunning = true;
                if(Callback != null) {
                    Logger.logTrace($"TimedInvoke {InvokeId} Name {Name} Scheduled {When} Offset {DateTime.Now - When}");
                    Callback.Invoke(this);
                    if(IsRescheduled) {
                        IsRescheduled = false;
                        Done = false;
                        When = DateTime.Now + Delay;
                        return;
                    }

                    if(!Persistent)
                        Done = true;
                    else
                        When = DateTime.Now + Delay;
                }
            } catch(Exception ex) {
                Logger.logError($"logError in InvokeCntroller: {ex.ToString()}");
                Done = true;
            } finally { IsRunning = false; }
        }

        public override string ToString() {
            return $"TimedInvoke {InvokeId:X4} Name {Name} Interval {Delay} Persistant {Persistent} Scheduled {When}";
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if(!disposedValue) {
                if(disposing) {
                    EndSchedule();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
