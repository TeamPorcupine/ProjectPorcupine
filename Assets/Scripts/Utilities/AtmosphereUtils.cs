#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using ProjectPorcupine.Rooms;
using UnityEngine;

public static class AtmosphereUtils
{
    public static void EqualizeRooms(Room room1, Room room2, float maxGasToMove)
    {
        Room highPressureRoom, lowPressureRoom;
        if (room1.GetGasPressure() >= room2.GetGasPressure() || room2.IsOutsideRoom())
        {
            highPressureRoom = room1;
            lowPressureRoom = room2;
        }
        else
        {
            highPressureRoom = room2;
            lowPressureRoom = room1;
        }

        float targetPressure = (room1.Atmosphere.GetGasAmount() + room2.Atmosphere.GetGasAmount()) / (room1.TileCount + room2.TileCount);
        float gasNeededForTargetPressure = (highPressureRoom.GetGasPressure() - targetPressure) * highPressureRoom.TileCount;
        float gasMoved = Mathf.Min(maxGasToMove, gasNeededForTargetPressure);

        highPressureRoom.Atmosphere.MoveGasTo(lowPressureRoom.Atmosphere, gasMoved);
    }

    public static void MovePercentageOfAtmosphere(AtmosphereComponent source, AtmosphereComponent destination, float ratio)
    {
        if (ratio < 0 || ratio > 1)
        {
            UnityDebugger.Debugger.Log("MovePercentageOfAtmosphere -- Ratio is out of bounds: " + ratio);
        }

        source.MoveGasTo(destination, ratio * source.GetGasAmount());
    }
}