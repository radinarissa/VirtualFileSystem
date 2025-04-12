using System;
using System.IO;
using System.Text;
using VirtualFileSystem2Console.DataStructures;

namespace VirtualFileSystem2Console
{
    public class FileSystem
    {
        public const string ContainerFileName = "container.bin";
        public const int ContainerSize = 1024 * 1024; 
        private const int MetadataSize = 64 * 1024; 
        private const int MaxFileSize = ContainerSize - MetadataSize; 
        private LinkedListRadi<FileMetadata> fileEntries = new LinkedListRadi<FileMetadata>();
        private const int BlockSize = 4096; 
        private Bitmap bitmap;
        private long currentDirectoryOffset;

        
        private bool ContainsChar(string text, char search)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == search)
                    return true;
            }
            return false;
        }

        private FileMetadata FindFileByName(string fileName)
        {
            FileMetadata[] entries = fileEntries.GetAllItems();
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Name == fileName && !entries[i].IsDeleted &&
                   entries[i].ParentOffset == currentDirectoryOffset)
                    return entries[i];
            }
            return default(FileMetadata);
        }

        private FileMetadata FindDirectoryByOffset(long offset)
        {
            FileMetadata[] entries = fileEntries.GetAllItems();
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Offset == offset && entries[i].IsDirectory && !entries[i].IsDeleted)
                    return entries[i];
            }
            return default(FileMetadata);
        }

        private bool IsEmpty(string str)
        {
            return str == null || str.Length == 0;
        }

        public FileSystem()
        {
            fileEntries = new LinkedListRadi<FileMetadata>();
            currentDirectoryOffset = 0; 
            bitmap = new Bitmap(ContainerSize, BlockSize); 
        }
        public void InitializeContainer()
        {
            using (var fs = new FileStream(ContainerFileName, FileMode.Create, FileAccess.Write))
            {
                fs.SetLength(ContainerSize);
                byte[] emptyData = new byte[ContainerSize];
                for (int i = 0; i < ContainerSize; i++)
                {
                    emptyData[i] = 0;
                }
                fs.Write(emptyData, 0, ContainerSize);
            }

            bitmap = new Bitmap(ContainerSize, BlockSize);
            int metadataBlocks = (MetadataSize + BlockSize - 1) / BlockSize;
            bitmap.MarkBlocks(0, metadataBlocks, true);
            bitmap.SaveBitmap(ContainerFileName);

            Console.WriteLine("Container initialized with bitmap allocation.");
        }

        private long AllocateBlocks(long size)
        {
            int requiredBlocks = (int)((size + BlockSize - 1) / BlockSize);
            int startBlock = bitmap.FindFreeBlocks(requiredBlocks);

            if (startBlock == -1)
                return -1;

            bitmap.MarkBlocks(startBlock, requiredBlocks, true);
            bitmap.SaveBitmap(ContainerFileName);

            return startBlock * BlockSize;
        }

        private void FreeBlocks(long offset, long size)
        {
            int startBlock = (int)(offset / BlockSize);
            int numberOfBlocks = (int)((size + BlockSize - 1) / BlockSize);

            bitmap.MarkBlocks(startBlock, numberOfBlocks, false);
            bitmap.SaveBitmap(ContainerFileName);
        }
        public void CreateFile()
        {
            Console.Write("Enter file name: ");
            string fileName = Console.ReadLine();

            if (fileEntries.Contains(new FileMetadata { Name = fileName }))
            {
                Console.WriteLine("File with this name already exists.");
                return;
            }

            Console.Write("Enter file content: ");
            string content = Console.ReadLine();

            byte[] fileData = StringToBytes(content);
            if (fileData.Length > MaxFileSize)
            {
                Console.WriteLine("File is too large for the container.");
                return;
            } 

            long fileOffset = AllocateBlocks(fileData.Length);
            if (fileOffset == -1)
            {
                Console.WriteLine("Not enough continuous space in container.");
                return;
            }

            try
            {
                using (var fs = new FileStream(ContainerFileName, FileMode.Open, FileAccess.Write))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.BaseStream.Position = fileOffset;
                    bw.Write(fileData);
                }

                var metadata = new FileMetadata
                {
                    Name = fileName,
                    Size = fileData.Length,
                    Offset = fileOffset,
                    NextOffset = -1,
                    IsDeleted = false,
                    ParentOffset = currentDirectoryOffset
                };

                fileEntries.AddLast(metadata);
                SaveFileEntries();

                Console.WriteLine("File created successfully.");
            }
            catch (Exception ex)
            {
                FreeBlocks(fileOffset, fileData.Length);
                Console.WriteLine($"Error creating file: {ex.Message}");
            }
        }

        public void ReadFile()
        {
            Console.Write("Enter file name to read: ");
            string fileName = Console.ReadLine();

            var entry = FindFileByName(fileName);
            if (entry.Equals(default(FileMetadata)))
            {
                Console.WriteLine("File not found.");
                return;
            }
            if (entry.IsDirectory)
            {
                Console.WriteLine("Cannot read a directory. Use ls instead.");
                return;
            }

            try
            {
                using (var fs = new FileStream(ContainerFileName, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    br.BaseStream.Position = entry.Offset;
                    byte[] data = br.ReadBytes((int)entry.Size);
                    string content = BytesToString(data);
                    Console.WriteLine($"File content: {content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        public void CopyFileIn(string sourcePath, string destName)
        {
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine("Source file does not exist.");
                return;
            }

            if (fileEntries.Contains(new FileMetadata { Name = destName }))
            {
                Console.WriteLine("File with this name already exists.");
                return;
            }

            byte[] fileData = File.ReadAllBytes(sourcePath);
            if (fileData.Length > MaxFileSize)
            {
                Console.WriteLine("File is too large for the container.");
                return;
            }

            long fileOffset = AllocateBlocks(fileData.Length);
            if (fileOffset == -1)
            {
                Console.WriteLine("Not enough continuous space in container.");
                return;
            }

            try
            {
                using (var fs = new FileStream(ContainerFileName, FileMode.Open, FileAccess.Write))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.BaseStream.Position = fileOffset;
                    bw.Write(fileData);
                }

                var metadata = new FileMetadata
                {
                    Name = destName,
                    Size = fileData.Length,
                    Offset = fileOffset,
                    NextOffset = -1,
                    IsDeleted = false
                };

                fileEntries.AddLast(metadata);
                SaveFileEntries();

                Console.WriteLine($"File copied successfully as {destName}");
            }
            catch (Exception ex)
            {
                FreeBlocks(fileOffset, fileData.Length); 
                Console.WriteLine($"Error copying file: {ex.Message}");
            }
        }
        private void SaveFileEntries()
        {
            using (var fs = new FileStream(ContainerFileName, FileMode.Open, FileAccess.Write))
            {
                fs.Seek(0, SeekOrigin.Begin);
                byte[] emptyMetadata = new byte[MetadataSize];
                fs.Write(emptyMetadata, 0, MetadataSize);
            }

            using (var fs = new FileStream(ContainerFileName, FileMode.Open, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Seek(0, SeekOrigin.Begin);
                FileMetadata[] entries = fileEntries.GetAllItems();
                for (int i = 0; i < entries.Length; i++)
                {
                    writer.Write(entries[i].Name);
                    writer.Write(entries[i].Size);
                    writer.Write(entries[i].Offset);
                    writer.Write(entries[i].NextOffset);
                    writer.Write(entries[i].IsDeleted);
                    writer.Write(entries[i].IsDirectory);
                    writer.Write(entries[i].ParentOffset);
                }
            }
        }

        public void LoadFileEntries()
        {
            fileEntries.Clear();
            bitmap.LoadBitmap(ContainerFileName);

            using (var fs = new FileStream(ContainerFileName, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                while (fs.Position < MetadataSize)
                {
                    try
                    {
                        var entry = new FileMetadata
                        {
                            Name = reader.ReadString(),
                            Size = reader.ReadInt64(),
                            Offset = reader.ReadInt64(),
                            NextOffset = reader.ReadInt64(),
                            IsDeleted = reader.ReadBoolean(),
                            IsDirectory = reader.ReadBoolean(),
                            ParentOffset = reader.ReadInt64()
                        };

                        if (!string.IsNullOrEmpty(entry.Name) && !entry.IsDeleted)
                        {
                            fileEntries.AddLast(entry);
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }
                }
            }
        }

        public void Rm()
        {
            Console.Write("Enter file name to delete: ");
            string fileName = Console.ReadLine();

            var entry = FindFileByName(fileName);
            if (entry.Equals(default(FileMetadata)))
            {
                Console.WriteLine("File not found.");
                return;
            }
            if (entry.IsDirectory)
            {
                Console.WriteLine("Cannot delete directory with rm. Use rd instead.");
                return;
            }
            try
            {
                FreeBlocks(entry.Offset, entry.Size);
                fileEntries.Remove(entry);
                SaveFileEntries();

                Console.WriteLine("File deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        public void Ls()
        {
            if (fileEntries.Count == 0)
            {
                Console.WriteLine("No files or directories in container.");
                return;
            }

            Console.WriteLine($"Current path: {GetCurrentPath()}");
            Console.WriteLine("Contents:");

            bool hasContents = false;
            FileMetadata[] entries = fileEntries.GetAllItems();

            for (int i = 0; i < entries.Length; i++)
            {
                if (!entries[i].IsDeleted && entries[i].ParentOffset == currentDirectoryOffset && entries[i].IsDirectory)
                {
                    hasContents = true;
                    Console.WriteLine($"[DIR] {entries[i].Name}");
                }
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (!entries[i].IsDeleted && entries[i].ParentOffset == currentDirectoryOffset && !entries[i].IsDirectory)
                {
                    hasContents = true;
                    Console.WriteLine($"      {entries[i].Name} - {entries[i].Size} bytes");
                }
            }

            if (!hasContents)
            {
                Console.WriteLine("(empty)");
            }
        }

        public void Cpout(string sourceName, string destPath)
        {
            var entry = FindFileByName(sourceName);
            if (entry.Equals(default(FileMetadata)))
            {
                Console.WriteLine("Source file not found in container.");
                return;
            }

            try
            {
                using (var containerFs = new FileStream(ContainerFileName, FileMode.Open, FileAccess.Read))
                using (var destFs = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                using (var br = new BinaryReader(containerFs))
                using (var bw = new BinaryWriter(destFs))
                {
                    br.BaseStream.Position = entry.Offset;
                    byte[] data = br.ReadBytes((int)entry.Size);
                    bw.Write(data);
                }

                Console.WriteLine("File copied out successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file out: {ex.Message}");
            }
        }
        private bool IsNullOrEmpty(string value)
        {
            return value == null || value.Length == 0;
        }
        private bool IsValidDirectoryName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            return !ContainsChar(name, '\\') &&
                   !ContainsChar(name, '/') &&
                   !ContainsChar(name, ':');
        }
        public void Md(string directoryName)
        {
            if (IsEmpty(directoryName))
            {
                Console.WriteLine("Directory name cannot be empty.");
                return;
            }
            if (!IsValidDirectoryName(directoryName))
            {
                Console.WriteLine("Invalid directory name.");
                return;
            }

            FileMetadata existing = FindFileByName(directoryName);
            if (!existing.Equals(default(FileMetadata)))
            {
                Console.WriteLine("Directory with this name already exists.");
                return;
            }

            if (!existing.Equals(default(FileMetadata)))
            {
                Console.WriteLine("Directory with this name already exists.");
                return;
            }

            try
            {
                long dirOffset = AllocateBlocks(BlockSize);
                if (dirOffset == -1)
                {
                    Console.WriteLine("Not enough space for new directory.");
                    return;
                }

                var directoryMetadata = new FileMetadata
                {
                    Name = directoryName,
                    Size = BlockSize,  
                    Offset = dirOffset,
                    NextOffset = -1,
                    IsDeleted = false,
                    IsDirectory = true,
                    ParentOffset = currentDirectoryOffset
                };

                fileEntries.AddLast(directoryMetadata);
                SaveFileEntries();

                Console.WriteLine($"Directory {directoryName} created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory: {ex.Message}");
            }
        }
        private string GetCurrentPath()
        {
            if (currentDirectoryOffset == 0)
            {
                return "\\";
            }

            StackRadi<string> pathParts = new StackRadi<string>();
            long currentOffset = currentDirectoryOffset;

            while (currentOffset != 0)
            {
                FileMetadata currentDir = FindDirectoryByOffset(currentOffset);
                if (!currentDir.Equals(default(FileMetadata)))
                {
                    pathParts.Push(currentDir.Name);
                    currentOffset = currentDir.ParentOffset;
                }
                else
                {
                    break;
                }
            }

            string path = "\\";
            string[] parts = pathParts.ToArray();
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                path = path + parts[i];
                if (i > 0)
                {
                    path = path + "\\";
                }
            }

            return path;
        }
        public void Cd(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Path cannot be empty.");
                return;
            }

            if (path == "\\")
            {
                currentDirectoryOffset = 0;
                Console.WriteLine("Changed to root directory.");
                return;
            }

            if (path == "..")
            {
                if (currentDirectoryOffset == 0)
                {
                    Console.WriteLine("Already in root directory.");
                    return;
                }

                FileMetadata currentDir = default(FileMetadata);
                FileMetadata[] entries = fileEntries.GetAllItems();
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Offset == currentDirectoryOffset && entries[i].IsDirectory)
                    {
                        currentDir = entries[i];
                        break;
                    }
                }

                if (!currentDir.Equals(default(FileMetadata)))
                {
                    currentDirectoryOffset = currentDir.ParentOffset;
                    Console.WriteLine($"Changed to parent directory: {GetCurrentPath()}");
                }
                return;
            }

            FileMetadata targetDir = default(FileMetadata);
            FileMetadata[] allEntries = fileEntries.GetAllItems();
            for (int i = 0; i < allEntries.Length; i++)
            {
                if (allEntries[i].Name == path &&
                    allEntries[i].IsDirectory &&
                    allEntries[i].ParentOffset == currentDirectoryOffset)
                {
                    targetDir = allEntries[i];
                    break;
                }
            }

            if (targetDir.Equals(default(FileMetadata)))
            {
                Console.WriteLine($"Directory '{path}' not found in current directory.");
                return;
            }

            currentDirectoryOffset = targetDir.Offset;
            Console.WriteLine($"Changed to directory: {GetCurrentPath()}");
        }

        public void Rd(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                Console.WriteLine("Directory name cannot be empty.");
                return;
            }

            FileMetadata dirToDelete = default(FileMetadata);
            FileMetadata[] entries = fileEntries.GetAllItems();
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Name == directoryName &&
                    entries[i].IsDirectory &&
                    entries[i].ParentOffset == currentDirectoryOffset)
                {
                    dirToDelete = entries[i];
                    break;
                }
            }

            if (dirToDelete.Equals(default(FileMetadata)))
            {
                Console.WriteLine($"Directory '{directoryName}' not found in current directory.");
                return;
            }

            try
            {
                DeleteDirectoryContents(dirToDelete.Offset);
                FreeBlocks(dirToDelete.Offset, dirToDelete.Size);
                fileEntries.Remove(dirToDelete);
                SaveFileEntries();

                Console.WriteLine($"Directory '{directoryName}' and its contents deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting directory: {ex.Message}");
            }
        }

        private void DeleteDirectoryContents(long directoryOffset)
        {
            ListRadi<FileMetadata> contentsToDelete = new ListRadi<FileMetadata>();
            FileMetadata[] entries = fileEntries.GetAllItems();

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].ParentOffset == directoryOffset)
                {
                    contentsToDelete.Add(entries[i]);
                }
            }

            FileMetadata[] contents = contentsToDelete.ToArray();
            for (int i = 0; i < contents.Length; i++)
            {
                if (!contents[i].IsDirectory)
                {
                    FreeBlocks(contents[i].Offset, contents[i].Size);
                }
                if (contents[i].IsDirectory)
                {
                    DeleteDirectoryContents(contents[i].Offset);
                }
                fileEntries.Remove(contents[i]);
            }
        }
        private byte[] StringToBytes(string content)
        {
            if (content == null)
                return new byte[0];

            byte[] bytes = new byte[content.Length * 2];
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                bytes[i * 2] = (byte)(c >> 8);    
                bytes[i * 2 + 1] = (byte)(c & 0xFF); 
            }
            return bytes;
        }

        private string BytesToString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return "";

            char[] chars = new char[bytes.Length / 2];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)((bytes[i * 2] << 8) | bytes[i * 2 + 1]);
            }

            string result = "";
            for (int i = 0; i < chars.Length; i++)
            {
                result += chars[i];
            }
            return result;
        }
    }
}
