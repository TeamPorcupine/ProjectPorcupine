#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using Priority_Queue;

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
    protected FastPriorityQueue<WrappedNode> underlyingQueue;

    /// <summary>
    /// The map between data and WrappedNodes.
    /// Used to make operations like Contains and UpdatePriority more efficient.
    /// </summary>
    protected Dictionary<T, WrappedNode> mapDataToWrappedNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathfindingPriorityQueue`1"/> class.
    /// </summary>
    /// <param name="startingSize">The starting size.</param>
    public PathfindingPriorityQueue(int startingSize = 10)
    {
        underlyingQueue = new FastPriorityQueue<WrappedNode>(startingSize);
        mapDataToWrappedNode = new Dictionary<T, WrappedNode>();
    }

    /// <summary>
    /// Returns the number of items in the queue.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
        get
        {
            return underlyingQueue.Count;
        }
    }

    /// <summary>
    /// Checks whether the PQ contains the specified data.
    /// Uses a Dictionary for lookup, so it should only take O(1).
    /// </summary>
    /// <param name="data">Datakey to check.</param>
    public bool Contains(T data)
    {
        return mapDataToWrappedNode.ContainsKey(data);
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
        if (mapDataToWrappedNode.ContainsKey(data))
        {
            UnityDebugger.Debugger.LogError("PathfindingPriorityQueue", "Priority Queue can't re-enqueue a node that's already enqueued.");
            return;
        }

        if (underlyingQueue.Count == underlyingQueue.MaxSize)
        {
            underlyingQueue.Resize((2 * underlyingQueue.MaxSize) + 1);
        }

        WrappedNode newNode = new WrappedNode(data);
        underlyingQueue.Enqueue(newNode, priority);
        mapDataToWrappedNode[data] = newNode;
    }

    /// <summary>
    /// Updates the priority associated with the given data.
    /// </summary>
    /// <param name="data">The data whose priority needs updating.</param>
    /// <param name="priority">The new priority value.</param>
    public void UpdatePriority(T data, float priority)
    {
        WrappedNode node = mapDataToWrappedNode[data];
        underlyingQueue.UpdatePriority(node, priority);
    }

    /// <summary>
    /// Enqueues or updates data.
    /// </summary>
    /// <param name="data">Datakey to enqueue or update..</param>
    /// <param name="priority">New priority level.</param>
    public void EnqueueOrUpdate(T data, float priority)
    {
        if (mapDataToWrappedNode.ContainsKey(data))
        {
            UpdatePriority(data, priority);
        }
        else
        {
            Enqueue(data, priority);
        }
    }

    /// <summary>
    /// Pops the item with the lowest priority off of the queue.
    /// </summary>
    public T Dequeue()
    {
        WrappedNode popped = underlyingQueue.Dequeue();
        mapDataToWrappedNode.Remove(popped.Data);
        return popped.Data;
    }

    /// <summary>
    /// A version of a PriorityQueueNode that contains a reference to data.
    /// </summary>
    protected class WrappedNode : FastPriorityQueueNode
    {
        /// <summary>
        /// The data that this WrappedNode represents in the queue.
        /// </summary>
        public readonly T Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathfindingPriorityQueue`1+WrappedNode"/> class.
        /// </summary>
        /// <param name="data">The data that this WrappedNode represents in the queue.</param>
        public WrappedNode(T data)
        {
            this.Data = data;
        }
    }
}