using System.Diagnostics;
using System.IO;

namespace ProcessorEmulator.Tools
{
    public class QemuManager
    {
        public string QemuPath { get; set; } = "qemu-system-mips.exe"; // Default, can be changed per arch

        public static void Launch(string imagePath, string architecture)
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

        public static void LaunchWithArgs(string imagePath, string architecture, string extraArgs)
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

        private static string GetQemuExecutable(string architecture)
        {
            return architecture switch
            {
                "MIPS32" => "qemu-system-mips.exe",
                "MIPS64" => "qemu-system-mips64.exe",
                "ARM" => "qemu-system-arm.exe",
                "ARM64" => "qemu-system-aarch64.exe",
                "PowerPC" => "qemu-system-ppc.exe",
                "x86" => "qemu-system-i386.exe",
                "x86-64" => "qemu-system-x86_64.exe",
                "RISC-V" => "qemu-system-riscv64.exe",
                _ => "qemu-system-mips.exe",
            };
        }

        private static string GetQemuArgs(string imagePath, string architecture)
        {
            // Basic args, can be extended for more options
            return $"-m 256 -drive file=\"{imagePath}\",format=raw -nographic";
        }
    }
}
