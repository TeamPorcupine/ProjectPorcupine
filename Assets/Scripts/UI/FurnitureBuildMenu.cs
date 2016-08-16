using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FurnitureBuildMenu : MonoBehaviour {

	public GameObject buildFurnitureButtonPrefab;

	// Use this for initialization
	void Start () {

		BuildModeController bmc = GameObject.FindObjectOfType<BuildModeController>();
	
		// For each furniture prototype in our world, create one instance
		// of the button to be clicked!


		foreach( string s in World.current.furniturePrototypes.Keys ) {
			GameObject go = (GameObject)Instantiate(buildFurnitureButtonPrefab);
			go.transform.SetParent(this.transform);

			string objectId = s;
			string objectName = World.current.furniturePrototypes[s].Name;

			go.name = "Button - Build " + objectId;

			go.transform.GetComponentInChildren<Text>().text = "Build " + objectName;

			Button b = go.GetComponent<Button>();

			b.onClick.AddListener( delegate { bmc.SetMode_BuildFurniture(objectId); } );

		}

	}
	
}
