using System.Collections;
using DeveloperConsole;
using UnityEngine;
using UnityEngine.UI;
using ProjectPorcupine.Rooms;

public static class CommandFunctions
{
    /// <summary>
    /// To prevent C# Modding side of commands from getting lost
    /// </summary>
    public static void SetTimeStamp(bool on)
    {
        SettingsKeyHolder.TimeStamps = on;
        DevConsole.Log("Change successful :D", "green");
    }

    public static void ChangeCameraPosition(Vector3 newPos)
    {
        Camera.main.transform.position = newPos;
    }

    /// <summary>
    /// Run the passed lua code.
    /// </summary>
    /// <param name="luaCode"> The LUA Code to run.</param>
    /// <remarks> 
    /// The code isn't optimised since its just a nice little command to run LUA from the command interface.
    /// </remarks>
    public static void Run_LUA(string code)
    {
        new LuaFunctions().LoadScript(code, "User Script");
    }

    public static void SetFontSize(int size)
    {
        if (size < 10)
        {
            DevConsole.LogError("Font size would be too small");
        }
        else if (size > 20)
        {
            DevConsole.LogError("Font size would be too big");
        }
        else
        {
            DevConsole.TextObject().fontSize = size;
            DevConsole.Log("Change successful :D", "green");
        }
    }

    public static void SetText(string text = "")
    {
        DevConsole.TextObject().text = "\n" + text;
    }

