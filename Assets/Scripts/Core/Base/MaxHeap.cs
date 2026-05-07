using System.Collections.Generic;
using System;

public class MaxHeap<T> where T : IComparable<T>
{
    private List<T> m_Heap = new List<T>();

    public int count => m_Heap.Count;
    public bool isEmpty => m_Heap.Count == 0;

    public void Enqueue(T item)
    {
        m_Heap.Add(item);
        HeapifyUp(m_Heap.Count - 1);
    }    

    public T Dequeue()
    {
        if (isEmpty) throw new InvalidOperationException("Heap is empty");

        T max = m_Heap[0];
        m_Heap[0] = m_Heap[m_Heap.Count - 1];
        m_Heap.RemoveAt(m_Heap.Count - 1);

        if (!isEmpty)
            HeapifyDown(0);

        return max;
    }

    public bool TryDequeue(out T result)
    {
        if (isEmpty)
        {
            result = default;
            return false;
        }

        result = Dequeue();
        return true;
    }

    public T Peek()
    {
        if (isEmpty) throw new InvalidOperationException("Heap is empty");
        return m_Heap[0];
    }

    public void Clear() => m_Heap.Clear();

    public bool Contains(T item) => m_Heap.Contains(item);

    public T[] ToArray() => m_Heap.ToArray();

    public List<T> ToList() => new List<T>(m_Heap);

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (m_Heap[index].CompareTo(m_Heap[parent]) > 0)
            {
                Swap(index, parent);
                index = parent;
            }
            else
            {
                break;
            }
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = m_Heap.Count - 1;
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int largest = index;

            if (leftChild <= lastIndex && m_Heap[leftChild].CompareTo(m_Heap[largest]) > 0)
                largest = leftChild;

            if (rightChild <= lastIndex && m_Heap[rightChild].CompareTo(m_Heap[largest]) > 0)
                largest = rightChild;

            if (largest == index)
                break;

            Swap(index, largest);
            index = largest;
        }
    }

    private void Swap(int i, int j)
    {
        T temp = m_Heap[i];
        m_Heap[i] = m_Heap[j];
        m_Heap[j] = temp;
    }
}
