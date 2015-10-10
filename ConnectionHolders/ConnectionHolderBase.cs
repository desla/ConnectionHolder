namespace ConnectionHolders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Timers;
    using Timer = System.Timers.Timer;

    /// <summary>
    /// Базовый класс для всех держателей подключений.
    /// </summary>
    internal abstract class ConnectionHolderBase<TConnection> 
        : IConnectionHolder<TConnection>
    {
        private string holderName;
        private TimeSpan reconnectionInterval;
        private TimeSpan checkConnectionInterval;
        
        private bool isWorking;
        private object stateLock = new object();

        private Mutex connectionLock;

        private BackgroundWorker backgroundWorker;
        private Timer reconnectionTimer;

        private ConnectionHolders.ConnectionState lastState;

        protected ConnectionHolderBase() {
            holderName = "undefined";
            lastState = ConnectionState.DISCONNECTED;
            reconnectionInterval = TimeSpan.FromSeconds(5);
            checkConnectionInterval = TimeSpan.FromSeconds(5);

            connectionLock = new Mutex();            

            reconnectionTimer = new Timer(0);
            reconnectionTimer.Elapsed += ReconnectionTimerTick;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += DoCheckAndReconnection;            
        }        

        public string GetHolderName()
        {
            return holderName;
        }

        public void SetHolderName(string aHolderName)
        {
            if (string.IsNullOrEmpty(aHolderName)) {
                throw new ArgumentNullException(aHolderName);
            }

            holderName = aHolderName;
        }

        public TimeSpan GetReconnectionInterval()
        {
            return reconnectionInterval;
        }

        public void SetReconnectionInterval(TimeSpan aReconectionInterval)
        {
            reconnectionInterval = aReconectionInterval;
        }

        public TimeSpan GetCheckConnectionInterval()
        {
            return checkConnectionInterval;
        }

        public void SetCheckConnectionInterval(TimeSpan aCheckConnectionInterval)
        {
            checkConnectionInterval = aCheckConnectionInterval;
        }

        public void Start()
        {
            lock (stateLock) {                
                isWorking = true;
                reconnectionTimer.Start();
            }
        }

        public void Stop()
        {
            lock (stateLock) {
                isWorking = false;
                reconnectionTimer.Stop();
            }
        }

        public bool IsWorking()
        {
            return isWorking;
        }

        public void Dispose()
        {
            Stop();
            FreeConnection();
        }

        public TConnection WaitConnection() 
        {
            connectionLock.WaitOne();
            return GetConnection();
        }

        public void ReleaseConnection()
        {
            connectionLock.ReleaseMutex();
        }

        /// <summary>
        /// Возвращает подключение.
        /// </summary>
        /// <returns>Подключение.</returns>
        protected abstract TConnection GetConnection();

        /// <summary>
        /// Освобождает ресурсы подключения.
        /// </summary>
        protected abstract void FreeConnection();

        /// <summary>
        /// Делает попытку подключения.
        /// </summary>
        /// <returns>True - если подключение выполнено, false - иначе.</returns>
        protected abstract bool TryConnect();

        /// <summary>
        /// Тестирует подключение.
        /// </summary>
        /// <returns>True - если подключение активно, false - иначе.</returns>
        protected abstract bool TryCheckConnection();

        /// <summary>
        /// Таймерный метод. Должен запускать проверку подключения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReconnectionTimerTick(object sender, ElapsedEventArgs e)
        {
            if (!backgroundWorker.IsBusy) {
                backgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Проверка подключения. Выполняется в отдельном потоке.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoCheckAndReconnection(object sender, DoWorkEventArgs e)
        {
            if (TryCheckConnection()) {
                if (lastState == ConnectionState.DISCONNECTED) {
                    reconnectionTimer.Interval = checkConnectionInterval.TotalMilliseconds;
                    lastState = ConnectionState.CONNECTED;
                }                
            }
            else {
                if (lastState == ConnectionState.CONNECTED) {
                    lastState = ConnectionState.DISCONNECTED;
                    reconnectionTimer.Interval = reconnectionInterval.TotalMilliseconds;                
                }
                TryConnect();
            }
        }        
    }
}
