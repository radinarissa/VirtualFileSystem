using System;
using System.IO;


namespace VirtualFileSystem2Console
{
    internal class Program
    {
        private static FileSystem fileSystem = new FileSystem();
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the File System Simulation!");
            if (!File.Exists(FileSystem.ContainerFileName))
            {
                fileSystem.InitializeContainer();
            }
            else
            {
                fileSystem.LoadFileEntries();
            }

            while (true)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Create File");
                Console.WriteLine("2. Read File");
                Console.WriteLine("3. Copy File In (cpin)");
                Console.WriteLine("4. List Files");
                Console.WriteLine("5. Delete File");
                Console.WriteLine("6. Copy File Out (cpout)");
                Console.WriteLine("7. Make Directory (md)");
                Console.WriteLine("8. Change Directory (cd)");
                Console.WriteLine("9. Remove Directory (rd)");
                Console.WriteLine("10. Exit");
                Console.Write("Choose an option: ");

                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        fileSystem.CreateFile();
                        break;
                    case "2":
                        fileSystem.ReadFile();
                        break;
                    case "3":
                        Console.Write("Enter source file path: ");
                        string sourcePath = Console.ReadLine();
                        Console.Write("Enter destination file name: ");
                        string destName = Console.ReadLine();
                        fileSystem.CopyFileIn(sourcePath, destName);
                        break;
                    case "4":
                        fileSystem.Ls();
                        break;
                    case "5":
                        fileSystem.Rm();
                        break;
                    case "6":
                        Console.Write("Enter source file name from container: ");
                        string sourceFileName = Console.ReadLine();
                        Console.Write("Enter destination path: ");
                        string destPath = Console.ReadLine();
                        fileSystem.Cpout(sourceFileName, destPath);
                        break;
                    case "7":
                        Console.Write("Enter directory name: ");
                        string dirName = Console.ReadLine();
                        fileSystem.Md(dirName);
                        break;
                    case "8":
                        Console.Write("Enter path (use \\ for root, .. for parent directory): ");
                        string path = Console.ReadLine();
                        fileSystem.Cd(path);
                        break;
                    case "9":
                        Console.Write("Enter directory name to delete: ");
                        string dirToDelete = Console.ReadLine();
                        fileSystem.Rd(dirToDelete);
                        break;
                    case "10":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }
}
