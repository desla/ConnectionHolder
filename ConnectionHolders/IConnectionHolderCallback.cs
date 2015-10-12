namespace ConnectionHolders
{
    /// <summary>
    /// Интерфейс обратной связи ConnectionHolder'а.
    /// </summary>
    public interface IConnectionHolderCallback<TConnection>
    {
        /// <summary>
        /// Возникает при подключении.
        /// </summary>
        /// <param name="aConnectionHolder">Отправитель.</param>
        void OnConnected(IConnectionHolder<TConnection> aConnectionHolder);

        /// <summary>
        /// Возникает при отключении.
        /// </summary>
        /// <param name="aConnectionHolder">Отправитель.</param>
        void OnDisconnected(IConnectionHolder<TConnection> aConnectionHolder);
    }
}
