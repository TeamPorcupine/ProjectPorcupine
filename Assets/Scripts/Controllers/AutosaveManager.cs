#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.IO;
using System.Xml.Serialization;
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
        if (Directory.Exists(WorldController.Instance.FileSaveBasePath()))
        {
            string[] autosaveFileNames = Directory.GetFiles(WorldController.Instance.FileSaveBasePath(), AutosaveBaseName + "*.sav");
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
        string filePath = System.IO.Path.Combine(WorldController.Instance.FileSaveBasePath(), fileName + ".sav");

        Debug.ULogChannel("AutosaveManager", "Autosaving to '{0}'.", filePath);
        if (File.Exists(filePath) == true)
        {
            Debug.ULogErrorChannel("AutosaveManager", "File already exists -- overwriting the file for now.");
        }

        SaveWorld(filePath);
    }

    // FIXME: This is mostly just copied from DialogBoxSaveGame.cs
    // Both functions could probably be mostly refactored onto World???
    private void SaveWorld(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, WorldController.Instance.World);
        writer.Close();

        try
        {
            if (Directory.Exists(WorldController.Instance.FileSaveBasePath()) == false)
            {
                Directory.CreateDirectory(WorldController.Instance.FileSaveBasePath());
            }

            File.WriteAllText(filePath, writer.ToString());
        }
        catch (Exception e)
        {
            Debug.ULogErrorChannel("AutosaveManager", "Could not create autosave file. Error: '{0}'.", e.ToString());
        }
    }
}
