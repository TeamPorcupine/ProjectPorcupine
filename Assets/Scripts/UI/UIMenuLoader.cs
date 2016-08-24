using UnityEngine;
using System.Collections;

public class UIMenuLoader : MonoBehaviour
{
    
    // Use this for initialization
    void Start()
    {
        GameObject Canvas = this.gameObject;
        GameObject tempGoObj;

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/MenuTop"), Canvas.transform.position, Canvas.transform.rotation, Canvas.transform);
        tempGoObj.name = "Top Menu";

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/MenuBottom"), Canvas.transform.position, Canvas.transform.rotation, Canvas.transform);
        tempGoObj.name = "Bottom Menu";
      
        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/MenuRight"),  Canvas.transform.position, Canvas.transform.rotation, Canvas.transform);
        tempGoObj.name = "Right Menu";

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/MenuConstruction"), Canvas.transform.position, Canvas.transform.rotation, Canvas.transform);
        tempGoObj.name = "Construction Menu";

        /*
        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/MenuContext"), Canvas.transform.position, Canvas.transform.rotation, Canvas.transform);
        tempGoObj.name = "Context Menu";
        */
    }
	
    // Update is called once per frame
    void Update()
    {
	
    }
}
