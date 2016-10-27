#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

/// <summary>
/// Currently not used but may be used in future is just network.
/// </summary>
public class NetworkPerformanceComponent : BasePerformanceComponent
{
    private TextPerformanceComponentUI component;

    public override int PriorityID()
    {
        return 2;
    }

    public override void Update()
    {
        component.ChangeText("0ms");
    }

    public override BasePerformanceComponentUI UIComponent()
    {
        return component;
    }

    public override string NameOfComponent()
    {
        return "UI/TextPerformanceComponentUI";
    }

    public override void Start(BasePerformanceComponentUI componentUI)
    {
        component = (TextPerformanceComponentUI)componentUI;
    }
}
