using UnityEngine;
using System.Collections.Generic;

public class GameEvent 
{

    public string Name { get; protected set; }

    protected List<string> preconditions;
    protected List<string> executionActions;

    private bool executed;

    public GameEvent(string name){
        Name = name;
        preconditions = new List<string>();
        executionActions = new List<string>();
    }

    public void Update(){
        int conditionsMet = 0;
        foreach(string precondition in preconditions){
            // Call lua precondition it should return 1 if met otherwise 0
            // conditionsMet += (int)(GameEventActions.CallFunctionWithEvent(precondition, this).Number);
        }

        if(conditionsMet >= preconditions.Count && executed == false){
            Execute();
        }
    }

    public void Execute()
    {
        if (executionActions != null)
        {
            // Execute Lua code like in Furniture ( FurnitureActions ) 
            // GameEventActions.CallFunctionsWithEvent(executionActions.ToArray(), this);
        }
        executed = true;
    }

    public void RegisterPrecondition(string luaFunctionName)
    {
        preconditions.Add(luaFunctionName);
    }

    public void RegisterPreconditions(string[] luaFunctionNames)
    {

        preconditions.AddRange(luaFunctionNames);
    }

    public void RegisterExecutionAction(string luaFunctionName)
    {
        executionActions.Add(luaFunctionName);
    }

    public void RegisterExecutionActions(string[] luaFunctionNames)
    {

        executionActions.AddRange(luaFunctionNames);
    }
}
