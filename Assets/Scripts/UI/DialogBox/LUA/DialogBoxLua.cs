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

    public Transform Content { get; protected set; }

    public DialogBoxResult Result { get; set; }

    private object extraData;

    private EventActions events;

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
        events.Trigger("OnClosed", this, Result);
        CloseDialog();
    }

    public void NoButtonClicked()
    {
        Result = DialogBoxResult.No;
        events.Trigger("OnClosed", this, Result);
        CloseDialog();
    }

    public void CancelButtonClicked()
    {
        Result = DialogBoxResult.Cancel;
        events.Trigger("OnClosed", this, Result);
        CloseDialog();
    }

    public void OkButtonClicked()
    {
        Result = DialogBoxResult.OK;
        events.Trigger("OnClosed", this, Result);
        CloseDialog();
    }

    public override void CloseDialog()
    {

        events.Trigger("OnClosed", this, Result);
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
            DialogBoxLuaInformation DialogBoxInfo = (DialogBoxLuaInformation)serializer.Deserialize(file.OpenRead());
            Title = DialogBoxInfo.title;
            foreach (DialogComponent goInfo in DialogBoxInfo.content)
            {
                // Implement new DialogComponents in here.
                switch (goInfo.ObjectType)
                {
                    case "Text":
                        GameObject TextObject = (GameObject)Instantiate(Resources.Load("Prefab/DialogBoxPrefabs/DialogText"), Content);
                        TextObject.GetComponent<Text>().text = (string)goInfo.data;
                        TextObject.GetComponent<RectTransform>().anchoredPosition = goInfo.position;
                        break;
                }
            }
            foreach (DialogBoxResult buttons in DialogBoxInfo.buttons)
            {
                switch(buttons)
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
                    case DialogBoxResult.OK:
                        gameObject.transform.GetChild(0).transform.Find("Buttons/btnOK").gameObject.SetActive(true);
                        break;
                }
            }
            events = DialogBoxInfo.events;
            FunctionsManager.Get("DialogBoxLua").RegisterGlobal(typeof(DialogBoxLua));
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
