using System;
using System.IO;

namespace VirtualFileSystem2Console
{
    public class Bitmap
    {
        private byte[] bits; 
        private readonly int blockSize;
        private readonly int totalBlocks;

        public Bitmap(int containerSize, int blockSize)
        {
            this.blockSize = blockSize;
            this.totalBlocks = containerSize / blockSize;
            this.bits = new byte[(totalBlocks + 7) / 8]; 
        }

        private bool GetBit(int index)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;
            return (bits[byteIndex] & (1 << bitIndex)) != 0;
        }

        private void SetBit(int index, bool value)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;
            if (value)
                bits[byteIndex] |= (byte)(1 << bitIndex);
            else
                bits[byteIndex] &= (byte)~(1 << bitIndex);
        }

        public int FindFreeBlocks(int requiredBlocks)
        {
            int consecutiveBlocks = 0;
            int startBlock = -1;
            for (int i = 0; i < totalBlocks; i++)
            {
                if (!GetBit(i)) 
                {
                    if (consecutiveBlocks == 0)
                    {
                        startBlock = i;
                    }
                    consecutiveBlocks++;
                    if (consecutiveBlocks == requiredBlocks)
                    {
                        return startBlock;
                    }
                }
                else
                {
                    consecutiveBlocks = 0;
                }
            }
            return -1;
        }

        public void MarkBlocks(int startBlock, int numberOfBlocks, bool isUsed)
        {
            for (int i = startBlock; i < startBlock + numberOfBlocks; i++)
            {
                if (i < totalBlocks)
                {
                    SetBit(i, isUsed); 
                }
            }
        }

        public void SaveBitmap(string containerFileName)
        {
            using (var fs = new FileStream(containerFileName, FileMode.Open, FileAccess.Write))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bits, 0, bits.Length);
            }
        }

        public void LoadBitmap(string containerFileName)
        {
            using (var fs = new FileStream(containerFileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(bits, 0, bits.Length);
            }
        }

        public bool IsBlockFree(int blockNumber)
        {
            return !GetBit(blockNumber);
        }

        public int GetTotalFreeBlocks()
        {
            int freeBlocks = 0;
            for (int i = 0; i < totalBlocks; i++)
            {
                if (!GetBit(i)) freeBlocks++;
            }
            return freeBlocks;
        }
    }
}
