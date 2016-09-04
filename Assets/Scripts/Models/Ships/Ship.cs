#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public enum BerthDirection
{
    NORTH, SOUTH, EAST, WEST
}

public enum ShipState
{
    TRANSIT, BERTHED
}

public class Ship
{
    private List<ShipStorage> storages;
    private string[,] tileTypes;
    private string[,] furnitureTypes;

    public Ship()
    {
        ShipType = null;
        Width = 0;
        Height = 0;
        BerthPointX = 0;
        BerthPointY = 0;
        BerthDirection = BerthDirection.NORTH;
        storages = new List<ShipStorage>();
        tileTypes = null;
        furnitureTypes = null;
    }

    public Ship(Ship proto)
    {
        ShipType = proto.ShipType;
        Width = proto.Width;
        Height = proto.Height;
        BerthPointX = proto.BerthPointX;
        BerthPointY = proto.BerthPointY;
        BerthDirection = proto.BerthDirection;
        storages = new List<ShipStorage>();
        foreach (ShipStorage s in proto.storages)
        {
            this.storages.Add(new ShipStorage(s.X, s.Y));
        }

        InstantiateTiles();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tileTypes[x, y] = proto.tileTypes[x, y];
                furnitureTypes[x, y] = proto.furnitureTypes[x, y];
            }
        }
    }

    public string ShipType { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int BerthPointX { get; private set; }

    public int BerthPointY { get; private set; }

    public BerthDirection BerthDirection { get; private set; }

    public void ReadXmlPrototype(XmlReader parentReader)
    {
        ShipType = parentReader.GetAttribute("type");
        Width = int.Parse(parentReader.GetAttribute("width"));
        Height = int.Parse(parentReader.GetAttribute("height"));

        InstantiateTiles();

        XmlReader reader = parentReader.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "BerthPoint":
                    BerthPointX = int.Parse(reader.GetAttribute("x"));
                    BerthPointY = int.Parse(reader.GetAttribute("y"));
                    BerthDirection = (BerthDirection)Enum.Parse(typeof(BerthDirection), reader.GetAttribute("direction"));
                    break;
                case "Storages":
                    ReadXmlStorages(reader);
                    break;
                case "Tiles":
                    ReadXmlTiles(reader);
                    break;
                case "Furnitures":
                    ReadXmlFurnitures(reader);
                    break;
            }
        }
    }

    private void ReadXmlStorages(XmlReader reader)
    {
        if (reader.ReadToDescendant("Storage"))
        {
            // We have at least one tile, so do something with it.
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("x"));
                ShipStorage storage = new ShipStorage(x, y);
                storages.Add(storage);
            }
            while (reader.ReadToNextSibling("Storage"));
        }
    }

    private void ReadXmlTiles(XmlReader reader)
    {
        if (reader.ReadToDescendant("Tile"))
        {
            // We have at least one tile, so do something with it.
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                tileTypes[x, y] = reader.GetAttribute("type");
            }
            while (reader.ReadToNextSibling("Tile"));
        }
    }

    private void ReadXmlFurnitures(XmlReader reader)
    {
        if (reader.ReadToDescendant("Furniture"))
        {
            // We have at least one tile, so do something with it.
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                furnitureTypes[x, y] = reader.GetAttribute("type");
            }
            while (reader.ReadToNextSibling("Furniture"));
        }
    }

    private void InstantiateTiles()
    {
        tileTypes = new string[Width, Height];
        furnitureTypes = new string[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tileTypes[x, y] = null;
                furnitureTypes[x, y] = null;
            }
        }
    }
}