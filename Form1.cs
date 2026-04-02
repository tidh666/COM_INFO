using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace COM_INFO


{
    public partial class Form1 : Form
    {
        private enum NotificationMode
        {
            BalloonTip,
            Dialog,
            None
        }

        private const int TimerIntervalMilliseconds = 5000;
        private const int BalloonTipTimeoutMilliseconds = 3000;
        private static readonly TimeSpan NewPortHighlightDuration = TimeSpan.FromMinutes(1);
        private const string NewPortBalloonTitle = "Nuevo puerto COM detectado";
        private const string NoPortsText = "Sin puertos COM disponibles";
        private const string PortsHeader = "Puertos COM:";
        private const string NewPortSuffix = " [NUEVO]";
        private const string NotificationMenuText = "Notificacion";
        private const string BalloonNotificationText = "Globo de bandeja";
        private const string DialogNotificationText = "Ventana emergente";
        private const string NoNotificationText = "Sin notificacion";

        private Timer timer;
        private ComPortMonitor monitor;
        private Dictionary<string, DateTime> recentPorts;
        private ToolStripMenuItem notificationMenuItem;
        private ToolStripMenuItem balloonNotificationMenuItem;
        private ToolStripMenuItem dialogNotificationMenuItem;
        private ToolStripMenuItem noNotificationMenuItem;
        private NotificationMode notificationMode;
        private string notificationModeFilePath;
        private bool exitRequested;

        public Form1()
        {
            InitializeComponent();

            FormClosing += Form1_FormClosing;
            Shown += Form1_Shown;
            salirToolStripMenuItem.Click += MenuItemSalir_Click;

            notificationModeFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "COM_INFO",
                "notification-mode.txt");

            InitializeNotificationMenu();
            notificationMode = LoadNotificationMode();
            UpdateNotificationMenuState();

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
                ShowNewPortNotification(e.AddedPorts);
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
            string balloonText = string.Join(Environment.NewLine, addedPorts);

            if (string.IsNullOrWhiteSpace(balloonText))
            {
                return;
            }

            COM_info.BalloonTipTitle = NewPortBalloonTitle;
            COM_info.BalloonTipText = balloonText;
            COM_info.BalloonTipIcon = ToolTipIcon.Info;
            COM_info.ShowBalloonTip(BalloonTipTimeoutMilliseconds);
        }

        private void ShowNewPortsDialog(System.Collections.Generic.IReadOnlyList<string> addedPorts)
        {
            string dialogText = string.Join(Environment.NewLine, addedPorts);

            if (string.IsNullOrWhiteSpace(dialogText))
            {
                return;
            }

            MessageBox.Show(dialogText, NewPortBalloonTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowNewPortNotification(System.Collections.Generic.IReadOnlyList<string> addedPorts)
        {
            switch (notificationMode)
            {
                case NotificationMode.Dialog:
                    ShowNewPortsDialog(addedPorts);
                    break;

                case NotificationMode.None:
                    break;

                default:
                    ShowNewPortsBalloonTip(addedPorts);
                    break;
            }
        }

        private void InitializeNotificationMenu()
        {
            notificationMenuItem = new ToolStripMenuItem(NotificationMenuText);
            balloonNotificationMenuItem = CreateNotificationModeMenuItem(BalloonNotificationText, NotificationMode.BalloonTip);
            dialogNotificationMenuItem = CreateNotificationModeMenuItem(DialogNotificationText, NotificationMode.Dialog);
            noNotificationMenuItem = CreateNotificationModeMenuItem(NoNotificationText, NotificationMode.None);

            notificationMenuItem.DropDownItems.Add(balloonNotificationMenuItem);
            notificationMenuItem.DropDownItems.Add(dialogNotificationMenuItem);
            notificationMenuItem.DropDownItems.Add(noNotificationMenuItem);

            contextMenuStrip1.Items.Insert(0, new ToolStripSeparator());
            contextMenuStrip1.Items.Insert(0, notificationMenuItem);
        }

        private ToolStripMenuItem CreateNotificationModeMenuItem(string text, NotificationMode mode)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem(text);
            menuItem.Tag = mode;
            menuItem.Click += NotificationModeMenuItem_Click;
            return menuItem;
        }

        private void NotificationModeMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

            if (menuItem == null || !(menuItem.Tag is NotificationMode))
            {
                return;
            }

            notificationMode = (NotificationMode)menuItem.Tag;
            UpdateNotificationMenuState();
            SaveNotificationMode();
        }

        private void UpdateNotificationMenuState()
        {
            if (balloonNotificationMenuItem == null)
            {
                return;
            }

            balloonNotificationMenuItem.Checked = notificationMode == NotificationMode.BalloonTip;
            dialogNotificationMenuItem.Checked = notificationMode == NotificationMode.Dialog;
            noNotificationMenuItem.Checked = notificationMode == NotificationMode.None;
        }

        private NotificationMode LoadNotificationMode()
        {
            try
            {
                if (!File.Exists(notificationModeFilePath))
                {
                    return NotificationMode.BalloonTip;
                }

                string value = File.ReadAllText(notificationModeFilePath).Trim();
                NotificationMode loadedMode;

                if (Enum.TryParse(value, true, out loadedMode))
                {
                    return loadedMode;
                }
            }
            catch
            {
            }

            return NotificationMode.BalloonTip;
        }

        private void SaveNotificationMode()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(notificationModeFilePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(notificationModeFilePath, notificationMode.ToString());
            }
            catch
            {
            }
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
