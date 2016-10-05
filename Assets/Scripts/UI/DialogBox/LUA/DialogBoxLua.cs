#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

[MoonSharpUserData]
public class DialogBoxLua : DialogBox
{
    private string title;

    private List<object> extraData;

    private EventActions events;

    public Transform Content { get; protected set; }

    public DialogBoxResult Result { get; set; }

    public string Title
    {
        get
        {
            return title;
        }

        protected set
        {
            title = value;
            transform.GetChild(0).GetChild(0).GetComponentInChildren<Text>().text = value;
        }
    }
    
    public override void ShowDialog()
    {
        base.ShowDialog();
    }

    public void YesButtonClicked()
    {
        Result = DialogBoxResult.Yes;
        CloseDialog();
    }

    public void NoButtonClicked()
    {
        Result = DialogBoxResult.No;
        CloseDialog();
    }

    public void CancelButtonClicked()
    {
        Result = DialogBoxResult.Cancel;
        CloseDialog();
    }

    public void OkButtonClicked()
    {
        Result = DialogBoxResult.Okay;
        CloseDialog();
    }

    public override void CloseDialog()
    {
        foreach(GameObject control in Content.transform)
        {
            if(control.GetComponent<DialogControl>() == true)
            {
                extraData.Add(control.GetComponent<DialogControl>().result);
            }
        }
        events.Trigger("OnClosed", this, Result, extraData);
        base.CloseDialog();
    }

    public void LoadFromXML(FileInfo file)
    {
        // TODO: Find a better way to do this. Not user friendly/Expansible.
        // DialogBoxLua -> Dialog Background
        //                 |-> Title
        //                 |-> Content
        Content = transform.GetChild(0).GetChild(1);

        XmlSerializer serializer = new XmlSerializer(typeof(DialogBoxLuaInformation));

        try
        {
            DialogBoxLuaInformation dialogBoxInfo = (DialogBoxLuaInformation)serializer.Deserialize(file.OpenRead());
            Title = dialogBoxInfo.title;
            foreach (DialogComponent gameObjectInfo in dialogBoxInfo.content)
            {
                // Implement new DialogComponents in here.
                switch (gameObjectInfo.ObjectType)
                {
                    case "Text":
                        GameObject textObject = (GameObject)Instantiate(Resources.Load("Prefab/DialogBoxPrefabs/DialogText"), Content);
                        textObject.GetComponent<Text>().text = (string)gameObjectInfo.data;
                        textObject.GetComponent<RectTransform>().anchoredPosition = gameObjectInfo.position;
                        break;
                    case "Input":
                        GameObject inputObject = (GameObject)Instantiate(Resources.Load("Prefab/DialogBoxPrefabs/DialogInput"), Content);
                        inputObject.GetComponent<RectTransform>().anchoredPosition = gameObjectInfo.position;
                        break;
                }
            }

            foreach (DialogBoxResult buttons in dialogBoxInfo.buttons)
            {
                switch (buttons)
                {
                    case DialogBoxResult.Yes:
                        gameObject.transform.GetChild(0).transform.Find("Buttons/btnYes").gameObject.SetActive(true);
                        break;
                    case DialogBoxResult.No:
                        gameObject.transform.GetChild(0).transform.Find("Buttons/btnNo").gameObject.SetActive(true);
                        break;
                    case DialogBoxResult.Cancel:
                        gameObject.transform.GetChild(0).transform.Find("Buttons/btnCancel").gameObject.SetActive(true);
                        break;
                    case DialogBoxResult.Okay:
                        gameObject.transform.GetChild(0).transform.Find("Buttons/btnOK").gameObject.SetActive(true);
                        break;
                }
            }

            events = dialogBoxInfo.events;
            FunctionsManager.Get("DialogBoxLua").RegisterGlobal(typeof(DialogBoxLua));
            extraData = new List<object>();
        }
        catch (System.Exception error)
        {
            Debug.ULogErrorChannel("DialogBoxLua", "Error deserializing data:" + error.Message);
        }

        // Temporary testing serializer... will remove later
        /*DialogBoxLuaInformation info = new DialogBoxLuaInformation();
        info.title = "Testing";

        DialogBoxResult[] buttons = new DialogBoxResult[3];
        buttons[0] = DialogBoxResult.Yes;
        buttons[1] = DialogBoxResult.No;
        buttons[2] = DialogBoxResult.Cancel;
        info.buttons = buttons;

        DialogComponent[] content = new DialogComponent[1];
        DialogComponent text = new DialogComponent();
        text.ObjectType = "Text";
        text.position = new Vector3(100, -50);
        text.data = "Hello!";
        content[0] = text;
        info.content = content;

        EventActions newEvents = new EventActions();
        newEvents.Register("OnClosed", "Testing_DialogClosed");
        info.events = newEvents;

        serializer.Serialize(file.OpenWrite(), info);
        */
    }
}
