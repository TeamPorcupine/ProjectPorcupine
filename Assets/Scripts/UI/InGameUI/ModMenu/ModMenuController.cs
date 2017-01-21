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

public class ModMenuController : MonoBehaviour
{
    public static GameObject Instance;
    public Transform ModParent;

    public void DisableAll()
    {
        ModMenu.DisableAll();
    }

    public void Save()
    {
        ModMenu.Commit(true);
        ModMenu.Save();
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        ModMenu.Reset();
        gameObject.SetActive(false);
    }

    public void Apply()
    {
        ModMenu.Commit();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ModMenu.DisplaySettings(ModParent);
    }
}
