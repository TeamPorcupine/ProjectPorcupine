#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Every PerformanceComponent Derives from this
/// Built with future modding in mind
/// </summary>
public abstract class BasePerformanceComponent
{
    /// <summary>
    /// Ascending order with the lowest number being a higher prority.
    /// This is the order for components to be displayed in
    /// Could be a short?  But its 16 bytes; Not a huge deal
    /// NOTE: Could be changed by user later on
    /// </summary>
    public abstract int priorityID();

    /// <summary>
    /// The name of the gameobject that will be shown
    /// </summary>
    public abstract string nameOfComponent();

    /// <summary>
    /// The gameobject to be shown (so we don't need to keep creating it/hold copies)
    /// </summary>
    public abstract BasePerformanceComponentUI UIComponent();

    /// <summary>
    /// Update Action
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Start Action
    /// </summary>
    public virtual void Start(BasePerformanceComponentUI UIComponent) { }
}
