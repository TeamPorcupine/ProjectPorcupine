//=======================================================================
// Copyright Martin "quill18" Glaude 2015.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;


// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa)

[MoonSharpUserData]
public class Furniture : IXmlSerializable, ISelectableInterface {

	/// <summary>
	/// Custom parameter for this particular piece of furniture.  We are
	/// using a dictionary because later, custom LUA function will be
	/// able to use whatever parameters the user/modder would like.
	/// Basically, the LUA code will bind to this dictionary.
	/// </summary>
	protected Dictionary<string, float> furnParameters;

	/// <summary>
	/// These actions are called every update. They get passed the furniture
	/// they belong to, plus a deltaTime.
	/// </summary>
	//protected Action<Furniture, float> updateActions;
	protected List<string> updateActions;

	//public Func<Furniture, ENTERABILITY> IsEnterable;
	protected string isEnterableAction;


	List<Job> jobs;

	// If this furniture gets worked by a person,
	// where is the correct spot for them to stand,
	// relative to the bottom-left tile of the sprite.
	// NOTE: This could even be something outside of the actual
	// furniture tile itself!  (In fact, this will probably be common).
	public Vector2 jobSpotOffset = Vector2.zero;

	// If the job causes some kind of object to be spawned, where will it appear?
	public Vector2 jobSpawnSpotOffset = Vector2.zero;

	public void Update(float deltaTime) {
		if(updateActions != null) {
			//updateActions(this, deltaTime);
			FurnitureActions.CallFunctionsWithFurniture( updateActions.ToArray(), this, deltaTime );
		}
	}

	public ENTERABILITY IsEnterable() {
		if(isEnterableAction == null || isEnterableAction.Length == 0) {
			return ENTERABILITY.Yes;
		}

		//FurnitureActions.CallFunctionsWithFurniture( isEnterableActions.ToArray(), this );

		DynValue ret = FurnitureActions.CallFunction( isEnterableAction, this );

		return (ENTERABILITY)ret.Number;

	}

	// This represents the BASE tile of the object -- but in practice, large objects may actually occupy
	// multile tiles.
	public Tile tile {
		get; protected set;
	}

	// This "objectType" will be queried by the visual system to know what sprite to render for this object
	public string objectType {
		get; protected set;
	}

	private string _Name = null;
	public string Name {
		get {
			if( _Name == null || _Name.Length == 0) {
				return objectType;
			}
			return _Name;
		}
		set {
			_Name = value;
		}
	}

	// This is a multipler. So a value of "2" here, means you move twice as slowly (i.e. at half speed)
	// Tile types and other environmental effects may be combined.
	// For example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
	// would have a total movement cost of (2+3+3 = 8), so you'd move through this tile at 1/8th normal speed.
	// SPECIAL: If movementCost = 0, then this tile is impassible. (e.g. a wall).
	public float movementCost { get; protected set; }

	public bool roomEnclosure { get; protected set; }

	// For example, a sofa might be 3x2 (actual graphics only appear to cover the 3x1 area, but the extra row is for leg room.)
	public int Width { get; protected set; }
	public int Height { get; protected set; }

	public Color tint = Color.white;

	public bool linksToNeighbour{
		get; protected set;
	}

	public Action<Furniture> cbOnChanged;
	public Action<Furniture> cbOnRemoved;

	Func<Tile, bool> funcPositionValidation;

	// TODO: Implement larger objects
	// TODO: Implement object rotation

	// Empty constructor is used for serialization
	public Furniture() {
		updateActions = new List<string>();
		furnParameters = new Dictionary<string, float>();
		jobs = new List<Job>();
		this.funcPositionValidation = this.DEFAULT__IsValidPosition;
		this.Height = 1;
		this.Width = 1;
	}

