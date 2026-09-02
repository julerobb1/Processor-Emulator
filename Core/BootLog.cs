using System;
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
