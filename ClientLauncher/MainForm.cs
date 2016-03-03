using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Lidgren.Network;
using TCoSServer.Network;
using Debug = System.Diagnostics.Debug;

namespace ClientLauncher
{
    public partial class MainForm : Form
    {
        const string EditListTooltip = "Edit Server list";
        const string ClientConfigError = "Server adress could not be set! Make sure the launcher is placed in the right folder";
        const string CouldntOpenConfigError = "couldn't open clientConfig, maybe it's locked by another program";
        const string CouldntSaveConfigError = "couldn't write to client config file, maybe it's readonly";
        const string CouldntFindMainExeError = "Couldn't start the game, make sure its main executable filename is: " + MainExename;
        const string LaunchTooltip = "Launch the game for the selected Server";
        const string ClientSetAdressIdentifier = @"client/net/login_addr = ";
        const string PingTooltip = "Double click to test if the server is open for connections";
        const string MainExename = "Sb_client.exe";
        LauncherConfig _launcherConfig;
        bool _mouseFormDragCaptured;
        Point _mouseMoveStart;

        LauncherConfig.ServerEntry selectedEntry;

        NetClient proxyConnector;

        public MainForm()
        {
            InitializeComponent();
            var proxyConfig = new NetPeerConfiguration("TCoSReborn");
            proxyConfig.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            proxyConnector = new NetClient(proxyConfig);
            proxyConnector.RegisterReceivedCallback(OnProxyServerMessage, SynchronizationContext.Current);
            proxyConnector.Start();
        }

        void ShowAccountPanel()
        {
            lblWaitText.Visible = false;
            pnlNews.Visible = false;
            pnlAccount.Visible = true;
            pnlAccount.BringToFront();
            btnAccount.Visible = true;
            btnAccount.BringToFront();
            btnNews.Visible = true;
            btnNews.BringToFront();
        }

        void ShowNewsPanel()
        {
            lblWaitText.Visible = false;
            pnlAccount.Visible = false;
            pnlNews.Visible = true;
            pnlNews.BringToFront();
            btnAccount.Visible = true;
            btnAccount.BringToFront();
            btnNews.Visible = true;
            btnNews.BringToFront();
        }

        void HideAllPanels()
        {
            lblWaitText.Visible = true;
            pnlAccount.Visible = false;
            pnlNews.Visible = false;
            btnNews.Visible = false;
            btnAccount.Visible = false;
        }

        void ShowNewsOnly()
        {
            lblWaitText.Visible = false;
            pnlAccount.Visible = false;
            pnlNews.Visible = true;
            pnlNews.BringToFront();
            btnAccount.Visible = false;
            btnNews.Visible = false;
        }

        void OnProxyServerMessage(object o)
        {
            var peer = o as NetPeer;
            var msg = peer.ReadMessage();
            if (msg.MessageType == NetIncomingMessageType.UnconnectedData && msg.LengthBytes > 1)
            {
                var header = (CommunicationHeader) msg.ReadByte();
                switch (header)
                {
                    case CommunicationHeader.L2CL_UPDATE_INFO:
                        var registrationEnabled = msg.ReadBoolean();
                        var universesOnline = msg.ReadBoolean();
                        var infoText = msg.ReadString().Replace(@"\\n", Environment.NewLine);
                        rtNews.Text = infoText;
                        if (!registrationEnabled)
                        {
                            ShowNewsOnly();
                        }
                        else
                        {
                            ShowNewsPanel();
                        }
                        _launcherConfig.server.State = universesOnline ? LauncherConfig.ServerEntry.OnlineState.Online : LauncherConfig.ServerEntry.OnlineState.Offline;
                        if (_launcherConfig.server.State == LauncherConfig.ServerEntry.OnlineState.Offline)
                        {
                            btnRun.Text = btnRun.Text + " (offline)";
                        }
                        break;
                    case CommunicationHeader.L2CL_REGISTER_ACCOUNT_ACK:
                        var result = msg.ReadInt32();
                        if (result == 0)
                        {
                            MessageBox.Show("Account registered successfully");
                            ShowNewsPanel();
                            break;
                        }
                        if (result == 1)
                        {
                            MessageBox.Show("Account registration failed. Name or Email already taken");
                            break;
                        }
                        if (result == 3)
                        {
                            MessageBox.Show("Registration is currently closed");
                            ShowNewsPanel();
                            break;
                        }
                        MessageBox.Show("Account registration failed. Unknown Error");
                        break;
                }
            }
        }

        void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            StartDragForm(e.Location);
        }

        void StartDragForm(Point location)
        {
            _mouseFormDragCaptured = true;
            _mouseMoveStart = location;
        }

        void UpdateDragForm(Point location)
        {
            if (!_mouseFormDragCaptured) return;
            Location = new Point(Location.X + (location.X - _mouseMoveStart.X), Location.Y + (location.Y - _mouseMoveStart.Y));
        }

        void StopDragForm()
        {
            _mouseFormDragCaptured = false;
        }

        void SetTooltips()
        {
            toolTip1.SetToolTip(btnRun, LaunchTooltip);
        }