	// Copy Constructor -- don't call this directly, unless we never
	// do ANY sub-classing. Instead use Clone(), which is more virtual.
	protected Furniture( Furniture other ) {
		this.objectType = other.objectType;
		this.Name = other.Name;
		this.movementCost = other.movementCost;
		this.roomEnclosure = other.roomEnclosure;
		this.Width = other.Width;
		this.Height = other.Height;
		this.tint = other.tint;
		this.linksToNeighbour = other.linksToNeighbour;

		this.jobSpotOffset = other.jobSpotOffset;
		this.jobSpawnSpotOffset = other.jobSpawnSpotOffset;

		this.furnParameters = new Dictionary<string, float>(other.furnParameters);
		jobs = new List<Job>();

		if(other.updateActions != null)
			this.updateActions = new List<string>( other.updateActions );

		this.isEnterableAction = other.isEnterableAction;

		if(other.funcPositionValidation != null)
			this.funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();

	}

	// Make a copy of the current furniture.  Sub-classed should
	// override this Clone() if a different (sub-classed) copy
	// constructor should be run.
	virtual public Furniture Clone(  ) {
		return new Furniture( this );
	}

	// Create furniture from parameters -- this will probably ONLY ever be used for prototypes
/*	public Furniture ( string objectType, float movementCost = 1f, int width=1, int height=1, bool linksToNeighbour=false, bool roomEnclosure = false ) {
		this.objectType = objectType;
		this.movementCost = movementCost;
		this.roomEnclosure = roomEnclosure;
		this.Width = width;
		this.Height = height;
		this.linksToNeighbour = linksToNeighbour;

		this.funcPositionValidation = this.DEFAULT__IsValidPosition;

		updateActions = new List<string>();

		furnParameters = new Dictionary<string, float>();
	}
*/

