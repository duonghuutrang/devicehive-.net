using System;
using Fleck;
using log4net;

namespace DeviceHive.WebSockets.Core.Network.Fleck
{
    public class FleckWebSocketConnection : WebSocketConnectionBase
    {
        private readonly ILog _logger;
        private readonly IWebSocketConnection _fleckConnection;

        #region Constructor

        public FleckWebSocketConnection(IWebSocketConnection fleckConnection)
        {
            _logger = LogManager.GetLogger(GetType());
            _fleckConnection = fleckConnection;
        }

        #endregion

        #region WebSocketConnectionBase Members

        public override Guid Identity
        {
            get { return _fleckConnection.ConnectionInfo.Id; }
        }

        public override string Host
        {
            get { return _fleckConnection.ConnectionInfo.Host; }
        }

        public override string Path
        {
            get { return _fleckConnection.ConnectionInfo.Path; }
        }

        public override void Send(string message)
        {
            _logger.Debug("Sending message for connection: " + Identity);
            _fleckConnection.Send(message);
        }

        public override void Close()
        {
            _logger.Debug("Closing connection: " + Identity);
            _fleckConnection.Close();
        }
        #endregion
    }
}