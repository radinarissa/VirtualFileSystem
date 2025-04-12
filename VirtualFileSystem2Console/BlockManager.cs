using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualFileSystem2Console
{
    public class BlockManager
    {
        //private BitArray usedBlocks;
        //private readonly int blockSize;
        //private readonly FileStream containerStream;
        //private const int HeaderSize = 11 + sizeof(int) + sizeof(int) + sizeof(long);
        //public BlockManager(FileStream stream, int blockSize)
        //{
        //    this.containerStream = stream;
        //    this.blockSize = blockSize;

        //    // Инициализираме с минимален брой блокове
        //    int initialBlocks = Math.Max(100, (int)(stream.Length / blockSize) + 1);
        //    this.usedBlocks = new BitArray(initialBlocks);

        //    // Разширяваме файла до минималния размер
        //    long minSize = (long)initialBlocks * blockSize;
        //    if (stream.Length < minSize)
        //    {
        //        stream.SetLength(minSize);
        //    }

        //    InitializeBlockMap();
        //}

        //private void InitializeBlockMap()
        //{
        //    // Маркираме блоковете за хедъра
        //    int headerBlocks = (HeaderSize + blockSize - 1) / blockSize;
        //    for (int i = 0; i < headerBlocks; i++)
        //    {
        //        usedBlocks[i] = true;
        //    }

        //    // Четем съществуващите файлове
        //    using (var reader = new BinaryReader(containerStream, Encoding.UTF8, true))
        //    {
        //        reader.BaseStream.Position = 19;
        //        long firstFilePointer = reader.ReadInt64();

        //        if (firstFilePointer > 0)
        //        {
        //            MarkFileBlocks(firstFilePointer, reader);
        //        }
        //    }
        //}

        //private void EnsureCapacity(int blocksNeeded)
        //{
        //    // Проверяваме дали имаме достатъчно място
        //    if (usedBlocks.Length < blocksNeeded)
        //    {
        //        // Създаваме нов масив с двойно по-голям размер
        //        int newSize = Math.Max(usedBlocks.Length * 2, blocksNeeded + 100);
        //        var newArray = new BitArray(newSize);

        //        // Копираме старите стойности
        //        for (int i = 0; i < usedBlocks.Length; i++)
        //        {
        //            newArray[i] = usedBlocks[i];
        //        }

        //        usedBlocks = newArray;

        //        // Разширяваме файла
        //        long newFileSize = (long)newSize * blockSize;
        //        containerStream.SetLength(newFileSize);
        //    }
        //}

        //public long AllocateBlocks(long size)
        //{
        //    // Изчисляваме нужните блокове
        //    int blocksNeeded = (int)((size + blockSize - 1) / blockSize);

        //    // Осигуряваме достатъчно място
        //    EnsureCapacity(blocksNeeded + 10); // Добавяме буфер

        //    // Търсим свободни последователни блокове
        //    long startBlock = FindConsecutiveFreeBlocks(blocksNeeded);

        //    if (startBlock < 0)
        //    {
        //        // Ако не намерим място, разширяваме и опитваме отново
        //        EnsureCapacity(usedBlocks.Length + blocksNeeded);
        //        startBlock = FindConsecutiveFreeBlocks(blocksNeeded);

        //        if (startBlock < 0)
        //        {
        //            throw new IOException($"Not enough consecutive free blocks available. Need {blocksNeeded} blocks");
        //        }
        //    }

        //    // Маркираме блоковете като заети
        //    for (int i = 0; i < blocksNeeded; i++)
        //    {
        //        usedBlocks[(int)startBlock + i] = true;
        //    }

        //    return (startBlock * blockSize) + HeaderSize;
        //}
        //private long FindConsecutiveFreeBlocks(int blocksNeeded)
        //{
        //    // Проверка за валиден вход
        //    if (blocksNeeded <= 0)
        //        throw new ArgumentException("Blocks needed must be positive", nameof(blocksNeeded));

        //    // Пропускаме първите блокове заети от хедъра
        //    int headerBlocks = (HeaderSize + blockSize - 1) / blockSize;

        //    // Променливи за проследяване на последователни свободни блокове
        //    int consecutiveFree = 0;
        //    long potentialStart = -1;

        //    // Търсим в целия масив от блокове
        //    for (int i = headerBlocks; i < usedBlocks.Length; i++)
        //    {
        //        if (!usedBlocks[i]) // Ако блокът е свободен
        //        {
        //            if (consecutiveFree == 0) // Ако това е първият свободен блок
        //            {
        //                potentialStart = i;
        //            }
        //            consecutiveFree++;

        //            if (consecutiveFree >= blocksNeeded)
        //            {
        //                return potentialStart; // Намерихме достатъчно последователни блокове
        //            }
        //        }
        //        else // Ако блокът е зает
        //        {
        //            consecutiveFree = 0;
        //            potentialStart = -1;
        //        }
        //    }

        //    // Не намерихме достатъчно последователни свободни блокове
        //    return -1;
        //}
        //private void MarkFileBlocks(long startPosition, BinaryReader reader)
        //{
        //    if (startPosition <= 0) return;

        //    reader.BaseStream.Position = startPosition;
        //    var metadata = FileMetadata.ReadFrom(reader);

        //    // Изчисляваме кои блокове заема файла
        //    long startBlock = (metadata.DataOffset - HeaderSize) / blockSize;
        //    long metadataBlocks = (FileMetadata.Size + blockSize - 1) / blockSize;
        //    long dataBlocks = (metadata.FileSize + blockSize - 1) / blockSize;

        //    // Маркираме блоковете за метаданните
        //    for (long i = startBlock - metadataBlocks; i < startBlock; i++)
        //    {
        //        if (i >= 0 && i < usedBlocks.Length)
        //            usedBlocks[(int)i] = true;
        //    }

        //    // Маркираме блоковете за данните
        //    for (long i = startBlock; i < startBlock + dataBlocks; i++)
        //    {
        //        if (i >= 0 && i < usedBlocks.Length)
        //            usedBlocks[(int)i] = true;
        //    }

        //    if (metadata.NextBlock > 0)
        //    {
        //        MarkFileBlocks(metadata.NextBlock, reader);
        //    }
        //}

        //public void FreeBlocks(long position, long size)
        //{
        //    try
        //    {
        //        // Изчисляваме реалната позиция без хедъра
        //        position = Math.Max(0, position - HeaderSize);

        //        // Изчисляваме кои блокове трябва да освободим
        //        long startBlock = position / blockSize;
        //        int blocksToFree = (int)((size + blockSize - 1) / blockSize);

        //        // Освобождаваме блоковете
        //        for (int i = 0; i < blocksToFree; i++)
        //        {
        //            int blockIndex = (int)startBlock + i;
        //            if (blockIndex >= 0 && blockIndex < usedBlocks.Length)
        //            {
        //                if (usedBlocks[blockIndex]) // Проверяваме дали блокът е бил зает
        //                {
        //                    usedBlocks[blockIndex] = false; // Маркираме блока като свободен
        //                }
        //            }
        //        }

        //        // Опционално - може да намалим размера на файла ако освобождаваме блокове в края
        //        TryCompactContainer(startBlock + blocksToFree);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new IOException($"Error freeing blocks at position {position}: {ex.Message}", ex);
        //    }
        //}

        //private void TryCompactContainer(long lastUsedBlock)
        //{
        //    // Проверяваме дали има свободни блокове в края
        //    long lastBlock = lastUsedBlock;
        //    while (lastBlock > 0 && lastBlock < usedBlocks.Length && !usedBlocks[(int)lastBlock])
        //    {
        //        lastBlock--;
        //    }

        //    // Ако има значителен брой свободни блокове в края (например повече от 10),
        //    // можем да намалим размера на файла
        //    if (lastBlock + 1 < usedBlocks.Length - 10)
        //    {
        //        long newLength = (lastBlock + 2) * blockSize + HeaderSize; // +2 за буфер
        //        containerStream.SetLength(newLength);

        //        // Актуализираме размера на BitArray
        //        var newArray = new BitArray((int)(lastBlock + 2));
        //        for (int i = 0; i <= lastBlock; i++)
        //        {
        //            newArray[i] = usedBlocks[i];
        //        }
        //        usedBlocks = newArray;
        //    }
        //}
    }
}
