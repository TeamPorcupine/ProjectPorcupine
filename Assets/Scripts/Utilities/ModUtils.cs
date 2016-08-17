using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public static class ModUtils {

	public static float Clamp01(float value) {
		return Mathf.Clamp01 (value);
	}
}
