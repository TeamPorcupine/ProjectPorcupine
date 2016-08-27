#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;

public class Trader
{
    public string Name;
    public string CurrencyName;
    public float CurrencyBalance;
    public float SaleMarginMultiplier;
    public List<Inventory> Stock;
}
