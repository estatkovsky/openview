using Fleck;
using LitJson;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebRtc.NET;

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
        }

        public void Start()
        {
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
            _logger.Info($"Started web socket server {_server.Location}");
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
            _logger.Info("Stopped web socket server");

            _connectedUsers.Clear();
            _streams.Clear();
        }

        private void OnConnected(IWebSocketConnection socket)
        {
            _logger.Info($"Web socket connection started ({GetClient(socket)})");
            if (_connectedUsers.Count <= AgentConfiguration.WebSocketConcurrentConnectionsLimit)
            {
                _connectedUsers.AddOrUpdate(socket.ConnectionInfo.Id, socket, (key, val) => socket);
            }
            else
            {
                _logger.Warn($"OverLimit, rejected connection ({GetClient(socket)})");
                socket.Close();
            }
        }

        private void OnMessageReceived(IWebSocketConnection socket, string message)
        {
            _logger.Debug($"Web socket data received ({GetClient(socket)})");

            if (string.IsNullOrEmpty(message) || !message.Contains("command"))
            {
                // unexpected message format
                return;
            }

            if (!_connectedUsers.ContainsKey(socket.ConnectionInfo.Id))
            {
                // unknown client
                return;
            }

            JsonData msgJson = null;
            try
            {
                msgJson = JsonMapper.ToObject(message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Web socket message parse error ({GetClient(socket)})", ex);
                return;
            }

            string command = msgJson["command"].ToString();

            if (!string.IsNullOrEmpty(command))
            {
                ProcessCommand(command, msgJson, socket);
            }
        }

        private void OnDisconnected(IWebSocketConnection socket)
        {
            _logger.Info($"Web socked connection closed ({GetClient(socket)})");

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
            _logger.Error($"Web socket connection error ({GetClient(socket)})", e);
            OnDisconnected(socket);
            socket.Close();
        }

        private void ProcessCommand(string command, JsonData msgJson, IWebSocketConnection socket)
        {
            switch (command)
            {
                case WebRtcCommand.Offer:
                    ProcessOffer(msgJson, socket);
                    break;

                case WebRtcCommand.OnIceCandidate:
                    ProcessIceCandidate(msgJson, socket);
                    break;
            }
        }

        private void ProcessOffer(JsonData msgJson, IWebSocketConnection socket)
        {
            if (_connectedUsers.Count > AgentConfiguration.WebSocketConcurrentConnectionsLimit)
            {
                return;
            }

            if (_streams.ContainsKey(socket.ConnectionInfo.Id))
            {
                return;
            }

            var session = _streams[socket.ConnectionInfo.Id] = new WebRtcSession();

            using (var waitEvent = new ManualResetEvent(false))
            {
                var sessionParams = new WebRtcSessionParams();
                sessionParams.Session = session;
                sessionParams.WaitEvent = waitEvent;
                sessionParams.Socket = socket;
                sessionParams.EnableAudio = false;
                sessionParams.ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
                sessionParams.ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
                sessionParams.CaptureFps = 5;
                sessionParams.BarCodeScreen = false;
                var streamTask = Task.Factory.StartNew(InitializeAndRunStream, sessionParams,
                    session.Cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                if (waitEvent.WaitOne(9999))
                {
                    session.WebRtc.OnIceCandidate += delegate (string sdp_mid, int sdp_mline_index, string sdp)
                    {
                        if (socket.IsAvailable)
                        {
                            JsonData j = new JsonData();
                            j["command"] = "OnIceCandidate";
                            j["sdp_mid"] = sdp_mid;
                            j["sdp_mline_index"] = sdp_mline_index;
                            j["sdp"] = sdp;
                            socket.Send(j.ToJson());
                        }
                    };

                    session.WebRtc.OnSuccessAnswer += delegate (string sdp)
                    {
                        if (socket.IsAvailable)
                        {
                            JsonData j = new JsonData();
                            j["command"] = "OnSuccessAnswer";
                            j["sdp"] = sdp;
                            socket.Send(j.ToJson());
                        }
                    };

                    session.WebRtc.OnFailure += delegate (string error)
                    {
                        _logger.Error($"WebRTC connection failure {error}");
                    };

                    session.WebRtc.OnError += delegate
                    {
                        _logger.Error("WebRTC connection encountered an error");
                    };

                    session.WebRtc.OnDataMessage += delegate (string dmsg)
                    {
                        _logger.Debug($"WebRTC data received {dmsg}");
                    };

                    /*
                    unsafe
                    {
                        session.WebRtc.OnRenderRemote += delegate (byte* frame_buffer, uint w, uint h)
                        {
                            OnRenderRemote(frame_buffer, w, h);
                        };

                        session.WebRtc.OnRenderLocal += delegate (byte* frame_buffer, uint w, uint h)
                        {
                            OnRenderLocal(frame_buffer, w, h);
                        };
                    }
                    */

                    var desc = msgJson["desc"];
                    var s = desc["sdp"].ToString();

                    session.WebRtc.OnOfferRequest(s);
                }
            }
        }

        private void ProcessIceCandidate(JsonData msgJson, IWebSocketConnection socket)
        {
            if (!_streams.ContainsKey(socket.ConnectionInfo.Id))
            {
                return;
            }

            var candidateInfo = msgJson["candidate"];

            var sdpMLineIndex = (int)candidateInfo["sdpMLineIndex"];
            var sdpMid = candidateInfo["sdpMid"].ToString();
            var candidate = candidateInfo["candidate"].ToString();

            var session = _streams[socket.ConnectionInfo.Id];
            session.WebRtc.AddIceCandidate(sdpMid, sdpMLineIndex, candidate);
        }

        private void InitializeAndRunStream(object state)
        {
            var sessionParams = (WebRtcSessionParams)state;
            var session = sessionParams.Session;

            ManagedConductor.InitializeSSL();

            using (session.WebRtc)
            {
                session.WebRtc.AddServerConfig("stun:stun.l.google.com:19302", string.Empty, string.Empty);
                session.WebRtc.AddServerConfig("stun:stun.anyfirewall.com:3478", string.Empty, string.Empty);
                session.WebRtc.AddServerConfig("stun:stun.stunprotocol.org:3478", string.Empty, string.Empty);
                //session.WebRtc.AddServerConfig("turn:192.168.0.100:3478", "test", "test");

                session.WebRtc.SetAudio(sessionParams.EnableAudio);

                session.WebRtc.SetVideoCapturer(sessionParams.ScreenWidth,
                                                sessionParams.ScreenHeight,
                                                sessionParams.CaptureFps,
                                                sessionParams.BarCodeScreen);

                var ok = session.WebRtc.InitializePeerConnection();
                if (ok)
                {
                    _logger.Info($"WebRTC peer connection initialized successfully ({GetClient(sessionParams.Socket)})");
                    sessionParams.WaitEvent.Set();

                    // javascript side makes the offer in this demo
                    //session.WebRtc.CreateDataChannel("msgDataChannel");

                    while (!session.Cancellation.Token.IsCancellationRequested && session.WebRtc.ProcessMessages(1000))
                    {
                    }
                    session.WebRtc.ProcessMessages(1000);
                }
                else
                {
                    _logger.Warn($"WebRTC peer connection failed ({GetClient(sessionParams.Socket)})");
                    sessionParams.Socket.Close();
                }
            }
        }

        private static string GetClient(IWebSocketConnection socket)
        {
            return socket.ConnectionInfo != null ? socket.ConnectionInfo.ClientIpAddress : "Unknown client";
        }
    }
}
