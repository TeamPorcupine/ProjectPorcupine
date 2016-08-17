using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AutomaticHorizontalSize))]
public class AutomaticHorizontalSizeEditor : Editor {

	public override void OnInspectorGUI () {
		 
		DrawDefaultInspector();

		if( GUILayout.Button("Recalc Size") ) {
            AutomaticHorizontalSize myScript = ((AutomaticHorizontalSize)target);
			myScript.AdjustSize();
		}

	}

}
