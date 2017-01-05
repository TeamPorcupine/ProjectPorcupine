#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using MoonSharp.Interpreter;

public interface IFunctions
{
    void RegisterType(System.Type type);

    bool HasFunction(string name);

    bool LoadScript(string text, string scriptName);

    DynValue Call(string functionName, params object[] args);

    T Call<T>(string functionName, params object[] args);

    DynValue CallWithError(string functionName, params object[] args);
}