#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// JobListEvents handles every possible JobListItem action, such as delete itself (along 
/// with the job), edit specific parameters of the actual job, or set priorities, etc.
/// </summary>
public class JobListItem : MonoBehaviour, IPointerClickHandler
{
    public Character character;
    public Color currentColor;

    /// <summary>
    /// Deletes the job list item after canceling the job.
    /// </summary>
    public void DeleteSelf()
    {
        character.InterruptState();
        transform.SetParent(null);
        GameObject.Destroy(transform.gameObject);
    }

    /// <summary>
    /// When the item is clicked, it will center the character that is doing
    /// the job.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Highlight the selected item
        JobListItem[] listItems = transform.parent.GetComponentsInChildren<JobListItem>();
        foreach (JobListItem listItem in listItems)
        {
            listItem.GetComponent<Image>().color = listItem.currentColor;
        }

        GetComponent<Image>().color = new Color32(0, 68, 101, 153);

        // Center the camera on the character
        Vector3 charPosition = new Vector3(character.X, character.Y, Camera.main.transform.position.z);
        Camera.main.transform.position = charPosition;
    }
}
