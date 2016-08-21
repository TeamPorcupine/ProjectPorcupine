using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NameTagUpdater : MonoBehaviour {
    public Character character;
    public SettingHelper settings;
	void Update () {
		character.health = character.health -= 0.01f;
		if (character.health < 0)
		{
			character.health = 1;
		}
        GameObject canvas = gameObject.GetComponentInChildren<Canvas>().gameObject;
        Text[] TextFields = gameObject.GetComponentsInChildren<Text>();
        if (TextFields.Length == 2)
        {
            TextFields[0].text = character.name;
            if (settings.ntHealth.isOn)
            {
                int health = (int)(character.health * 100);
                TextFields[1].text = health.ToString() + "%";
            }
            else
            {
                int health = (int)(character.maxHealth * character.health);
                TextFields[1].text = health.ToString() + "/" + character.maxHealth;
            }
        }
		Image[] Panels = canvas.GetComponentsInChildren<Image>();
		if (Panels.Length == 2) {
			Panels[1].transform.localScale = new Vector3(character.health,0.5f,1);
		}
	}
}
