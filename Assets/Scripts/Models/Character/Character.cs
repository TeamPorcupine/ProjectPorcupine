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
using System.Xml;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using ProjectPorcupine.Localization;
using ProjectPorcupine.State;
using UnityEngine;

public enum Facing
{
    NORTH,
    EAST,
    SOUTH,
    WEST
}

/// <summary>
/// A Character is an entity on the map that can move between tiles and,
/// for now, grabs jobs from the work queue and performs this.
/// Later, the Character class will likely be refactored (possibly with
/// sub-classes or interfaces) to support friendly workers, enemies, etc...
/// </summary>
[MoonSharpUserData]
public class Character : ISelectable, IContextActionProvider, IUpdatable
{
    /// Name of the Character.
    public string name;

    /// The item we are carrying (not gear/equipment).
    public Inventory inventory;

    /// Holds all character animations.
    public Animation.CharacterAnimation animation;

    /// Is the character walking or idle.
    public bool IsWalking;

    /// What direction our character is looking.
    public Facing CharFacing;

    private Dictionary<string, Stat> stats;

    /// Current tile the character is standing on.
    private Tile currTile;

    /// The next tile in the pathfinding sequence (the one we are about to enter).
    private Tile nextTile;

    /// Goes from 0 to 1 as we move from CurrTile to nextTile.
    private float movementPercentage;

    /// Holds the path to reach DestTile.
    private List<Tile> movementPath;

    /// Tiles per second.
    private float speed;
    private float baseSpeed = 5f;

    /// Used for health system.
    private HealthSystem health;

    /// Tile where job should be carried out, if different from MyJob.tile.
    private Tile jobTile;

    private bool selected = false;

    private Color characterColor;
    private Color characterUniformColor;
    private Color characterSkinColor;

    // The current state
    private State state;

    // List of global states that always run
    private List<State> globalStates;

    // Queue of states that aren't important enough to interrupt, but should run soon
    private Queue<State> stateQueue;

    /// Use only for serialization
    public Character()
    {
        Needs = new Need[PrototypeManager.Need.Count];
        InitializeCharacterValues();
    }

    public Character(Tile tile, Color color, Color uniformColor, Color skinColor)
    {
        CurrTile = tile;
        characterColor = color;
        characterUniformColor = uniformColor;
        characterSkinColor = skinColor;
        InitializeCharacterValues();
        stateQueue = new Queue<State>();
        globalStates = new List<State>
        {
            new NeedState(this)
        };
    }

    /// A callback to trigger when character information changes (notably, the position).
    public event Action<Character> OnCharacterChanged;

    /// All the needs of this character.
    public Need[] Needs { get; private set; }

    /// Tile offset for animation
    public Vector3 TileOffset { get; set; }

    /// Tiles per second.
    public float MovementSpeed
    {
        get
        {
            return speed;
        }
    }

    /// <summary>
    /// Returns a float representing the Character's X position, which can
    /// be part-way between two tiles during movement.
    /// </summary>
    public float X
    {
        get
        {
            return CurrTile.X + TileOffset.x;
        }
    }

    /// <summary>
    /// Returns a float representing the Character's Y position, which can
    /// be part-way between two tiles during movement.
    /// </summary>
    public float Y
    {
        get
        {
            return CurrTile.Y + TileOffset.y;
        }
    }

    /// <summary>
    /// Returns a float representing the Character's Z position, which can
    /// be part-way between two tiles during movement.
    /// </summary>
    public float Z
    {
        get
        {
            return CurrTile.Z + TileOffset.z;
        }
    }

    public Bounds Bounds
    {
        get
        {
            return new Bounds(
                new Vector3(X - 1, Y - 1, 0),
                new Vector3(1, 1));
        }
    }

    /// <summary>
    /// The tile the Character is considered to still be standing in.
    /// </summary>
    public Tile CurrTile
    {
        get
        {
            return currTile;
        }

        set
        {
            if (currTile != null)
            {
                currTile.Characters.Remove(this);
            }

            currTile = value;
            currTile.Characters.Add(this);

            TileOffset = Vector3.zero;
        }
    }

    /// Our job, if any.
    public Job MyJob
    {
        get
        {
            JobState jobState = FindInitiatingState() as JobState;
            if (jobState != null)
            {
                return jobState.Job;
            }

            return null;
        }
    }

