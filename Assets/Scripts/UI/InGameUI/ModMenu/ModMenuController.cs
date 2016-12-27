using UnityEngine;
using System.Collections;

public class ModMenuController : MonoBehaviour {
    public static GameObject Instance;
    public Transform ModParent;

    public void DisableAll()
    {
        ModMenu.DisableAll();
    }

    public void Save()
    {
        ModMenu.Commit(true);
        ModMenu.Save();
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        ModMenu.Reset();
        gameObject.SetActive(false);
    }

    public void Apply()
    {
        ModMenu.Commit();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ModMenu.DisplaySettings(ModParent);
    }
}
