#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.IO;
using System.Threading;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scheduler;
using UnityEngine;
using Random = UnityEngine.Random;

[MoonSharpUserData]
public class WorldController : MonoBehaviour
{
    public SoundController soundController;
    public TileSpriteController tileSpriteController;
    public CharacterSpriteController characterSpriteController;
    public JobSpriteController jobSpriteController;
    public InventorySpriteController inventorySpriteController;
    public ShipSpriteController shipSpriteController;
    public FurnitureSpriteController furnitureSpriteController;
    public UtilitySpriteController utilitySpriteController;
    public QuestController questController;
    public BuildModeController buildModeController;
    public MouseController mouseController;
    public CameraController cameraController;
    public SpawnInventoryController spawnInventoryController;
    public AutosaveManager autosaveManager;
    public TradeController TradeController;
    public ModsManager modsManager;
    public DialogBoxManager dialogBoxManager;
    public GameObject inventoryUI;
    public GameObject circleCursorPrefab;

    public static WorldController Instance { get; protected set; }

    // The world and tile data.
    public World World { get; protected set; }

    // Use this for initialization.
    public void OnEnable()
    {
        Debug.IsLogEnabled = true;
        if (Instance != null)
        {
            Debug.ULogErrorChannel("WorldController", "There should never be two world controllers.");
        }

        Instance = this;

        new FunctionsManager();
        new PrototypeManager();
        new CharacterNameManager();
        new SpriteManager();
        new AudioManager();

        // FIXME: Do something real here. This is just to show how to register a C# event prototype for the Scheduler.
        PrototypeManager.ScheduledEvent.Add(
            new ScheduledEvent(
                "ping_log",
                (evt) => Debug.ULogChannel("Scheduler", "Event {0} fired", evt.Name)));

        modsManager = new ModsManager();

        if (SceneController.loadWorldFromFileName != null)
        {
            CreateWorldFromSaveFile(SceneController.loadWorldFromFileName);
            SceneController.loadWorldFromFileName = null;
        }
        else
        {
            CreateEmptyWorld();
        }

        soundController = new SoundController(World);
    }

    public void Start()
    {
        // Create GameObject so we can have access to a transform which has a position of "Vector3.zero".
        new GameObject("VisualPath", typeof(VisualPath));
        GameObject go;

        tileSpriteController = new TileSpriteController(World);
        characterSpriteController = new CharacterSpriteController(World);
        furnitureSpriteController = new FurnitureSpriteController(World);
        utilitySpriteController = new UtilitySpriteController(World);
        jobSpriteController = new JobSpriteController(World, furnitureSpriteController, utilitySpriteController);
        inventorySpriteController = new InventorySpriteController(World, inventoryUI);
        shipSpriteController = new ShipSpriteController(World);

        buildModeController = new BuildModeController();
        spawnInventoryController = new SpawnInventoryController();
        mouseController = new MouseController(buildModeController, furnitureSpriteController, utilitySpriteController, circleCursorPrefab);
        questController = new QuestController();
        cameraController = new CameraController();
        TradeController = new TradeController();
        autosaveManager = new AutosaveManager();

        dialogBoxManager = GameObject.Find("Dialog Boxes").GetComponent<DialogBoxManager>();

        // Register inputs actions
        KeyboardManager.Instance.RegisterInputAction("DevMode", KeyboardMappedInputType.KeyDown, ChangeDevMode);

        // Hiding Dev Mode spawn inventory controller if devmode is off.
        spawnInventoryController.SetUIVisibility(Settings.GetSetting("DialogBoxSettings_developerModeToggle", false));

        cameraController.Initialize();
        cameraController.Moved += this.World.OnCameraMoved;

        // Initialising controllers.
        GameObject canvas = GameObject.Find("Canvas");
        go = Instantiate(Resources.Load("UI/ContextMenu"), canvas.transform.position, canvas.transform.rotation, canvas.transform) as GameObject;
        go.name = "ContextMenu";

        GameController.Instance.IsModal = false;
    }

    /// <summary>
    /// Gets the tile at the Unity-space coordinates.
    /// </summary>
    /// <returns>The tile at world coordinate.</returns>
    /// <param name="coord">Unity World-Space coordinates.</param>
    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x + 0.5f);
        int y = Mathf.FloorToInt(coord.y + 0.5f);

        return World.GetTileAt(x, y, (int)coord.z);
    }

    public void Destroy()
    {
        TimeManager.Instance.Destroy();
        KeyboardManager.Instance.Destroy();
        Scheduler.Scheduler.Current.Destroy();
        GameMenuManager.Instance.Destroy();
        AudioManager.Destroy();
    }

    /// <summary>
    /// Change the developper mode.
    /// </summary>
    public void ChangeDevMode()
    {
        bool developerMode = !Settings.GetSetting("DialogBoxSettings_developerModeToggle", false);
        Settings.SetSetting("DialogBoxSettings_developerModeToggle", developerMode);
        spawnInventoryController.SetUIVisibility(developerMode);
        ///FurnitureBuildMenu.instance.RebuildMenuButtons(developerMode);
    }

    /// <summary>
    /// Serializes current Instance of the World and starts a thread
    /// that actually saves serialized world to HDD.
    /// </summary>
    /// <param name="filePath">Where to save (Full path).</param>
    /// <returns>Returns the thread that is currently saving data to HDD.</returns>
    public Thread SaveWorld(string filePath)
    {
        // Make sure the save folder exists.
        if (Directory.Exists(GameController.Instance.FileSaveBasePath()) == false)
        {
            // NOTE: This can throw an exception if we can't create the folder,
            // but why would this ever happen? We should, by definition, have the ability
            // to write to our persistent data folder unless something is REALLY broken
            // with the computer/device we're running on.
            Directory.CreateDirectory(GameController.Instance.FileSaveBasePath());
        }

        StreamWriter sw = new StreamWriter(filePath);
        JsonWriter writer = new JsonTextWriter(sw);

        JObject worldJson = World.Current.ToJson();

        // Launch saving operation in a separate thread.
        // This reduces lag while saving by a little bit.
        Thread t = new Thread(new ThreadStart(delegate { SaveWorldToHdd(worldJson, writer); }));
        t.Start();

        return t;
    }

    /// <summary>
    /// Create/overwrite the save file with the XML text.
    /// </summary>
    /// <param name="filePath">Full path to file.</param>
    /// <param name="writer">TextWriter that contains serialized World data.</param>
    private void SaveWorldToHdd(JObject worldJson, JsonWriter writer)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.NullValueHandling = NullValueHandling.Ignore;
        serializer.Formatting = Formatting.Indented;

        serializer.Serialize(writer, worldJson);

        writer.Flush();
    }

    private void CreateEmptyWorld()
    {
        // get world size from settings
        int width = Settings.GetSetting("worldWidth", 100);
        int height = Settings.GetSetting("worldHeight", 100);

        // FIXME: Need to read this from settings.
        int depth = 5;

        // Create a world with Empty tiles
        World = new World(width, height, depth);

        // Center the Camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }

    private void CreateWorldFromSaveFile(string fileName)
    {
        Debug.ULogChannel("WorldController", "CreateWorldFromSaveFile");

        World = new World();
        World.ReadJson(fileName);
    }
}
