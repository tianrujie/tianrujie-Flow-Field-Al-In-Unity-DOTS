using System;
using System.Collections;
using System.Collections.Generic;
public class MinHeap<T> where T : IComparable
{
    private List<T> elements = new List<T>(); 

    public int GetSize()
    {
        return elements.Count;
    }

    public T GetMin()
    {
        return this.elements.Count > 0 ? this.elements[0] : default(T);
    }

    public void Add(T item)
    {
        elements.Add(item);
        this.HeapifyUp(elements.Count - 1);
    }

    public T PopMin()
    {
        if (elements.Count > 0)
        {
            T item = elements[0];
            elements[0] = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);

            this.HeapifyDown(0);
            return item;
        }

        throw new InvalidOperationException("no element in heap");
    }

    private void HeapifyUp(int index)
    {
        var parent = this.GetParent(index);
        if (parent >= 0 && elements[index].CompareTo(elements[parent]) < 0)
        {
            var temp = elements[index];
            elements[index] = elements[parent];
            elements[parent] = temp;

            this.HeapifyUp(parent);
        }
    }

    private void HeapifyDown(int index)
    {
        var smallest = index;

        var left = this.GetLeft(index);
        var right = this.GetRight(index);

        if (left < this.GetSize() && elements[left].CompareTo(elements[index]) < 0)
        {
            smallest = left;
        }

        if (right < this.GetSize() && elements[right].CompareTo(elements[smallest]) < 0)
        {
            smallest = right;
        }

        if (smallest != index)
        {
            var temp = elements[index];
            elements[index] = elements[smallest];
            elements[smallest] = temp;

            this.HeapifyDown(smallest);
        }

    }

    private int GetParent(int index)
    {
        if (index <= 0)
        {
            return -1;
        }

        return (index - 1) / 2;
    }

    private int GetLeft(int index)
    {
        return 2 * index + 1;
    }

    private int GetRight(int index)
    {
        return 2 * index + 2;
    }

    public void Clear()
    {
        elements.Clear();
    }
    
}

public class MinHeapNode : IComparable
{
    //一维坐标
    public int index;
    //二维坐标
    // public int two_dimession_x;
    // public int two_dimession_y;
    //估值
    public float cost;

    public int CompareTo(object obj)
    {
        return cost.CompareTo((obj as MinHeapNode).cost);
    }
}