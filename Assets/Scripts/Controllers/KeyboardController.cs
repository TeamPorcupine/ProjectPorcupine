#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections;

public class KeyboardController : MonoBehaviour {

    [Range(0, 3)]
    public float scrollSpeed = 0.1f;
	
    void Update () {
        
        // React to hor./vert. axis (WASD or up/down/...)
        Camera.main.transform.position +=
            Camera.main.orthographicSize * scrollSpeed *
            new Vector3(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                0
            );

    }
}
