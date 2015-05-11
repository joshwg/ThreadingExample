using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    class Node<T>
    {
        public T Value { get; set; }
        public Node<T> Next;
        public Node(T value, Node<T> next) {
            this.Value = value;
            this.Next = next;
        }
    }

    class LinkedList<T>
    {
        private Node<T> first = null;
        public void Insert(T value)
        {
            Node<T> newNode = new Node<T>(value, first);
            first.Next = newNode;
        }
    }
}
