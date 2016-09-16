using System;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ProjectPorcupine.Jobs
{
    [MoonSharpUserData]
    public class RequestedItem
    {
        public string Type { get; set; }

        public int MinAmountRequested { get; set; }

        public int MaxAmountRequested { get; set; }

        public RequestedItem(string type, int minAmountRequested, int maxAmountRequested)
        {
            Type = type;
            MinAmountRequested = minAmountRequested;
            MaxAmountRequested = maxAmountRequested;
        }

        public RequestedItem(string type, int maxAmountRequested)
            : this(type, maxAmountRequested, maxAmountRequested)
        {
        }

        public RequestedItem(RequestedItem item)
            : this(item.Type, item.MinAmountRequested, item.MaxAmountRequested)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPorcupine.Job.RequestedItem"/> class with the amount needed to fill the inventory item passed in.
        /// </summary>
        public RequestedItem(Inventory inventory)
            : this(inventory.Type, inventory.MaxStackSize - inventory.StackSize)
        {
        }

        public RequestedItem Clone()
        {
            return new RequestedItem(this);
        }

        public bool NeedsMore(Inventory inventory)
        {
            return AmountNeeded(inventory) > 0;
        }

        public bool DesiresMore(Inventory inventory)
        {
            return AmountDesired(inventory) > 0;
        }

        public int AmountDesired(Inventory inventory)
        {
            if (inventory == null || inventory.Type != Type)
            {
                return MaxAmountRequested;
            }

            return MaxAmountRequested - inventory.StackSize;
        }

        public int AmountNeeded(Inventory inventory)
        {
            if (inventory == null || inventory.Type != Type)
            {
                return MinAmountRequested;
            }

            return Mathf.Max(MinAmountRequested - inventory.StackSize, 0);
        }
    }
}

