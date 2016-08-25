﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseCursor
{
    public bool cursorOverride = false;

    private MouseController mc;
    private BuildModeController bmc;
    private CursorInfoDisplay cid;        

    private GameObject cursorGO;
    private SpriteRenderer cursorSR;

    private CursorTextBox upperLeft;
    private CursorTextBox upperRight;
    private CursorTextBox lowerLeft;
    private CursorTextBox lowerRight;

    private Vector3 upperLeftPostion = new Vector3(-64f, 32f, 0);
    private Vector3 upperRightPostion = new Vector3(96f, 32f, 0);
    private Vector3 lowerLeftPostion = new Vector3(-64f, -32f, 0);
    private Vector3 lowerRightPostion = new Vector3(96f, -32f, 0);

    private Vector2 cursorTextBoxSize = new Vector2(120, 50);

    private Texture2D cursorTexture;    

    private GUIStyle style = new GUIStyle();

    private Color characterTint = new Color(1, .7f, 0);
    private Color furnitureTint = new Color(.1f, .7f, .3f);
    private Color defaultTint = Color.white;

    public MouseCursor(MouseController mouseController, BuildModeController buildModeController)
    {
        mc = mouseController;
        bmc = buildModeController;
        cid = new CursorInfoDisplay(mc, bmc);

        style.font = Resources.Load<Font>("Fonts/Arial/Arial") as Font;
        style.fontSize = 15;
        
        LoadCursorTexture();
        BuildCursor();
    }

    public void Update()
    {
        // Hold Ctrl and press M to activate
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.M))
        {
            // Toggle cursorOverride
            if (cursorOverride == false)
            {
                cursorOverride = true;
            }
            else
            {
                cursorOverride = false;
            }                
        }

        ShowCursor();

        if (cursorOverride == true)
        {
            return;
        }

        UpdateCursor();
        DisplayCursorInfo();        
    }    

    private void LoadCursorTexture()
    {
        cursorTexture = Resources.Load<Texture2D>("UI/Cursors/Ship");        
    }

    private void BuildCursor()
    {
        cursorGO = new GameObject();
        cursorGO.name = "CURSOR";
        cursorGO.transform.SetParent(mc.GetCursorParent().transform, true);
        mc.GetCursorParent().name = "Cursor Canvas";

        Canvas cursor_canvas = mc.GetCursorParent().AddComponent<Canvas>();
        cursor_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cursor_canvas.worldCamera = Camera.main;
        cursor_canvas.sortingLayerName = "TileUI";
        cursor_canvas.referencePixelsPerUnit = 411.1f;
        RectTransform rt = mc.GetCursorParent().GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 200);

        CanvasScaler cs = mc.GetCursorParent().AddComponent<CanvasScaler>();
        cs.dynamicPixelsPerUnit = 100.6f;
        cs.referencePixelsPerUnit = 411.1f;

        RectTransform rt1 = cursorGO.AddComponent<RectTransform>();
        rt1.sizeDelta = new Vector2(64, 64);
        cursorSR = cursorGO.AddComponent<SpriteRenderer>();
        cursorSR.sortingLayerName = "TileUI";
       
        Cursor.SetCursor(cursorTexture, new Vector2(0, 0), CursorMode.Auto);
        
        upperLeft = new CursorTextBox(cursorGO, TextAnchor.MiddleRight, style, upperLeftPostion, cursorTextBoxSize);
        upperRight = new CursorTextBox(cursorGO, TextAnchor.MiddleLeft, style, upperRightPostion, cursorTextBoxSize);
        lowerLeft = new CursorTextBox(cursorGO, TextAnchor.MiddleRight, style, lowerLeftPostion, cursorTextBoxSize);
        lowerRight = new CursorTextBox(cursorGO, TextAnchor.MiddleLeft, style, lowerRightPostion, cursorTextBoxSize);        

        Debug.Log("MouseCursor::Cursor Built");
    }   

    private void UpdateCursor()
    {
        cursorGO.transform.position = Input.mousePosition;       
    }
    
    private void ShowCursor()
    {
        if (EventSystem.current.IsPointerOverGameObject() || cursorOverride == true)
        {            
            cursorGO.SetActive(false);
        }
        else
        {            
            cursorGO.SetActive(true);
        }
    }

    private void DisplayCursorInfo()
    {        
        lowerLeft.myText.text = upperLeft.myText.text = lowerRight.myText.text = upperRight.myText.text = string.Empty;        

        Tile t = WorldController.Instance.GetTileAtWorldCoord(mc.GetMousePosition());        
        
        if (mc.GetCurrentMode() == MouseController.MouseMode.BUILD)
        {
            // Placing furniture object
            if (bmc.buildMode == BuildMode.FURNITURE)
            {
                lowerRight.myText.text = World.current.furniturePrototypes[bmc.buildModeObjectType].Name;

                upperLeft.myText.color = Color.green;
                upperRight.myText.color = Color.red;

                // Dragging and placing multiple furniture
                if (t != null && mc.GetIsDragging() == true && mc.GetDragObjects().Count > 1)
                {
                    cid.GetPlacementValidationCounts();
                    upperLeft.myText.text = cid.ValidBuildPositionCount();
                    upperRight.myText.text = cid.InvalidBuildPositionCount();
                    lowerLeft.myText.text = cid.GetCurrentBuildRequirements();
                }
            }
            else if (bmc.buildMode == BuildMode.FLOOR)
            {
                lowerRight.myText.text = string.Empty;
                upperLeft.myText.color = upperRight.myText.color = defaultTint;

                // Placing tiles and dragging
                if (t != null && mc.GetIsDragging() == true && mc.GetDragObjects().Count >= 1)
                {
                    upperLeft.myText.text = mc.GetDragObjects().Count.ToString();
                    lowerLeft.myText.text = bmc.GetFloorTile();
                }                
            }
        }
        else
        {
            lowerRight.myText.text = cid.MousePosition(t);            
        }        
    }

    public class CursorTextBox
    {
        public GameObject textObject;
        public Text myText;
        public RectTransform myRectTranform;

        public CursorTextBox(GameObject parentObject, TextAnchor textAlignment, GUIStyle style, Vector3 localPosition, Vector2 textWidthHeight)
        {
            textObject = new GameObject("Cursor-Text");
            textObject.transform.SetParent(parentObject.transform);
            textObject.transform.localPosition = localPosition;

            myText = textObject.AddComponent<Text>();
            myText.alignment = textAlignment;
            myText.font = style.font;
            myText.fontSize = style.fontSize;

            Outline outline = textObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            myRectTranform = textObject.GetComponentInChildren<RectTransform>();
            myRectTranform.sizeDelta = textWidthHeight;
        }
    }
}
