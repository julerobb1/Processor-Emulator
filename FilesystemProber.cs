using System;
using System.IO;

namespace ProcessorEmulator.Tools
{
    public class FilesystemProber
    {
        public static void ProbeDrive(string drivePath)
        {
            Console.WriteLine($"Probing drive: {drivePath}");

            if (!Directory.Exists(drivePath))
            {
                Console.WriteLine("Drive path does not exist.");
                return;
            }

            foreach (var file in Directory.GetFiles(drivePath))
            {
                Console.WriteLine($"Found file: {file}");
                ProbeFile(file);
            }
        }

        public static void ProbeFile(string filePath)
        {
            Console.WriteLine($"Probing file: {filePath}");

            byte[] fileData = File.ReadAllBytes(filePath);

            if (fileData.Length > 0 && fileData[0] == 0x7F && fileData[1] == 'E' && fileData[2] == 'L' && fileData[3] == 'F')
            {
                Console.WriteLine("Detected ELF binary.");
            }
            else if (fileData.Length > 0 && fileData[0] == 0x42 && fileData[1] == 0x5A)
            {
                Console.WriteLine("Detected BZ2 compressed file.");
            }
            else
            {
                Console.WriteLine("Unknown file type.");
            }
        }
    }
}