	static public Furniture PlaceInstance( Furniture proto, Tile tile ) {
		if( proto.funcPositionValidation(tile) == false ) {
			Debug.LogError("PlaceInstance -- Position Validity Function returned FALSE.");
			return null;
		}

		// We know our placement destination is valid.
		Furniture obj = proto.Clone();
		obj.tile = tile;

		// FIXME: This assumes we are 1x1!
		if( tile.PlaceFurniture(obj) == false ) {
			// For some reason, we weren't able to place our object in this tile.
			// (Probably it was already occupied.)

			// Do NOT return our newly instantiated object.
			// (It will be garbage collected.)
			return null;
		}

		if(obj.linksToNeighbour) {
			// This type of furniture links itself to its neighbours,
			// so we should inform our neighbours that they have a new
			// buddy.  Just trigger their OnChangedCallback.

			Tile t;
			int x = tile.X;
			int y = tile.Y;

			t = World.current.GetTileAt(x, y+1);
			if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
				// We have a Northern Neighbour with the same object type as us, so
				// tell it that it has changed by firing is callback.
				t.furniture.cbOnChanged(t.furniture);
			}
			t = World.current.GetTileAt(x+1, y);
			if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
				t.furniture.cbOnChanged(t.furniture);
			}
			t = World.current.GetTileAt(x, y-1);
			if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
				t.furniture.cbOnChanged(t.furniture);
			}
			t = World.current.GetTileAt(x-1, y);
			if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
				t.furniture.cbOnChanged(t.furniture);
			}

		}

		return obj;
	}

	public void RegisterOnChangedCallback(Action<Furniture> callbackFunc) {
		cbOnChanged += callbackFunc;
	}

	public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc) {
		cbOnChanged -= callbackFunc;
	}

	public void RegisterOnRemovedCallback(Action<Furniture> callbackFunc) {
		cbOnRemoved += callbackFunc;
	}

	public void UnregisterOnRemovedCallback(Action<Furniture> callbackFunc) {
		cbOnRemoved -= callbackFunc;
	}

	public bool IsValidPosition(Tile t) {
		return funcPositionValidation(t);
	}

	// FIXME: These functions should never be called directly,
	// so they probably shouldn't be public functions of Furniture
	// This will be replaced by validation checks fed to use from 
	// LUA files that will be customizable for each piece of furniture.
	// For example, a door might specific that it needs two walls to
	// connect to.
	protected bool DEFAULT__IsValidPosition(Tile t) {
		for (int x_off = t.X; x_off < (t.X + Width); x_off++) {
			for (int y_off = t.Y; y_off < (t.Y + Height); y_off++) {
				Tile t2 = World.current.GetTileAt(x_off, y_off);

				// Make sure tile is FLOOR
				if( t2.Type != TileType.Floor ) {
					return false;
				}

				// Make sure tile doesn't already have furniture
				if( t2.furniture != null ) {
					return false;
				}

			}
		}


		return true;
	}

	public XmlSchema GetSchema() {
		return null;
	}

	public void WriteXml(XmlWriter writer) {
		writer.WriteAttributeString( "X", tile.X.ToString() );
		writer.WriteAttributeString( "Y", tile.Y.ToString() );
		writer.WriteAttributeString( "objectType", objectType );
		//writer.WriteAttributeString( "movementCost", movementCost.ToString() );

		foreach(string k in furnParameters.Keys) {
			writer.WriteStartElement("Param");
			writer.WriteAttributeString("name", k);
			writer.WriteAttributeString("value", furnParameters[k].ToString());
			writer.WriteEndElement();
		}

	}

	public void ReadXmlPrototype(XmlReader reader_parent) {
		//Debug.Log("ReadXmlPrototype");

		objectType = reader_parent.GetAttribute("objectType");

		XmlReader reader = reader_parent.ReadSubtree();


		while(reader.Read()) {
			switch(reader.Name) {
				case "Name":
					reader.Read();
					Name = reader.ReadContentAsString();
					break;
				case "MovementCost":
					reader.Read();
					movementCost = reader.ReadContentAsFloat();
					break;
				case "Width":
					reader.Read();
					Width = reader.ReadContentAsInt();
					break;
				case "Height":
					reader.Read();
					Height = reader.ReadContentAsInt();
					break;
				case "LinksToNeighbours":
					reader.Read();
					linksToNeighbour = reader.ReadContentAsBoolean();
					break;
				case "EnclosesRooms":
					reader.Read();
					roomEnclosure = reader.ReadContentAsBoolean();
					break;
				case "BuildingJob":
					float jobTime = float.Parse( reader.GetAttribute("jobTime") );

					List<Inventory> invs = new List<Inventory>();

					XmlReader invs_reader = reader.ReadSubtree();

					while(invs_reader.Read()) {
						if(invs_reader.Name == "Inventory") {
							// Found an inventory requirement, so add it to the list!
							invs.Add(new Inventory( 
								invs_reader.GetAttribute("objectType"),
								int.Parse( invs_reader.GetAttribute("amount") ),
								0
							));
						}
					}

					Job j = new Job( null, 
						objectType, 
						FurnitureActions.JobComplete_FurnitureBuilding, jobTime, 
						invs.ToArray()
					);

					World.current.SetFurnitureJobPrototype(j, this);

					break;
				case "OnUpdate":

					string functionName = reader.GetAttribute("FunctionName");
					RegisterUpdateAction( functionName );

					break;
				case "IsEnterable":

					isEnterableAction = reader.GetAttribute("FunctionName");

					break;

				case "JobSpotOffset":
					jobSpotOffset = new Vector2(
						int.Parse(reader.GetAttribute("X")),
						int.Parse(reader.GetAttribute("Y"))
					);

					break;
				case "JobSpawnSpotOffset":
					jobSpawnSpotOffset = new Vector2(
						int.Parse(reader.GetAttribute("X")),
						int.Parse(reader.GetAttribute("Y"))
					);

					break;
				case "Params":
					ReadXmlParams(reader);	// Read in the Param tag
					break;
			}
		}


	}

	public void ReadXml(XmlReader reader) {
		// X, Y, and objectType have already been set, and we should already
		// be assigned to a tile.  So just read extra data.

		//movementCost = int.Parse( reader.GetAttribute("movementCost") );

		ReadXmlParams(reader);
	}

	public void ReadXmlParams(XmlReader reader) {
		// X, Y, and objectType have already been set, and we should already
		// be assigned to a tile.  So just read extra data.

		//movementCost = int.Parse( reader.GetAttribute("movementCost") );

		if(reader.ReadToDescendant("Param")) {
			do {
				string k = reader.GetAttribute("name");
				float v = float.Parse( reader.GetAttribute("value") );
				furnParameters[k] = v;
			} while (reader.ReadToNextSibling("Param"));
		}
	}

	/// <summary>
	/// Gets the custom furniture parameter from a string key.
	/// </summary>
	/// <returns>The parameter value (float).</returns>
	/// <param name="key">Key string.</param>
	/// <param name="default_value">Default value.</param>
	public float GetParameter( string key, float default_value ) {
		if( furnParameters.ContainsKey(key) == false ) {
			return default_value;
		}

		return furnParameters[key];
	}

	public float GetParameter( string key ) {
		return GetParameter(key, 0);
	}
		

	public void SetParameter( string key, float value ) {
		furnParameters[key] = value;
	}

	public void ChangeParameter( string key, float value ) {
		if( furnParameters.ContainsKey(key) == false ) {
			furnParameters[key] = value;
		}

		furnParameters[key] += value;
	}

	/// <summary>
	/// Registers a function that will be called every Update.
	/// (Later this implementation might change a bit as we support LUA.)
	/// </summary>
	public void RegisterUpdateAction(string luaFunctionName) {
		updateActions.Add(luaFunctionName);
	}

	public void UnregisterUpdateAction(string luaFunctionName) {
		updateActions.Remove(luaFunctionName);
	}

	public int JobCount() {
		return jobs.Count;
	}

	public void AddJob(Job j) {
		j.furniture = this;
		jobs.Add(j);
		j.RegisterJobStoppedCallback(OnJobStopped);
		World.current.jobQueue.Enqueue(j);
	}

	void OnJobStopped(Job j) {
		RemoveJob(j);
	}

	protected void RemoveJob(Job j) {
		j.UnregisterJobStoppedCallback(OnJobStopped);
		jobs.Remove(j);
		j.furniture = null;
	}

	protected void ClearJobs() {
		Job[] jobs_array = jobs.ToArray();
		foreach(Job j in jobs_array) {
			RemoveJob(j);
		}
	}

	public void CancelJobs() {
		Job[] jobs_array = jobs.ToArray();
		foreach(Job j in jobs_array) {
			j.CancelJob();
		}
	}

	public bool IsStockpile() {
		return objectType == "Stockpile";
	}

	public void Deconstruct() {
		Debug.Log("Deconstruct");

		tile.UnplaceFurniture();

		if(cbOnRemoved != null)
			cbOnRemoved(this);

		// Do we need to recalculate our rooms?
		if(roomEnclosure) {
			Room.DoRoomFloodFill(this.tile);
		}

		World.current.InvalidateTileGraph();

		// At this point, no DATA structures should be pointing to us, so we
		// should get garbage-collected.

	}

	public Tile GetJobSpotTile() {
		return World.current.GetTileAt( tile.X + (int)jobSpotOffset.x, tile.Y + (int)jobSpotOffset.y );
	}

	public Tile GetSpawnSpotTile() {
		return World.current.GetTileAt( tile.X + (int)jobSpawnSpotOffset.x, tile.Y + (int)jobSpawnSpotOffset.y );
	}

	#region ISelectableInterface implementation
	public string GetName() {
		return this.Name;
	}

	public string GetDescription() {
		return "This is a piece of furniture."; // TODO: Add "Description" property and matching XML field.
	}

	public string GetHitPointString() {
		return "18/18";	// TODO: Add a hitpoint system to...well...everything
	}
	#endregion
}
