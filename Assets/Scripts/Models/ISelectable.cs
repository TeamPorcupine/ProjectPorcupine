using UnityEngine;
using System.Collections;

public interface ISelectable
{

    string GetName();

    string GetDescription();

    string GetHitPointString();
    // For indestructible things (if any?) this is allowed to return blank (or null maybe??)

}
