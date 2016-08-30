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
using ProjectPorcupine.Localization;
using UnityEngine;

[MoonSharpUserData]
public class Job : ISelectable
{
    // This class holds info for a queued up job, which can include
    // things like placing furniture, moving stored inventory,
    // working at a desk, and maybe even fighting enemies.
    public Tile tile;

    public Furniture furniturePrototype;

    // The piece of furniture that owns this job. Frequently will be null.
    public Furniture furniture;

    public bool canTakeFromStockpile = true;

    public Dictionary<string, Inventory> inventoryRequirements;

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

    protected float jobTimeRequired;

    protected bool jobRepeats = false;

    private List<string> jobWorkedLua;
   
    // The job has been stopped, either because it's non-repeating or was cancelled.
    private List<string> jobCompletedLua;

    public Job(Tile tile, string jobObjectType, Action<Job> jobComplete, float jobTime, Inventory[] inventoryRequirements, Job.JobPriority jobPriority, bool jobRepeats = false, bool need = false, bool critical = false)
    {
        this.tile = tile;
        this.JobObjectType = jobObjectType;
        this.OnJobCompleted += jobComplete;
        this.jobTimeRequired = this.JobTime = jobTime;
        this.jobRepeats = jobRepeats;
        this.IsNeed = need;
        this.Critical = critical;
        this.Priority = jobPriority;
        this.JobDescription = "job_error_missing_desc";

        jobWorkedLua = new List<string>();
        jobCompletedLua = new List<string>();

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in inventoryRequirements)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    public Job(Tile tile, TileType jobTileType, Action<Job> jobCompleted, float jobTime, Inventory[] inventoryRequirements, Job.JobPriority jobPriority, bool jobRepeats = false, bool adjacent = false)
    {
        this.tile = tile;
        this.JobTileType = jobTileType;
        this.OnJobCompleted += jobCompleted;
        this.jobTimeRequired = this.JobTime = jobTime;
        this.jobRepeats = jobRepeats;
        this.Priority = jobPriority;
        this.adjacent = adjacent;
        this.JobDescription = "job_error_missing_desc";

        jobWorkedLua = new List<string>();
        jobCompletedLua = new List<string>();

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
        this.JobObjectType = other.JobObjectType;
        this.JobTileType = other.JobTileType;
        this.OnJobCompleted = other.OnJobCompleted;
        this.JobTime = other.JobTime;
        this.Priority = other.Priority;
        this.adjacent = other.adjacent;
        this.JobDescription = other.JobDescription;
        this.acceptsAny = other.acceptsAny;

        jobWorkedLua = new List<string>(other.jobWorkedLua);
        jobCompletedLua = new List<string>(other.jobWorkedLua);

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in other.inventoryRequirements.Values)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    // We have finished the work cycle and so things should probably get built or whatever.
    public event Action<Job> OnJobCompleted;

    public event Action<Job> OnJobStopped;

    // Gets called each time some work is performed -- maybe update the UI?
    public event Action<Job> OnJobWorked;

    public enum JobPriority
    {
        High, Medium, Low
    }

    public string JobDescription { get; set; }

    public string JobObjectType
    {
        get;
        protected set;
    }

    public bool IsNeed
    {
        get;
        protected set;
    }

    public bool Critical
    {
        get;
        protected set;
    }

    public bool IsBeingWorked { get; set; }

    public TileType JobTileType
    {
        get;
        protected set;
    }

    public float JobTime
    {
        get;
        protected set;
    }

    public JobPriority Priority
    {
        get;
        protected set;
    }

    public bool IsSelected
    {
        get; set;
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
        jobCompletedLua.Add(cb);
    }

    public void UnregisterJobCompletedCallback(string cb)
    {
        jobCompletedLua.Remove(cb);
    }
    
    public void RegisterJobWorkedCallback(string cb)
    {
        jobWorkedLua.Add(cb);
    }

    public void UnregisterJobWorkedCallback(string cb)
    {
        jobWorkedLua.Remove(cb);
    }

