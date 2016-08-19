using UnityEngine;
using System.Collections.Generic;

public class GameEvent 
{

    public string Name { get; protected set; }

    protected List<string> executionActions;

    public GameEvent(string name){
        Name = name;
        executionActions = new List<string>();
    }

    public void Execute()
    {
        if (executionActions != null)
        {
            // Execute Lua code like with furniture
            // FurnitureActions.CallFunctionsWithFurniture(updateActions.ToArray(), this, deltaTime);
        }
    }

    public void RegisterExecutionAction(string luaFunctionName)
    {
        executionActions.Add(luaFunctionName);
    }

    public void UnregisterExecutionAction(string luaFunctionName)
    {
        executionActions.Remove(luaFunctionName);
    }
}
