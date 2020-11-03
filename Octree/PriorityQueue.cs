using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octree
{
    internal class PriorityQueueNode<T>
    {
        internal T value;
        internal float priority;
        internal PriorityQueueNode<T> prev;
        internal PriorityQueueNode<T> next;

        public PriorityQueueNode(T value, float priority)
        {
            this.value = value;
            this.priority = priority;
        }

        public override string ToString()
        {
            return $"value: {value}, priority { priority }";
        }
    }

    /// <summary>
    /// Priority Queue implementation, used for KNN searches
    /// </summary>
    /// <typeparam name="T">The type of data the queue holds</typeparam>
    public class PriorityQueue<T>
    {
        PriorityQueueNode<T> First;

        /// <summary>
        /// The number of items in the queue
        /// </summary>
        public int Count { get; private set; }

        public PriorityQueue()
        {
            First = null;
        }
        
        /// <summary>
        /// Enqueue the given value by priority
        /// </summary>
        /// <param name="val"></param>
        /// <param name="priority"></param>
        public void Enqueue(T val, float priority)
        {
            var node = new PriorityQueueNode<T>(val, priority);
            if (First == null)
            {
                this.First = node;
            }
            else
            {
                if (this.First.priority > priority)
                {
                    node.next = this.First;
                    this.First.prev = node;
                    this.First = node;
                }
                else
                {
                    var n = this.First;
                    var end = false;
                    while (n.priority < node.priority)
                    {                     
                        if (n.next == null)
                        {
                            end = true;
                            break;
                        }
                        n = n.next;
                    }
                    if (!end)
                    {
                        if (n.prev != null)
                        {
                            n.prev.next = node;
                            node.prev = n.prev;
                        }
                        n.prev = node;
                        node.next = n;
                    }
                    else
                    {
                        n.next = node;
                        node.prev = n;
                    }
                }
            }

            Count++;
        }

        /// <summary>
        /// Remove the first item from the queue and return it
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            var n = this.First;
            this.First = this.First.next;
            Count--;
            return n.value;
        }

    }
}
