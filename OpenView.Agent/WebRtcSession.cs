using System.Threading;
using WebRtc.NET;

namespace OpenView.Agent
{
    public class WebRtcSession
    {
        public WebRtcSession()
        {
            WebRtc = new ManagedConductor();
            Cancellation = new CancellationTokenSource();
        }

        public ManagedConductor WebRtc { get; private set; }

        public CancellationTokenSource Cancellation { get; private set; }
    }
}
