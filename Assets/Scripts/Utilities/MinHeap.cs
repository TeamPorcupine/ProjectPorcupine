#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;

/// Simple pooling for Unity.
/// Author: Benjamin Ward (ward.programm3r@gmail.com)
/// Latest Version: https://gist.github.com/WardBenjamin/991dfa64e94892924b67efe569e35050
/// License: CC0 (http://creativecommons.org/publicdomain/zero/1.0/)
/// UPDATES: N/A
/// 8/31/16 - Fixed according to ProjectPorcupine StyleCop settings
public class MinHeap<T> : IEnumerable<T> where T : class, IComparable<T>
{
    #region Fields

    private IComparer<T> comparer;
    private List<T> items = new List<T>();

    #endregion

    #region Constructors

    public MinHeap() : this(Comparer<T>.Default)
    {
    }

    public MinHeap(IComparer<T> comp)
    {
        comparer = comp;
    }

    #endregion

    #region Properties

    public int Count
    {
        get { return items.Count; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        items.Clear();
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the BinaryHeap,
    /// if that number is less than a threshold value.
    /// </summary>
    /// <remarks>
    /// The current threshold value is 90% (.NET 3.5), but might change in a future release.
    /// </remarks>
    public void TrimExcess()
    {
        items.TrimExcess();
    }

    /// <summary>
    /// Inserts an item onto the heap.
    /// </summary>
    /// <param name="newItem">The item to be inserted.</param>
    public void Insert(T newItem)
    {
        int i = Count;
        items.Add(newItem);

        while (i > 0 && comparer.Compare(items[(i - 1) / 2], newItem) > 0)
        {
            items[i] = items[(i - 1) / 2];
            i = (i - 1) / 2;
        }

        items[i] = newItem;
    }

    /// <summary>
    /// Return the root item from the collection, without removing it.
    /// </summary>
    /// <returns>Returns the item at the root of the heap.</returns>
    public T Peek()
    {
        if (items.Count == 0)
        {
            throw new InvalidOperationException("The heap is empty.");
        }

        return items[0];
    }

    /// <summary>
    /// Removes and returns the root item from the collection.
    /// </summary>
    /// <returns>Returns the item at the root of the heap.</returns>
    public T RemoveRoot()
    {
        if (items.Count == 0)
        {
            throw new InvalidOperationException("The heap is empty.");
        }

        // Get the first item
        T rslt = items[0];

        // Get the last item and bubble it down.
        T tmp = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);

        if (items.Count > 0)
        {
            int i = 0;
            while (i < items.Count / 2)
            {
                int j = (2 * i) + 1;
                if ((j < items.Count - 1) && (comparer.Compare(items[j], items[j + 1]) > 0))
                {
                    ++j;
                }

                if (comparer.Compare(items[j], tmp) >= 0)
                {
                    break;
                }

                items[i] = items[j];
                i = j;
            }

            items[i] = tmp;
        }

        return rslt;
    }

    #endregion

    #region IEnumerable implementation

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        foreach (T item in items)
        {
            yield return item;
        }
    }

    public IEnumerator GetEnumerator()
    {
        return items.GetEnumerator();
    }

    #endregion
}
