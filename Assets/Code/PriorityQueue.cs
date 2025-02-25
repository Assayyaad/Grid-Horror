using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly List<(T item, float priority)> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
        elements.Sort((x, y) => x.priority.CompareTo(y.priority));
    }

    public T Dequeue()
    {
        T item = elements[0].item;
        elements.RemoveAt(0);
        return item;
    }

    public bool Contains(T item)
    {
        return elements.Exists(x => x.item.Equals(item));
    }
}