    public static void SetCharacterHealth(string name, float health)
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            world.CharacterManager.GetFromName(name).Health.CurrentHealth = health;
        }
    }

    public static void DamageCharacter(string name, float amount)
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            world.CharacterManager.GetFromName(name).Health.DamageEntity(amount);
        }
    }

    // Deprecated, but don't remove.  Since later on we may want this so just create a struct to hold variables since too many
    public static void CharacterHealthSystemSet(string name, float hp, bool overheal, bool healable, bool invincible, bool revivable)
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            HealthSystem health = world.CharacterManager.GetFromName(name).Health;
            health.CanOverheal = overheal;
            health.CurrentHealth = hp;
            health.IsHealable = healable;
            health.IsInvincible = invincible;
            health.IsRevivable = revivable;
        }
    }

    public static void CharacterClearStateQueue(string name)
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            world.CharacterManager.GetFromName(name).ClearStateQueue();
        }
    }

    public static void AllCharactersClearStateQueue()
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            foreach (Character character in world.CharacterManager)
            {
                character.ClearStateQueue();
            }
        }
    }

    public static void AddCurrency(string name, float amount)
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            world.Wallet.AddCurrency(name, amount);
        }
    }

    public static void ConsumeInventory(Vector3 pos, int amount)
    {
        World world;
        Tile t;

        if (ModUtils.GetCurrentWorld(out world) && ModUtils.GetTileAt(pos, out t))
        {
            world.InventoryManager.ConsumeInventory(t, amount);
        }
    }

    public static void PlaceInventory(Vector3 pos, string type, Vector2 stackSizeRange)
    {
        World world;
        Tile t;

        if (ModUtils.GetCurrentWorld(out world) && ModUtils.GetTileAt(pos, out t))
        {
            world.InventoryManager.PlaceInventory(t, new Inventory(type, (int)stackSizeRange.x, (int)stackSizeRange.y));
        }
    }

    public static void PlaceInventoryAmount(string name, int amount, string type, Vector2 stackSizeRange)
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            world.InventoryManager.PlaceInventory(world.CharacterManager.GetFromName(name), new Inventory(type, (int)stackSizeRange.x, (int)stackSizeRange.y), amount);
        }
    }

    public static void RemoveInventoryOfType(string type, int amount, bool onlyFromStockpiles)
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            world.InventoryManager.RemoveInventoryOfType(type, amount, onlyFromStockpiles);
        }
    }

    public static void PlaceFurniture(string type, Vector3 pos, float rotation)
    {
        World world;
        Tile t;

        if (ModUtils.GetCurrentWorld(out world) && ModUtils.GetTileAt(pos, out t))
        {
            world.FurnitureManager.PlaceFurniture(type, t, true, rotation);
        }
    }

    public static void IsWorkSpotClear(string type, Vector3 pos)
    {
        World world;
        Tile t;

        if (ModUtils.GetCurrentWorld(out world) && ModUtils.GetTileAt(pos, out t))
        {
            if (world.FurnitureManager.IsWorkSpotClear(type, t))
            {
                DevConsole.Log("Work spot is clear!", "green");
            }
            else
            {
                DevConsole.LogWarning("Work spot isn't clear!");
            }
        }
    }

    public static void IsPlacementValid(string type, Vector3 pos, float rotation)
    {
        World world;
        Tile t;

        if (ModUtils.GetCurrentWorld(out world) && ModUtils.GetTileAt(pos, out t))
        {
            if (world.FurnitureManager.IsPlacementValid(type, t, rotation))
            {
                DevConsole.Log("Spot is valid!", "green");
            }
            else
            {
                DevConsole.LogWarning("Spot isn't valid!");
            }
        }
    }

    public static void GetTemperature(Vector3 pos)
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            DevConsole.Log("Temperature: " + world.temperature.GetTemperature((int)pos.x, (int)pos.y, (int)pos.z), "green");
        }
    }

    public static void FloodFillRoomAt(Vector3 pos)
    {
        World world;
        Tile t;

        if (ModUtils.GetCurrentWorld(out world) && ModUtils.GetTileAt(pos, out t))
        {
            world.RoomManager.DoRoomFloodFill(t, false, false);
        }
    }

    public static void GetAllRoomIDs()
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            DevConsole.Log("Room IDs:");
            foreach (Room room in world.RoomManager)
            {
                DevConsole.Log("Room " + room.ID, "green");
            }
        }
    }

    public static void Exit()
    {
        DevConsole.Close();
    }

    public static void DevMode(bool isOn)
    {
        SettingsKeyHolder.DeveloperMode = isOn;
    }

    public static void Status()
    {
        DevConsole.Log("Developer Mode is " + (SettingsKeyHolder.DeveloperMode ? "on" : "off"), "yellow");
        DevConsole.Log("Time is " + (TimeManager.Instance.IsPaused ? "paused" : TimeManager.Instance.TimeScale + "x"), "yellow");
    }

    public static void NewCharacter(Vector3 pos, string name = "")
    {
        World world;
        Tile t;

        if (ModUtils.GetCurrentWorld(out world) && ModUtils.GetTileAt(pos, out t))
        {
            Character character = world.CharacterManager.Create(t);

            if (character != null)
            {
                if (name != string.Empty)
                {
                    character.name = name;
                }

                DevConsole.Log("Say hello to: " + character.GetName());
            }
        }
    }

    /// <summary>
    /// Build an object.
    /// </summary>
    /// <param name="buildMode"> Build mode, with int in this order: FLOOR, ROOMBEHAVIOR, FURNITURE, UTILITY, DECONSTRUCT. </param>
    public static void DoBuild(int buildMode, string type, Vector3 pos)
    {
        Tile t;
        if (ModUtils.GetTileAt(pos, out t))
        {
            BuildModeController.Instance.buildMode = (BuildMode)buildMode;
            BuildModeController.Instance.buildModeType = type;
            BuildModeController.Instance.DoBuild(t);

            BuildModeController.Instance.buildModeType = string.Empty;
            BuildModeController.Instance.buildMode = BuildMode.FLOOR;
        }
    }

    public static void DoBuildHelp()
    {
        DevConsole.Log("Does build mode using the furniture/floor/whatever type provided at position pos");
        DevConsole.Log("The options for build mode are: FLOOR = 0, ROOMBEHAVIOUR= 1, FURNITURE= 2, UTILITY = 3, and DECONSTRUCT= 4");
    }

    public static void InvalidateTileGraph()
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            world.InvalidateTileGraph();
        }
    }

    public static void GetCharacterNames()
    {
        World world;

        if (ModUtils.GetCurrentWorld(out world))
        {
            foreach (Character character in world.CharacterManager)
            {
                DevConsole.Log("Say hello to " + character.GetName(), "green");
            }
        }
    }

    public static void SetRoomGas(int roomID, string gas, float pressure)
    {
        // Adding gas to room
        Room room = World.Current.RoomManager[roomID];
        room.Atmosphere.SetGas(gas, pressure * room.TileCount);
    }

    public static void SetAllRoomsGas(string gas, float pressure)
    {
        // Adding gas to all rooms
        foreach (Room room in World.Current.RoomManager)
        {
            if (room.ID > 0)
            {
                room.Atmosphere.SetGas(gas, pressure * room.TileCount);
            }
        }
    }

    public static void FillRoomWithAir(int roomID)
    {
        // Adding air to room
        Room room = World.Current.RoomManager[roomID];
        foreach (string gas in room.Atmosphere.GetGasNames())
        {
            room.Atmosphere.SetGas(gas, 0);
        }

        room.Atmosphere.SetGas("O2", 0.2f * room.TileCount);
        room.Atmosphere.SetGas("N2", 0.8f * room.TileCount);
    }

    public static void FillAllRoomsWithAir()
    {
        // Adding air to all rooms
        foreach (Room room in World.Current.RoomManager)
        {
            foreach (string gas in room.Atmosphere.GetGasNames())
            {
                room.Atmosphere.SetGas(gas, 0);
            }

            if (room.ID > 0)
            {
                room.Atmosphere.SetGas("O2", 0.2f * room.TileCount);
                room.Atmosphere.SetGas("N2", 0.8f * room.TileCount);
            }
        }
    }

    public static void EmptyRoom(int roomId)
    {
        Room room = World.Current.RoomManager[roomId];
        foreach (string gas in room.Atmosphere.GetGasNames())
        {
            room.Atmosphere.SetGas(gas, 0);
        }
    }

    public static void EmptyAllRooms()
    {
        foreach (Room room in World.Current.RoomManager)
        {
            if (room.ID > 0)
            {
                foreach (string gas in room.Atmosphere.GetGasNames())
                {
                    room.Atmosphere.SetGas(gas, 0);
                }
            }
        }
    }
}