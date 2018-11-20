using Fleck;
using log4net;
using System;
using System.Collections.Concurrent;

namespace OpenView.Agent
{
    public class WebRtcServer : IDisposable
    {
        private const string _webSocketUrl = "ws://0.0.0.0:9000";
        private readonly ILog _logger = LogManager.GetLogger(typeof(WebRtcServer));
        private readonly ConcurrentDictionary<Guid, IWebSocketConnection> _connectedUsers;
        private readonly ConcurrentDictionary<Guid, WebRtcSession> _streams;

        private readonly WebSocketServer _server;

        public WebRtcServer()
        {
            _connectedUsers = new ConcurrentDictionary<Guid, IWebSocketConnection>();
            _streams = new ConcurrentDictionary<Guid, WebRtcSession>();

            _server = new WebSocketServer(_webSocketUrl);

            _server.Start(socket => {
                socket.OnOpen = () => {
                    OnConnected(socket);
                };
                socket.OnMessage = message =>
                {
                    OnMessageReceived(socket, message);
                };
                socket.OnClose = () =>
                {
                    OnDisconnected(socket);
                };
                socket.OnError = (e) =>
                {
                    OnError(socket, e);
                };
            });
        }

        public void Dispose()
        {
            foreach (var stream in _streams.Values)
            {
                if (!stream.Cancellation.IsCancellationRequested)
                {
                    stream.Cancellation.Cancel();
                }
            }

            foreach (var connection in _connectedUsers.Values)
            {
                connection.Close();
            }

            _server.Dispose();

            _connectedUsers.Clear();
            _streams.Clear();
        }

        private void OnConnected(IWebSocketConnection socket)
        {
            _logger.Info($"Web socket connection started from remote host {socket.ConnectionInfo.ClientIpAddress}");
            if (_connectedUsers.Count <= AgentConfiguration.WebSocketConcurrentConnectionsLimit)
            {
                _connectedUsers.AddOrUpdate(socket.ConnectionInfo.Id, socket, (key, val) => socket);
            }
            else
            {
                _logger.Warn($"OverLimit, rejected remote connection from {socket.ConnectionInfo.ClientIpAddress}");
                socket.Close();
            }
        }

        private void OnMessageReceived(IWebSocketConnection socket, string message)
        {
            _logger.Debug($"Received data from remote host {socket.ConnectionInfo.ClientIpAddress}");
        }

        private void OnDisconnected(IWebSocketConnection socket)
        {
            _logger.Info($"Web socked connection closed from remote host {socket.ConnectionInfo.ClientIpAddress}");

            IWebSocketConnection ctx;
            _connectedUsers.TryRemove(socket.ConnectionInfo.Id, out ctx);

            WebRtcSession session;
            if (_streams.TryRemove(socket.ConnectionInfo.Id, out session))
            {
                session.Cancellation.Cancel();
            }
        }

        private void OnError(IWebSocketConnection socket, Exception e)
        {
            _logger.Error("Web socket connection error", e);
            OnDisconnected(socket);
            socket.Close();
        }
    }
}
