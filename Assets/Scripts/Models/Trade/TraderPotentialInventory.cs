#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

public class TraderPotentialInventory
{
    private float rarity;

    public string Category { get; set; }

    public string Type { get; set; }

    public int MinQuantity { get; set; }

    public int MaxQuantity { get; set; }

    /// <summary>
    /// Value from 0 to 1, higher value represent higher availability of the trade resource.
    /// </summary>
    public float Rarity
    {
        get
        {
            return rarity;
        }

        set
        {
            rarity = Mathf.Clamp(value, 0f, 1f);
        }
    }
}