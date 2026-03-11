
using System;
using System.IO;
using System.Collections.Generic;

namespace ProcessorEmulator
{
    /// <summary>
    /// Utility that locates a Windows CE kernel (nk.exe) or other firmware
    /// binary by walking a directory tree in a low-impact fashion.
    /// </summary>
    public static class FirmwareScanner
    {
        /// <summary>
        /// Recursively searches for the first file matching <paramref name="pattern"/>
        /// starting at <paramref name="startPath"/>.  The search uses
        /// <see cref="Directory.EnumerateFiles"/> so that the disk is only read as
        /// far as necessary; the operation stops as soon as a candidate is found.
        /// </summary>
        /// <param name="startPath">Directory to begin the hunt.</param>
        /// <param name="pattern">Filename pattern (e.g. "nk.exe" or "*.bin").</param>
        /// <param name="maxDepth">Maximum recursion depth (-1 for unlimited).</param>
        /// <returns>Full path of the first matching file, or null if none.</returns>
        public static string? FindFirst(string startPath, string pattern, int maxDepth = 3)
        {
            if (string.IsNullOrEmpty(startPath) || !Directory.Exists(startPath))
                return null;

            // try the top level first (cheap)
            var top = Directory.EnumerateFiles(startPath, pattern, SearchOption.TopDirectoryOnly);            
            foreach (var f in top)
                return f;

            if (maxDepth == 0)
                return null;

            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                MaxRecursionDepth = maxDepth,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
            };

            foreach (var f in Directory.EnumerateFiles(startPath, pattern, options))
                return f;

            return null;
        }

        /// <summary>
        /// Helper that attempts to locate a CE kernel (nk.exe) inside the given
        /// path.  If the path is actually a file it is returned verbatim.
        /// </summary>
        public static string? FindKernelGently(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (File.Exists(path))
                return path;

            if (!Directory.Exists(path))
                return null;

            // look for nk.exe or nk.bin first (U-verse kernels may use .bin)
            var candidate = FindFirst(path, "nk.exe");
            if (!string.IsNullOrEmpty(candidate))
                return candidate;
            candidate = FindFirst(path, "nk.bin");
            if (!string.IsNullOrEmpty(candidate))
                return candidate;

            // fallback to any executable binary
            candidate = FindFirst(path, "*.exe");
            if (!string.IsNullOrEmpty(candidate))
                return candidate;

            // allow finding raw binaries as a last resort
            return FindFirst(path, "*.bin");
        }
    }
}