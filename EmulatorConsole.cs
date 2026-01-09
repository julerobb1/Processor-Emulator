using System;
using System.Drawing;
using System.Windows.Forms;

using System.Linq;

namespace ProcessorEmulator
{
    public partial class EmulatorConsole : Form
    {
        private RichTextBox _terminal;

        public EmulatorConsole()
        {
            // Set the classic Win32 look
            this.Text = "MIPS System Console";
            this.BackColor = SystemColors.Control; // Classic Gray
            this.Size = new Size(800, 600);

            _terminal = new RichTextBox
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
        
        public void AppendChar(char c)
        {
            AppendText(c.ToString());
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Convert the key to an ASCII character
            char c = (char)0;

            if (keyData == Keys.Enter) c = '\r';
            else if (keyData == Keys.Back) c = '\b';
            else if (keyData >= Keys.A && keyData <= Keys.Z)
            {
                // Handle Shift for uppercase/lowercase
                bool shift = (ModifierKeys & Keys.Shift) != 0;
                c = (char)(keyData.ToString()[0]);
                if (!shift) c = char.ToLower(c);
            }
            else if (keyData >= Keys.D0 && keyData <= Keys.D9)
            {
                c = keyData.ToString().Last();
            }
            // Add more cases for symbols/space as needed...
            else if (keyData == Keys.Space) c = ' ';

            if (c != 0)
            {
                // Access your UART instance and send the key
                // Assuming you have a reference to the active UART
                if (Program.CurrentUart != null)
                {
                    Program.CurrentUart.SendKey(c);
                    return true; // Mark as handled
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
