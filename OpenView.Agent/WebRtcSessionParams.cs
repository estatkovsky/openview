using Fleck;
using System.Threading;

namespace OpenView.Agent
{
    public class WebRtcSessionParams
    {
        public WebRtcSession Session { get; set; }

        public ManualResetEvent WaitEvent { get; set; }

        public IWebSocketConnection Socket { get; set; }

        public bool EnableAudio { get; set; }

        public int ScreenWidth { get; set; }

        public int ScreenHeight { get; set; }

        public int CaptureFps { get; set; }

        public bool BarCodeScreen { get; set; }
    }
}
