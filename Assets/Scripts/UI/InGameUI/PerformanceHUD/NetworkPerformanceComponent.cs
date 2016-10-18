#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPerformanceComponent : BasePerformanceComponent
{
    public override int priorityID()
    {
        return 2;
    }

    public override void Update()
    {

    }

    public override BasePerformanceComponentUI UIComponent()
    {
        throw new NotImplementedException();
    }

    public override string nameOfComponent()
    {
        return "UI/TextPerformanceComponentUI";
    }
}
