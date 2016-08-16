//=======================================================================
// Copyright Martin "quill18" Glaude 2015-2016.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections;
using System;
using MoonSharp.Interpreter;


// Inventory are things that are lying on the floor/stockpile, like a bunch of metal bars
// or potentially a non-installed copy of furniture (e.g. a cabinet still in the box from Ikea)


[MoonSharpUserData]
public class Inventory : ISelectableInterface {
	public string objectType = "Steel Plate";
	public int maxStackSize = 50;

	protected int _stackSize = 1;
	public int stackSize {
		get { return _stackSize; }
		set {
			if( _stackSize != value ) {
				_stackSize = value;
				if( cbInventoryChanged != null) {
					cbInventoryChanged(this);
				}
			}
		}
	}

	// The function we callback any time our tile's data changes
	Action<Inventory> cbInventoryChanged;

	public Tile tile;
	public Character character;

	public Inventory() {
		
	}

	static public Inventory New(string objectType, int maxStackSize, int stackSize) {
		return new Inventory(objectType, maxStackSize, stackSize);
	}

	public Inventory(string objectType, int maxStackSize, int stackSize) {
		this.objectType   = objectType;
		this.maxStackSize = maxStackSize;
		this.stackSize    = stackSize;
	}

	protected Inventory(Inventory other) {
		objectType   = other.objectType;
		maxStackSize = other.maxStackSize;
		stackSize    = other.stackSize;
	}

	public virtual Inventory Clone() {
		return new Inventory(this);
	}

	public void RegisterChangedCallback(Action<Inventory> callback) {
		cbInventoryChanged += callback;
	}

	public void UnregisterChangedCallback(Action<Inventory> callback) {
		cbInventoryChanged -= callback;
	}


	#region ISelectableInterface implementation
	public string GetName() {
		return this.objectType;
	}

	public string GetDescription() {
		return "A stack of inventory.";
	}
	public string GetHitPointString()
	{
		return "";	// Does inventory have hitpoints? How does it get destroyed? Maybe it's just a percentage chance based on damage.
	}
	#endregion
}
