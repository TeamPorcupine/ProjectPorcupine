#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

public class ShipStorage
{
    public ShipStorage(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public int X { get; private set; }

    public int Y { get; private set; }
}