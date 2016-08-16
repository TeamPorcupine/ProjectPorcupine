using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {
	float soundCooldown = 0;

	// Use this for initialization
	void Start () {
		WorldController.Instance.world.RegisterFurnitureCreated( OnFurnitureCreated );

		WorldController.Instance.world.RegisterTileChanged( OnTileChanged );
	}
	
	// Update is called once per frame
	void Update () {
		soundCooldown -= Time.deltaTime;
	}

	void OnTileChanged( Tile tile_data ) {
		// FIXME

		if(soundCooldown > 0)
			return;

		AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
		AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
		soundCooldown = 0.1f;
	}

	public void OnFurnitureCreated( Furniture furn ) {
		// FIXME
		if(soundCooldown > 0)
			return;
		
		AudioClip ac = Resources.Load<AudioClip>("Sounds/"+ furn.objectType +"_OnCreated");

		if(ac == null) {
			// WTF?  What do we do?
			// Since there's no specific sound for whatever Furniture this is, just
			// use a default sound -- i.e. the Wall_OnCreated sound.
			ac = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
		}

		AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
		soundCooldown = 0.1f;
	}
}
