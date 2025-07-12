using System.Diagnostics;
using System.IO;
using System;

namespace ProcessorEmulator.Tools
{
    public class QemuManager
    {
        public string QemuPath { get; set; } = "qemu-system-mips.exe"; // Default, can be changed per arch

        private static string LocateExecutable(string exeName)
        {
            // Check current directory or absolute path
            if (File.Exists(exeName))
                return Path.GetFullPath(exeName);

            // Search in PATH
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(';') ?? Array.Empty<string>();
            foreach (var dir in paths)
            {
                try
                {
                    var full = Path.Combine(dir, exeName);
                    if (File.Exists(full))
                        return full;
                }
                catch { }
            }

            // Check common installation directories
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            foreach (var baseDir in new[] { programFiles, programFilesX86 })
            {
                var qemuDir = Path.Combine(baseDir, "qemu");
                var full = Path.Combine(qemuDir, exeName);
                if (File.Exists(full))
                    return full;
            }

            throw new FileNotFoundException($"QEMU executable not found: {exeName}");
        }

        private static string GetQemuExecutable(string architecture)
        {
            var exe = architecture switch
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
            return LocateExecutable(exe);
        }

        private static string GetQemuArgs(string imagePath, string architecture)
        {
            // Basic args, can be extended for more options
            // Use default graphical display; remove nographic to show firmware UI
            return $"-m 256 -drive file=\"{imagePath}\",format=raw";
        }

        public static void Launch(string imagePath, string architecture)
        {
            string qemuExe = GetQemuExecutable(architecture);
            if (!File.Exists(qemuExe))
                throw new FileNotFoundException($"QEMU executable not found: {qemuExe}");

            var args = GetQemuArgs(imagePath, architecture);
            // Launch QEMU with GUI window
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = qemuExe,
                    Arguments = args,
                    UseShellExecute = true,
                    CreateNoWindow = false
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
            // Launch QEMU with GUI window and extra args
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = qemuExe,
                    Arguments = args,
                    UseShellExecute = true,
                    CreateNoWindow = false
                }
            };
            process.Start();
        }
    }
}
