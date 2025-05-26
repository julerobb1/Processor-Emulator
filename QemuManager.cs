using System.Diagnostics;
using System.IO;

namespace ProcessorEmulator.Tools
{
    public class QemuManager
    {
        public string QemuPath { get; set; } = "qemu-system-mips.exe"; // Default, can be changed per arch

        public void Launch(string imagePath, string architecture)
        {
            string qemuExe = GetQemuExecutable(architecture);
            if (!File.Exists(qemuExe))
                throw new FileNotFoundException($"QEMU executable not found: {qemuExe}");

            var args = GetQemuArgs(imagePath, architecture);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = qemuExe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
        }

        public void LaunchWithArgs(string imagePath, string architecture, string extraArgs)
        {
            string qemuExe = GetQemuExecutable(architecture);
            if (!File.Exists(qemuExe))
                throw new FileNotFoundException($"QEMU executable not found: {qemuExe}");

            var args = GetQemuArgs(imagePath, architecture) + " " + extraArgs;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = qemuExe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
        }

        private string GetQemuExecutable(string architecture)
        {
            switch (architecture)
            {
                case "MIPS32": return "qemu-system-mips.exe";
                case "MIPS64": return "qemu-system-mips64.exe";
                case "ARM": return "qemu-system-arm.exe";
                case "ARM64": return "qemu-system-aarch64.exe";
                case "PowerPC": return "qemu-system-ppc.exe";
                case "x86": return "qemu-system-i386.exe";
                case "x86-64": return "qemu-system-x86_64.exe";
                case "RISC-V": return "qemu-system-riscv64.exe";
                default: return "qemu-system-mips.exe";
            }
        }

        private string GetQemuArgs(string imagePath, string architecture)
        {
            // Basic args, can be extended for more options
            return $"-m 256 -drive file=\"{imagePath}\",format=raw -nographic";
        }
    }
}
