#region License
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
    public bool cursorOverride = true;

    private MouseController mc;
    private BuildModeController bmc;    

    private GameObject cursorGO;
    private SpriteRenderer cursorSR;

    private CursorTextBox upperLeft;
    private CursorTextBox upperRight;
    private CursorTextBox lowerLeft;
    private CursorTextBox lowerRight;

    private Vector3 upperLeftPostion = new Vector3(-3.5f, 0.75f, 0);
    private Vector3 upperRightPostion = new Vector3(3.5f, 0.75f, 0);
    private Vector3 lowerLeftPostion = new Vector3(-6.5f, -0.75f, 0);
    private Vector3 lowerRightPostion = new Vector3(6.5f, -0.75f, 0);

    private Vector2 size1 = new Vector2(6, 2);
    private Vector2 size2 = new Vector2(12, 2);    

    private Sprite imgCursorSelect;    
    private Sprite imgCursorText;
    private Sprite imgCursorPointer;
    private Sprite imgCursorShip;    

    private string upperLeftString = string.Empty;
    private string upperRightString = string.Empty;
    private string lowerLeftString = string.Empty;
    private string lowerRightString = string.Empty;

    private int validPostionCount;
    private int invalidPositionCount;

    #region Cursor Offsets
    private Vector3 arrowOffset = new Vector3(.5f, -.5f, 0);
    private Vector3 fingerOffset = new Vector3(.1f, -.5f, 0);
    private Vector3 selectionOffset = Vector3.zero;
    #endregion

    private GUIStyle style = new GUIStyle();

    private Color characterTint = new Color(1, .7f, 0);
    private Color furnitureTint = new Color(.1f, .7f, .3f);
    private Color defaultTint = Color.white;

    public MouseCursor(MouseController mouseController, BuildModeController buildModeController)
    {
        mc = mouseController;
        bmc = buildModeController;

        style.font = Resources.Load<Font>("Fonts/Arial/Arial") as Font;
        style.fontSize = 1;
        
        LoadCursorSprites();
        BuildCursor();
    }

    public void Update()
    {
        // Hold Ctrl and press M to activate
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyUp(KeyCode.M))
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
        UpdateCursor();
        DisplayCursorInfo();
    }

    public void MousePosition(Tile t)
    {
        string x = string.Empty;
        string y = string.Empty;

        if (t != null)
        {
            x = t.X.ToString();
            y = t.Y.ToString();

            upperLeftString = "X:" + x + " Y:" + y;
        }
        else
        {
            upperLeftString = string.Empty;
        }        
    }

    public void GetPlacementValidationCounts()
    {
        for (int i = 0; i < mc.GetDragObjects().Count; i++)
        {
            Tile t1 = GetTileUnderDrag(mc.GetDragObjects()[i].transform.position);
            if (WorldController.Instance.world.IsFurniturePlacementValid(bmc.buildModeObjectType, t1) && t1.PendingBuildJob == null)
            {
                validPostionCount++;
            }
            else
            {
                invalidPositionCount++;
            }
        }

        upperLeftString = validPostionCount.ToString(); // + "/" + mm.dragPreviewGOs.Count;
        upperRightString = invalidPositionCount.ToString(); // + "/" + mm.dragPreviewGOs.Count;
    }

    public void GetCurrentBuildRequirements()
    {
        if (World.current.furnitureJobPrototypes != null)
        {
            foreach (string itemName in WorldController.Instance.world.furnitureJobPrototypes[bmc.buildModeObjectType].inventoryRequirements.Keys)
            {
                string temp = (WorldController.Instance.world.furnitureJobPrototypes[bmc.buildModeObjectType].inventoryRequirements[itemName].maxStackSize * validPostionCount).ToString();
                if (WorldController.Instance.world.furnitureJobPrototypes[bmc.buildModeObjectType].inventoryRequirements.Count > 1)
                {
                    lowerLeftString += temp + " " + itemName + "\n";
                }
                else
                {
                    lowerLeftString += temp + " " + itemName;
                }
            }
        }
    }

    private Tile GetTileUnderDrag(Vector3 gameObject_Position)
    {
        return WorldController.Instance.GetTileAtWorldCoord(gameObject_Position);
    }

    private void LoadCursorSprites()
    {
        imgCursorSelect = SpriteManager.current.GetSprite("UI", "CursorSelect");
        imgCursorPointer = SpriteManager.current.GetSprite("UI", "CursorPointer");
        imgCursorText = SpriteManager.current.GetSprite("UI", "CursorText");
        imgCursorShip = SpriteManager.current.GetSprite("UI", "CursorShip");
    }

    private void BuildCursor()
    {
        cursorGO = new GameObject();
        cursorGO.name = "CURSOR";
        cursorGO.transform.SetParent(mc.GetCursorParent().transform, true);

        Canvas cursor_canvas = cursorGO.AddComponent<Canvas>();
        cursor_canvas.renderMode = RenderMode.WorldSpace;
        cursor_canvas.sortingLayerName = "TileUI";
        cursor_canvas.referencePixelsPerUnit = 411.1f;
        RectTransform rt = cursorGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(10, 10);

        CanvasScaler cs = cursorGO.AddComponent<CanvasScaler>();
        cs.dynamicPixelsPerUnit = 100.6f;
        cs.referencePixelsPerUnit = 411.1f;

        cursorSR = cursorGO.AddComponent<SpriteRenderer>();
        cursorSR.sortingLayerName = "TileUI";

        cursorSR.sprite = imgCursorSelect;

        upperLeft = new CursorTextBox(cursorGO, TextAnchor.MiddleRight, style, upperLeftPostion, size1);
        upperRight = new CursorTextBox(cursorGO, TextAnchor.MiddleLeft, style, upperRightPostion, size1);
        lowerLeft = new CursorTextBox(cursorGO, TextAnchor.MiddleRight, style, lowerLeftPostion, size2);
        lowerRight = new CursorTextBox(cursorGO, TextAnchor.MiddleLeft, style, lowerRightPostion, size2);        

        Debug.Log("MouseCursorBuildInfo::Cursor Built");
    }    
   
    private void UpdateCursor()
    {
        Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoord(mc.GetPlacingPosition());
        cursorSR.color = Color.white;

        if (tileUnderMouse != null)
        {
            if (mc.GetCurrentMode() == MouseController.MouseMode.BUILD)
            {
                if (bmc.buildMode == BuildMode.FURNITURE)
                {
                    cursorSR.sprite = null;
                    Furniture proto = World.current.furniturePrototypes[bmc.buildModeObjectType];
                    cursorGO.transform.position = new Vector3(tileUnderMouse.X + ((proto.Width - 1) / 2f), tileUnderMouse.Y + ((proto.Height - 1) / 2f), 0);
                }
                else if (bmc.buildMode == BuildMode.FLOOR)
                {
                    cursorSR.sprite = imgCursorSelect;
                    cursorGO.transform.position = CursorPosition(selectionOffset);
                }
            }
            else if (mc.GetCurrentMode() == MouseController.MouseMode.SELECT)
            {
                cursorSR.sprite = imgCursorShip;

                if (tileUnderMouse.Characters.Count > 0)
                {
                    cursorSR.color = characterTint;
                }
                else if (tileUnderMouse.Furniture != null)
                {
                    cursorSR.color = furnitureTint;
                }                
                
                cursorGO.transform.position = CursorPosition(arrowOffset);
            }            
            else
            {
                cursorSR.sprite = imgCursorPointer;
                cursorSR.color = defaultTint;
                cursorGO.transform.position = CursorPosition(fingerOffset);
            }            
        }
    }

    private Vector3 CursorPosition(Vector3 spriteOffset)
    {
        Vector3 tempPOS = mc.GetPlacingPosition();
        return tempPOS += spriteOffset;        
    }

    private void ShowCursor()
    {
        if (EventSystem.current.IsPointerOverGameObject() || cursorOverride == true)
        {
            Cursor.visible = true;
            cursorGO.SetActive(false);
        }
        else
        {
            Cursor.visible = false;
            cursorGO.SetActive(true);
        }
    }

    private void DisplayCursorInfo()
    {
        upperLeftString = upperRightString = lowerLeftString = lowerRightString = string.Empty;
        validPostionCount = invalidPositionCount = 0;

        Tile t = WorldController.Instance.GetTileAtWorldCoord(mc.GetMousePosition());        
        
        // Placing furniture object
        if (bmc.buildMode == BuildMode.FURNITURE)
        {            
            lowerRightString = World.current.furniturePrototypes[bmc.buildModeObjectType].Name;

            upperLeft.myText.color = Color.green;
            upperRight.myText.color = Color.red;            

            // Dragging and placing multiple furniture
            if (t != null && mc.GetIsDragging() == true && mc.GetDragObjects().Count > 1)
            {
                GetPlacementValidationCounts();               
                GetCurrentBuildRequirements();                
            }
        }
        else
        {
            lowerRightString = string.Empty;
            upperLeft.myText.color = upperRight.myText.color = defaultTint;

            // Placing tiles and dragging
            if (t != null && mc.GetIsDragging() == true && mc.GetDragObjects().Count >= 1)
            {
                upperLeftString = mc.GetDragObjects().Count.ToString();
                lowerLeftString = "mat TEXT"; ////bmc.tileTypetxt;
            }
            else
            {
                MousePosition(t);
            }
        }

        lowerLeft.myText.text = lowerLeftString;
        upperLeft.myText.text = upperLeftString;
        upperRight.myText.text = upperRightString;
        lowerRight.myText.text = lowerRightString;
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

            myRectTranform = textObject.GetComponentInChildren<RectTransform>();
            myRectTranform.sizeDelta = textWidthHeight;
        }
    }
}
