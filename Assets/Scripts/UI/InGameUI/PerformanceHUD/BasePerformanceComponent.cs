#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

/// <summary>
/// Every PerformanceComponent Derives from this
/// Built with future modding in mind.
/// </summary>
public abstract class BasePerformanceComponent
{
    /// <summary>
    /// Ascending order with the lowest number being a higher prority.
    /// This is the order for components to be displayed in
    /// Could be a short?  But its 16 bytes; Not a huge deal
    /// NOTE: Could be changed by user later on.
    /// </summary>
    public abstract int PriorityID();

    /// <summary>
    /// The name of the gameobject that will be shown.
    /// </summary>
    public abstract string NameOfComponent();

    /// <summary>
    /// The gameobject to be shown (so we don't need to keep creating it/hold copies)
    /// You get the only copy you will ever get in the start function
    /// And you have to store it somewhere to return it in this function.
    /// </summary>
    public abstract BasePerformanceComponentUI UIComponent();

    /// <summary>
    /// Update Action called once per frame.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Start Action.
    /// </summary>
    public abstract void Start(BasePerformanceComponentUI componentUI);
}
