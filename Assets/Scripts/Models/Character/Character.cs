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
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
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
public class Character : IXmlSerializable, ISelectable, IContextActionProvider
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
    private float speed = 5f;

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
            return 5f;
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

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            Text = "Poke " + GetName(),
            RequireCharacterSelected = false,
            Action = (cm, c) => Debug.ULogChannel("Character", GetDescription())
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
    public void Update(float deltaTime)
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

    #region IXmlSerializable implementation

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("name", name);
        writer.WriteAttributeString("X", CurrTile.X.ToString());
        writer.WriteAttributeString("Y", CurrTile.Y.ToString());
        writer.WriteAttributeString("Z", CurrTile.Z.ToString());

        // TODO: It is more verbose, but easier to parse if these are represented as key-value elements rather than a string with delimiters.
        string needString = string.Empty;
        foreach (Need n in Needs)
        {
            int storeAmount = (int)(n.Amount * 10);
            needString = needString + n.Type + ";" + storeAmount.ToString() + ":";
        }

        writer.WriteAttributeString("needs", needString);

        writer.WriteAttributeString("r", characterColor.r.ToString());
        writer.WriteAttributeString("b", characterColor.b.ToString());
        writer.WriteAttributeString("g", characterColor.g.ToString());
        writer.WriteAttributeString("rUni", characterUniformColor.r.ToString());
        writer.WriteAttributeString("bUni", characterUniformColor.b.ToString());
        writer.WriteAttributeString("gUni", characterUniformColor.g.ToString());
        writer.WriteAttributeString("rSkin", characterSkinColor.r.ToString());
        writer.WriteAttributeString("bSkin", characterSkinColor.b.ToString());
        writer.WriteAttributeString("gSkin", characterSkinColor.g.ToString());

        writer.WriteStartElement("Stats");
        foreach (Stat stat in stats.Values)
        {
            writer.WriteStartElement("Stat");
            stat.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        if (inventory != null)
        {
            writer.WriteStartElement("Inventories");
            writer.WriteStartElement("Inventory");
            inventory.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader reader)
    {
        if (reader.GetAttribute("needs") == null)
        {
            return;
        }

        string[] needListA = reader.GetAttribute("needs").Split(new char[] { ':' });
        foreach (string s in needListA)
        {
            string[] needListB = s.Split(new char[] { ';' });
            foreach (Need n in Needs)
            {
                if (n.Type == needListB[0])
                {
                    int storeAmount;
                    if (int.TryParse(needListB[1], out storeAmount))
                    {
                        n.Amount = (float)storeAmount / 10;
                    }
                    else
                    {
                        Debug.ULogErrorChannel("Character", "Character.ReadXml() expected an int when deserializing needs");
                    }
                }
            }
        }
    }

    #endregion

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
        yield return string.Format("HitPoints: 100/100");

        foreach (Need n in Needs)
        {
            yield return LocalizationTable.GetLocalization(n.LocalisationID, n.DisplayAmount);
        }

        foreach (Stat stat in stats.Values)
        {
            // TODO: Localization
            yield return string.Format("{0}: {1}", stat.Type, stat.Value);
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

        return MyJob.JobDescription;
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
                Debug.ULogErrorChannel("Character", "Stat element did not have a value!");
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

        Debug.ULogChannel("Character", "Initialized " + stats.Count + " Stats.");
    }
}
