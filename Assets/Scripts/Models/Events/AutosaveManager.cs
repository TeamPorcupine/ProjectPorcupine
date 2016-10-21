﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================

#endregion
using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scheduler;

public class AutosaveManager
{
    private const string AutosaveBaseName = "Autosave";

    private int autosaveCounter = 0;

    private Scheduler.Scheduler scheduler;

    private ScheduledEvent autosaveEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutosaveManager"/> class.
    /// This class schedules autosave events which save the game at regular intervals
    /// of <see cref="AutosaveInterval"/> minutes with the default filename "Autosave###.sav".
    /// </summary>
    public AutosaveManager()
    {
        if (scheduler == null)
        {
            scheduler = Scheduler.Scheduler.Current;
        }

        AutosaveInterval = Settings.GetSetting<int>("AutosaveInterval", 2); // in minutes

        // autosaves disabled if AutosaveInterval <= 0
        if (AutosaveInterval <= 0)
        {
            return;
        }

        autosaveEvent = new ScheduledEvent("autosave", DoAutosave, AutosaveInterval * 60.0f, true, 0);
        scheduler.RegisterEvent(autosaveEvent);

        // set autosaveCounter = maximum index of existing autosaves (so as not to clobber autosaves from previous games)
        if (Directory.Exists(GameController.Instance.FileSaveBasePath()))
        {
            string[] autosaveFileNames = Directory.GetFiles(GameController.Instance.FileSaveBasePath(), AutosaveBaseName + "*.sav");
            if (autosaveFileNames.Length == 0)
            {
                // no existing autosaves found
                return;
            }

            foreach (string fileName in autosaveFileNames)
            {
                // get the numeric part of the filename
                string autosaveNumAsString = Path.GetFileNameWithoutExtension(fileName).Replace(AutosaveBaseName, string.Empty);

                int autosaveNumber;
                if (int.TryParse(autosaveNumAsString, out autosaveNumber) == false)
                {
                    // filename not in the format of "Autosave<number>.sav" so move on
                    continue;
                }

                // if necessary, bump up autosaveCounter
                autosaveCounter = Math.Max(autosaveCounter, autosaveNumber);
            }
        }
    }

    /// <summary>
    /// Gets the autosave interval in minutes.
    /// If less than or equal to zero autosaves are disabled.
    /// </summary>
    /// <value>The autosave interval in minutes.</value>
    public int AutosaveInterval { get; private set; }

    /// <summary>
    /// Callback for the autosave event. Called automatically by the Scheduler when the cooldown expires.
    /// </summary>
    /// <param name="evt">The ScheduledEvent object which triggered the autosave.</param>
    public void DoAutosave(ScheduledEvent evt)
    {
        // autosaves disabled if AutosaveInterval <= 0
        if (AutosaveInterval <= 0)
        {
            return;
        }

        autosaveCounter += 1;

        string fileName = AutosaveBaseName + autosaveCounter.ToString();
        string filePath = System.IO.Path.Combine(GameController.Instance.FileSaveBasePath(), fileName + ".sav");

        Debug.ULogChannel("AutosaveManager", "Autosaving to '{0}'.", filePath);
        if (File.Exists(filePath) == true)
        {
            Debug.ULogErrorChannel("AutosaveManager", "File already exists -- overwriting the file for now.");
        }

        SaveWorld(filePath);
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
}
