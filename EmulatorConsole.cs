using System;
using System.Drawing;
using WinForms = System.Windows.Forms;
using System.Linq;

namespace ProcessorEmulator
{
    public partial class EmulatorConsole : WinForms.Form
    {
        private WinForms.RichTextBox _terminal;

        public EmulatorConsole()
        {
            this.Text = "MIPS System Console";
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Size = new System.Drawing.Size(800, 600);

            _terminal = new WinForms.RichTextBox
            {
                Dock = WinForms.DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new System.Drawing.Font("Lucida Console", 10, System.Drawing.FontStyle.Regular),
                ReadOnly = true,
                Multiline = true,
                ScrollBars = WinForms.RichTextBoxScrollBars.Vertical
            };

            this.Controls.Add(_terminal);
        }

        public void AppendText(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendText), text);
                return;
            }
            _terminal.AppendText(text);
            _terminal.SelectionStart = _terminal.Text.Length;
            _terminal.ScrollToCaret();
        }

        // Append a single character (used as a callback from UART/OnCharReceived)
        public void AppendChar(char c)
        {
            AppendText(c.ToString());
        }

        // Intercepts key presses to send to the emulated UART.
        protected override bool ProcessCmdKey(ref WinForms.Message msg, WinForms.Keys keyData)
        {
            char c = (char)0;

            if (keyData == WinForms.Keys.Enter) c = '\r';
            else if (keyData == WinForms.Keys.Back) c = '\b';
            else if (keyData >= WinForms.Keys.A && keyData <= WinForms.Keys.Z)
            {
                bool shift = (ModifierKeys & WinForms.Keys.Shift) != 0;
                c = (char)(keyData.ToString()[0]);
                if (!shift) c = char.ToLower(c);
            }
            else if (keyData >= WinForms.Keys.D0 && keyData <= WinForms.Keys.D9)
            {
                c = keyData.ToString().Last();
            }
            else if (keyData == WinForms.Keys.Space) c = ' ';

            // Send the character to the UART if it's a valid one.
            if (c != 0 && Program.CurrentUart != null)
            {
                Program.CurrentUart.SendKey(c);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}