﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using DeveloperConsole.CommandTypes;
using System;
using UnityEngine;

namespace DeveloperConsole
{
    public static class CoreCommands
    {
        public static void ChangeCameraPosition(Vector3 newPos)
        {
            Camera.main.transform.position = newPos;
        }

        public static void ShowTimeStamp(bool value)
        {
            CommandSettings.ShowTimeStamp = value;
            DevConsole.Log("Change successful :D", "green");
        }

        /// <summary>
        /// Run the passed lua code.
        /// </summary>
        /// <param name="luaCode"> The LUA Code to run.</param>
        /// <remarks> 
        /// The code isn't vastly optimised since it should'nt be used for any large thing, 
        /// just to run a single command.
        /// </remarks>
        public static void Run_LUA(string code)
        {
            new LuaFunctions().RunText_Unsafe(code);
        }

        public static void Help()
        {
            string text = string.Empty;

            CommandBase[] consoleCommands = DevConsole.CommandArray();

            for (int i = 0; i < consoleCommands.Length; i++)
            {
                text += "\n<color=orange>" + consoleCommands[i].Title + DevConsole.GetParameters(consoleCommands[i]) + "</color>" + (consoleCommands[i].DescriptiveText == null ? string.Empty : " //" + consoleCommands[i].DescriptiveText);
            }

            text += "\n<color=orange>Note:</color> If the function has no parameters you <color=red>don't</color> need to use the parameter modifier.";
            text += "\n<color=orange>Note:</color> You <color=red>don't</color> need to use the trailing parameter modifier either";

            DevConsole.Log("-- Help --" + text);
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

        public static void CharacterHealthSystemSet(string name, float HP, bool overheal, bool healable, bool invincible, bool revivable)
        {
            HealthSystem health = GetCurrentWorld().CharacterManager.GetFromName(name).Health;
            health.CanOverheal = overheal;
            health.CurrentHealth = HP;
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

        static Tile GetTileAt(Vector3 pos)
        {
            return GetCurrentWorld().GetTileAt((int)pos.x, (int)pos.y, (int)pos.z);
        }
    }
}