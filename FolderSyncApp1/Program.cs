using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography;


namespace FolderSyncApp1
{
    class Program
    {
        static void ParseArguments(string[] args, out string sourceFolder, out string replicaFolder, out int interval, out string logFile)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: FolderSyncApp <sourceFolder> <replicaFolder> <interval> <logFile>");
                Environment.Exit(1);
            }

            sourceFolder = args[0];
            replicaFolder = args[1];
            interval = int.Parse(args[2]);
            logFile = args[3];
        }

        static void SetupLogging(string logFile)
        {
            Console.WriteLine($"Logging to {logFile}");
            string logDirectory = Path.GetDirectoryName(logFile);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            if (!File.Exists(logFile))
            {
                using (StreamWriter sw = File.CreateText(logFile))
                {
                    sw.WriteLine($"Log started at {DateTime.Now}");
                }
            }
        }

        static void LogOperation(string message, string logFile)
        {
            Console.WriteLine(message);
            using (StreamWriter sw = File.AppendText(logFile))
            {
                sw.WriteLine($"{DateTime.Now} - {message}");
            }
        }

        static void CreateFolderWithSampleFiles(string folderPath, string logFile)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                LogOperation($"Created folder: {folderPath}", logFile);

                // Add sample files to the newly created folder
                for (int i = 1; i <= 3; i++)
                {
                    string sampleFile = Path.Combine(folderPath, $"SampleFile{i}.txt");
                    File.WriteAllText(sampleFile, $"This is sample file {i}.");
                    LogOperation($"Created sample file: {sampleFile}", logFile);
                }
            }
        }
        static string GetFileChecksum(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        static bool FileEquals(string file1, string file2)
        {
            string hash1 = GetFileChecksum(file1);
            string hash2 = GetFileChecksum(file2);

            return hash1 == hash2;
        }
        static void SyncFolders(string source, string replica, string logFile)
        {
            try
            {
                var sourceFiles = Directory.GetFiles(source);
                var replicaFiles = Directory.GetFiles(replica);

                // Copy files from source to replica
                foreach (var sourceFile in sourceFiles)
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string replicaFile = Path.Combine(replica, fileName);

                    if (!File.Exists(replicaFile) || !FileEquals(sourceFile, replicaFile))
                    {
                        File.Copy(sourceFile, replicaFile, true);
                        LogOperation($"Copied {fileName} from source to replica.", logFile);
                    }
                }

                // Remove extra files in replica
                foreach (var replicaFile in replicaFiles)
                {
                    string fileName = Path.GetFileName(replicaFile);
                    string sourceFile = Path.Combine(source, fileName);

                    if (!File.Exists(sourceFile))
                    {
                        File.Delete(replicaFile);
                        LogOperation($"Removed {fileName} from replica.", logFile);
                    }
                }
            }
            catch (Exception ex)
            {
                LogOperation($"Error: {ex.Message}", logFile);
            }
        }

        static void Main(string[] args)
        {
            ParseArguments(args, out string sourceFolder, out string replicaFolder, out int interval, out string logFile);
            SetupLogging(logFile);

            // Automatically create folders and sample files if missing
            CreateFolderWithSampleFiles(sourceFolder, logFile);
            CreateFolderWithSampleFiles(replicaFolder, logFile);

            while (true)
            {
                LogOperation($"Starting synchronization of {sourceFolder} and {replicaFolder}.", logFile);
                SyncFolders(sourceFolder, replicaFolder, logFile);
                LogOperation("Synchronization complete.", logFile);

                Thread.Sleep(interval * 1000); // Perform synchronization periodically
            }
        }
    }
}






















