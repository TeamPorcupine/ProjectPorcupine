#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rescales text component to fit horizontally, and resets text area to anchor locations. Avoiding the use of "Best Fit" resulting in smaller atlas.
/// Add to any text element with localization.
/// </summary>
public class TextScaling : MonoBehaviour
{
    // List of all scripts on buttons.
    private static List<TextScaling> activationList = new List<TextScaling>();
    private Text textScript;
    private RectTransform rectTrans;
    private float originalWidth;

    // Trigger to rescale everything when language is changed.
    public static void ScaleAllTexts()
    {
        foreach (TextScaling dtsScript in activationList)
        {
            dtsScript.UpdateScale();
        }
    }

    public void UpdateScale()
    {
        // Reset values.
        rectTrans.localScale = new Vector3(1, 1, 1);
        rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalWidth);

        float stringWidth = textScript.preferredWidth;
        float maxWidth = originalWidth;
        float capacityPercent = stringWidth / (maxWidth / 100);

        // 95% width to pad button borders.
        if (capacityPercent <= 95)
        {
            return;
        }
        else
        {
            // (0.9f) 90% of starting scale to compensate irregularities when near 100%, and 170% to compensate extremely small sizes.
            float scale = 0.9f - ((capacityPercent - 100) / 170);
            rectTrans.localScale = new Vector3(scale, scale, 1);

            // Resets text area to anchor locations after scale.
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth / scale);
        }
    }

    private void OnDestroy()
    {
        activationList.Remove(this);
        StopCoroutine(LateStart());
    }

    private void Start()
    {
        activationList.Add(this);
        textScript = GetComponent<Text>();
        rectTrans = GetComponent<RectTransform>();

        // Delayed, width is 0 on start.
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return null;
        originalWidth = transform.parent.GetComponent<RectTransform>().rect.width;

        // Resize localization texts on start.
        UpdateScale();
    }
}