//=======================================================================
// Copyright Martin "quill18" Glaude 2015.
//		http://quill18.com
//=======================================================================

using System;
using System.Linq;
using UnityEngine;

using System.Collections.Generic;
using MoonSharp.Interpreter;
public class LuaController : MonoBehaviour
{
    
    public static LuaController current;
    Script LuaScripts;
    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    void Awake () {
        LuaScripts = new Script();
        current = this;
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        LoadLuaFromDirectory(filePath);

    }
    void LoadLuaFromDirectory(string path) {
        foreach(string subDirectory in System.IO.Directory.GetDirectories(path)) {
            LoadLuaFromDirectory(subDirectory);
        }
        foreach (string file in System.IO.Directory.GetFiles(path)) {
            if (file.Contains(".lua"))                
                LuaScripts.DoString(System.IO.File.ReadAllText(file));
        }
    }
    public void AddCode(string RawCode) {
        if (LuaScripts == null)
            LuaScripts = new Script();
        LuaScripts.DoString(RawCode);
    }
    public DynValue Call(string functionName,params object[] args) {
        object func = getGlobal(functionName);
        return LuaScripts.Call(func, args);
    }
    public DynValue Call(object functionName,params object[] args) {
        return LuaScripts.Call(functionName, args);
    }
    public void SetGlobal(string name, object value) {
        LuaScripts.Globals[name] = value;
    }
    public object getGlobal(string name) {
        return LuaScripts.Globals[name];
    }
}
