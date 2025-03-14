using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

class FolderSync
{
    static bool keepRunning = true;
    static void Main(string[] args)
    {
        //Validate command line args
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: FolderSync <source_folder> <replica_folder> <interval> <log_file>");
            return;
        }

        string sourcePath = args[0];
        string replicaPath = args[1];
        int interval;

        //Log file path
        string logFile = @"C:\Temp\sync_log.txt";

        //Validate interval input
        if (!int.TryParse(args[2], out interval) || interval <= 0)
        {
            Console.WriteLine("Error: Interval must be a positive integer (in seconds).");
            return;
        }

        Console.WriteLine($"Starting synchronization every {interval} seconds.");
        LogMessage(logFile, $"Synchronization started. Interval: {interval} seconds.");

        //Listen to user input
        Thread inputThread = new Thread(() =>
        {
            Console.WriteLine("Press [Enter] to close the program.");
            Console.ReadLine();
            keepRunning = false;
        });

        inputThread.Start();

        //While true, it will sync every inputed seconds
        while (keepRunning)
        {
            try
            {
                SyncFolders(sourcePath, replicaPath, logFile);
                Thread.Sleep(interval * 1000); //Waits for the inputed interval
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        Console.WriteLine("Closing Program..");
    }

    static void SyncFolders(string sourcePath, string replicaPath, string logFile)
    {
        //Check if source folder exists
        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"Error: Source folder '{sourcePath}' doesn't exist.");
            LogMessage(logFile, $"Error: Source folder '{sourcePath}' doesn't exist.");
            return;
        }

        //Check if replica folder exists, if it doesn't, it will create it
        if (!Directory.Exists(replicaPath))
        {
            Console.WriteLine($"Replica folder '{replicaPath}' doesn't exist, creating one...");
            LogMessage(logFile, $"Info: Replica folder '{replicaPath}' doesn't exist, created a new one.");
            Directory.CreateDirectory(replicaPath);
        }

        Console.WriteLine($"Synchronizing {DateTime.Now}");
        LogMessage(logFile, $"Synchronization started at {DateTime.Now}");

        //Get files from both folders
        string[] sourceFiles = Directory.GetFiles(sourcePath);
        string[] replicaFiles = Directory.GetFiles(replicaPath);

        //Copy/update files from source folder
        foreach (string file in sourceFiles)
        {
            string fileName = Path.GetFileName(file);
            string replicaFilePath = Path.Combine(replicaPath, fileName);
            if (!File.Exists(replicaFilePath) || File.GetLastWriteTime(file) > File.GetLastWriteTime(replicaFilePath))
            {
                File.Copy(file, replicaFilePath, true); //True to overwrite if it's the same
                Console.WriteLine($"Copied: {fileName}");
                LogMessage(logFile, $"Copied: {fileName}");
            }
        }

        //Update list if there's new items added after last copy
        string[] updatedReplicaFiles = Directory.GetFiles(replicaPath);

        //Delete extra files that don't exist in source folder
        foreach (string file in updatedReplicaFiles)
        {
            string fileName = Path.GetFileName(file);
            string sourceFilePath = Path.Combine(sourcePath, fileName);

            if (!File.Exists(sourceFilePath))
            {
                File.Delete(file);
                Console.WriteLine($"Deleted: {fileName}");
                LogMessage(logFile, $"Deleted: {fileName}");
            }
        }

        Console.WriteLine("Folders are ready for synchronization.");
    }

    static void LogMessage(string logFile, string  message)
    {
        string logEntry = $"{DateTime.Now}: {message}";
        File.AppendAllText(logFile, logEntry + Environment.NewLine);
    }
}