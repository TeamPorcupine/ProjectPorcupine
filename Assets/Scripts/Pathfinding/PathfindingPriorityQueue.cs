using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;
using System;

/// <summary>
/// Wraps the FastPriorityQueue class so that it's both easy-to-use,
/// and faster than SimplePriorityQueue (which sports an O(n) Contains
/// and an O(n) UpdatePriority -- not exactly ideal).
/// </summary>
public class PathfindingPriorityQueue<T>
{
    /// <summary>
    /// The underlying FastPriorityQueue instance.
    /// </summary>
    protected FastPriorityQueue<WrappedNode> _underlyingQueue;

    /// <summary>
    /// The map between data and WrappedNodes.
    /// Used to make operations like Contains and UpdatePriority more efficient.
    /// </summary>
    protected Dictionary<T, WrappedNode> _mapDataToWrappedNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathfindingPriorityQueue`1"/> class.
    /// </summary>
    /// <param name="startingSize">The starting size.</param>
    public PathfindingPriorityQueue(int startingSize = 10)
    {
        _underlyingQueue = new FastPriorityQueue<WrappedNode>(startingSize);
        _mapDataToWrappedNode = new Dictionary<T, WrappedNode>();
    }

    /// <summary>
    /// A version of a PriorityQueueNode that contains a reference to data.
    /// </summary>
    protected class WrappedNode : FastPriorityQueueNode
    {
        /// <summary>
        /// The data that this WrappedNode represents in the queue.
        /// </summary>
        public readonly T data;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathfindingPriorityQueue`1+WrappedNode"/> class.
        /// </summary>
        /// <param name="data">The data that this WrappedNode represents in the queue.</param>
        public WrappedNode(T data)
        {
            this.data = data;
        }
    }

    /// <summary>
    /// Checks whether the PQ contains the specified data.
    /// Uses a Dictionary for lookup, so it should only take O(1).
    /// </summary>
    /// <param name="data">Data.</param>
    public bool Contains(T data)
    {
        return _mapDataToWrappedNode.ContainsKey(data);
    }

    /// <summary>
    /// Enqueue the specified data and priority.
    /// If the data already exists in the queue, it updates the priority instead.
    /// Should take O(log n) -- O(1) amortized for the resizing, and O(log n) for the insertion.
    /// </summary>
    /// <param name="data">The data to be enqueued.</param>
    /// <param name="priority">The priority of the data.</param>
    public void Enqueue(T data, float priority)
    {
        if (_mapDataToWrappedNode.ContainsKey(data))
        {
            Logger.LogError("Priority Queue can't re-enqueue a node that's already enqueued.");
            return;
        }

        if (_underlyingQueue.Count == _underlyingQueue.MaxSize)
        {
            _underlyingQueue.Resize(2 * _underlyingQueue.MaxSize + 1);
        }

        WrappedNode toAdd = new WrappedNode(data);
        _underlyingQueue.Enqueue(toAdd, priority);
        _mapDataToWrappedNode[data] = toAdd;
    }

    /// <summary>
    /// Updates the priority associated with the given data.
    /// </summary>
    /// <param name="data">The data whose priority needs updating.</param>
    /// <param name="priority">The new priority value.</param>
    public void UpdatePriority(T data, float priority)
    {
        WrappedNode node = _mapDataToWrappedNode[data];
        _underlyingQueue.UpdatePriority(node, priority);
    }

    /// <summary>
    /// Enqueues the or update.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="priority">Priority.</param>
    public void EnqueueOrUpdate(T data, float priority)
    {
        if (_mapDataToWrappedNode.ContainsKey(data))
            UpdatePriority(data, priority);
        else
            Enqueue(data, priority);
    }

    /// <summary>
    /// Pops the item with the lowest priority off of the queue.
    /// </summary>
    public T Dequeue()
    {
        WrappedNode popped = _underlyingQueue.Dequeue();
        _mapDataToWrappedNode.Remove(popped.data);
        return popped.data;
    }

    /// <summary>
    /// Returns the number of items in the queue.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
        get
        {
            return _underlyingQueue.Count;
        }
    }
}

