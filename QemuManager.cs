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
            // Select machine type based on architecture: malta for MIPS, virt for ARM
            string machine = null;
            if (architecture.StartsWith("MIPS", StringComparison.OrdinalIgnoreCase)) machine = "malta";
            else if (architecture.StartsWith("ARM", StringComparison.OrdinalIgnoreCase)) machine = "virt";
            string machineArg = string.IsNullOrEmpty(machine) ? string.Empty : $"-machine {machine}";
            // If this is a raw firmware blob, boot it as a kernel with serial console
            var ext = Path.GetExtension(imagePath).ToLowerInvariant();
            if (ext == ".bin")
            {
                return $"{machineArg} -m 256 -kernel \"{imagePath}\" -serial stdio";
            }
            // Otherwise boot from image drive with VGA display
            return $"{machineArg} -m 256 -drive file=\"{imagePath}\",format=raw -boot order=c -vga std -display sdl";
        }

        public static void Launch(string imagePath, string architecture)
        {
            string qemuExe = GetQemuExecutable(architecture);
            if (!File.Exists(qemuExe))
                throw new FileNotFoundException($"QEMU executable not found: {qemuExe}");

            var args = GetQemuArgs(imagePath, architecture);
            // Launch QEMU with GUI window
            // Launch QEMU directly with GUI
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
            // Launch QEMU with extra args directly
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = qemuExe,
                    Arguments = args + " " + extraArgs,
                    UseShellExecute = true,
                    CreateNoWindow = false
                }
            };
            process.Start();
        }
    }
}
