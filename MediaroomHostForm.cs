using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ProcessorEmulator
{
    // Thin Win7/WinForms shell. Shows dump hunt + honest boot log.
    // Not a TV UI. Not a product shell.
    public sealed class MediaroomHostForm : Form
    {
        private readonly TextBox _dumpBox;
        private readonly Button _browse;
        private readonly Button _boot;
        private readonly Button _stop;
        private readonly Label _status;
        private readonly TextBox _log;
        private MediaroomSession _session;
        private Thread _worker;

        public string DumpPath
        {
            get { return _dumpBox.Text; }
            set { _dumpBox.Text = value ?? ""; }
        }

        public MediaroomHostForm()
        {
            Text = "Mediaroom";
            Width = 900;
            Height = 640;
            StartPosition = FormStartPosition.CenterScreen;
            Font = SystemFonts.MessageBoxFont;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = true;
            MaximizeBox = true;

            var top = new Panel { Dock = DockStyle.Top, Height = 64 };
            var dumpLabel = new Label { Text = "Dump", Left = 8, Top = 10, Width = 44, AutoSize = false };
            _dumpBox = new TextBox { Left = 56, Top = 8, Width = 620, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            _browse = new Button { Text = "Browse", Left = 684, Top = 6, Width = 80, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            _boot = new Button { Text = "Boot", Left = 56, Top = 34, Width = 72 };
            _stop = new Button { Text = "Stop", Left = 134, Top = 34, Width = 72, Enabled = false };
            _status = new Label { Text = "idle", Left = 220, Top = 38, Width = 540, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            _browse.Click += BrowseClick;
            _boot.Click += BootClick;
            _stop.Click += StopClick;
            top.Controls.Add(dumpLabel);
            top.Controls.Add(_dumpBox);
            top.Controls.Add(_browse);
            top.Controls.Add(_boot);
            top.Controls.Add(_stop);
            top.Controls.Add(_status);
            top.Resize += (_, __) =>
            {
                _dumpBox.Width = Math.Max(80, top.ClientSize.Width - 56 - 96);
                _browse.Left = top.ClientSize.Width - 88;
            };

            _log = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font(FontFamily.GenericMonospace, 9f),
                BackColor = SystemColors.Window,
                ForeColor = SystemColors.WindowText
            };

            Controls.Add(_log);
            Controls.Add(top);

            string env = Environment.GetEnvironmentVariable(Core.HostHardDisk.EnvName);
            if (!string.IsNullOrEmpty(env))
                DumpPath = env;

            HandleCreated += (_, __) => Win7VisualStyle.ApplyToHwnd(Handle);
            FormClosing += (_, __) =>
            {
                _session?.RequestStop();
            };
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            using var d = new FolderBrowserDialog { Description = "Mediaroom / WinCE dump folder" };
            if (d.ShowDialog(this) == DialogResult.OK)
                _dumpBox.Text = d.SelectedPath;
        }

        private void StopClick(object sender, EventArgs e)
        {
            _session?.RequestStop();
            _status.Text = "stopping";
        }

        private void BootClick(object sender, EventArgs e)
        {
            if (_worker != null && _worker.IsAlive)
                return;
            _log.Clear();
            _boot.Enabled = false;
            _stop.Enabled = true;
            _status.Text = "booting";
            string feed = _dumpBox.Text;
            _session = new MediaroomSession(AppendLog);
            _worker = new Thread(() =>
            {
                TextWriter old = Console.Out;
                Console.SetOut(new MediaroomSession.ConsoleTap(old, AppendLog));
                try
                {
                    bool ok = _session.Run(feed, 90000000);
                    BeginInvoke(new Action(() =>
                    {
                        _status.Text = ok
                            ? ("done steps=" + _session.Steps + " PC=0x" + _session.ProgramCounter.ToString("X8"))
                            : "failed";
                        _boot.Enabled = true;
                        _stop.Enabled = false;
                    }));
                }
                catch (Exception ex)
                {
                    AppendLog(ex.ToString());
                    BeginInvoke(new Action(() =>
                    {
                        _status.Text = "failed";
                        _boot.Enabled = true;
                        _stop.Enabled = false;
                    }));
                }
                finally
                {
                    try { Console.SetOut(old); } catch { }
                }
            });
            _worker.IsBackground = true;
            _worker.Start();
        }

        private void AppendLog(string line)
        {
            if (IsDisposed)
                return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<string>(AppendLog), line); }
                catch { }
                return;
            }
            _log.AppendText(line + Environment.NewLine);
        }
    }
}
