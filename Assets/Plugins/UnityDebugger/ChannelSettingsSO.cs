using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChannelSettingsSO : ScriptableObject {
    public bool DefaultState;
    public ChannelDictionary ChannelState = new ChannelDictionary();
}
