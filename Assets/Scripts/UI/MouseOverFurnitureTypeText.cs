#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MouseOverFurnitureTypeText implements the abstract class MouseOver.
/// It returns info strings that represent the tiles furniture type.
/// </summary>
public class MouseOverFurnitureTypeText : MouseOver
{
    protected override string GetMouseOverString(Tile tile)
    {
        if (tile != null && tile.Furniture != null)
        {
            return LocalizationTable.GetLocalization("furniture", LocalizationTable.GetLocalization(tile.Furniture.LocalizationCode));
        }
        else
        {
            return string.Empty;
        }
    }
}
