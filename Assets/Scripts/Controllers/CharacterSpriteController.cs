using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour {

	Dictionary<Character, GameObject> characterGameObjectMap;

	World world {
		get { return WorldController.Instance.world; }
	}

	// Use this for initialization
	void Start () {
		// Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
		characterGameObjectMap = new Dictionary<Character, GameObject>();

		// Register our callback so that our GameObject gets updated whenever
		// the tile's type changes.
		world.RegisterCharacterCreated(OnCharacterCreated);

		// Check for pre-existing characters, which won't do the callback.
		foreach(Character c in world.characters) {
			OnCharacterCreated(c);
		}


		//c.SetDestination( world.GetTileAt( world.Width/2 + 5, world.Height/2 ) );
	}

	public void OnCharacterCreated( Character c ) {
//		Debug.Log("OnCharacterCreated");
		// Create a visual GameObject linked to this data.

		// FIXME: Does not consider multi-tile objects nor rotated objects

		// This creates a new GameObject and adds it to our scene.
		GameObject char_go = new GameObject();

		// Add our tile/GO pair to the dictionary.
		characterGameObjectMap.Add( c, char_go );

		char_go.name = "Character";
		char_go.transform.position = new Vector3( c.X, c.Y, 0);
		char_go.transform.SetParent(this.transform, true);

		SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
		sr.sprite = SpriteManager.current.GetSprite("Character", "p1_front");
		sr.sortingLayerName = "Characters";

		// Register our callback so that our GameObject gets updated whenever
		// the object's into changes.
		c.RegisterOnChangedCallback( OnCharacterChanged );

	}

	void OnCharacterChanged( Character c ) {
		//Debug.Log("OnFurnitureChanged");
		// Make sure the furniture's graphics are correct.

		if(characterGameObjectMap.ContainsKey(c) == false) {
			Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
			return;
		}

		GameObject char_go = characterGameObjectMap[c];
		//Debug.Log(furn_go);
		//Debug.Log(furn_go.GetComponent<SpriteRenderer>());

		//char_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

		char_go.transform.position = new Vector3( c.X, c.Y, 0);
	}


	
}
