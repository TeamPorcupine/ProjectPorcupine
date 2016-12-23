using UnityEngine;
using System.Collections;

public class ModMenuMod : MonoBehaviour {
    string modName;
    // Use this for initialization
    void Start ()
    {
        modName = gameObject.name;
    }
    
    // Update is called once per frame
    public void Toggle (bool tg)
    {
        ModMenu.setEnabled(modName, tg);
    }
    public void Move(bool up)
    {
        ModMenu.reorderMod(modName, up ? 1 : -1);
    }
}
