using System.Collections;
using System.Collections.Generic;
using System;
using com.spacepuppy.Collections;
using UnityEngine;


[Serializable]
public class ChannelDictionary : SerializableDictionaryBase<string, bool>
{
    public ChannelDictionary()
    {
    }

    public ChannelDictionary(ChannelDictionary channelState)
    {
        foreach (KeyValuePair<string,bool> kvp in channelState)
        {
            this.Add(kvp.Key, kvp.Value);
        }
    }
}
