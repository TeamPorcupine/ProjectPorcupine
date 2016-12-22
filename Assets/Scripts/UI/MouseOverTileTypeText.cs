#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using System.Linq;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MouseOverRoomIndex impliments the abstact class MouseOver.
/// It returns info strings that represent the tiles type.
/// </summary>
public class MouseOverTileTypeText : MouseOver
{
    protected override string GetMouseOverString(Tile tile)
    {
        string tileType = "N/A";

        if (tile != null)
        {
            tileType = tile.Type.ToString();
        }

        string tileInfo = LocalizationTable.GetLocalization("tile_type", tileType);

        return tileInfo;
    }
}
