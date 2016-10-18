#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

public static class PerformanceComponentGroups
{
    public static PerformanceComponentGroup[] groups;

    //NULL FORM
    public static readonly PerformanceComponentGroup none = new PerformanceComponentGroup(new BasePerformanceComponent[] { }, -1, "No FPS");
    public static readonly PerformanceComponentGroup basic = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent() }, 0, "Basic");
    public static readonly PerformanceComponentGroup extended = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent(), new FPSRangePerformanceComponent(), new FPSAveragePerformanceComponent() }, 2, "Extended");
    //TODO: Not fully done yet needs others
    public static readonly PerformanceComponentGroup verbose = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent(), new FPSRangePerformanceComponent(), new FPSAveragePerformanceComponent(), new MemoryPerformanceComponent() }, 3, "Verbose");
}

public sealed class PerformanceComponentGroup
{
    public BasePerformanceComponent[] groupElements;
    public int groupID;
    public string groupName;

    public PerformanceComponentGroup(BasePerformanceComponent[] elements, int id, string name)
    {
        groupElements = elements;
        groupID = id;
        groupName = name;
    }
}
