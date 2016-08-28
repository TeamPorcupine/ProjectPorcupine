#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController
{
    public SelectionInfo mySelection;

    private GameObject cursorParent;    
    private GameObject circleCursorPrefab;    
    private GameObject furnitureParent;

    // The world-position of the mouse last frame.
    private Vector3 lastFramePosition;
    private Vector3 currFramePosition;

    private Vector3 currPlacingPosition;

    // The world-position start of our left-mouse drag operation.
    private Vector3 dragStartPosition;
    private List<GameObject> dragPreviewGameObjects;
    private BuildModeController bmc;
    private FurnitureSpriteController fsc;
    private MenuController menuController;
    private ContextMenu contextMenu;
    private MouseCursor mouseCursor;

    // Is dragging an area (eg. floor tiles).
    private bool isDragging = false;

    // Ìs panning the camera
    private bool isPanning = false;

    private float panningThreshold = .015f;
    private Vector3 panningMouseStart = Vector3.zero;

    private MouseMode currentMode = MouseMode.SELECT;

    // Use this for initialization.
    public MouseController(BuildModeController buildModeController, FurnitureSpriteController furnitureSpriteController, GameObject cursorObject)
    {
        bmc = buildModeController;
        bmc.SetMouseController(this);
        circleCursorPrefab = cursorObject;
        fsc = furnitureSpriteController;
        menuController = GameObject.FindObjectOfType<MenuController>();
        contextMenu = GameObject.FindObjectOfType<ContextMenu>();
        dragPreviewGameObjects = new List<GameObject>();
        cursorParent = new GameObject("Cursor");
        mouseCursor = new MouseCursor(this, bmc);
        furnitureParent = new GameObject("Furniture Preview Sprites");
    }

    public enum MouseMode
    {
        SELECT,
        BUILD,
        SPAWN_INVENTORY
    }

    /// <summary>
    /// Gets the mouse position in world space.
    /// </summary>
    public Vector3 GetMousePosition()
    {
        return currFramePosition;
    }

    public Vector3 GetPlacingPosition()
    {
        return currPlacingPosition;
    }

    public MouseMode GetCurrentMode()
    {
        return currentMode;
    }

    public bool GetIsDragging()
    {
        return isDragging;
    }

    public List<GameObject> GetDragObjects()
    {
        return dragPreviewGameObjects;
    }

    public Tile GetMouseOverTile()
    {
        return WorldController.Instance.GetTileAtWorldCoord(currFramePosition);
    }

    public GameObject GetCursorParent()
    {
        return cursorParent;
    }

    public void StartBuildMode()
    {
        currentMode = MouseMode.BUILD;
    }

    public void StartSpawnMode()
    {
        currentMode = MouseMode.SPAWN_INVENTORY;
    }

    // Update is called once per frame.
    public void Update(bool isModal)
    {
        if (isModal)
        {
            // A modal dialog is open, so don't process any game inputs from the mouse.
            return;
        }

        UpdateCurrentFramePosition();

        CalculatePlacingPosition();
        CheckModeChanges();
        CheckIfContextMenuActivated();

        mouseCursor.Update();
        UpdateDragging();
        UpdateCameraMovement();
        UpdateSelection();
        if (Settings.GetSettingAsBool("DialogBoxSettings_developerModeToggle", false))
        {
            UpdateSpawnClicking();
        }

        // Save the mouse position from this frame.
        // We don't use currFramePosition because we may have moved the camera.
        StoreFramePosition();
    }

    public bool IsCharacterSelected()
    {
        if (mySelection != null)
        {
            return mySelection.IsCharacterSelected();
        }

        return false;
    }

    private void UpdateCurrentFramePosition()
    {
        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;
    }

    private void CheckModeChanges()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1))
        {
            if (currentMode == MouseMode.BUILD)
            {
                isDragging = false;
                currentMode = MouseMode.SELECT;
            }
            else if (currentMode == MouseMode.SPAWN_INVENTORY)
            {
                currentMode = MouseMode.SELECT;
            }
        }
    }

    private void CheckIfContextMenuActivated()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1))
        {
            // Is the context also supposed to open on ESCAPE? That seems wrong
            if (currentMode == MouseMode.SELECT)
            {
                if (contextMenu != null && GetMouseOverTile() != null)
                {
                    if (isPanning)
                    {
                        contextMenu.Close();
                    }
                    else if (contextMenu != null)
                    {
                        contextMenu.Open(GetMouseOverTile());
                    }
                }
            }
        }
    }

    private void StoreFramePosition()
    {
        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;
    }

    private void CalculatePlacingPosition()
    {
        // If we are placing a multitile object we would like to modify the posiotion where the mouse grabs it.
        if (currentMode == MouseMode.BUILD
            && bmc.buildMode == BuildMode.FURNITURE
            && World.Current.furniturePrototypes.ContainsKey(bmc.buildModeObjectType)
            && (World.Current.furniturePrototypes[bmc.buildModeObjectType].Width > 1 ||
            World.Current.furniturePrototypes[bmc.buildModeObjectType].Height > 1))
        {
            // If the furniture has af jobSpot set we would like to use that.
            if (World.Current.furniturePrototypes[bmc.buildModeObjectType].JobSpotOffset.Equals(Vector2.zero) == false)
            {
                currPlacingPosition = new Vector3(
                    currFramePosition.x - World.Current.furniturePrototypes[bmc.buildModeObjectType].JobSpotOffset.x,
                    currFramePosition.y - World.Current.furniturePrototypes[bmc.buildModeObjectType].JobSpotOffset.y,
                    0);
            }
            else
            {   
                // Otherwise we use the center.
                currPlacingPosition = new Vector3(
                    currFramePosition.x - ((World.Current.furniturePrototypes[bmc.buildModeObjectType].Width - 1f) / 2f),
                    currFramePosition.y - ((World.Current.furniturePrototypes[bmc.buildModeObjectType].Height - 1f) / 2f),
                    0);
            }
        }
        else
        {
            currPlacingPosition = currFramePosition;
        }
    }

    private void UpdateSelection()
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

        if (Input.GetMouseButtonUp(0))
        {
            if (contextMenu != null)
            {
                contextMenu.Close();
            }

            // We just release the mouse button, so that's our queue to update our selection.
            Tile tileUnderMouse = GetMouseOverTile();

            if (tileUnderMouse == null)
            {
                // No valid tile under mouse.
                return;
            }

            if (mySelection == null || mySelection.Tile != tileUnderMouse)
            {
                if (mySelection != null)
                {
                    mySelection.GetSelectedStuff().IsSelected = false;
                }

                // We have just selected a brand new tile, reset the info.
                mySelection = new SelectionInfo(tileUnderMouse);
                mySelection.GetSelectedStuff().IsSelected = true;
            }
            else
            {
                // This is the same tile we already have selected, so cycle the subSelection to the next non-null item.
                // Not that the tile sub selection can NEVER be null, so we know we'll always find something.

                // Rebuild the array of possible sub-selection in case characters moved in or out of the tile.
                // [IsSelected] Set our last stuff to be not selected because were selecting the next stuff
                mySelection.GetSelectedStuff().IsSelected = false;
                mySelection.BuildStuffInTile();
                mySelection.SelectNextStuff();
                mySelection.GetSelectedStuff().IsSelected = true;
            }
        }
    }

    private void UpdateDragging()
    {
        CleanUpDragPreviews();

        if (currentMode != MouseMode.BUILD)
        {
            return;
        }

        UpdateIsDragging();

        if (isDragging == false || bmc.IsObjectDraggable() == false)
        {
            dragStartPosition = currPlacingPosition;
        }

        DragParameters dragParams = GetDragParameters();

        ShowPreviews(dragParams);

        // End Drag.
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // If we're over a UI element, then bail out from this.
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            BuildOnDraggedTiles(dragParams);
        }
    }

    private void CleanUpDragPreviews()
    {
        while (dragPreviewGameObjects.Count > 0)
        {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }
    }

    private void UpdateIsDragging()
    {
        // TODO Keyboard input does not belong in MouseController. Move to KeyboardController?
        if (isDragging && (Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.Escape)))
        {
            isDragging = false;
        }
        else if (isDragging == false && Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }
    }

    private DragParameters GetDragParameters()
    {
        int startX = Mathf.FloorToInt(dragStartPosition.x + 0.5f);
        int endX = Mathf.FloorToInt(currPlacingPosition.x + 0.5f);
        int startY = Mathf.FloorToInt(dragStartPosition.y + 0.5f);
        int endY = Mathf.FloorToInt(currPlacingPosition.y + 0.5f);
        return new DragParameters(startX, endX, startY, endY);
    }

    private void ShowPreviews(DragParameters dragParams)
    {
        for (int x = dragParams.StartX; x <= dragParams.EndX; x++)
        {
            for (int y = dragParams.StartY; y <= dragParams.EndY; y++)
            {
                Tile t = WorldController.Instance.World.GetTileAt(x, y);
                if (t != null)
                {
                    // Display the building hint on top of this tile position.
                    if (bmc.buildMode == BuildMode.FURNITURE)
                    {
                        Furniture proto = World.Current.furniturePrototypes[bmc.buildModeObjectType];
                        if (IsPartOfDrag(t, dragParams, proto.DragType))
                        {
                            ShowFurnitureSpriteAtTile(bmc.buildModeObjectType, t);
                        }
                    }
                    else
                    {
                        ShowGenericVisuals(x, y);
                    }
                }
            }
        }
    }

    private void ShowGenericVisuals(int x, int y)
    {
        GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
        go.transform.SetParent(cursorParent.transform, true);
        go.GetComponent<SpriteRenderer>().sprite = SpriteManager.current.GetSprite("UI", "CursorCircle");
        dragPreviewGameObjects.Add(go);
    }

    private void BuildOnDraggedTiles(DragParameters dragParams)
    {
        for (int x = dragParams.StartX; x <= dragParams.EndX; x++)
        {
            for (int y = dragParams.StartY; y <= dragParams.EndY; y++)
            {
                Tile t = WorldController.Instance.World.GetTileAt(x, y);
                if (bmc.buildMode == BuildMode.FURNITURE)
                {
                    // Check for furniture dragType.
                    Furniture proto = World.Current.furniturePrototypes[bmc.buildModeObjectType];

                    if (IsPartOfDrag(t, dragParams, proto.DragType))
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

    // Checks whether a tile is valid for the drag type, given the drag parameters
    // Returns true if tile should be included, false otherwise
    private bool IsPartOfDrag(Tile tile, DragParameters dragParams, string dragType)
    {
        switch (dragType)
        {
            case "border":
                return tile.X == dragParams.StartX || tile.X == dragParams.EndX || tile.Y == dragParams.StartY || tile.Y == dragParams.EndY;
            case "path":
                bool withinXBounds = dragParams.StartX <= tile.X && tile.X <= dragParams.EndX;
                bool onPath = tile.Y == dragParams.RawStartY || tile.X == dragParams.RawEndX;
                return withinXBounds && onPath;
            default:
                return true;
        }
    }

    private void UpdateSpawnClicking()
    {
        if (currentMode != MouseMode.SPAWN_INVENTORY)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            Tile t = GetMouseOverTile();
            WorldController.Instance.spawnInventoryController.SpawnInventory(t);
        }
    }

    private void UpdateCameraMovement()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            panningMouseStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            panningMouseStart.z = 0;
        }

        if (!isPanning)
        {
            Vector3 currentMousePosition;
            currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentMousePosition.z = 0;

            if (Vector3.Distance(panningMouseStart, currentMousePosition) > panningThreshold * Camera.main.orthographicSize)
            {
                isPanning = true;
            }
        }

        // Handle screen panning.
        if (isPanning && (Input.GetMouseButton(1) || Input.GetMouseButton(2)))
        {   // Right or Middle Mouse Button.
            Vector3 diff = lastFramePosition - currFramePosition;

            if (diff != Vector3.zero)
            {
                contextMenu.Close();
                Camera.main.transform.Translate(diff);
            }

            if (Input.GetMouseButton(1))
            {
                isDragging = false;
            }
        }

        if (!Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            isPanning = false;
        }

        // If we're over a UI element or the settings/options menu is open, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject()
            || WorldController.Instance.IsModal)
        {
            return;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            WorldController.Instance.cameraController.ChangeZoom(Input.GetAxis("Mouse ScrollWheel"));
        }
        
        UpdateCameraBounds();
    }

    /// <summary>
    /// Make the camera stay within the world boundaries.
    /// </summary>
    private void UpdateCameraBounds()
    {
        Vector3 oldPos = Camera.main.transform.position;

        oldPos.x = Mathf.Clamp(oldPos.x, 0, (float)World.Current.Width - 1);
        oldPos.y = Mathf.Clamp(oldPos.y, 0, (float)World.Current.Height - 1);

        Camera.main.transform.position = oldPos;
    }

    private void ShowFurnitureSpriteAtTile(string furnitureType, Tile t)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(furnitureParent.transform, true);
        dragPreviewGameObjects.Add(go);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Jobs";
        sr.sprite = fsc.GetSpriteForFurniture(furnitureType);

        if (WorldController.Instance.World.IsFurniturePlacementValid(furnitureType, t) &&
            bmc.DoesBuildJobOverlapExistingBuildJob(t, furnitureType) == false)
        {
            sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        }
        else
        {
            sr.color = new Color(1f, 0.5f, 0.5f, 0.25f);
        }

        Furniture proto = World.Current.furniturePrototypes[furnitureType];

        go.transform.position = new Vector3(t.X + ((proto.Width - 1) / 2f), t.Y + ((proto.Height - 1) / 2f), 0);
    }    

    public class DragParameters
    {
        public DragParameters(int startX, int endX, int startY, int endY)
        {
            this.RawStartX = startX;
            this.RawEndX = endX;
            this.RawStartY = startY;
            this.RawEndY = endY;

            this.StartX = Mathf.Min(startX, endX);
            this.EndX = Mathf.Max(startX, endX);
            this.StartY = Mathf.Min(startY, endY);
            this.EndY = Mathf.Max(startY, endY);
        }

        public int RawStartX { get; private set; }

        public int RawEndX { get; private set; }

        public int RawStartY { get; private set; }

        public int RawEndY { get; private set; }

        public int StartX { get; private set; }

        public int EndX { get; private set; }

        public int StartY { get; private set; }

        public int EndY { get; private set; }
    }
}
