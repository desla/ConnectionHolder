namespace ConnectionHolders
{
    using System;
    using log4net;
    using OPCAutomation;

    /// <summary>
    /// Держатель ОРС-соединения.
    /// </summary>
    public class OpcConnectionHolder : ConnectionHolderBase<OPCServer>
    {
        private static readonly ILog logger = LogManager.GetLogger("OpcConnectionHolder");

        private OPCServer server;
        private string serverName;
        private string serverHost;

        public OpcConnectionHolder(string aServerName, string aServerHost = null)
        {
            if (string.IsNullOrEmpty(aServerName)) {
                throw new ArgumentNullException("aServerName");
            }

            serverName = aServerName;
            serverHost = aServerHost;
            server = new OPCServer();            
        }

        public override bool TryConnect()
        {
            try {
                server.Connect(serverName, serverHost);
                return true;
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время подключения: " + ex.Message);
                return false;
            }
        }

        protected override OPCServer GetConnection()
        {
            return server;
        }

        protected override void TryFreeConnection()
        {
            try {
                server.Disconnect();
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время отключения: " + ex.Message);
            }
        }        

        protected override bool TryCheckConnection()
        {
            try {
                return server.ServerState == (int) OPCServerState.OPCRunning;
            }
            catch {
                return false;
            }
        }
    }
}
