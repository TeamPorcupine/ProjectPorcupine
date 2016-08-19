using UnityEngine;
using UnityEngine.UI;

public class SelectionInfoInteractButton : MonoBehaviour
{
    private MouseController mc;
    private Button btt;
    private Furniture furniture;

    public DialogBoxGalacticMarket dialogBoxGalacticMarket;

    // Use this for initialization
    private void Start()
    {
        mc = FindObjectOfType<MouseController>();
        btt = GetComponent<Button>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (mc.mySelection == null)
        {
            return;
        }

        var actualSelection = mc.mySelection.stuffInTile[mc.mySelection.subSelection];

        furniture = actualSelection as Furniture;
        bool interactable = furniture != null && !string.IsNullOrEmpty(furniture.InteractFunctionName);

        btt.interactable = interactable;
    }

    public void OnInteract()
    {
        if (furniture != null && !string.IsNullOrEmpty(furniture.InteractFunctionName))
        {
            Debug.Log("Interact with furniture " + furniture.Name);
            // FIXME: Currently just call c# code

            this.GetType().GetMethod(furniture.InteractFunctionName).Invoke(this, null);
        }
    }

    public void OpenTradeTerminal()
    {
        Debug.Log("Open Trade Terminal");

        dialogBoxGalacticMarket.ShowDialog();
    }
}