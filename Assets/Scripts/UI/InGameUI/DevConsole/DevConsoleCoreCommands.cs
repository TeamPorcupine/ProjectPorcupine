﻿#region License
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

        public static void PlaceInventory(string name, int amount, string type, Vector2 stackSizeRange)
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

        public static void GetThermallDiffusivity(Vector3 pos)
        {
            World world;

            if (ModUtils.GetCurrentWorld(out world))
            {
                DevConsole.Log("Thermal Diffusivity: " + world.temperature.GetThermalDiffusivity((int)pos.x, (int)pos.y, (int)pos.z), "green");
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
    }
}