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
using UnityEngine.UI;

public class MenuWork : MonoBehaviour
{
    public GameObject workerPrefabParent;

    void DestoryAllChilderen()
    {
        GameObject tempGoObj;

        foreach(Transform child in workerPrefabParent.transform) 
        {
            tempGoObj = child.gameObject;
            tempGoObj.transform.parent = null;
            Destroy(tempGoObj);
        }  
    }

    // This can be called everytime a character is add to our list or removed.
    public void MakeWorkerPrefabs(int howMany)
    {
        GameObject tempGoObj;
        WorkerInfo workerInfo;

        // Add a character and disply its nanme.
        for (int i = 0; i < howMany; i++)
        {
            tempGoObj = (GameObject)Instantiate(Resources.Load("Prefab/WorkerPrefab"), workerPrefabParent.transform);
            workerInfo = tempGoObj.GetComponent<WorkerInfo>();
            workerInfo.textCharacter.text = World.Current.characters[i].GetName();
        }   
    }

    void OnEnable()
    {
        int numberOfChatacters = World.Current.characters.Count;

        DestoryAllChilderen();

        MakeWorkerPrefabs(numberOfChatacters);
    }
}