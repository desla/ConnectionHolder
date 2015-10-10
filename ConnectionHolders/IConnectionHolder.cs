namespace ConnectionHolders
{
    using System;
    
    /// <summary>
    /// Интерфейс держателя соединения.
    /// </summary>
    public interface IConnectionHolder<TConnection> : IDisposable
    {
        /// <summary>
        /// Возвращает имя держателя.
        /// </summary>
        /// <returns>Имя держателя.</returns>
        string GetHolderName();

        /// <summary>
        /// Устанавливает имя держателя.
        /// </summary>
        /// <param name="aHolderName">Имя держателя.</param>
        void SetHolderName(string aHolderName);

        /// <summary>
        /// Возвращает интервал переподключения.
        /// </summary>
        /// <returns>Интервал переподключения.</returns>
        TimeSpan GetReconnectionInterval();

        /// <summary>
        /// Устанавливает интервал переподключения.
        /// </summary>
        /// <param name="aReconectionInterval">Интервал переподключения.</param>
        void SetReconnectionInterval(TimeSpan aReconectionInterval);

        /// <summary>
        /// Возаращает интервал проверки соединения.
        /// </summary>
        /// <returns>Интервал проверки соединения.</returns>
        TimeSpan GetCheckConnectionInterval();

        /// <summary>
        /// Устанавливает интервал проверки соединения.
        /// </summary>
        /// <param name="aCheckConnectionInterval">Интервал проверки соединения.</param>
        void SetCheckConnectionInterval(TimeSpan aCheckConnectionInterval);


        /// <summary>
        /// Запускает в работу держатель подключения.
        /// </summary>
        void Start();

        /// <summary>
        /// Останавливает держатель соединения.
        /// </summary>
        void Stop();

        /// <summary>
        /// Возвращает текущее состояние держателя подключения.
        /// </summary>
        /// <returns></returns>
        bool IsWorking();

        /// <summary>
        /// Возвращает подключение, блокируя его.
        /// </summary>
        /// <returns>Подключение.</returns>
        TConnection WaitConnection();

        /// <summary>
        /// Освобождает подключение.
        /// </summary>
        void ReleaseConnection();
    }
}
