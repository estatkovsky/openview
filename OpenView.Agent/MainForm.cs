using System.Windows.Forms;

namespace OpenView.Agent
{
    public partial class MainForm : Form
    {
        private readonly WebRtcServer _server;

        public MainForm()
        {
            InitializeComponent();

            _server = new WebRtcServer();
            _server.Start();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _server.Dispose();
        }
    }
}
