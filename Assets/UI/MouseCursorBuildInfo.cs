#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MouseCursorBuildInfo {

    GameObject cursor_go;
    SpriteRenderer cursorSR;

    GameObject upperLeftGO;
    GameObject upperRightGO;
    GameObject lowerLeftGO;
    GameObject lowerRightGO;
    
    Sprite imgCircleCursor;

    Text upperLeftText;
    Text upperRightText;
    Text lowerLeftText;
    Text lowerRightText;
    string upperLeftString = "";
    string upperRightString = "";
    string lowerLeftString = "";
    string lowerRightString = "";
    
    MouseController mc;
    BuildModeController bmc;   
    int validPostionCount;
    int invalidPositionCount;

    GUIStyle style = new GUIStyle();

    public MouseCursorBuildInfo(MouseController mouseController, BuildModeController buildModeController)
    {
        mc = mouseController;
        bmc = buildModeController;

        style.font = Resources.Load<Font>("Fonts/Arial/Arial") as Font;
        style.fontSize = 1;

        BuildCursor();        
    }

    public void Update(bool isModal)
    {
        if (isModal)
        {
            // A modal dialog is open, so don't process any game inputs from the mouse.
            return;
        }

        ShowCursor();
        UpdateCursorPosition();
        DisplayCursorInfo();
    }

    void BuildCursor()
    {
        cursor_go = new GameObject();
        cursor_go.name = "CURSOR";
        cursor_go.transform.SetParent(mc.cursorParent.transform, true);

        Canvas cursor_canvas = cursor_go.AddComponent<Canvas>();
        cursor_canvas.renderMode = RenderMode.WorldSpace;
        cursor_canvas.sortingLayerName = "TileUI";
        cursor_canvas.referencePixelsPerUnit = 411.1f;
        RectTransform rt = cursor_go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(10,10);

        CanvasScaler cs = cursor_go.AddComponent<CanvasScaler>();
        cs.dynamicPixelsPerUnit = 100.6f;
        cs.referencePixelsPerUnit = 411.1f;

        upperLeftGO = new GameObject();
        upperLeftGO.name = "CURSOR-upperLeftGO";
        upperLeftGO.transform.SetParent(cursor_go.transform);
        upperLeftGO.transform.localPosition = new Vector3(-3.5f,0.75f,0);
        upperLeftText = upperLeftGO.AddComponent<Text>();
        upperLeftText.font = style.font;
        upperLeftText.fontSize = style.fontSize;
        upperLeftText.alignment = TextAnchor.MiddleRight;
        RectTransform rt1 = upperLeftGO.GetComponentInChildren<RectTransform>();
        rt1.sizeDelta = new Vector2(6, 2);

        upperRightGO = new GameObject();
        upperRightGO.name = "CURSOR-upperRightGO";
        upperRightGO.transform.SetParent(cursor_go.transform);
        upperRightGO.transform.localPosition = new Vector3(3.5f, 0.75f, 0);
        upperRightText = upperRightGO.AddComponent<Text>();
        upperRightText.font = style.font;
        upperRightText.fontSize = style.fontSize;
        upperRightText.alignment = TextAnchor.MiddleLeft;
        RectTransform rt2 = upperRightGO.GetComponentInChildren<RectTransform>();
        rt2.sizeDelta = new Vector2(6, 2);

        lowerLeftGO = new GameObject();
        lowerLeftGO.name = "CURSOR-lowerLeftGO";
        lowerLeftGO.transform.SetParent(cursor_go.transform);
        lowerLeftGO.transform.localPosition = new Vector3(-6.5f, -0.75f, 0);
        lowerLeftText = lowerLeftGO.AddComponent<Text>();
        lowerLeftText.font = style.font;
        lowerLeftText.fontSize = style.fontSize;
        lowerLeftText.alignment = TextAnchor.MiddleRight;
        RectTransform rt3 = lowerLeftGO.GetComponentInChildren<RectTransform>();
        rt3.sizeDelta = new Vector2(12, 2);

        lowerRightGO = new GameObject();
        lowerRightGO.name = "CURSOR-lowerRightGO";
        lowerRightGO.transform.SetParent(cursor_go.transform);
        lowerRightGO.transform.localPosition = new Vector3(6.5f, -0.75f, 0);
        lowerRightText = lowerRightGO.AddComponent<Text>();
        lowerRightText.font = style.font;
        lowerRightText.fontSize = style.fontSize;
        lowerRightText.alignment = TextAnchor.MiddleLeft;
        RectTransform rt4 = lowerRightGO.GetComponentInChildren<RectTransform>();
        rt4.sizeDelta = new Vector2(12, 2);

        cursorSR = cursor_go.AddComponent<SpriteRenderer>();
        cursorSR.sortingLayerName = "TileUI";
        cursorSR.sprite = imgCircleCursor = SpriteManager.current.GetSprite("UI", "CursorSelect");        

        Debug.Log("MouseCursorBuildInfo::Cursor Built");        
    }

    void UpdateCursorPosition()
    {
        Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoord(mc.GetPlacingPosition());

        if (tileUnderMouse != null)
        {
            if (bmc.buildMode == BuildMode.FURNITURE)
            {
                cursorSR.sprite = null;
                Furniture proto = World.current.furniturePrototypes[bmc.buildModeObjectType];
                cursor_go.transform.position = new Vector3(tileUnderMouse.X + ((proto.Width - 1) / 2f), tileUnderMouse.Y + ((proto.Height - 1) / 2f), 0);
            }
            else
            {
                cursorSR.sprite = imgCircleCursor;
                cursor_go.transform.position = mc.GetPlacingPosition();
            }
        }
    }

    void ShowCursor()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.visible = true;
            cursor_go.SetActive(false);
        }
        else
        {
            Cursor.visible = false;
            cursor_go.SetActive(true);
        }
    }

    void DisplayCursorInfo()
    {
        Tile t = WorldController.Instance.GetTileAtWorldCoord(mc.GetMousePosition());
        string _x = "";
        string _y = "";
        upperLeftString = "";
        upperRightString = "";
        lowerLeftString = "";
        lowerRightString = "";
        validPostionCount = 0;
        invalidPositionCount = 0;

        //For placing furniture objects
        if (bmc.buildMode == BuildMode.FURNITURE)
        {
            //ItemNameTxt = bmm.buildModeObjectType;
            lowerRightString = World.current.furniturePrototypes[bmc.buildModeObjectType].Name;

            upperLeftText.color = Color.green;
            upperRightText.color = Color.red;

            for (int i = 0; i < mc.dragPreviewGameObjects.Count; i++)
            {
                Tile t1 = GetTileUnderDrag(mc.dragPreviewGameObjects[i].transform.position);
                if (WorldController.Instance.world.IsFurniturePlacementValid(bmc.buildModeObjectType, t1) && t1.pendingBuildJob == null)
                {
                    // myText.color = Color.green;
                    validPostionCount++;
                    // mm.dragPreviewGOs[i].GetComponent<SpriteRenderer>().color = Color.green;
                }
                else
                {
                    invalidPositionCount++;
                    // mm.dragPreviewGOs[i].GetComponent<SpriteRenderer>().color = Color.red;
                    //myText.color = Color.red;
                }
            }

            if (t != null && mc.isDragging == true && mc.dragPreviewGameObjects.Count > 1)
            {

                upperLeftString = validPostionCount.ToString();// + "/" + mm.dragPreviewGOs.Count;
                upperRightString = invalidPositionCount.ToString();// + "/" + mm.dragPreviewGOs.Count;
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
        }
        else
        {
            upperLeftText.color = Color.white;
            //for placing tiles 
            if (t != null && mc.isDragging == true && mc.dragPreviewGameObjects.Count >= 1)
            {
                upperLeftString = mc.dragPreviewGameObjects.Count.ToString();
                lowerLeftString = "mat TEXT";//bmc.tileTypetxt;
            }
            else
            {
                if (t != null)
                {
                    _x = t.X.ToString();
                    _y = t.Y.ToString();
                }

                upperLeftString = "X:" + _x + " Y:" + _y;
            }
        }

        lowerLeftText.text = lowerLeftString;
        upperLeftText.text = upperLeftString;
        upperRightText.text = upperRightString;
        lowerRightText.text = lowerRightString;
    }    

    public Tile GetTileUnderDrag(Vector3 GO_Position)
    {
        return WorldController.Instance.GetTileAtWorldCoord(GO_Position);
    }
}



//if (bmm.buildModeObjectType != null) {
//if (WorldManager.Instance.world.IsFurniturePlacementVaild(bmm.buildModeObjectType, t) && t.pendingFurnitureJob == null) {
//    validText.color = Color.green;

//}
//else {
//    validText.color = Color.red;
//}