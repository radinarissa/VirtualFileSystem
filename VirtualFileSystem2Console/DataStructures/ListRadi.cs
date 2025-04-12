using System;

namespace VirtualFileSystem2Console.DataStructures
{
    public class ListRadi<T>
    {
        private T[] items;
        private int count;
        private const int DefaultCapacity = 4;

        public ListRadi()
        {
            items = new T[DefaultCapacity];
            count = 0;
        }

        public void Add(T item)
        {
            if (count == items.Length)
            {
                T[] newItems = new T[items.Length * 2];
                for (int i = 0; i < items.Length; i++)
                {
                    newItems[i] = items[i];
                }
                items = newItems;
            }
            items[count++] = item;
        }

        public T[] ToArray()
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = items[i];
            }
            return result;
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < count; i++)
            {
                action(items[i]);
            }
        }
    }
}
