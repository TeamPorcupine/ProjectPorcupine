using UnityEngine;

public class KeyboadMappedInput
{
    public InputNames InputName { get; set; }
    public KeyCode Primary { get; set; }
    public KeyCode Alternate { get; set; }

    public KeyboadMappedInput()
    {
        
    }

    public KeyboadMappedInput(InputNames inputName, KeyCode primary, KeyCode alternate)
    {
        InputName = inputName;
        Primary = primary;
        Alternate = alternate;
    }
}