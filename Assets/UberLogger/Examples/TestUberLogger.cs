using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestUberLogger : MonoBehaviour
{
    // Use this for initialization
    void Start ()
    {
        UberLogger.Logger.AddLogger(new UberLoggerFile("UberLogger.log"), false);
        DoTest();
        var t = new List<int>();
        t[0] = 5;

    }

    public void DoTest()
    {
        // UnityEngine.Debug.Log("Starting");
        Debug.LogWarning("Log Warning with GameObject", gameObject);
        Debug.LogError("Log Error with GameObject", gameObject);
        Debug.Log("Log Message with GameObject", gameObject);
        Debug.LogFormat("Log Format param {0}", "Test");
        Debug.LogFormat(gameObject, "Log Format with GameObject and param {0}", "Test");

        Debug.ULog("ULog");
        Debug.ULog("ULog with param {0}", "Test");
        Debug.ULog(gameObject, "ULog with GameObject");
        Debug.ULog(gameObject, "ULog with GameObject and param {0}", "Test");

        Debug.ULogChannel("Test", "ULogChannel");
        Debug.ULogChannel("Test", "ULogChannel with param {0}", "Test");
        Debug.ULogChannel(gameObject, "Test", "ULogChannel with GameObject");
        Debug.ULogChannel(gameObject, "Test", "ULogChannel with GameObject and param {0}", "Test");
	
        Debug.ULogWarning("ULogWarning");
        Debug.ULogWarning("ULogWarning with param {0}", "Test");
        Debug.ULogWarning(gameObject, "ULogWarning with GameObject");
        Debug.ULogWarning(gameObject, "ULogWarning with GameObject and param {0}", "Test");

        Debug.ULogWarningChannel("Test", "ULogWarningChannel");
        Debug.ULogWarningChannel("Test", "ULogWarningChannel with param {0}", "Test");
        Debug.ULogWarningChannel(gameObject, "Test", "ULogWarningChannel with GameObject");
        Debug.ULogWarningChannel(gameObject, "Test", "ULogWarningChannel with GameObject and param {0}", "Test");

        Debug.ULogError("ULogError");
        Debug.ULogError("ULogError with param {0}", "Test");
        Debug.ULogError(gameObject, "ULogError with GameObject");
        Debug.ULogError(gameObject, "ULogError with GameObject and param {0}", "Test");

        Debug.ULogErrorChannel("Test", "ULogErrorChannel");
        Debug.ULogErrorChannel("Test", "ULogErrorChannel with param {0}", "Test");
        Debug.ULogErrorChannel(gameObject, "Test", "ULogErrorChannel with GameObject");
        Debug.ULogErrorChannel(gameObject, "Test", "ULogErrorChannel with GameObject and param {0}", "Test");
    }
	
	// Update is called once per frame
    void Update () {
        DoTest();
    }
}

