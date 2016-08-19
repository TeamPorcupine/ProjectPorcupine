//=======================================================================
// Copyright Martin "quill18" Glaude 2015-2016.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections.Generic;
using System;
using MoonSharp.Interpreter;
using System.Linq;

[MoonSharpUserData]
public class Job
{

    // This class holds info for a queued up job, which can include
    // things like placing furniture, moving stored inventory,
    // working at a desk, and maybe even fighting enemies.

    public Tile Tile { get; set; }

    public float JobTime { get; protected set; }

    protected float _jobTimeRequired;

    protected bool _jobRepeats = false;

    public string JobObjectType { get; protected set; }

    public TileType JobTileType { get; protected set; }

    public Furniture FurniturePrototype { get; set; }

    public Furniture Furniture { get; set; }
    // The piece of furniture that owns this job. Frequently will be null.

    public bool acceptsAnyInventoryItem = false;

    public event Action<Job> JobCompleted;
    // We have finished the work cycle and so things should probably get built or whatever.
    private List<string> JobCompletedLua;
    public event Action<Job> JobStopped;
    // The job has been stopped, either because it's non-repeating or was cancelled.
    public event Action<Job> JobWorked;
    // Gets called each time some work is performed -- maybe update the UI?
    private List<string> _jobWorkedLua;

    public bool canTakeFromStockpile = true;

    public Dictionary<string, Inventory> InventoryRequirements { get; set; }

    public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirements, bool jobRepeats = false)
    {
        this.Tile = tile;
        this.JobObjectType = jobObjectType;
        this.JobCompleted += cbJobComplete;
        this._jobTimeRequired = this.JobTime = jobTime;
        this._jobRepeats = jobRepeats;

        _jobWorkedLua = new List<string>();
        JobCompletedLua = new List<string>();

        this.InventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in inventoryRequirements)
            {
                this.InventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    public Job(Tile tile, TileType jobTileType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirements, bool jobRepeats = false)
    {
        this.Tile = tile;
        this.JobTileType = jobTileType;
        this.JobCompleted += cbJobComplete;
        this._jobTimeRequired = this.JobTime = jobTime;
        this._jobRepeats = jobRepeats;

        _jobWorkedLua = new List<string>();
        JobCompletedLua = new List<string>();

        this.InventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in inventoryRequirements)
            {
                this.InventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    protected Job(Job other)
    {
        this.Tile = other.Tile;
        this.JobObjectType = other.JobObjectType;
        this.JobTileType = other.JobTileType;
        this.JobCompleted = other.JobCompleted;
        this.JobTime = other.JobTime;

        _jobWorkedLua = new List<string>(other._jobWorkedLua);
        JobCompletedLua = new List<string>(other._jobWorkedLua);


        this.InventoryRequirements = new Dictionary<string, Inventory>();
        if (InventoryRequirements != null)
        {
            foreach (Inventory inv in other.InventoryRequirements.Values)
            {
                this.InventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    public Inventory[] GetInventoryRequirementValues()
    {
        return InventoryRequirements.Values.ToArray();
    }

    virtual public Job Clone()
    {
        return new Job(this);
    }

    public void RegisterJobCompletedCallback(string cb)
    {
        JobCompletedLua.Add(cb);
    }

    public void UnregisterJobCompletedCallback(string cb)
    {
        JobCompletedLua.Remove(cb);
    }

    public void RegisterJobWorkedCallback(string cb)
    {
        _jobWorkedLua.Add(cb);
    }

    public void UnregisterJobWorkedCallback(string cb)
    {
        _jobWorkedLua.Remove(cb);
    }

    public void DoWork(float workTime)
    {
        // Check to make sure we actually have everything we need. 
        // If not, don't register the work time.
        if (HasAllMaterial() == false)
        {
            //Debug.LogError("Tried to do work on a job that doesn't have all the material.");

            // Job can't actually be worked, but still call the callbacks
            // so that animations and whatnot can be updated.
            if (JobWorked != null)
                JobWorked(this);

            if (_jobWorkedLua != null)
            {
                foreach (string luaFunction in _jobWorkedLua)
                {
                    FurnitureActions.CallFunction(luaFunction, this);
                }
            }

            return;
        }

        JobTime -= workTime;

        if (JobWorked != null)
            JobWorked(this);

        if (_jobWorkedLua != null)
        {
            foreach (string luaFunction in _jobWorkedLua)
            {
                FurnitureActions.CallFunction(luaFunction, this);
            }
        }

        if (JobTime <= 0)
        {
            // Do whatever is supposed to happen with a job cycle completes.
            if (JobCompleted != null)
                JobCompleted(this);

            foreach (string luaFunc in JobCompletedLua)
            {
                FurnitureActions.CallFunction(luaFunc, this);
            }

            if (_jobRepeats == false)
            {
                // Let everyone know that the job is officially concluded
                if (JobStopped != null)
                    JobStopped(this);
            }
            else
            {
                // This is a repeating job and must be reset.
                JobTime += _jobTimeRequired;
            }
        }
    }

    public void CancelJob()
    {
        if (JobStopped != null)
            JobStopped(this);

        World.Current.JobQueue.Remove(this);
    }

    public bool HasAllMaterial()
    {
        foreach (Inventory inv in InventoryRequirements.Values)
        {
            if (inv.maxStackSize > inv.StackSize)
                return false;
        }

        return true;
    }

    public int DesiresInventoryType(Inventory inv)
    {
        if (acceptsAnyInventoryItem)
        {
            return inv.maxStackSize;
        }

        if (InventoryRequirements.ContainsKey(inv.objectType) == false)
        {
            return 0;
        }

        if (InventoryRequirements[inv.objectType].StackSize >= InventoryRequirements[inv.objectType].maxStackSize)
        {
            // We already have all that we need!
            return 0;
        }

        // The inventory is of a type we want, and we still need more.
        return InventoryRequirements[inv.objectType].maxStackSize - InventoryRequirements[inv.objectType].StackSize;
    }

    public Inventory GetFirstDesiredInventory()
    {
        foreach (Inventory inv in InventoryRequirements.Values)
        {
            if (inv.maxStackSize > inv.StackSize)
                return inv;
        }

        return null;
    }
}