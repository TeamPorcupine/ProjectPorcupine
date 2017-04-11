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
using System.Linq;
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

        AutosaveInterval = SettingsKeyHolder.AutosaveInterval;

        // autosaves disabled if AutosaveInterval <= 0
        if (AutosaveInterval > 0)
        {
            autosaveEvent = new ScheduledEvent("autosave", DoAutosave, AutosaveInterval * 60.0f, true, 0);
            scheduler.RegisterEvent(autosaveEvent);
        }

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

        string fileName;

        string saveDirectoryPath = GameController.Instance.FileSaveBasePath();
        DirectoryInfo saveDir = new DirectoryInfo(saveDirectoryPath);
        FileInfo[] saveGames = saveDir.GetFiles(AutosaveBaseName + "*.sav").OrderByDescending(f => f.LastWriteTime).ToArray();

        if (saveGames.Length >= SettingsKeyHolder.AutosaveMaxFiles)
        {
            // Get list of files in save location
            fileName = Path.GetFileNameWithoutExtension(saveGames.Last().Name);
        }
        else
        {
            autosaveCounter += 1;

            fileName = AutosaveBaseName + autosaveCounter.ToString();
        }

        string filePath = Path.Combine(saveDir.ToString(), fileName + ".sav");
        UnityDebugger.Debugger.LogFormat("AutosaveManager", "Autosaving to '{0}'.", filePath);
        if (File.Exists(filePath) == true)
        {
            UnityDebugger.Debugger.LogError("AutosaveManager", "File already exists -- overwriting the file for now.");
        }

        WorldController.Instance.SaveWorld(filePath);
    }

    public void SetAutosaveInterval(int newInterval)
    {
        AutosaveInterval = newInterval;
        if (newInterval == 0)
        {
            scheduler.DeregisterEvent(autosaveEvent);
            return;
        }

        if (autosaveEvent == null)
        {
            autosaveEvent = new ScheduledEvent("autosave", DoAutosave, newInterval * 60.0f, true, 0);
            scheduler.RegisterEvent(autosaveEvent);
        }
        else
        {
            autosaveEvent.SetCooldown(newInterval * 60.0f);
        }
    }
}
