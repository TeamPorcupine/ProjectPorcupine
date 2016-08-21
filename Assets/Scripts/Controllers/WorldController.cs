#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour
{

    public static WorldController Instance { get; protected set; }

    // The world and tile data
    public World world { get; protected set; }

    static string loadWorldFromFile = null;

    private bool _isPaused = false;

    public bool IsPaused
    {
        get
        {
            return _isPaused || IsModal;
        }
        set
        {
            _isPaused = value;
        }
    }

    // If true, a modal dialog box is open so normal inputs should be ignored.
    public bool IsModal;

    // Multiplier of Time.deltaTime.
    private float timeScale = 1f;

    // An array of possible time multipliers.
    private float[] possibleTimeScales = new float[6] { 0.1f, 0.5f, 1f, 2f, 4f, 8f };
    // Current position in that array.
    int currentTimeScalePosition = 2;

    public bool devMode = false;

    // Use this for initialization
    void OnEnable()
    {
        if (Instance != null)
        {
            Logger.LogError("There should never be two world controllers.");
        }
        Instance = this;

        if (loadWorldFromFile != null)
        {
            CreateWorldFromSaveFile();
            loadWorldFromFile = null;
        }
        else
        {
            CreateEmptyWorld();
        }

        //Initialising controllers
        GameObject Controllers = GameObject.Find("Controllers");
        Instantiate(Resources.Load("UIController"), Controllers.transform);
    }

    void Update()
    {
        CheckTimeInput();

        if (IsPaused == false)
        {
            world.Update(Time.deltaTime * timeScale);
        }
    }

    void CheckTimeInput()
    {
        if (IsModal)
        {
            // A modal dialog box is open. Bail.
            return;
        }

        // TODO: Move this into centralized keyboard manager where
        // all of the buttons can be rebinded.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IsPaused = !IsPaused;
            Logger.Log("Game " + (IsPaused ? "paused" : "resumed"));
        }

        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (currentTimeScalePosition == possibleTimeScales.Length - 1)
            {
                // We are on the top of possibleTimeScales so just bail out.
                return;
            }

            currentTimeScalePosition++;
            SetTimeScale(possibleTimeScales[currentTimeScalePosition]);
        }
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (currentTimeScalePosition == 0)
            {
                // We are on the bottom of possibleTimeScales so just bail out.
                return;
            }

            currentTimeScalePosition--;
            SetTimeScale(possibleTimeScales[currentTimeScalePosition]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SetTimeScale(1f);
            currentTimeScalePosition = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SetTimeScale(2f);
            currentTimeScalePosition = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SetTimeScale(4f);
            currentTimeScalePosition = 4;
        }
    }

    /// <summary>
    /// Set's game speed (it's a multiplier so 1 == normal game speed).
    /// </summary>
    /// <param name="timeScale">Desired time scale</param>
    public void SetTimeScale(float timeScale)
    {
        this.timeScale = timeScale;
        Logger.Log("Game speed set to " + timeScale + "x");
    }

    /// <summary>
    /// Gets the tile at the unity-space coordinates
    /// </summary>
    /// <returns>The tile at world coordinate.</returns>
    /// <param name="coord">Unity World-Space coordinates.</param>
    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x + 0.5f);
        int y = Mathf.FloorToInt(coord.y + 0.5f);

        return world.GetTileAt(x, y);
    }

    public void NewWorld()
    {
        Logger.Log("NewWorld button was clicked.");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public string FileSaveBasePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "Saves");
    }

    public void LoadWorld(string fileName)
    {
        Logger.Log("LoadWorld button was clicked.");

        // Reload the scene to reset all data (and purge old references)
        loadWorldFromFile = fileName;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CreateEmptyWorld()
    {
        // get world size from settings
        int width = int.Parse(Settings.getSetting("worldWidth", "100"));
        int height = int.Parse(Settings.getSetting("worldHeight", "100"));

        // Create a world with Empty tiles
        world = new World(width, height);

        // Center the Camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);
    }

    void CreateWorldFromSaveFile()
    {
        Logger.Log("CreateWorldFromSaveFile");
        // Create a world from our save file data.

        XmlSerializer serializer = new XmlSerializer(typeof(World));

        // This can throw an exception.
        // TODO: Show a error message to the user.
        string saveGameText = File.ReadAllText(loadWorldFromFile);

        TextReader reader = new StringReader(saveGameText);


        Logger.Log(reader.ToString());
        world = (World)serializer.Deserialize(reader);
        reader.Close();


        // Center the Camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);
    }
}
