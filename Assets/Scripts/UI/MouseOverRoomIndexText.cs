#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MouseOverRoomIndex impliments the abstact class MouseOver.
/// It returns info strings that represent the tiles room ID.
/// </summary>
public class MouseOverRoomIndexText : MouseOver
{
    protected override string GetMouseOverString(Tile tile)
    {
        string roomID = "N/A";

        if (tile != null && tile.Room != null)
        {
            roomID = tile.Room.ID.ToString();
        }

        return LocalizationTable.GetLocalization("room_index", roomID);
    }
}
