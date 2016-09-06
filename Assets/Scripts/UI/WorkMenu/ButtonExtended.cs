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
using UnityEngine.Events;

public class ButtonExtended : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent leftClick;
    public UnityEvent rightClick;
    public UnityEvent controlLeftClick;
    public UnityEvent controlRightClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                controlLeftClick.Invoke();
            }
            else
            {
                leftClick.Invoke();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                controlRightClick.Invoke();
            }
            else
            {
                rightClick.Invoke();
            }
        }
    }
}
