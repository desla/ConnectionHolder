namespace ConnectionHolders
{
    using System;
    using log4net;
    using Oracle.ManagedDataAccess.Client;

    /// <summary>
    /// Держатель подключения для БД Oracle.
    /// </summary>
    public class OracleConnectionHolder : ConnectionHolderBase<OracleConnection>
    {
        private static readonly ILog logger = LogManager.GetLogger("OracleConnectionHolder");

        private OracleConnection connection;

        public OracleConnectionHolder(string aServerHost, string aUserName, string aPassword)
        {
            if (string.IsNullOrEmpty("aServerHost")) {
                throw new ArgumentNullException("aServerHost");
            }

            if (string.IsNullOrEmpty("aUserName")) {
                throw new ArgumentNullException("aUserName");
            }

            if (string.IsNullOrEmpty("aPassword")) {
                throw new ArgumentNullException("aPassword");
            }

            connection = new OracleConnection();
            connection.ConnectionString = string.Format(
                "Data Source={0};User Id={1};Password={2};", 
                aServerHost, aUserName, aPassword);            
        }

        public OracleConnectionHolder(string aConnectionString)
        {
            if (string.IsNullOrEmpty(aConnectionString)) {
                throw new ArgumentNullException("aConnectionString");
            }

            connection = new OracleConnection();
            connection.ConnectionString = aConnectionString;
        }

        public OracleConnectionHolder(ConnectionHolderConfiguration aConfiguration)
            : this(aConfiguration.Host, aConfiguration.User, aConfiguration.Password)
        {
        }

        public override bool TryConnect()
        {
            try {
                if (connection.State == System.Data.ConnectionState.Open) {
                    connection.Close();
                }

                connection.Open();
                return true;
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время подключения: " + ex.Message);
                return false;
            }
        }

        protected override OracleConnection GetConnection()
        {
            return connection;
        }

        protected override void TryFreeConnection()
        {
            try {                
                connection.Close();
                connection.Dispose();
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время освобождения подключения: " + ex.Message);
            }
        }        

        protected override bool TryCheckConnection()
        {
            try {
                using (var command = new OracleCommand("select * from v$version", connection)) {
                    using (command.ExecuteReader()) {                        
                        return true;
                    }
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время проверки подключения: " + ex.Message);
                return false;
            }
        }
    }
}
