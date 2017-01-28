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

/// <summary>
/// Currently not used but may be used in future is just network.
/// </summary>
public class NetworkPerformanceComponent : BasePerformanceHUDElement
{
    public Text UITextElement { get; set; }

    public override void Update()
    {
        UITextElement.text = "0ms";
    }

    //  public override string NameOfComponent()
    //  {
    //      return "UI/TextPerformanceComponentUI";
    //   }

    public override GameObject InitializeElement()
    {
        throw new NotImplementedException();
    }
}
