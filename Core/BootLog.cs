using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ProcessorEmulator.Core
{
    // Text log of ExtraROM FILE/TOC attach and OpenFile/LoadLibrary
    // names. WinExe has no console, so NkBinLoader WriteLine is
    // otherwise lost. Written next to ProcessorEmulator.exe, or
    // %TEMP%\ProcessorEmulator-extrarom\boot.log if that folder is
    // not writable. Never the dump folder. Flush each line so a
    // live Boot can be tailed. Do not invent FILE[26] bytes,
    // 0x81360000, or xdrm.dll.
    public static class BootLog
    {
        public const string FileName = "boot.log";
        public const string TempFolderName = "ProcessorEmulator-extrarom";

        private static readonly object Gate = new object();
        private static StreamWriter _writer;
        private static string _path = "";
        private static string _dumpFolder = "";
        private static string _lastLine = "";
        private static Action<string> _listener;
        private static StringBuilder _uart;

        public static string FilePath
        {
            get { lock (Gate) return _path; }
        }

        public static string LastLine
        {
            get { lock (Gate) return _lastLine; }
        }

        public static Action<string> Listener
        {
            get { lock (Gate) return _listener; }
            set { lock (Gate) _listener = value; }
        }

        public static string ResolvePath(string exeDir, string dumpFolder)
        {
            if (CanWriteBesideExe(exeDir, dumpFolder))
                return Path.Combine(exeDir, FileName);
            string temp = Path.Combine(Path.GetTempPath(), TempFolderName);
            Directory.CreateDirectory(temp);
            return Path.Combine(temp, FileName);
        }

        public static void Open(string dumpFolder)
        {
            lock (Gate)
            {
                CloseUnlocked();
                if (_uart != null)
                    _uart.Length = 0;
                _dumpFolder = dumpFolder ?? "";
                string exeDir = ExeDirectory();
                _path = ResolvePath(exeDir, _dumpFolder);
                string dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                var fs = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                _writer = new StreamWriter(fs, new UTF8Encoding(false)) { AutoFlush = true };
                _writer.WriteLine("boot log " + DateTime.UtcNow.ToString("o"));
                _writer.WriteLine("file=" + _path);
                if (!string.IsNullOrEmpty(_dumpFolder))
                    _writer.WriteLine("dump=" + _dumpFolder + " (not written)");
                _writer.Flush();
            }
        }

        public static void Write(string line)
        {
            if (line == null)
                return;
            Action<string> listener;
            lock (Gate)
            {
                if (_writer == null)
                    OpenUnlocked(_dumpFolder);
                _lastLine = line;
                if (_writer != null)
                {
                    _writer.WriteLine(line);
                    _writer.Flush();
                }
                listener = _listener;
            }
            try { Console.WriteLine(line); }
            catch { }
            if (listener != null)
            {
                try { listener(line); }
                catch { }
            }
        }

        // WinExe hides Console.Write of MipsUart UART_DR. Buffer
        // printable TX into lines, flush each line (and leftover
        // on Stop). Do not invent a second UART or a NIC.
        public static void UartTx(byte value)
        {
            char c = (char)(value & 0xFF);
            try { Console.Write(c); }
            catch { }
            try { Debug.Write(c); }
            catch { }
            if (c == '\0')
                return;
            string line = null;
            string hex = null;
            lock (Gate)
            {
                if (_writer == null)
                    OpenUnlocked(_dumpFolder);
                if (_uart == null)
                    _uart = new StringBuilder();
                if (c == '\n' || c == '\r')
                {
                    if (_uart.Length > 0)
                    {
                        line = _uart.ToString();
                        _uart.Length = 0;
                    }
                }
                else if (c >= 32 && c < 127)
                {
                    _uart.Append(c);
                    if (_uart.Length >= 240)
                    {
                        line = _uart.ToString();
                        _uart.Length = 0;
                    }
                }
                else if (c == '\t')
                {
                    _uart.Append('\t');
                }
                else
                {
                    if (_uart.Length > 0)
                    {
                        line = _uart.ToString();
                        _uart.Length = 0;
                    }
                    hex = "0x" + ((int)c).ToString("X2");
                }
            }
            if (line != null)
                Write("[Uart] " + line);
            if (hex != null)
                Write("[Uart] byte=" + hex);
        }

        public static void UartFlush()
        {
            string line = null;
            lock (Gate)
            {
                if (_uart != null && _uart.Length > 0)
                {
                    line = _uart.ToString();
                    _uart.Length = 0;
                }
            }
            if (line != null)
                Write("[Uart] " + line);
        }

        public static bool IsGuestIoName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            return ContainsFold(name, "rtl8139")
                || ContainsFold(name, "bcm7038mac")
                || ContainsFold(name, "ndis")
                || ContainsFold(name, "iptvdriver")
                || ContainsFold(name, "bcmuart")
                || ContainsFold(name, "com16550")
                || ContainsFold(name, "serial.dll")
                || EndsWithFold(name, "serial");
        }

        // LoadE32 / CEDecompressROM outer return only. WinExe hides
        // Hive Console.WriteLine. Do not log every inner LZX page.
        public static void LoadE32(string name, int index, uint v0, string why)
        {
            var sb = new StringBuilder();
            sb.Append("[Hive] LoadE32");
            if (index >= 0)
                sb.Append(" ExtraROM TOC[").Append(index).Append(']');
            if (!string.IsNullOrEmpty(name))
                sb.Append(' ').Append(name);
            sb.Append(" ret v0=0x").Append(v0.ToString("X8"));
            if (!string.IsNullOrEmpty(why))
                sb.Append(" (").Append(why).Append(')');
            Write(sb.ToString());
        }

        public static void DecompressRom(string name, uint dest, uint v0, string why)
        {
            var sb = new StringBuilder();
            sb.Append("[Hive] ExtraROM CEDecompressROM");
            if (!string.IsNullOrEmpty(name))
                sb.Append(' ').Append(name);
            sb.Append(" ret v0=0x").Append(v0.ToString("X8"));
            if (dest != 0)
                sb.Append(" dest=0x").Append(dest.ToString("X8"));
            if (!string.IsNullOrEmpty(why))
                sb.Append(" (").Append(why).Append(')');
            Write(sb.ToString());
        }

        public static void Rom(string result, string source, string kind, int index,
            string name, int type, uint dest, uint real, uint comp, string why)
        {
            var sb = new StringBuilder();
            sb.Append("[Rom] ").Append(string.IsNullOrEmpty(result) ? "?" : result);
            if (!string.IsNullOrEmpty(source))
                sb.Append(' ').Append(source);
            if (!string.IsNullOrEmpty(kind))
            {
                sb.Append(' ').Append(kind);
                if (index >= 0)
                    sb.Append('[').Append(index).Append(']');
            }
            if (!string.IsNullOrEmpty(name))
                sb.Append(' ').Append(name);
            if (type == 7 || type == 8)
                sb.Append(" type=").Append(type);
            if (dest != 0)
                sb.Append(" dest=0x").Append(dest.ToString("X8"));
            if (real != 0 || comp != 0)
                sb.Append(" real=").Append(real).Append(" comp=").Append(comp);
            if (!string.IsNullOrEmpty(why))
                sb.Append(" (").Append(why).Append(')');
            Write(sb.ToString());
        }

        public static bool SameFolder(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;
            try
            {
                string na = Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string nb = Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return string.Equals(na, nb, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool FolderIsDumpOrInside(string folder, string dumpFolder)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(dumpFolder))
                return false;
            try
            {
                string f = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string d = Path.GetFullPath(dumpFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (string.Equals(f, d, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (!d.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    d += Path.DirectorySeparatorChar;
                return f.StartsWith(d, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool ContainsFold(string name, string token)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(token))
                return false;
            return name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool EndsWithFold(string name, string token)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(token) || name.Length < token.Length)
                return false;
            return name.EndsWith(token, StringComparison.OrdinalIgnoreCase);
        }

        private static void OpenUnlocked(string dumpFolder)
        {
            _dumpFolder = dumpFolder ?? "";
            string exeDir = ExeDirectory();
            _path = ResolvePath(exeDir, _dumpFolder);
            string dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var fs = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(fs, new UTF8Encoding(false)) { AutoFlush = true };
        }

        private static void CloseUnlocked()
        {
            if (_writer != null)
            {
                try { _writer.Flush(); }
                catch { }
                try { _writer.Dispose(); }
                catch { }
                _writer = null;
            }
        }

        private static bool CanWriteBesideExe(string exeDir, string dumpFolder)
        {
            if (string.IsNullOrEmpty(exeDir))
                return false;
            try
            {
                if (!Directory.Exists(exeDir))
                    return false;
                if (FolderIsDumpOrInside(exeDir, dumpFolder))
                    return false;
                string probe = Path.Combine(exeDir, ".bootlog-write-probe");
                File.WriteAllText(probe, "ok");
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ExeDirectory()
        {
            try
            {
                string file = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(file))
                {
                    string dir = Path.GetDirectoryName(file);
                    if (!string.IsNullOrEmpty(dir))
                        return dir;
                }
            }
            catch
            {
            }
            try
            {
                string bas = AppDomain.CurrentDomain.BaseDirectory;
                if (!string.IsNullOrEmpty(bas))
                    return Path.GetFullPath(bas);
            }
            catch
            {
            }
            return "";
        }
    }
}
