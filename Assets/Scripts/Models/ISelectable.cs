#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectable
{
    bool IsSelected { get; set; }

    string GetName();

    string GetDescription();
    
    string GetJobDescription();

    IEnumerable<string> GetAdditionalInfo();
}
