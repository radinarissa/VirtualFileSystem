using System.IO;

namespace VirtualFileSystem2Console
{
    public struct FileMetadata
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public long Offset { get; set; }
        public long NextOffset { get; set; }  
        public bool IsDeleted { get; set; }
        public bool IsDirectory { get; set; }  
        public long ParentOffset { get; set; } 


        public override bool Equals(object obj)
        {
            if (obj is FileMetadata other)
            {
                return Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Name)) return 0;

            int hash = 0;
            for (int i = 0; i < Name.Length; i++)
            {
                hash = 31 * hash + Name[i];
            }
            return hash;
        }
    }
}
