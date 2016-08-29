#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion



using System;
using System.Collections.Generic;
using System.Xml;

public class HeadlineGenerator 
{
    protected Action<string> UpdatedHeadline;

    List<string> quotes = new List<string>();
    private float time,nextTime,minInterval,maxInterval;

    public HeadlineGenerator(XmlNode baseNode)
    {
        //TODO Consider default values for these. Also consider reasonable limits
        minInterval = float.Parse(baseNode.Attributes.GetNamedItem("minInterval").Value);
        maxInterval = float.Parse(baseNode.Attributes.GetNamedItem("maxInterval").Value);
        foreach (XmlNode node in baseNode.SelectNodes("Headline"))
        {
            quotes.Add(node.InnerText);
        }
        ResetNextTime();
        time = 0f;
    }

    private void ResetNextTime()
    {
        nextTime = UnityEngine.Random.Range(minInterval, maxInterval);
    }

    public void Update(float deltaTime)
    {
        time += deltaTime;
        if (time>nextTime)
        {
            if (UpdatedHeadline!=null)
            {
                UpdatedHeadline(quotes[UnityEngine.Random.Range(0, quotes.Count)]);
            }
            time -= nextTime;
            ResetNextTime();
        }
    }

    public void Headline(string headline)
    {
        quotes.Add(headline);
    }

    public void RegisterUpdateHeadline(Action<string> action)
    {
        UpdatedHeadline += action;
    }

    public void UnregisterUpdateHeadline(Action<string> action)
    {
        UpdatedHeadline -= action;
    }
}
