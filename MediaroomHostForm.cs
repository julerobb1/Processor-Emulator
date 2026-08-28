using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ProcessorEmulator
{
    // Thin Win7 host. Framebuffer pane is the surface. Black until
    // the guest writes video RAM. No boot-log theater.
    public sealed class MediaroomHostForm : Form
    {
        private readonly TextBox _dumpBox;
        private readonly Button _browse;
        private readonly Button _boot;
        private readonly Button _stop;
        private readonly Label _status;
        private readonly PictureBox _frame;
        private readonly System.Windows.Forms.Timer _tick;
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
            // Host chrome only. Not guest video and not a framebuffer size.
            Width = 900;
            Height = 640;
            StartPosition = FormStartPosition.CenterScreen;
            Font = SystemFonts.MessageBoxFont;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = true;
            MaximizeBox = true;

            var top = new Panel { Dock = DockStyle.Top, Height = 36 };
            _dumpBox = new TextBox { Left = 8, Top = 6, Width = 520, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            _browse = new Button { Text = "Dump", Left = 536, Top = 4, Width = 56, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            _boot = new Button { Text = "Boot", Left = 596, Top = 4, Width = 56, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            _stop = new Button { Text = "Stop", Left = 656, Top = 4, Width = 56, Enabled = false, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            _browse.Click += BrowseClick;
            _boot.Click += BootClick;
            _stop.Click += StopClick;
            top.Controls.Add(_dumpBox);
            top.Controls.Add(_browse);
            top.Controls.Add(_boot);
            top.Controls.Add(_stop);
            top.Resize += (_, __) =>
            {
                _dumpBox.Width = Math.Max(80, top.ClientSize.Width - 200);
                _browse.Left = top.ClientSize.Width - 184;
                _boot.Left = top.ClientSize.Width - 124;
                _stop.Left = top.ClientSize.Width - 64;
            };

            _status = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = "idle",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _frame = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Controls.Add(_frame);
            Controls.Add(_status);
            Controls.Add(top);

            string env = Environment.GetEnvironmentVariable(Core.HostHardDisk.EnvName);
            if (!string.IsNullOrEmpty(env))
                DumpPath = env;

            _tick = new System.Windows.Forms.Timer { Interval = 250 };
            _tick.Tick += (_, __) => RefreshStatus();
            _tick.Start();

            HandleCreated += (_, __) => Win7VisualStyle.ApplyToHwnd(Handle);
            FormClosing += (_, __) =>
            {
                _tick.Stop();
                _session?.RequestStop();
            };
        }

        private void RefreshStatus()
        {
            if (_session == null)
                return;
            string note = _session.MemsetNote;
            _status.Text = "Hz=" + _session.Hertz
                + " PC=0x" + _session.ProgramCounter.ToString("X8")
                + " steps=" + _session.Steps
                + (string.IsNullOrEmpty(note) ? "" : "  " + note);
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            using var d = new OpenFileDialog
            {
                Title = "Mediaroom / WinCE dump",
                Filter = "nk.bin / etc.bin|nk.bin;etc.bin|BIN files (*.bin)|*.bin|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };
            string current = DumpPath?.Trim();
            if (!string.IsNullOrEmpty(current) && Directory.Exists(current))
                d.InitialDirectory = current;
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                string dir = Path.GetDirectoryName(d.FileName);
                if (!string.IsNullOrEmpty(dir))
                    DumpPath = dir;
            }
        }

        private void StopClick(object sender, EventArgs e)
        {
            _session?.RequestStop();
        }

        private void BootClick(object sender, EventArgs e)
        {
            if (_worker != null && _worker.IsAlive)
                return;
            _boot.Enabled = false;
            _stop.Enabled = true;
            _status.Text = "booting";
            _frame.Image = null;
            _frame.BackColor = Color.Black;
            string feed = _dumpBox.Text;
            _session = new MediaroomSession(s =>
            {
                if (IsDisposed || !IsHandleCreated)
                    return;
                try
                {
                    BeginInvoke(new Action(() => { _status.Text = s; }));
                }
                catch
                {
                }
            });
            _worker = new Thread(() =>
            {
                try
                {
                    _session.Run(feed);
                }
                catch (Exception ex)
                {
                    try
                    {
                        BeginInvoke(new Action(() => { _status.Text = ex.GetType().Name; }));
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            _boot.Enabled = true;
                            _stop.Enabled = false;
                            RefreshStatus();
                        }));
                    }
                    catch
                    {
                    }
                }
            });
            _worker.IsBackground = true;
            _worker.Start();
        }
    }
}
