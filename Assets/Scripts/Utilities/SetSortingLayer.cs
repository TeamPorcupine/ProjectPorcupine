#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

public class SetSortingLayer : MonoBehaviour
{
    public string sortingLayerName = "default";

    // Use this for initialization
    private void Start()
    {
        GetComponent<Renderer>().sortingLayerName = sortingLayerName;
    }
}
