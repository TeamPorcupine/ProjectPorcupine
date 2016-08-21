using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FloorMenu : MonoBehaviour
{

    public Button floorBuild;
    public Button floorRemove;

    // Use this for initialization.
    void Start()
    {
        BuildModeController bmc = WorldController.Instance.buildModeController;

        floorBuild.onClick.AddListener(delegate
            {
                bmc.SetMode_BuildFloor();
            });
        floorRemove.onClick.AddListener(delegate
            {
                bmc.SetMode_Bulldoze();
            });
    }

    // Update is called once per frame.
    void Update()
    {

    }
}