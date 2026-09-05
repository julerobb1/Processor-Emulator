using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ProcessorEmulator.Core;

namespace ProcessorEmulator
{
    // Thin Win7 guest console. The window is the guest display
    // (black until video RAM). Start/Stop + one attached folder.
    // Same MediaroomSession path. No dump/boot theater.
    public sealed class MediaroomHostForm : Form
    {
        private readonly TextBox _folderBox;
        private readonly Button _folder;
        private readonly Button _start;
        private readonly Button _stop;
        private readonly Label _status;
        private readonly PictureBox _frame;
        private MediaroomSession _session;
        private Thread _worker;

        public string DiskFolder
        {
            get { return _folderBox.Text; }
            set { _folderBox.Text = value ?? ""; }
        }

        public MediaroomHostForm()
        {
            Text = "MIPS Guest";
            Width = 900;
            Height = 640;
            StartPosition = FormStartPosition.CenterScreen;
            Font = SystemFonts.MessageBoxFont;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = true;
            MaximizeBox = true;

            var top = new Panel { Dock = DockStyle.Top, Height = 36 };
            _folderBox = new TextBox { Left = 8, Top = 6, Width = 520, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            _folder = new Button { Text = "Folder", Left = 536, Top = 4, Width = 56, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            _start = new Button { Text = "Start", Left = 596, Top = 4, Width = 56, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            _stop = new Button { Text = "Stop", Left = 656, Top = 4, Width = 56, Enabled = false, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            _folder.Click += FolderClick;
            _start.Click += StartClick;
            _stop.Click += StopClick;
            top.Controls.Add(_folderBox);
            top.Controls.Add(_folder);
            top.Controls.Add(_start);
            top.Controls.Add(_stop);
            top.Resize += (_, __) =>
            {
                _folderBox.Width = Math.Max(80, top.ClientSize.Width - 200);
                _folder.Left = top.ClientSize.Width - 184;
                _start.Left = top.ClientSize.Width - 124;
                _stop.Left = top.ClientSize.Width - 64;
            };

            _status = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = "Stopped",
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

            AutoFillFolder();

            HandleCreated += (_, __) => Win7VisualStyle.ApplyToHwnd(Handle);
            FormClosing += (_, __) => { _session?.RequestStop(); BootLog.UartFlush(); };
        }

        private void AutoFillFolder()
        {
            string env = Environment.GetEnvironmentVariable(HostHardDisk.EnvName);
            if (string.IsNullOrEmpty(env))
                env = Environment.GetEnvironmentVariable(HostHardDisk.EnvNameAlt);
            if (!string.IsNullOrEmpty(env) && Directory.Exists(env))
            {
                DiskFolder = env;
                return;
            }

            string here = ShallowNkFolder(Environment.CurrentDirectory);
            if (string.IsNullOrEmpty(here))
                here = ShallowNkFolder(AppDomain.CurrentDomain.BaseDirectory);
            if (!string.IsNullOrEmpty(here))
                DiskFolder = here;
        }

        private static string ShallowNkFolder(string dir)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return "";
            try
            {
                if (File.Exists(Path.Combine(dir, "nk.bin")))
                    return Path.GetFullPath(dir);
            }
            catch
            {
            }
            return "";
        }

        private void SetRunning(bool running)
        {
            _start.Enabled = !running;
            _stop.Enabled = running;
            if (!running)
                BootLog.Write("Stopped");
        }

        private void ShowStatus(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;
            void apply()
            {
                _status.Text = line.Length > 140 ? line.Substring(0, 140) : line;
            }
            try
            {
                if (IsHandleCreated && InvokeRequired)
                    BeginInvoke(new Action(apply));
                else
                    apply();
            }
            catch
            {
            }
        }

        private void FolderClick(object sender, EventArgs e)
        {
            using var d = new FolderBrowserDialog
            {
                Description = "Guest disk folder"
            };
            string current = DiskFolder?.Trim();
            if (!string.IsNullOrEmpty(current) && Directory.Exists(current))
                d.SelectedPath = current;
            if (d.ShowDialog(this) == DialogResult.OK)
                DiskFolder = d.SelectedPath;
        }

        private void StopClick(object sender, EventArgs e)
        {
            _session?.RequestStop();
        }

        private void StartClick(object sender, EventArgs e)
        {
            if (_worker != null && _worker.IsAlive)
                return;
            SetRunning(true);
            _frame.Image = null;
            _frame.BackColor = Color.Black;
            string feed = _folderBox.Text;
            BootLog.Open(feed);
            BootLog.Listener = ShowStatus;
            BootLog.Write("start folder=" + (feed ?? "") + " log=" + BootLog.FilePath);
            BootLog.Write("cli=tail that log (ExtraROM FILE/TOC attach + UART TX). guest frame black; no _frame.Image blit; GuestVideoWrote=false; Display=ddi_nop.dll ExtraROM TOC[33] stub; MipsUart 0xB0000000 TX -> this file; no NIC on the MIPS bus");
            _session = new MediaroomSession(BootLog.Write);
            _worker = new Thread(() =>
            {
                try
                {
                    _session.Run(feed);
                }
                catch
                {
                }
                finally
                {
                    try
                    {
                        BeginInvoke(new Action(() => { SetRunning(false); }));
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
