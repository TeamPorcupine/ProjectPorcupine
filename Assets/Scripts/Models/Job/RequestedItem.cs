#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using MoonSharp.Interpreter;
using UnityEngine;

namespace ProjectPorcupine.Jobs
{
    [MoonSharpUserData]
    public class RequestedItem
    {
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

        public string Type { get; set; }

        public int MinAmountRequested { get; set; }

        public int MaxAmountRequested { get; set; }

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

        public int AmountDesired(Inventory inventory = null)
        {
            if (inventory == null || inventory.Type != Type)
            {
                return MaxAmountRequested;
            }

            return MaxAmountRequested - inventory.StackSize;
        }

        public int AmountNeeded(Inventory inventory = null)
        {
            if (inventory == null || inventory.Type != Type)
            {
                return MinAmountRequested;
            }

            return Mathf.Max(MinAmountRequested - inventory.StackSize, 0);
        }
    }
}