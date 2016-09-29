﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;

public class MainMenuManager : IEnumerable<MainMenuItem>
{
    private static MainMenuManager instance;

    private List<MainMenuItem> menuItems;
    private Dictionary<string, List<MainMenuItem>> itemsToAdd;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainMenu"/> class.
    /// </summary>
    public MainMenuManager()
    {
        instance = this;
        menuItems = new List<MainMenuItem>();
        itemsToAdd = new Dictionary<string, List<MainMenuItem>>();
    }

    /// <summary>
    /// Occurs when a new menu item is added.
    /// </summary>
    public event Action Added;

    /// <summary>
    /// Gets or sets the Main Menu instance.
    /// </summary>
    /// <value>The Main Menu instance.</value>
    public static MainMenuManager Instance
    {
        get
        {
            if (instance == null)
            {
                new MainMenuManager();
            }

            return instance;
        }

        set
        {
            instance = value;
        }
    }

    /// <summary>
    /// Adds the given main menu item at the given position.
    /// </summary>
    /// <param name="menuItem">The main menu item to add.</param>
    /// <param name="position">The position where to place the item.</param>
    public void AddMenuItem(MainMenuItem menuItem, int position = 0)
    {
        menuItems.Insert(position, menuItem);

        AddFromItemsToAdd(menuItem.Key, position);

        if (Added != null)
        {
            Added();
        }
    }

    /// <summary>
    /// Adds a new menu item at the given position.
    /// </summary>
    /// <param name="key">The menu item key.</param>
    /// <param name="callback">The menu item callback. Called when the menu item is clicked.</param>
    /// <param name="position">The position where to place the item.</param>
    public void AddMenuItem(string key, Action callback, int position = 0)
    {
        position = MathUtilities.Clamp(position, 0, menuItems.Count);
        AddMenuItem(new MainMenuItem(key, callback), position);
    }

    /// <summary>
    /// Adds a new menu item behind the menu item with the given key.
    /// </summary>
    /// <param name="key">The menu item key.</param>
    /// <param name="callback">The menu item callback. Called when the menu item is clicked.</param>
    /// <param name="afterKey">PLace the given menu item after a menu item with this key.</param>
    /// <param name="addLater">If true and if there are no menu items with the given key, the menu item will be added later.</param>
    public void AddMenuItem(string key, Action callback, string afterKey, bool addLater = false)
    {
        int position = menuItems.FindIndex((mi) => { return mi.Key == afterKey; });
        if (position == -1 && addLater == false)
        {
            position = menuItems.Count;
        }

        if (position > -1)
        {
            AddMenuItem(key, callback, position + 1);
        }
        else
        {
            MainMenuItem menuItem = new MainMenuItem(key, callback);
            if (itemsToAdd.ContainsKey(afterKey) == false)
            {
                itemsToAdd[afterKey] = new List<MainMenuItem>();
            }

            itemsToAdd[afterKey].Add(menuItem);
        }
    }

    /// <summary>
    /// Gets the menu items enumerator.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator GetEnumerator()
    {
        return menuItems.GetEnumerator();
    }

    /// <summary>
    /// Gets each menu item.
    /// </summary>
    /// <returns>Each menu item.</returns>
    IEnumerator<MainMenuItem> IEnumerable<MainMenuItem>.GetEnumerator()
    {
        foreach (MainMenuItem menuItem in menuItems)
        {
            yield return menuItem;
        }
    }

    /// <summary>
    /// Destroy this instance.
    /// </summary>
    public void Destroy()
    {
        instance = null;
    }

    /// <summary>
    /// Adds the menu items from items to add that go after the given key.
    /// </summary>
    /// <param name="key">The key of the menu item just added.</param>
    /// <param name="position">The position of the menu item just added.</param>
    private void AddFromItemsToAdd(string key, int position)
    {
        if (itemsToAdd.ContainsKey(key) && itemsToAdd[key].Count > 0)
        {
            foreach (MainMenuItem menuItem in itemsToAdd[key])
            {
                AddMenuItem(menuItem, position + 1);
            }
        }
    }
}
