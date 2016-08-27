#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;

public interface ISelectable
{
    bool IsSelected { get; set; }

    string GetName();

    string GetDescription();

    // TODO: Decide whether to allow indestructible thing.
    // For indestructible things (if any) this is allowed to return blank.
    string GetHitPointString();

    string GetJobDescription();
}
