using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace COM_INFO


{
    public partial class Form1 : Form
    {
        private const int TimerIntervalMilliseconds = 5000;
        private const int BalloonTipTimeoutMilliseconds = 3000;
        private static readonly TimeSpan NewPortHighlightDuration = TimeSpan.FromMinutes(1);
        private const string NewPortBalloonTitle = "Nuevo puerto COM detectado";
        private const string NoPortsText = "Sin puertos COM disponibles";
        private const string PortsHeader = "Puertos COM:";
        private const string NewPortSuffix = " [NUEVO]";

        private Timer timer;
        private ComPortMonitor monitor;
        private Dictionary<string, DateTime> recentPorts;
        private bool exitRequested;

        public Form1()
        {
            InitializeComponent();

            FormClosing += Form1_FormClosing;
            Shown += Form1_Shown;
            salirToolStripMenuItem.Click += MenuItemSalir_Click;

            monitor = new ComPortMonitor();
            monitor.PortsChanged += Monitor_PortsChanged;
            monitor.Initialize();

            recentPorts = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

            timer = new Timer();
            timer.Interval = TimerIntervalMilliseconds;
            timer.Tick += Timer_Tick;
            timer.Start();

            UpdateNotifyIconText(monitor.CurrentPorts);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            monitor.CheckPorts();
            UpdateNotifyIconText(monitor.CurrentPorts);
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void MenuItemSalir_Click(object sender, EventArgs e)
        {
            exitRequested = true;
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exitRequested && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideWindow();
                return;
            }

            ReleaseRuntimeResources();
        }

        private void Monitor_PortsChanged(object sender, PortChangeEventArgs e)
        {
            RegisterRecentPorts(e.AddedPorts);
            RemoveRecentPorts(e.RemovedPorts);
            UpdateNotifyIconText(e.AllPorts);

            if (e.AddedPorts != null && e.AddedPorts.Count > 0)
            {
                ShowNewPortsBalloonTip(e.AddedPorts);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            HideWindow();
        }

        private void UpdateNotifyIconText(System.Collections.Generic.IReadOnlyList<string> ports)
        {
            RemoveExpiredRecentPorts();
            string text = BuildNotifyIconText(ports, recentPorts);

            if (!string.Equals(COM_info.Text, text, StringComparison.Ordinal))
            {
                COM_info.Text = text;
            }
        }

        private static string BuildNotifyIconText(System.Collections.Generic.IReadOnlyList<string> ports, IDictionary<string, DateTime> highlightedPorts)
        {
            if (ports == null || ports.Count == 0)
            {
                return NoPortsText;
            }

            List<string> lines = new List<string>();
            lines.Add(PortsHeader);

            foreach (string port in ports)
            {
                string line = port;

                if (highlightedPorts != null && highlightedPorts.ContainsKey(port))
                {
                    line += NewPortSuffix;
                }

                lines.Add(line);
            }

            List<string> visibleLines = new List<string>();
            int maxLength = 63;
            int currentLength = 0;

            foreach (string line in lines)
            {
                int projectedLength = currentLength;

                if (visibleLines.Count > 0)
                {
                    projectedLength += Environment.NewLine.Length;
                }

                projectedLength += line.Length;

                if (projectedLength > maxLength)
                {
                    break;
                }

                visibleLines.Add(line);
                currentLength = projectedLength;
            }

            if (visibleLines.Count == 0)
            {
                return NoPortsText;
            }

            if (visibleLines.Count < lines.Count)
            {
                string lastLine = visibleLines[visibleLines.Count - 1];
                int allowedLineLength = Math.Max(0, maxLength - (currentLength - lastLine.Length) - 3);

                if (lastLine.Length > allowedLineLength)
                {
                    lastLine = lastLine.Substring(0, allowedLineLength);
                }

                visibleLines[visibleLines.Count - 1] = lastLine + "...";
            }

            return string.Join(Environment.NewLine, visibleLines);
        }

        private void HideWindow()
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Hide();
        }

        private void ShowNewPortsBalloonTip(System.Collections.Generic.IReadOnlyList<string> addedPorts)
        {
            string balloonText = string.Join(", ", addedPorts);

            if (string.IsNullOrWhiteSpace(balloonText))
            {
                return;
            }

            COM_info.BalloonTipTitle = NewPortBalloonTitle;
            COM_info.BalloonTipText = balloonText;
            COM_info.BalloonTipIcon = ToolTipIcon.Info;
            COM_info.ShowBalloonTip(BalloonTipTimeoutMilliseconds);
        }

        private void RegisterRecentPorts(System.Collections.Generic.IReadOnlyList<string> addedPorts)
        {
            if (addedPorts == null)
            {
                return;
            }

            DateTime expiresAt = DateTime.Now.Add(NewPortHighlightDuration);

            foreach (string port in addedPorts)
            {
                recentPorts[port] = expiresAt;
            }
        }

        private void RemoveRecentPorts(System.Collections.Generic.IReadOnlyList<string> removedPorts)
        {
            if (removedPorts == null)
            {
                return;
            }

            foreach (string port in removedPorts)
            {
                recentPorts.Remove(port);
            }
        }

        private void RemoveExpiredRecentPorts()
        {
            if (recentPorts.Count == 0)
            {
                return;
            }

            DateTime now = DateTime.Now;
            List<string> expiredPorts = recentPorts
                .Where(entry => entry.Value <= now)
                .Select(entry => entry.Key)
                .ToList();

            foreach (string port in expiredPorts)
            {
                recentPorts.Remove(port);
            }
        }

        private void ReleaseRuntimeResources()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                timer.Dispose();
                timer = null;
            }

            if (monitor != null)
            {
                monitor.PortsChanged -= Monitor_PortsChanged;
                monitor.Dispose();
                monitor = null;
            }

            COM_info.Visible = false;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }
    }
}
