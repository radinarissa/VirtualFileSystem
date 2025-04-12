using System;

namespace VirtualFileSystem2Console
{
    public class LinkedListRadi<T>
    {
        private class Node
        {
            public T Data { get; set; }
            public Node Next { get; set; }

            public Node(T data)
            {
                Data = data;
                Next = null;
            }
        }

        private Node head;
        private int count;

        public LinkedListRadi()
        {
            head = null;
            count = 0;
        }

        public int Count => count;

        public void AddFirst(T data)
        {
            Node newNode = new Node(data);
            newNode.Next = head;
            head = newNode;
            count++;
        }

        public void AddLast(T data)
        {
            Node newNode = new Node(data);

            if (head == null)
            {
                head = newNode;
            }
            else
            {
                Node current = head;
                while (current.Next != null)
                {
                    current = current.Next;
                }
                current.Next = newNode;
            }
            count++;
        }

        public bool Remove(T data)
        {
            if (head == null)
                return false;

            if (AreEqual(head.Data, data))
            {
                head = head.Next;
                count--;
                return true;
            }

            Node current = head;
            while (current.Next != null)
            {
                if (AreEqual(current.Next.Data, data))
                {
                    current.Next = current.Next.Next;
                    count--;
                    return true;
                }
                current = current.Next;
            }

            return false;
        }

        public bool Contains(T data)
        {
            Node current = head;
            while (current != null)
            {
                if (AreEqual(current.Data, data))
                    return true;
                current = current.Next;
            }
            return false;
        }

        public void Clear()
        {
            head = null;
            count = 0;
        }

        public T[] GetAllItems()
        {
            T[] items = new T[count];
            Node current = head;
            int index = 0;

            while (current != null)
            {
                items[index++] = current.Data;
                current = current.Next;
            }

            return items;
        }

        public void AddAfter(T existingData, T newData)
        {
            Node current = head;
            while (current != null)
            {
                if (AreEqual(current.Data, existingData))
                {
                    Node newNode = new Node(newData);
                    newNode.Next = current.Next;
                    current.Next = newNode;
                    count++;
                    return;
                }
                current = current.Next;
            }
            throw new ArgumentException("The specified node was not found in the list.");
        }

        public void AddBefore(T existingData, T newData)
        {
            if (head == null)
                throw new ArgumentException("The list is empty.");

            if (AreEqual(head.Data, existingData))
            {
                AddFirst(newData);
                return;
            }

            Node current = head;
            while (current.Next != null)
            {
                if (AreEqual(current.Next.Data, existingData))
                {
                    Node newNode = new Node(newData);
                    newNode.Next = current.Next;
                    current.Next = newNode;
                    count++;
                    return;
                }
                current = current.Next;
            }
            throw new ArgumentException("The specified node was not found in the list.");
        }

        public T First(Func<T, bool> predicate)
        {
            Node current = head;
            while (current != null)
            {
                if (predicate(current.Data))
                    return current.Data;
                current = current.Next;
            }
            return default(T);
        }

        public T Last
        {
            get
            {
                if (head == null)
                    throw new InvalidOperationException("The list is empty.");

                Node current = head;
                while (current.Next != null)
                {
                    current = current.Next;
                }
                return current.Data;
            }
        }

        private bool AreEqual(T x, T y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Equals(y);
        }

        public void ForEach(Action<T> action)
        {
            Node current = head;
            while (current != null)
            {
                action(current.Data);
                current = current.Next;
            }
        }
    }
}