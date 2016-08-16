using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum BuildMode {
	FLOOR,
	FURNITURE,
	DECONSTRUCT
}

public class BuildModeController : MonoBehaviour {

	public BuildMode     buildMode = BuildMode.FLOOR;
	TileType buildModeTile = TileType.Floor;
	public string   buildModeObjectType;



	// Use this for initialization
	void Start () {


	}

	public bool IsObjectDraggable() {
		if(buildMode == BuildMode.FLOOR || buildMode == BuildMode.DECONSTRUCT) {
			// floors are draggable
			return true;
		}

		Furniture proto = WorldController.Instance.world.furniturePrototypes[buildModeObjectType];

		return proto.Width==1 && proto.Height==1;

	}

	public void SetMode_BuildFloor( ) {
		buildMode = BuildMode.FLOOR;
		buildModeTile = TileType.Floor;

		GameObject.FindObjectOfType<MouseController>().StartBuildMode();
	}
	
	public void SetMode_Bulldoze( ) {
		buildMode = BuildMode.FLOOR;
		buildModeTile = TileType.Empty;
		GameObject.FindObjectOfType<MouseController>().StartBuildMode();
	}

	public void SetMode_BuildFurniture( string objectType ) {
		// Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
		buildMode = BuildMode.FURNITURE;
		buildModeObjectType = objectType;
		GameObject.FindObjectOfType<MouseController>().StartBuildMode();
	}

	public void SetMode_Deconstruct(  ) {
		buildMode = BuildMode.DECONSTRUCT;
		GameObject.FindObjectOfType<MouseController>().StartBuildMode();
	}

	public void DoPathfindingTest() {
		WorldController.Instance.world.SetupPathfindingExample();
	}

	public void DoBuild( Tile t ) {
		if(buildMode == BuildMode.FURNITURE) {
			// Create the Furniture and assign it to the tile

			// FIXME: This instantly builds the furnite:
			//WorldController.Instance.World.PlaceFurniture( buildModeObjectType, t );

			// Can we build the furniture in the selected tile?
			// Run the ValidPlacement function!

			string furnitureType = buildModeObjectType;

			if( 
				WorldController.Instance.world.IsFurniturePlacementValid( furnitureType, t ) &&
				t.pendingFurnitureJob == null
			) {
				// This tile position is valid for this furniture
				// Create a job for it to be build

				Job j;

				if(WorldController.Instance.world.furnitureJobPrototypes.ContainsKey(furnitureType)) {
					// Make a clone of the job prototype
					j = WorldController.Instance.world.furnitureJobPrototypes[furnitureType].Clone();
					// Assign the correct tile.
					j.tile = t;
				}
				else {
					Debug.LogError("There is no furniture job prototype for '"+furnitureType+"'");
					j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null);
				}

				j.furniturePrototype = WorldController.Instance.world.furniturePrototypes[furnitureType];


				// FIXME: I don't like having to manually and explicitly set
				// flags that preven conflicts. It's too easy to forget to set/clear them!
				t.pendingFurnitureJob = j;
				j.RegisterJobStoppedCallback( (theJob) => { theJob.tile.pendingFurnitureJob = null; } );

				// Add the job to the queue
				WorldController.Instance.world.jobQueue.Enqueue( j );

			}



		}
		else if (buildMode == BuildMode.FLOOR){
			// We are in tile-changing mode.
			t.Type = buildModeTile;
		}
		else if(buildMode == BuildMode.DECONSTRUCT) {
			// TODO
			if(t.furniture != null) {
				t.furniture.Deconstruct();
			}

		}
		else {
			Debug.LogError("UNIMPLMENTED BUILD MODE");
		}

	}


}