        void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateDragForm(e.Location);
        }

        void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            StopDragForm();
        }

        void label1_MouseDown(object sender, MouseEventArgs e)
        {
            StartDragForm(e.Location);
        }

        void label1_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateDragForm(e.Location);
        }

        void label1_MouseUp(object sender, MouseEventArgs e)
        {
            StopDragForm();
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            HideAllPanels();
            var cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs.Contains("createconfig", StringComparer.OrdinalIgnoreCase))
            {
                var lc = new LauncherConfig {server = new LauncherConfig.ServerEntry()};
                lc.Save();
                MessageBox.Show("Config file created");
                Environment.Exit(0);
            }
            if (cmdArgs.Contains("runlocal", StringComparer.OrdinalIgnoreCase))
            {
                foreach (var arg in cmdArgs)
                {
                    if (arg.StartsWith("port:", StringComparison.OrdinalIgnoreCase))
                    {
                        var splits = arg.Split(':');
                        if (splits.Length == 2)
                        {
                            int port;
                            if (int.TryParse(splits[1], out port))
                            {
                                var se = new LauncherConfig.ServerEntry();
                                se.Address = "127.0.0.1";
                                se.Port = port;
                                se.InfoPort = -1;
                                se.AdditionalArgs = " --show_console";
                                if (SetClientAdress(se))
                                {
                                    LaunchGame(se);
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    MessageBox.Show("Error updating client rc file");
                                    Environment.Exit(0);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error retrieving port. Use port:xxx as argument");
                                Environment.Exit(0);
                            }
                        }
                    }
                }
                MessageBox.Show("no port argument specified (port:xxx)");
                Environment.Exit(0);
            }
            SetTooltips();
            LoadConfig();
        }

        void LoadConfig()
        {
            _launcherConfig = LauncherConfig.Load();
            btnRun.Enabled = _launcherConfig != null && _launcherConfig.server != null;
            if (!btnRun.Enabled) return;
            HideAllPanels();
            rtNews.Text = string.Empty;
            if (_launcherConfig.server.InfoPort != -1)
            {
                var msg = proxyConnector.CreateMessage();
                msg.Write((byte)CommunicationHeader.CL2L_QUERY_INFO);
                proxyConnector.SendUnconnectedMessage(msg, _launcherConfig.server.Address, _launcherConfig.server.InfoPort);
            }
        }

        bool SetClientAdress(LauncherConfig.ServerEntry entry)
        {
            var path = Environment.CurrentDirectory;
            var dirInfo = Directory.GetParent(path);
            if (dirInfo != null && dirInfo.Parent != null)
            {
                path = dirInfo.Parent.FullName;
                path = Path.Combine(path, "etc");
                path = Path.Combine(path, "client");
                path = Path.Combine(path, "sb_client.rc");
            }
            else
            {
                return false;
            }
            if (File.Exists(path))
            {
                var newAdressString = string.Format("{0} {1}:{2}", ClientSetAdressIdentifier, entry.Address, entry.Port);
                var configStrings = new string[0];
                try
                {
                    configStrings = File.ReadAllLines(path);
                }
                catch (Exception)
                {
                    MessageBox.Show(CouldntOpenConfigError);
                }

                bool replaced = false;
                for (var line = 0; line < configStrings.Length; line++)
                {
                    if (configStrings[line].StartsWith(ClientSetAdressIdentifier, StringComparison.OrdinalIgnoreCase)) //replace existing
                    {
                        configStrings[line] = newAdressString;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced) //add new if not existing (might be commented out or something)
                {
                    Array.Resize(ref configStrings, configStrings.Length + 1);
                    configStrings[configStrings.Length - 1] = newAdressString;
                }
                try
                {
                    File.WriteAllLines(path, configStrings);
                }
                catch (Exception)
                {
                    MessageBox.Show(CouldntSaveConfigError);
                    return false;
                }
                return true;
            }
            return false;
        }

        void LaunchGame(LauncherConfig.ServerEntry entry, bool saveConfig = false)
        {
            var path = Path.Combine(Environment.CurrentDirectory, MainExename);
            if (File.Exists(path))
            {
                var arguments = " "+entry.AdditionalArgs;
                Process.Start(path, arguments);
            }
            else
            {
                MessageBox.Show(CouldntFindMainExeError);
            }
            if (saveConfig)
            {
                _launcherConfig.Save();
            }
            Close();
        }

        void btnRun_Click(object sender, EventArgs e)
        {
            var selectedEntry = _launcherConfig.server;
            if (selectedEntry == null)
            {
                MessageBox.Show("No server selected");
                return;
            }
            if (SetClientAdress(selectedEntry))
            {
                LaunchGame(selectedEntry, true);
            }
            else
            {
                MessageBox.Show(ClientConfigError);
            }
        }

        void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnNews_Click(object sender, EventArgs e)
        {
            ShowNewsPanel();
        }

        private void btnAccount_Click(object sender, EventArgs e)
        {
            ShowAccountPanel();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Length <= 2)
            {
                MessageBox.Show("Name too short");
                return;
            }
            if (txtMail.Text.Length <= 5 || !txtMail.Text.Contains("@") || !txtMail.Text.Contains("."))
            {
                MessageBox.Show("Mail format wrong");
                return;
            }
            if (txtRepeatPass.Text != txtPass.Text)
            {
                MessageBox.Show("Password fields don't match");
                return;
            }
            var msg = proxyConnector.CreateMessage();
            msg.Write((byte) CommunicationHeader.CL2L_REGISTER_ACCOUNT);
            msg.Write(txtName.Text);
            msg.Write(txtPass.Text);
            msg.Write(txtMail.Text);
            proxyConnector.SendUnconnectedMessage(msg, _launcherConfig.server.Address, _launcherConfig.server.InfoPort);
        }
    }
}