using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeveloperButtons : MonoBehaviour {

    DialogBoxTrade dialogTrade;
    DialogBoxManager dialogManager;

    public Button buttonTrade;
    public Button buttonPath;

	// Use this for initialization
	void Start () {
        dialogManager = GameObject.FindObjectOfType<DialogBoxManager>();
        dialogTrade = dialogManager.dialogBoxTrade;

        // Add liseners here.
        buttonTrade.onClick.AddListener(delegate
            {
                OnClickTrade();
            });

        buttonPath.onClick.AddListener(delegate
            {
                OnClickPath();
            });
	}
	
    void OnClickTrade () {

        dialogManager.dialogBoxOptions.CloseDialog();
        dialogTrade.ShowDialog();
        dialogTrade.DoTradingTestWithMockTraders();

    }

	void OnClickPath () {

        dialogManager.dialogBoxOptions.CloseDialog();
	}
}
