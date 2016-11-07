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
/// MouseOverRoomIndex impliments the abstact class MouseOver.
/// It returns info strings that represent the tiles furniture type.
/// </summary>
public class MouseOverFurnitureTypeText : MouseOver
{
    protected override string GetMouseOverString(Tile tile)
    {
        if (tile != null && tile.Furniture != null)
        {
            return LocalizationTable.GetLocalization("furniture", tile.Furniture.Name);
        }
        else
        {
            return string.Empty;
        }
    }
}
