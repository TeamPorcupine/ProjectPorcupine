using UnityEngine;
using System.Collections;

public class SetSortingLayer : MonoBehaviour {

	public string sortingLayerName = "default";

	// Use this for initialization
	void Start () {
		GetComponent<Renderer>().sortingLayerName = sortingLayerName;
	}
	
}
