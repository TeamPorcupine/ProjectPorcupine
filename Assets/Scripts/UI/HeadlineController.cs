using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml;
using System.IO;

public class HeadlineController : MonoBehaviour {

    public Text textBox;

	// Use this for initialization
	void Start () {
        string filePath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "Headlines.xml");
        string xmlText = System.IO.File.ReadAllText(filePath);
        XmlDocument doc = new XmlDocument();
        doc.Load(new StringReader(xmlText));
        HeadlineGenerator headlineGenerator=World.Current.CreateHeadlineGenerator(doc.SelectSingleNode("Headlines"), UpdateHeadline);
        UpdateHeadline(headlineGenerator.currentDisplay);
    }

    private void UpdateHeadline(string newHeadline)
    {
        Debug.ULogChannel("Headline", newHeadline);
        textBox.text = newHeadline;
    }
	
}