    public void DoWork(float workTime)
    {
        // We don't know if the Job can actually be worked, but still call the callbacks
        // so that animations and whatnot can be updated.
        if (OnJobWorked != null)
        {
            OnJobWorked(this);
        }

        if (jobWorkedLua != null)
        {
            foreach (string luaFunction in jobWorkedLua.ToList())
            {
                LuaUtilities.CallFunction(luaFunction, this);
            }
        }

        // Check to make sure we actually have everything we need. 
        // If not, don't register the work time.
        if (MaterialNeedsMet() == false)
        {
            return;
        }

        JobTime -= workTime;
        
        if (JobTime <= 0)
        {
            // Do whatever is supposed to happen with a job cycle completes.
            if (OnJobCompleted != null)
            {
                OnJobCompleted(this);
            }

            foreach (string luaFunction in jobCompletedLua.ToList())
            {
                LuaUtilities.CallFunction(luaFunction, this);
            }
            
            if (jobRepeats == false)
            {
                // Let everyone know that the job is officially concluded
                if (OnJobStopped != null)
                {
                    OnJobStopped(this);
                }
            }
            else
            {
                // This is a repeating job and must be reset.
                JobTime += jobTimeRequired;
            }
        }
    }

    public void CancelJob()
    {
        if (OnJobStopped != null)
        {
            OnJobStopped(this);
        }

        // Remove the job out of both job queues.
        World.Current.jobWaitingQueue.Remove(this);
        World.Current.jobQueue.Remove(this);
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
            if (inv.maxStackSize > inv.StackSize)
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
            if (inv.StackSize > 0)
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

        if (inventoryRequirements[objectType].StackSize >= inventoryRequirements[objectType].maxStackSize)
        {
            // We already have all that we need!
            return 0;
        }

        // The inventory is of a type we want, and we still need more.
        return inventoryRequirements[objectType].maxStackSize - inventoryRequirements[objectType].StackSize;
    }

    public int AmountDesiredOfInventoryType(Inventory inv)
    {
        return AmountDesiredOfInventoryType(inv.objectType);
    }

    /// <summary>
    /// Fulfillable inventory requirements for job.
    /// </summary>
    /// <returns>A list of (string) objectTypes for job inventory requirements that can be met. Returns null if the job requires materials which do not exist on the map.</returns>
    public List<string> FulfillableInventoryRequirements()
    {
        List<string> fulfillableInventoryRequirements = new List<string>();

        foreach (Inventory inv in this.GetInventoryRequirementValues())
        {
            if (this.acceptsAny == false)
            {
                if (World.Current.inventoryManager.QuickCheck(inv.objectType) == false)
                {
                    // the job requires ALL inventory requirements to be met, and there is no source of a desired objectType
                    return null;
                }
                else
                {
                    fulfillableInventoryRequirements.Add(inv.objectType);
                }
            }
            else if (World.Current.inventoryManager.QuickCheck(inv.objectType))
            {
                // there is a source for a desired objectType that the job will accept
                fulfillableInventoryRequirements.Add(inv.objectType);
            }
        }

        return fulfillableInventoryRequirements;
    }

    public Inventory GetFirstDesiredInventory()
    {
        foreach (Inventory inv in inventoryRequirements.Values)
        {
            if (inv.maxStackSize > inv.StackSize)
            {
                return inv;
            }
        }

        return null;
    }

    public void DropPriority()
    {
        // TODO: This casting to and from enums are a bit wierd. We should decide on ONE priority system.
        this.Priority = (Job.JobPriority)Mathf.Min((int)Job.JobPriority.Low, (int)Priority + 1);
    }

    public string GetName()
    {
        return LocalizationTable.GetLocalization(JobObjectType);
    }

    public string GetDescription()
    {
        string description = "Requirements:\n\t";
        foreach (KeyValuePair<string, Inventory> inv in inventoryRequirements)
        {
            description += inv.Value.StackSize + "/" + inv.Value.maxStackSize + "\n\t";
        }

        return description;
    }

    public string GetHitPointString()
    {
        return string.Empty;
    }

    public string GetJobDescription()
    {
        return GetDescription();
    }
}
