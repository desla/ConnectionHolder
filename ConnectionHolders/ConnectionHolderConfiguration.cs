namespace ConnectionHolders
{
    /// <summary>
    /// Конфигурация держателя подключения.
    /// </summary>
    public class ConnectionHolderConfiguration
    {
        /// <summary>
        /// Адрес хоста.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Имя клиента.
        /// </summary>
        public string Source { get; set; }
    }
}
