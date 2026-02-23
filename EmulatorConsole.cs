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
            this.BackColor = SystemColors.Control;
            this.Size = new Size(800, 600);

            _terminal = new WinForms.RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Lucida Console", 10, FontStyle.Regular),
                ReadOnly = true,
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
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

            if (keyData == Keys.Enter) c = '\r';
            else if (keyData == Keys.Back) c = '\b';
            else if (keyData >= Keys.A && keyData <= Keys.Z)
            {
                bool shift = (ModifierKeys & Keys.Shift) != 0;
                c = (char)(keyData.ToString()[0]);
                if (!shift) c = char.ToLower(c);
            }
            else if (keyData >= Keys.D0 && keyData <= Keys.D9)
            {
                c = keyData.ToString().Last();
            }
            else if (keyData == Keys.Space) c = ' ';

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