using System;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Provides cross-architecture binary translation (e.g. static recompilation) between supported ISAs.
    /// </summary>
    public static class BinaryTranslator
    {
        /// <summary>
        /// Translates a raw binary image from one architecture to another.
        /// </summary>
        /// <param name="fromArch">Source architecture (e.g. "x86", "ARM").</param>
        /// <param name="toArch">Target architecture (e.g. "x64", "MIPS").</param>
        /// <param name="input">Original binary data.</param>
        /// <returns>Translated binary data for the target ISA.</returns>
        public static byte[] Translate(string fromArch, string toArch, byte[] input)
        {
            // Bypass translation if arch matches
            if (string.Equals(fromArch, toArch, StringComparison.OrdinalIgnoreCase))
                return input;
            // Write input to temp file
            string tempIn = Path.GetTempFileName();
            string tempOut = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.WriteAllBytes(tempIn, input);
            // Build RetDec CLI arguments
            string args = $"--mode raw -e {fromArch} -t {toArch} -o \"{tempOut}\" \"{tempIn}\"";
            var psi = new ProcessStartInfo
            {
                FileName = "retdec-decompiler", // ensure in PATH
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                string err = proc.StandardError.ReadToEnd();
                MessageBox.Show($"RetDec error: {err}", "Translate Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return input;
            }
            // Read and clean up
            var output = File.ReadAllBytes(tempOut);
            File.Delete(tempIn);
            File.Delete(tempOut);
            return output;
        }
    }
}