    public bool IsSelected
    {
        get
        {
            return selected;
        }

        set
        {
            if (value == false)
            {
                VisualPath.Instance.RemoveVisualPoints(name);
            }

            selected = value;
        }
    }

    /// <summary>
    /// Gets the Health of this object.
    /// </summary>
    public HealthSystem Health
    {
        get
        {
            if (health == null)
            {
                health = new HealthSystem(-1f, true, false, false, false);
            }

            return health;
        }
    }

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            LocalizationKey = "Poke " + GetName(),
            RequireCharacterSelected = false,
            Action = (cm, c) => { UnityDebugger.Debugger.Log("Character", GetDescription()); health.CurrentHealth -= 5; }
        };

        yield return new ContextMenuAction
        {
            LocalizationKey = "Heal +5",
            RequireCharacterSelected = false,
            Action = (cm, c) => { health.CurrentHealth += 5; }
        };
    }

    #region State

    public void PrioritizeJob(Job job)
    {
        if (state != null)
        {
            state.Interrupt();
        }

        SetState(new JobState(this, job));
    }

    /// <summary>
    /// Stops the current state. Makes the character halt what is going on and start looking for something new to do, might be the same thing.
    /// </summary>
    public void InterruptState()
    {
        if (state != null)
        {
            state.Interrupt();

            // We can't use SetState(null), because it runs Exit on the state and we don't want to run both Interrupt and Exit.
            state = null;
        }
    }

    /// <summary>
    /// Removes all the queued up states.
    /// </summary>
    public void ClearStateQueue()
    {
        // If we interrupt, we get rid of the queue as well.
        while (stateQueue.Count > 0)
        {
            State queuedState = stateQueue.Dequeue();
            queuedState.Interrupt();
        }
    }

    public void QueueState(State newState)
    {
        stateQueue.Enqueue(newState);
    }

    public void SetState(State newState)
    {
        if (state != null)
        {
            state.Exit();
        }

        state = newState;

        if (state != null)
        {
            state.Enter();
        }
    }

    #endregion

    /// Runs every "frame" while the simulation is not paused
    public void EveryFrameUpdate(float deltaTime)
    {
        // Run all the global states first so that they can interrupt or queue up new states
        foreach (State globalState in globalStates)
        {
            globalState.Update(deltaTime);
        }

        // We finished the last state
        if (state == null)
        {
            if (stateQueue.Count > 0)
            {
                SetState(stateQueue.Dequeue());
            }
            else
            {
                Job job = World.Current.jobQueue.GetJob(this);
                if (job != null)
                {
                    SetState(new JobState(this, job));
                }
                else
                {
                    // TODO: Lack of job states should be more interesting. Maybe go to the pub and have a pint?
                    SetState(new IdleState(this));
                }
            }
        }

        state.Update(deltaTime);

        animation.Update(deltaTime);

        if (OnCharacterChanged != null)
        {
            OnCharacterChanged(this);
        }
    }

    public void FixedFrequencyUpdate(float deltaTime)
    {
        throw new NotImplementedException();
    }

    public object ToJSon()
    {
        JObject characterJson = new JObject();

        characterJson.Add("Name", name);
        characterJson.Add("X", CurrTile.X);
        characterJson.Add("Y", CurrTile.Y);
        characterJson.Add("Z", CurrTile.Z);

        JObject needsJSon = new JObject();
        foreach (Need need in Needs)
        {
            needsJSon.Add(need.Name, need.Amount);
        }

        characterJson.Add("Needs", needsJSon);

        JObject colorsJson = new JObject();
        colorsJson.Add("CharacterColor", new JArray(characterColor.r, characterColor.g, characterColor.b));
        colorsJson.Add("UniformColor", new JArray(characterUniformColor.r, characterUniformColor.g, characterUniformColor.b));
        colorsJson.Add("SkinColor", new JArray(characterSkinColor.r, characterSkinColor.g, characterSkinColor.b));
        characterJson.Add("Colors", colorsJson);

        JObject statsJSon = new JObject();
        foreach (Stat stat in stats.Values)
        {
            needsJSon.Add(stat.Name, stat.Value);
        }

        characterJson.Add("Stats", statsJSon);

        if (inventory != null)
        {
            characterJson.Add("Inventories", new JArray(inventory.ToJSon()));
        }

        return characterJson;
    }

    #region ISelectableInterface implementation

    public string GetName()
    {
        return name;
    }

    public string GetDescription()
    {
        return "A human astronaut.";
    }

    public IEnumerable<string> GetAdditionalInfo()
    {
        yield return health.TextForSelectionPanel();

        foreach (Need n in Needs)
        {
            yield return LocalizationTable.GetLocalization(n.LocalizationID, n.DisplayAmount);
        }

        foreach (Stat stat in stats.Values)
        {
            yield return LocalizationTable.GetLocalization("stat_" + stat.Type.ToLower(), stat.Value);
        }
    }

    public Color GetCharacterColor()
    {
        return characterColor;
    }

    public Color GetCharacterSkinColor()
    {
        return characterSkinColor;
    }

    public Color GetCharacterUniformColor()
    {
        return characterUniformColor;
    }

    public string GetJobDescription()
    {
        if (MyJob == null)
        {
            return "job_no_job_desc";
        }

        return MyJob.Description;
    }

    #endregion

    public Stat GetStat(string statType)
    {
        Stat stat = null;
        stats.TryGetValue(statType, out stat);
        return stat;
    }

    public void FaceTile(Tile nextTile)
    {
        // Find character facing
        if (nextTile.X > CurrTile.X)
        {
            CharFacing = Facing.EAST;
        }
        else if (nextTile.X < CurrTile.X)
        {
            CharFacing = Facing.WEST;
        }
        else if (nextTile.Y > CurrTile.Y)
        {
            CharFacing = Facing.NORTH;
        }
        else
        {
            CharFacing = Facing.SOUTH;
        }
    }

    public void ReadStatsFromSave(XmlReader reader)
    {
        // Protection vs. empty stats
        if (reader.IsEmptyElement)
        {
            return;
        }

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }

            string statType = reader.GetAttribute("type");
            Stat stat = GetStat(statType);
            if (stat == null)
            {
                continue;
            }

            int statValue;
            if (!int.TryParse(reader.GetAttribute("value"), out statValue))
            {
                UnityDebugger.Debugger.LogError("Character", "Stat element did not have a value!");
                continue;
            }

            stat.Value = statValue;
        }
    }

    private State FindInitiatingState()
    {
        if (state == null)
        {
            return null;
        }

        State rootState = state;
        while (rootState.NextState != null)
        {
            rootState = rootState.NextState;
        }

        return rootState;
    }

    private void InitializeCharacterValues()
    {
        LoadNeeds();
        LoadStats();
        UseStats();
    }

    private void LoadNeeds()
    {
        Needs = new Need[PrototypeManager.Need.Count];
        PrototypeManager.Need.Values.CopyTo(Needs, 0);
        for (int i = 0; i < PrototypeManager.Need.Count; i++)
        {
            Need need = Needs[i];
            Needs[i] = need.Clone();
            Needs[i].Character = this;
        }
    }

    private void LoadStats()
    {
        stats = new Dictionary<string, Stat>(PrototypeManager.Stat.Count);
        for (int i = 0; i < PrototypeManager.Stat.Count; i++)
        {
            Stat prototypeStat = PrototypeManager.Stat.Values[i];
            Stat newStat = prototypeStat.Clone();

            // Gets a random value within the min and max range of the stat.
            // TODO: Should there be any bias or any other algorithm applied here to make stats more interesting?
            newStat.Value = UnityEngine.Random.Range(1, 20);
            stats.Add(newStat.Type, newStat);
        }

        UnityDebugger.Debugger.Log("Character", "Initialized " + stats.Count + " Stats.");
    }

    /// <summary>
    /// Use the stats of the character to determine various traits.
    /// </summary>
    private void UseStats()
    {
        // The speed is equal to (baseSpeed +/-30% of baseSpeed depending on Dexterity)
        speed = baseSpeed + (0.3f * baseSpeed * ((Convert.ToSingle(stats["Dexterity"].Value) - 10) / 10));

        // Base character max health on their constitution.
        health = new HealthSystem(50 + (Convert.ToSingle(stats["Constitution"].Value) * 5));
    }
}
