#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using DeveloperConsole.CommandTypes;
using ProjectPorcupine.Rooms;
using UnityEngine;

namespace DeveloperConsole
{
    public static class CoreCommands
    {
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

        public static void Help()
        {
            DevConsole.Log("-- Help --", "green");

            string text = string.Empty;

            CommandBase[] consoleCommands = DevConsole.CommandArray();

            for (int i = 0; i < consoleCommands.Length; i++)
            {
                text += "\n<color=orange>" + consoleCommands[i].Title + DevConsole.GetParameters(consoleCommands[i]) + "</color>" + (consoleCommands[i].DescriptiveText == null ? string.Empty : " //" + consoleCommands[i].DescriptiveText);
            }

            DevConsole.Log(text);

            DevConsole.Log("<color=orange>Note:</color> If the function has no parameters you <color=red> don't</color> need to use the parameter modifier.");
            DevConsole.Log("<color=orange>Note:</color> You <color=red>don't</color> need to use the trailing parameter modifier either");
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

        public static void SetText(string text)
        {
            DevConsole.TextObject().text = "\n" + text;
        }

        /// <summary>
        /// Clears the text area and history.
        /// </summary>
        public static void Clear()
        {
            DevConsole.ClearHistory();
            SetText("\n<color=green>Clear Successful :D</color>");
        }

        public static World GetCurrentWorld()
        {
            return World.Current;
        }

        public static void SetCharacterHealth(string name, float health)
        {
            GetCurrentWorld().CharacterManager.GetFromName(name).Health.CurrentHealth = health;
        }

        public static void DamageCharacter(string name, float amount)
        {
            GetCurrentWorld().CharacterManager.GetFromName(name).Health.DamageEntity(amount);
        }

        // Deprecated, but don't remove.  Since later on we may want this so just create a struct to hold variables since too many
        public static void CharacterHealthSystemSet(string name, float hp, bool overheal, bool healable, bool invincible, bool revivable)
        {
            HealthSystem health = GetCurrentWorld().CharacterManager.GetFromName(name).Health;
            health.CanOverheal = overheal;
            health.CurrentHealth = hp;
            health.IsHealable = healable;
            health.IsInvincible = invincible;
            health.IsRevivable = revivable;
        }

        public static void CharacterClearStateQueue(string name)
        {
            GetCurrentWorld().CharacterManager.GetFromName(name).ClearStateQueue();
        }

        public static void AllCharactersClearStateQueue()
        {
            foreach (Character character in GetCurrentWorld().CharacterManager)
            {
                character.ClearStateQueue();
            }
        }

        public static void AddCurrency(string name, float amount)
        {
            GetCurrentWorld().Wallet.AddCurrency(name, amount);
        }

        public static void ConsumeInventory(Vector3 pos, int amount)
        {
            GetCurrentWorld().InventoryManager.ConsumeInventory(GetTileAt(pos), amount);
        }

        public static void PlaceInventory(Vector3 pos, string type, Vector2 stackSizeRange)
        {
            GetCurrentWorld().InventoryManager.PlaceInventory(GetTileAt(pos), new Inventory(type, (int)stackSizeRange.x, (int)stackSizeRange.y));
        }

        public static void PlaceInventory(string name, int amount, string type, Vector2 stackSizeRange)
        {
            GetCurrentWorld().InventoryManager.PlaceInventory(GetCurrentWorld().CharacterManager.GetFromName(name), new Inventory(type, (int)stackSizeRange.x, (int)stackSizeRange.y), amount);
        }

        public static void RemoveInventoryOfType(string type, int amount, bool onlyFromStockpiles)
        {
            GetCurrentWorld().InventoryManager.RemoveInventoryOfType(type, amount, onlyFromStockpiles);
        }

        public static void PlaceFurniture(string type, Vector3 pos, float rotation)
        {
            GetCurrentWorld().FurnitureManager.PlaceFurniture(type, GetTileAt(pos), true, rotation);
        }

        public static void IsWorkSpotClear(string type, Vector3 pos)
        {
            if (GetCurrentWorld().FurnitureManager.IsWorkSpotClear(type, GetTileAt(pos)))
            {
                DevConsole.Log("Work spot is clear!", "green");
            }
            else
            {
                DevConsole.LogWarning("Work spot isn't clear!");
            }
        }

        public static void IsPlacementValid(string type, Vector3 pos, float rotation)
        {
            if (GetCurrentWorld().FurnitureManager.IsPlacementValid(type, GetTileAt(pos), rotation))
            {
                DevConsole.Log("Spot is valid!", "green");
            }
            else
            {
                DevConsole.LogWarning("Spot isn't valid!");
            }
        }

        public static void GetTemperature(Vector3 pos)
        {
            DevConsole.Log("Temperature: " + GetCurrentWorld().temperature.GetTemperature((int)pos.x, (int)pos.y, (int)pos.z), "green");
        }

        public static void GetThermallDiffusivity(Vector3 pos)
        {
            DevConsole.Log("Thermal Diffusivity: " + GetCurrentWorld().temperature.GetThermalDiffusivity((int)pos.x, (int)pos.y, (int)pos.z), "green");
        }

        public static void FloodFillRoomAt(Vector3 pos)
        {
            GetCurrentWorld().RoomManager.DoRoomFloodFill(GetTileAt(pos), false, false);
        }

        public static void GetAllRoomIDs()
        {
            DevConsole.Log("Room IDs:");
            foreach (Room room in GetCurrentWorld().RoomManager)
            {
                DevConsole.Log("Room " + room.ID, "green");
            }
        }

        /// <summary>
        /// Build an object.
        /// </summary>
        /// <param name="buildMode"> Build mode, with int in this order: FLOOR, ROOMBEHAVIOR, FURNITURE, UTILITY, DECONSTRUCT. </param>
        public static void DoBuild(int buildMode, string type, Vector3 pos)
        {
            BuildModeController.Instance.buildMode = (BuildMode)buildMode;
            BuildModeController.Instance.buildModeType = type;
            BuildModeController.Instance.DoBuild(GetTileAt(pos));

            BuildModeController.Instance.buildModeType = string.Empty;
            BuildModeController.Instance.buildMode = BuildMode.FLOOR;
        }

        public static void DoBuildHelp()
        {
            DevConsole.Log("Does build mode using the furniture/floor/whatever type provided at position pos");
            DevConsole.Log("The options for build mode are: FLOOR = 0, ROOMBEHAVIOUR= 1, FURNITURE= 2, UTILITY = 3, and DECONSTRUCT= 4");
        }

        public static void InvalidateTileGraph()
        {
            GetCurrentWorld().InvalidateTileGraph();
        }

        public static void GetCharacterNames()
        {
            foreach (Character character in GetCurrentWorld().CharacterManager)
            {
                DevConsole.Log("Say hello to " + character.GetName(), "green");
            }
        }

        public static Tile GetTileAt(Vector3 pos)
        {
            return GetCurrentWorld().GetTileAt((int)pos.x, (int)pos.y, (int)pos.z);
        }

        public static void SetRoomGas(int roomID, string gas, float pressure)
        {
            // Adding gas to room
            Room room = World.Current.RoomManager[roomID];
            room.SetGas(gas, pressure * room.TileCount);
        }

        public static void SetAllRoomsGas(string gas, float pressure)
        {
            // Adding gas to all rooms
            foreach (Room room in World.Current.RoomManager)
            {
                if (room.ID > 0)
                {
                    room.SetGas(gas, pressure * room.TileCount);
                }
            }
        }

        public static void FillRoomWithAir(int roomID)
        {
            // Adding air to room
            Room room = World.Current.RoomManager[roomID];
            foreach (string gas in room.GetGasNames())
            {
                room.SetGas(gas, 0);
            }

            room.SetGas("O2", 0.2f * room.TileCount);
            room.SetGas("N2", 0.8f * room.TileCount);
        }

        public static void FillAllRoomsWithAir()
        {
            // Adding air to all rooms
            foreach (Room room in World.Current.RoomManager)
            {
                foreach (string gas in room.GetGasNames())
                {
                    room.SetGas(gas, 0);
                }

                if (room.ID > 0)
                {
                    room.SetGas("O2", 0.2f * room.TileCount);
                    room.SetGas("N2", 0.8f * room.TileCount);
                }
            }
        }

        public static void EmptyRoom(int roomId)
        {
            Room room = World.Current.RoomManager[roomId];
            foreach (string gas in room.GetGasNames())
            {
                room.SetGas(gas, 0);
            }
        }

        public static void EmptyAllRooms()
        {
            foreach (Room room in World.Current.RoomManager)
            {
                if (room.ID > 0)
                {
                    foreach (string gas in room.GetGasNames())
                    {
                        room.SetGas(gas, 0);
                    }
                }
            }
        }
    }
}