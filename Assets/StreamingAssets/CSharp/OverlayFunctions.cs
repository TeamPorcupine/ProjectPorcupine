using System.Linq;
using System.Collections;
using DeveloperConsole;
using ProjectPorcupine.Localization;
using UnityEngine.UI;
using UnityEngine;
using ProjectPorcupine.PowerNetwork;

public static class OverlayFunctions
{
    public static int PowerGridAt(Tile tile, World world)
    {
        int gridIndex = 0;

        Utility ut = tile.Utilities.Values.FirstOrDefault(x => x.HasTypeTag("Power"));
        if (ut != null && ut.Grid != null)
        {
            gridIndex = world.PowerNetwork.FindId(ut.Grid) + 1;
        }

        return gridIndex;
    }

    public static int? FluidGridAt(Tile tile, World world)
    {
        int gridIndex = 0;

        Utility ut = tile.Utilities.Values.FirstOrDefault(x => x.HasTypeTag("Fluid"));
        if (ut != null && ut.Grid != null)
        {
            gridIndex = world.FluidNetwork.FindId(ut.Grid) + 1;
        }

        return gridIndex;
    }
}