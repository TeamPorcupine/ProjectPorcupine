#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class Job
{
    public enum JobPriority 
    {
        High, Medium, Low 
    }

    // This class holds info for a queued up job, which can include
    // things like placing furniture, moving stored inventory,
    // working at a desk, and maybe even fighting enemies.

    public Tile tile;

    public float jobTime
    {
        get;
        protected set;
    }

    public JobPriority jobPriority
    {
        get;
        protected set;
    }

    protected float jobTimeRequired;

    protected bool jobRepeats = false;

    public string jobObjectType
    {
        get;
        protected set;
    }
    public bool isNeed
    {
        get;
        protected set;
    }
    public bool critical
    {
        get;
        protected set;
    }

    public TileType jobTileType
    {
        get;
        protected set;
    }

    public Furniture furniturePrototype;

    // The piece of furniture that owns this job. Frequently will be null.
    public Furniture furniture;

    // We have finished the work cycle and so things should probably get built or whatever.
    public event Action<Job> cbJobCompleted;
   
    // The job has been stopped, either because it's non-repeating or was cancelled.
    private List<string> cbJobCompletedLua;

    public event Action<Job> cbJobStopped;

    // Gets called each time some work is performed -- maybe update the UI?
    public event Action<Job> cbJobWorked;

    private List<string> cbJobWorkedLua;

    public bool canTakeFromStockpile = true;

    public Dictionary<string, Inventory> inventoryRequirements;
    
    public string JobDescription { get; set; }

    /// <summary>
    /// If true, the work will be carried out on any adjacent tile of the target tile rather than on it.
    /// </summary>
    public bool adjacent;

    /// <summary>
    /// If true the job is workable if ANY of the inventory requirements are met.
    /// Otherwise ALL requirements must be met before work can start.
    /// This is useful for stockpile/storage jobs which can accept many types of items.
    /// Defaults to false.
    /// </summary>
    public bool acceptsAny;

    public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirements, JobPriority jobPriority, bool jobRepeats = false, bool isNeed = false, bool critical = false)
    {
        this.tile = tile;
        this.jobObjectType = jobObjectType;
        this.cbJobCompleted += cbJobComplete;
        this.jobTimeRequired = this.jobTime = jobTime;
        this.jobRepeats = jobRepeats;
        this.isNeed = isNeed;
        this.critical = critical;
        this.jobPriority = jobPriority;
        this.JobDescription = "job_error_missing_desc";

        cbJobWorkedLua = new List<string>();
        cbJobCompletedLua = new List<string>();

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in inventoryRequirements)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    public Job(Tile tile, TileType jobTileType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirements, JobPriority jobPriority, bool jobRepeats = false, bool adjacent = false)
    {
        this.tile = tile;
        this.jobTileType = jobTileType;
        this.cbJobCompleted += cbJobComplete;
        this.jobTimeRequired = this.jobTime = jobTime;
        this.jobRepeats = jobRepeats;
        this.jobPriority = jobPriority;
        this.adjacent = adjacent;
        this.JobDescription = "job_error_missing_desc";

        cbJobWorkedLua = new List<string>();
        cbJobCompletedLua = new List<string>();

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in inventoryRequirements)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    protected Job(Job other)
    {
        this.tile = other.tile;
        this.jobObjectType = other.jobObjectType;
        this.jobTileType = other.jobTileType;
        this.cbJobCompleted = other.cbJobCompleted;
        this.jobTime = other.jobTime;
        this.jobPriority = other.jobPriority;
        this.adjacent = other.adjacent;
        this.JobDescription = other.JobDescription;
        this.acceptsAny = other.acceptsAny;

        cbJobWorkedLua = new List<string>(other.cbJobWorkedLua);
        cbJobCompletedLua = new List<string>(other.cbJobWorkedLua);

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in other.inventoryRequirements.Values)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    public Inventory[] GetInventoryRequirementValues()
    {
        return inventoryRequirements.Values.ToArray();
    }

    public virtual Job Clone()
    {
        return new Job(this);
    }
    
    public void RegisterJobCompletedCallback(string cb)
    {
        cbJobCompletedLua.Add(cb);
    }

    public void UnregisterJobCompletedCallback(string cb)
    {
        cbJobCompletedLua.Remove(cb);
    }
    
    public void RegisterJobWorkedCallback(string cb)
    {
        cbJobWorkedLua.Add(cb);
    }

    public void UnregisterJobWorkedCallback(string cb)
    {
        cbJobWorkedLua.Remove(cb);
    }

    public void DoWork(float workTime)
    {
        // We don't know if the Job can actually be worked, but still call the callbacks
        // so that animations and whatnot can be updated.
        if (cbJobWorked != null)
        {
            cbJobWorked(this);
        }

        if (cbJobWorkedLua != null)
        {
            foreach (string luaFunction in cbJobWorkedLua.ToList())
            {
                LuaUtilities.CallFunction(luaFunction, this);
            }
        }

        // Check to make sure we actually have everything we need. 
        // If not, don't register the work time.
        if (MaterialNeedsMet() == false)
        {
            ////Debug.LogError("Tried to do work on a job that doesn't have all the material.");
            return;
        }

        jobTime -= workTime;
        
        if (jobTime <= 0)
        {
            // Do whatever is supposed to happen with a job cycle completes.
            if (cbJobCompleted != null)
            {
                cbJobCompleted(this);
            }

            foreach (string luaFunction in cbJobCompletedLua.ToList())
            {
                LuaUtilities.CallFunction(luaFunction, this);
            }
            
            if (jobRepeats == false)
            {
                // Let everyone know that the job is officially concluded
                if (cbJobStopped != null)
                {
                    cbJobStopped(this);
                }
            }
            else
            {
                // This is a repeating job and must be reset.
                jobTime += jobTimeRequired;
            }
        }
    }

    public void CancelJob()
    {
        if (cbJobStopped != null)
        {
            cbJobStopped(this);
        }

        // Remove the job out of both job queues.
        World.current.jobWaitingQueue.Remove(this);
        World.current.jobQueue.Remove(this);
    }

    public bool MaterialNeedsMet()
    {
        if (acceptsAny && HasAnyMaterial())
        {
            return true;
        }
        if ((acceptsAny == false) && HasAllMaterial())
        {
            return true;
        }
        return false;
    }

    public bool HasAllMaterial()
    {
        if (inventoryRequirements == null)
        {
            return true;
        }
        foreach (Inventory inv in inventoryRequirements.Values)
        {
            if (inv.maxStackSize > inv.stackSize)
            {
                return false;
            }
        }

        return true;
    }

    public bool HasAnyMaterial()
    {
        foreach (Inventory inv in inventoryRequirements.Values)
        {
            if (inv.stackSize > 0)
            {
                return true;
            }
        }

        return false;
    }

    public int AmountDesiredOfInventoryType(string objectType)
    {
        if (inventoryRequirements.ContainsKey(objectType) == false)
        {
            return 0;
        }

        if (inventoryRequirements[objectType].stackSize >= inventoryRequirements[objectType].maxStackSize)
        {
            // We already have all that we need!
            return 0;
        }

        // The inventory is of a type we want, and we still need more.
        return inventoryRequirements[objectType].maxStackSize - inventoryRequirements[objectType].stackSize;
    }

    public int AmountDesiredOfInventoryType(Inventory inv)
    {
        return AmountDesiredOfInventoryType(inv.objectType);
    }

    public Inventory GetFirstDesiredInventory()
    {
        foreach (Inventory inv in inventoryRequirements.Values)
        {
            if (inv.maxStackSize > inv.stackSize)
            {
                return inv;
            }
        }

        return null;
    }

    public void DropPriority()
    {
        // TODO: This casting to and from enums are a bit wierd. We should decide on ONE priority system.
        jobPriority = (JobPriority)Mathf.Min((int)JobPriority.Low, (int)jobPriority + 1);
    }
}
