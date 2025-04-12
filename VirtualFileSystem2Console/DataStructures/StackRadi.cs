using System;

namespace VirtualFileSystem2Console.DataStructures
{
    public class StackRadi<T>
    {
        private T[] items;
        private int top;
        private const int DefaultCapacity = 10;

        public StackRadi()
        {
            items = new T[DefaultCapacity];
            top = -1;
        }

        public void Push(T item)
        {
            if (top == items.Length - 1)
            {
                T[] newItems = new T[items.Length * 2];
                for (int i = 0; i < items.Length; i++)
                {
                    newItems[i] = items[i];
                }
                items = newItems;
            }
            items[++top] = item;
        }

        public T Pop()
        {
            if (top < 0)
                throw new InvalidOperationException("Stack is empty");
            return items[top--];
        }

        public T[] ToArray()
        {
            T[] result = new T[top + 1];
            for (int i = 0; i <= top; i++)
            {
                result[i] = items[i];
            }
            return result;
        }
    }
}
