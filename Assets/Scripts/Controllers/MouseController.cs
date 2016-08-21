#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{

    public GameObject circleCursorPrefab;

    // The world-position of the mouse last frame.
    Vector3 lastFramePosition;
    Vector3 currFramePosition;

    Vector3 currPlacingPosition;

    // The world-position start of our left-mouse drag operation.
    Vector3 dragStartPosition;
    List<GameObject> dragPreviewGameObjects;

    BuildModeController bmc;
    FurnitureSpriteController fsc;
    MenuController menuController;

    bool isDragging = false;

    enum MouseMode
    {
        SELECT,
        BUILD
    }

    MouseMode currentMode = MouseMode.SELECT;

    // Use this for initialization.
    void Start()
    {
        bmc = GameObject.FindObjectOfType<BuildModeController>();
        fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
        menuController = GameObject.FindObjectOfType<MenuController>();
        dragPreviewGameObjects = new List<GameObject>();
    }

    /// <summary>
    /// Gets the mouse position in world space.
    /// </summary>
    public Vector3 GetMousePosition()
    {
        return currFramePosition;
    }

    public Tile GetMouseOverTile()
    {
        return WorldController.Instance.GetTileAtWorldCoord(currFramePosition);
    }

    // Update is called once per frame.
    void Update()
    {
        if (WorldController.Instance.IsModal)
        {
            // A modal dialog is open, so don't process any game inputs from the mouse.
            return;
        }

        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        CalculatePlacingPosition();


        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1))
        {
            if (currentMode == MouseMode.BUILD)
            {
                isDragging = false;
                currentMode = MouseMode.SELECT;
            }
            else if (currentMode == MouseMode.SELECT)
            {
                Logger.Log("Show game menu?");
            }
        }

        UpdateDragging();
        UpdateCameraMovement();
        UpdateSelection();

        // Save the mouse position from this frame.
        // We don't use currFramePosition because we may have moved the camera.
        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;
    }

    public class SelectionInfo
    {
        public Tile tile;
        public ISelectable[] stuffInTile;
        public int subSelection = 0;
    }

    public SelectionInfo mySelection;

    void CalculatePlacingPosition() {
        // If we are placing a multitile object we would like to modify the posiotion where the mouse grabs it.
        if (currentMode == MouseMode.BUILD
            && bmc.buildMode == BuildMode.FURNITURE
            && World.current.furniturePrototypes.ContainsKey(bmc.buildModeObjectType)
            && (World.current.furniturePrototypes[bmc.buildModeObjectType].Width > 1 || 
                World.current.furniturePrototypes[bmc.buildModeObjectType].Height > 1 ))
        {
            // If the furniture has af jobSpot set we would like to use that.
            if(World.current.furniturePrototypes[bmc.buildModeObjectType].jobSpotOffset.Equals(Vector2.zero) == false)
            {
                currPlacingPosition = new Vector3(currFramePosition.x - (World.current.furniturePrototypes[bmc.buildModeObjectType].jobSpotOffset.x) ,
                    currFramePosition.y - (World.current.furniturePrototypes[bmc.buildModeObjectType].jobSpotOffset.y) ,
                    0);
            }
            else
            {   
                // Otherwise we use the center.
                currPlacingPosition = new Vector3(currFramePosition.x - (World.current.furniturePrototypes[bmc.buildModeObjectType].Width - 1f) / 2f ,
                    currFramePosition.y - (World.current.furniturePrototypes[bmc.buildModeObjectType].Height - 1f) / 2f ,
                    0);
            }
        }
        else
        {
            currPlacingPosition = currFramePosition;
        }
    }

    void UpdateSelection()
    {
        // This handles us left-clicking on furniture or characters to set a selection.

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            mySelection = null;
        }
        if (currentMode != MouseMode.SELECT)
        {
            return;
        }

        // If we're over a UI element, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (Input.GetMouseButtonDown(1)) {
            Tile tileUnderMouse = GetMouseOverTile();
            if (tileUnderMouse.pendingBuildJob != null) {
                Debug.Log("Canceling!");
                tileUnderMouse.pendingBuildJob.CancelJob();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            // We just release the mouse button, so that's our queue to update our selection.
            Tile tileUnderMouse = GetMouseOverTile();

            if (tileUnderMouse == null)
            {
                // No valid tile under mouse.
                return;
            }


            if (mySelection == null || mySelection.tile != tileUnderMouse)
            {
                //Logger.Log("new tile");
                // We have just selected a brand new tile, reset the info.
                mySelection = new SelectionInfo();
                mySelection.tile = tileUnderMouse;
                RebuildSelectionStuffInTile();

                // Select the first non-null entry.
                for (int i = 0; i < mySelection.stuffInTile.Length; i++)
                {
                    if (mySelection.stuffInTile[i] != null)
                    {
                        mySelection.subSelection = i;
                        break;
                    }
                }
            }
            else
            {
                // This is the same tile we already have selected, so cycle the subSelection to the next non-null item.
                // Not that the tile sub selection can NEVER be null, so we know we'll always find something.

                // Rebuild the array of possible sub-selection in case characters moved in or out of the tile.
                RebuildSelectionStuffInTile();

                do
                {
                    mySelection.subSelection = (mySelection.subSelection + 1) % mySelection.stuffInTile.Length;
                } while(mySelection.stuffInTile[mySelection.subSelection] == null);
            }
        }
    }

    void RebuildSelectionStuffInTile()
    {

        // Make sure stuffInTile is big enough to handle all the characters, plus the 3 extra values.
        mySelection.stuffInTile = new ISelectable[ mySelection.tile.characters.Count + 3 ];

        // Copy the character references.
        for (int i = 0; i < mySelection.tile.characters.Count; i++)
        {
            mySelection.stuffInTile[i] = mySelection.tile.characters[i];
        }

        // Now assign references to the other three sub-selections available.
        mySelection.stuffInTile[mySelection.stuffInTile.Length - 3] = mySelection.tile.furniture;
        mySelection.stuffInTile[mySelection.stuffInTile.Length - 2] = mySelection.tile.inventory;
        mySelection.stuffInTile[mySelection.stuffInTile.Length - 1] = mySelection.tile;

    }

    void UpdateDragging()
    {
        // If we're over a UI element, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Clean up old drag previews.
        while (dragPreviewGameObjects.Count > 0)
        {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }

        if (currentMode != MouseMode.BUILD)
        {
            return;
        }

        // Start Drag.
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = currPlacingPosition;
            isDragging = true;
        }
        else if (isDragging == false)
        {
            dragStartPosition = currPlacingPosition;
        }

        if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape))
        {
            // The RIGHT mouse button was released, so we
            // are cancelling any dragging/build mode.
            isDragging = false;
        }

        if (bmc.IsObjectDraggable() == false)
        {
            dragStartPosition = currPlacingPosition;
        }

        int start_x = Mathf.FloorToInt(dragStartPosition.x + 0.5f);
        int end_x = Mathf.FloorToInt(currPlacingPosition.x + 0.5f);
        int start_y = Mathf.FloorToInt(dragStartPosition.y + 0.5f);
        int end_y = Mathf.FloorToInt(currPlacingPosition.y + 0.5f);

        // We may be dragging in the "wrong" direction, so flip things if needed.
        if (end_x < start_x)
        {
            int tmp = end_x;
            end_x = start_x;
            start_x = tmp;
        }
        if (end_y < start_y)
        {
            int tmp = end_y;
            end_y = start_y;
            start_y = tmp;
        }

        // Display a preview of the drag area.
        for (int x = start_x; x <= end_x; x++)
        {
            for (int y = start_y; y <= end_y; y++)
            {
                Tile t = WorldController.Instance.world.GetTileAt(x, y);
                if (t != null)
                {
                    // Display the building hint on top of this tile position.

                    if (bmc.buildMode == BuildMode.FURNITURE)
                    {
                        Furniture proto = World.current.furniturePrototypes[bmc.buildModeObjectType];
                        string dragType = proto.dragType;

                        bool isValid = false;

                        // Drag type validation.
                        if (dragType == "border")
                        {
                            if (x == start_x || x == end_x || y == start_y || y == end_y)
                            {
                                isValid = true;
                            } 
                        }
                        else
                        {
                            isValid = true;
                        }

                        if (isValid)
                        {
                            ShowFurnitureSpriteAtTile(bmc.buildModeObjectType, t);
                        }
                    }
                    else
                    {
                        // Show the generic dragging visuals.
                        GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        go.transform.SetParent(this.transform, true);
                        go.GetComponent<SpriteRenderer>().sprite = SpriteManager.current.GetSprite("UI", "CursorCircle");
                        dragPreviewGameObjects.Add(go);
                    }
                }
            }
        }

        // End Drag.
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // Loop through all the tiles.
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    Tile t = WorldController.Instance.world.GetTileAt(x, y);
                    if (bmc.buildMode == BuildMode.FURNITURE)
                    {
                        // Check for furniture dragType.
                        Furniture proto = World.current.furniturePrototypes[bmc.buildModeObjectType];
                        string dragType = proto.dragType;

                        bool isValid = false;

                        // Drag type validation.
                        if (dragType == "border")
                        {
                            if (x == start_x || x == end_x || y == start_y || y == end_y)
                            {
                                isValid = true;
                            } 
                        }
                        else
                        {
                            isValid = true;
                        }

                        if (isValid)
                        {
                            if (t != null)
                            {
                                // Call BuildModeController::DoBuild().
                                bmc.DoBuild(t);
                            }
                        }
                    }
                    else
                    {
                        bmc.DoBuild(t);
                    }
                }
            }
        }
    }

    void UpdateCameraMovement()
    {
        // Handle screen panning.
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {   // Right or Middle Mouse Button.
            Vector3 diff = lastFramePosition - currFramePosition;
            Camera.main.transform.Translate(diff);

            if (Input.GetMouseButton (1)) 
            {
                isDragging = false;
            }
        }

        // If we're over a UI element or the settings/options menu is open, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject()
            || menuController.settingsMenu.activeSelf
            || menuController.optionsMenu.activeSelf)
        {
            return;
        }

        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f);
    }

    void ShowFurnitureSpriteAtTile(string furnitureType, Tile t)
    {

        GameObject go = new GameObject();
        go.transform.SetParent(this.transform, true);
        dragPreviewGameObjects.Add(go);


        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Jobs";
        sr.sprite = fsc.GetSpriteForFurniture(furnitureType);

        if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t) &&
            bmc.DoesBuildJobOverlapExistingBuildJob(t, furnitureType) == false)
        {
            sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        }
        else
        {
            sr.color = new Color(1f, 0.5f, 0.5f, 0.25f);
        }

        Furniture proto = World.current.furniturePrototypes[furnitureType];

        go.transform.position = new Vector3(t.X + ((proto.Width - 1) / 2f), t.Y + ((proto.Height - 1) / 2f), 0);
    }

    public void StartBuildMode()
    {
        currentMode = MouseMode.BUILD;
    }
}
