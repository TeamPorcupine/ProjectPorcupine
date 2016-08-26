using ProjectPorcupine.Localization;
using System;
using UnityEngine;

public class WorldFactory
{
    private GameObject rootController;

    public void Create()
    {
        if (rootController != null)
        {
            throw new InvalidOperationException("Creating more than one world is currently not supported.");
        }

        Debug.Log("Creating world.");

        rootController = new GameObject("Controllers");

        GameObject worldControllerGameObject = new GameObject("WorldController", typeof(WorldController));
        worldControllerGameObject.transform.parent = rootController.transform.parent;

        rootController.AddComponent<SpriteManager>();
        rootController.AddComponent<GameEventManager>();
        rootController.AddComponent<LocalizationLoader>();
    }

    public void Destroy()
    {
        // TODO Theres currently no way to cleanly take down a world.
        throw new NotImplementedException("Theres currently no way to cleanly take down a world.");
    }
}
