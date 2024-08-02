using System.Security.Cryptography;

class FolderSync
{
    private static string source;
    private static string replica;
    private static string logFile;
    private static int interval;

    static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Write in cmd line this command: dotnet <ProjectName>.dll <sourcePath> <replicaPath> <intervalInSeconds> <logFilePath>");
            return;
        }

        source = args[0];
        replica = args[1];
        interval = int.Parse(args[2]) * 1000; 
        logFile = args[3];

        CreateDirectory(source, replica);
        Timer timer = new Timer(Synchronization, null, 0, interval);
        Console.ReadLine(); 
    }

    private static void CreateDirectory(string source, string replica)
    {
        try
        {
            string[] sourceDirectory = Directory.GetFiles(source);
            string[] replicaDirectory = Directory.GetFiles(replica);

            foreach (string directory in Directory.GetDirectories(source))
            {
                string directoryPath = directory.Replace(source,replica);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Log($"Created directory: {directoryPath}");
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("The process failed: {0}", e.ToString());
        }
        finally { }
    }

    private static void Synchronization(object state)
    {
        try
        {
            var sourceFiles = Directory.GetFiles(source);
            var replicaFiles = Directory.GetFiles(replica);

            // Copy or update files from source to replica
            foreach (var srcFilePath in sourceFiles)
            {
                var srcPath = srcFilePath.Substring(source.Length + 1);
                var replicaPath = Path.Combine(replica, srcPath);

                bool filesEquals = !File.Exists(replicaPath) || !CompareFiles(srcFilePath, replicaPath);

                if (filesEquals)
                {
                    File.Copy(srcFilePath, replicaPath, true);
                    Log($"The copied file in replica is {replicaPath}");
                }
            }

            // Delete files that are not present in the source directory
            foreach (var replicaFilePath in replicaFiles)
            {
                var filePath = replicaFilePath.Substring(replica.Length + 1);
                var srcPath = Path.Combine(source, filePath);

                if (!File.Exists(srcPath))
                {
                    File.Delete(replicaFilePath);
                    Log($"The replica file named {replicaFilePath} was deleted.");
                }
            }

        }
        catch (Exception ex)
        {
            Log($"Error message: {ex.Message}");
        }
    }

    private static bool CompareFiles(string fileStream1, string fileStream2)
    {
        using var md5Creator = MD5.Create();

        byte[] fileStream1Hash = md5Creator.ComputeHash(File.ReadAllBytes(fileStream1));
        byte[] fileStream2Hash = md5Creator.ComputeHash(File.ReadAllBytes(fileStream2));

        for (int i = 0; i < fileStream1Hash.Length; i++)
        {
            if (fileStream1Hash[i] != fileStream2Hash[i])
            {
                return false;
            }
        }
        return true;
    }

    private static void Log(string message)
    {
        string logMsg = $"{DateTime.Now}: {message}";
        Console.WriteLine(logMsg);
        File.AppendAllText(logFile, logMsg + Environment.NewLine);
    }
}
