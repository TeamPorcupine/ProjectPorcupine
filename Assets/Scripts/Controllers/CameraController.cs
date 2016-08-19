using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    [Range(0, 3)]
    public float scrollSpeed = 0.1f;
    
    void Start () {
	
	}
	
	void Update () {
        
        // React to hor./vert. axis (WASD or up/down/...)
        Camera.main.transform.position += Camera.main.orthographicSize * scrollSpeed * new Vector3(Input.GetAxis("Horizontal"),
                                                                        Input.GetAxis("Vertical"), 0);

    }
}
