#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// JobListEvents handles every possible JobListItem action, such as delete itself (along 
/// with the job), edit specific parameters of the actual job, or set priorities, etc.
/// </summary>
public class JobListEvents : MonoBehaviour
{
    public Transform JobListItem;

    public Text jobText;

    /// <summary>
    /// Deletes the job list item after cancelling the job.
    /// </summary>
    public void DeleteSelf()
    {
        string charName = GetNameFromItem();
        
        World.Current.GetCharacterFromName(charName).AbandonJob(true);
        JobListItem.SetParent(null);
        GameObject.Destroy(JobListItem.gameObject);
    }

    /// <summary>
    /// When the item is clicked, it will center the character that is doing
    /// the job.
    /// </summary>
    public void CenterCharacter()
    {
        string charName = GetNameFromItem();
        Vector3 charPosition;
        Character currentCharacter = World.Current.GetCharacterFromName(charName);

        charPosition = new Vector3(currentCharacter.X, currentCharacter.Y, -10); 
        GameObject.Find("Main Camera").transform.position = charPosition;
    }

    /// <summary>
    /// Util function to return the name of the character that's in the item.
    /// </summary>
    /// <returns>The character's name.</returns>
    private string GetNameFromItem()
    {
        string[] separators = new string[1];
        separators[0] = " - ";

        // TODO: Find a better selector.
        string[] charName = jobText.text.Split(separators, 0);
        return charName[0];
    }
}
