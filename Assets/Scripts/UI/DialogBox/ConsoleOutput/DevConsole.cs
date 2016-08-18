using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;

public class DevConsole : DialogBox {
	public static DevConsole Instance;
	public GameObject TextZone;
    static string outPath;

	Script LuaCommands;
    public override void ShowDialog()
    {
        gameObject.SetActive(true);
    }
    void Start() {
		string filePath = System.IO.Path.Combine( Application.streamingAssetsPath, "LUA" );
		filePath = System.IO.Path.Combine( filePath, "Command.lua" );
		string rawLua = File.ReadAllText(filePath);
			
		LuaCommands = new Script ();
		LuaCommands.DoString (rawLua);
        outPath = System.IO.Path.Combine( Application.streamingAssetsPath, "Output");
        outPath =  System.IO.Path.Combine( outPath, "Output" + System.DateTime.Now.ToString("yy-MM-dd-hh-mm-ss"))+".txt";
		//File.Create (outPath); 
        //this.GetComponentInChildren<InputField> ().onEndEdit += Input();
		Instance = this;
		Log (this, "HEY");
		LogWarning (this, "Oh");
		LogError (this, "NO!");

    }

    public void Log(object message) {
		string msg = message.ToString () + System.Environment.NewLine;
		File.AppendAllText (outPath, msg);
		TextZone.GetComponent<Text> ().text += msg;
    }
	public void Log(object from, object message) {
		string msg = from.ToString () + " : " + message.ToString () + System.Environment.NewLine;
		File.AppendAllText (outPath, msg);
		TextZone.GetComponent<Text> ().text += msg;
	}
	public void LogWarning(object message) {
		string msg = " WARNING :"+message.ToString () + System.Environment.NewLine;
		File.AppendAllText (outPath, msg);
		TextZone.GetComponent<Text> ().text += "<color=#ffff00ff>"+msg+"</color>";
	}
	public void LogWarning (object from, object message) {
		string msg = from.ToString () + " WARNING : " + message.ToString () + System.Environment.NewLine;
		File.AppendAllText (outPath, msg);
		TextZone.GetComponent<Text> ().text += "<color=#ffff00ff>"+msg+"</color>";
	}
	public void LogError(object message) {
		string msg = "ERROR : "+message.ToString () + System.Environment.NewLine;
		File.AppendAllText (outPath, msg);
		TextZone.GetComponent<Text> ().text += "<color=#ff0000ff>" + msg + "</color>";
	}
	public void LogError (object from, object message) {
		string msg = from.ToString () + " ERROR : " + message.ToString () + System.Environment.NewLine;
		File.AppendAllText (outPath, msg);
		TextZone.GetComponent<Text> ().text += "<color=#ff0000ff>" + msg + "</color>";
	}
	public void LogInput(object message){
		string msg = "INPUT : " + message.ToString () + System.Environment.NewLine;
		File.AppendAllText (outPath, msg);
		TextZone.GetComponent<Text> ().text += "<color=#0000ffff>" + msg + "</color>";
	}

	public void DoInput() {
        if ((Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return)) == false) return;
		string Command = this.GetComponentInChildren<InputField> ().text;
        this.GetComponentInChildren<InputField>().text = "";
		LogInput (Command);
        //TODO: GET best output from actions
		try {
			Log(LuaCommands.DoString (Command));
		} catch {
			LogError ("Some Error");
		}
        
	}


}

