namespace ConnectionHolders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using System.Timers;
    using log4net;
    using Timer = System.Timers.Timer;

    /// <summary>
    /// Базовый класс для всех держателей подключений.
    /// </summary>
    public abstract class ConnectionHolderBase<TConnection> 
        : IConnectionHolder<TConnection>
    {
        private static readonly ILog logger = LogManager.GetLogger("ConnectionHolderBase");

        private string holderName;
        private TimeSpan reconnectionInterval;
        private TimeSpan checkConnectionInterval;
        
        private bool isWorking;
        private object stateLock = new object();

        private List<IConnectionHolderCallback<TConnection>> listeners = 
            new List<IConnectionHolderCallback<TConnection>>();

        private Mutex connectionLock;

        private BackgroundWorker backgroundWorker;
        private Timer reconnectionTimer;

        private ConnectionState lastState;

        protected ConnectionHolderBase() {
            holderName = "undefined";
            lastState = ConnectionState.DISCONNECTED;
            reconnectionInterval = TimeSpan.FromSeconds(5);
            checkConnectionInterval = TimeSpan.FromSeconds(5);

            connectionLock = new Mutex();            

            reconnectionTimer = new Timer(reconnectionInterval.TotalMilliseconds);
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
                logger.Info(GetHolderName() + " - запуск держателя подключения.");
                isWorking = true;
                reconnectionTimer.Start();
            }
        }

        public void Stop()
        {
            lock (stateLock) {
                logger.Info(GetHolderName() + " - остановка держателя подключения.");
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
            TryFreeConnection();
            lastState = ConnectionState.DISCONNECTED;
            AlertListeners(lastState);
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

        public void Subscribe(IConnectionHolderCallback<TConnection> aCallback)
        {
            if (aCallback == null) {
                throw new ArgumentNullException("aCallback");
            }

            lock (listeners) {
                listeners.Add(aCallback);
            }
        }

        public void Unsubscribe(IConnectionHolderCallback<TConnection> aCallback)
        {
            if (aCallback == null) {
                throw new ArgumentNullException("aCallback");
            }

            lock (listeners) {
                if (!listeners.Contains(aCallback)) {
                    throw new ArgumentException("Callback не увляется подписчиком.");
                }

                listeners.Remove(aCallback);
            }
        }

        /// <summary>
        /// Делает попытку подключения.
        /// </summary>
        /// <returns>True - если подключение выполнено, false - иначе.</returns>
        public abstract bool TryConnect();

        /// <summary>
        /// Возвращает подключение.
        /// </summary>
        /// <returns>Подключение.</returns>
        protected abstract TConnection GetConnection();

        /// <summary>
        /// Освобождает ресурсы подключения.
        /// </summary>
        protected abstract void TryFreeConnection();        

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
                logger.Info(GetHolderName() + " - проверка подключения: успешно.");
                if (lastState == ConnectionState.DISCONNECTED) {                    
                    reconnectionTimer.Interval = checkConnectionInterval.TotalMilliseconds;
                    lastState = ConnectionState.CONNECTED;
                    AlertListeners(lastState);
                }                
            }
            else {
                logger.Info(GetHolderName() + " - проверка подключения: НЕ успешно.");
                if (lastState == ConnectionState.CONNECTED) {                    
                    reconnectionTimer.Interval = reconnectionInterval.TotalMilliseconds;
                    lastState = ConnectionState.DISCONNECTED;
                    AlertListeners(lastState);
                }
                TryConnect();
            }
        }

        /// <summary>
        /// Оповещает всех подписанных слушателей.
        /// </summary>
        /// <param name="aConnectionState">Состояние.</param>
        private void AlertListeners(ConnectionState aConnectionState)
        {
            lock (listeners) {
                foreach (var listener in listeners) {
                    try {
                        if (aConnectionState == ConnectionState.CONNECTED) {
                            listener.OnConnected(this);
                        }
                        else {
                            listener.OnDisconnected(this);
                        }
                    }
                    catch (Exception ex) {
                        logger.Error("Ошибка при оповещении слушателя: " + ex.Message);
                    }
                }
            }
        }
    }
}
