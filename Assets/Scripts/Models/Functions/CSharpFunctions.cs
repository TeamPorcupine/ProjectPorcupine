#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.CSharp;
using MoonSharp.Interpreter;

public class CSharpFunctions : IFunctions
{
    // this is just to support convertion of object to DynValue
    protected Script script;

    private Dictionary<string, MethodInfo> methods;

    private Evaluator evaluator;

    public CSharpFunctions()
    {
        script = new Script();

        methods = new Dictionary<string, MethodInfo>();

        CompilationResult = new CompilingResult();

        evaluator = null;
    }

    /// <summary>
    /// Gets the compiling result.
    /// </summary>
    /// <value>The compiling result.</value>
    private CompilingResult CompilationResult { get; set; }

    /// <summary>
    /// Little helper method to detect dynamic assemblies.
    /// </summary>
    /// <param name="assembly">Assembly to check.</param>
    /// <returns>True if assembly is dynamic, otherwise false.</returns>
    public static bool IsDynamic(Assembly assembly)
    {
        // http://bloggingabout.net/blogs/vagif/archive/2010/07/02/net-4-0-and-notsupportedexception-complaining-about-dynamic-assemblies.aspx
        // Will cover both System.Reflection.Emit.AssemblyBuilder and System.Reflection.Emit.InternalAssemblyBuilder
        return assembly.GetType().FullName.EndsWith("AssemblyBuilder") || assembly.Location == null || assembly.Location == string.Empty;
    }

    public bool HasFunction(string name)
    {
        return methods.ContainsKey(name);
    }

    /// <summary>
    /// Loads the script from the specified text.
    /// </summary>
    /// <param name="text">The code text.</param>
    /// <param name="scriptName">The script name.</param>
    public bool LoadScript(string text, string scriptName)
    {
        bool success = false;

        try
        {
            evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), CompilationResult));

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                // skip System.Core to prevent ambigious error when using System.Linq in scripts
                if (!assemblies[i].FullName.Contains("System.Core"))
                {
                    evaluator.ReferenceAssembly(assemblies[i]);
                }
            }

            // first, try if it already exists
            var resAssembly = GetCompiledAssembly(scriptName);

            if (resAssembly == null)
            {
                evaluator.Compile(text + GetConnectionPointClassDeclaration(scriptName));
                resAssembly = GetCompiledAssembly(scriptName);
            }

            if (resAssembly == null)
            {
                if (CompilationResult.HasErrors)
                {
                    UnityDebugger.Debugger.LogError(
                        "CSharp",
                        string.Format("[{0}] CSharp compile errors ({1}): {2}", scriptName, CompilationResult.Errors.Count, CompilationResult.GetErrorsLog()));
                }

                return false;
            }

            CreateDelegates(resAssembly);
            success = true;
        }
        catch (Exception ex)
        {
            UnityDebugger.Debugger.LogError(
                        "CSharp",
                        string.Format("[{0}] Problem loading functions from CSharp script: {1}", scriptName, ex.ToString()));
        }

        return success;
    }

    /// <summary>
    /// Call the specified lua function with the specified args.
    /// </summary>
    /// <param name="functionName">Function name.</param>
    /// <param name="args">Arguments.</param>
    public DynValue Call(string functionName, params object[] args)
    {
        object ret = methods[functionName].Invoke(null, args);
        return DynValue.FromObject(script, ret);
    }

    /// <summary>
    /// Call the specified lua function with the specified args.
    /// </summary>
    /// <param name="functionName">Function name.</param>
    /// <param name="args">Arguments.</param>
    public T Call<T>(string functionName, params object[] args)
    {
        return (T)methods[functionName].Invoke(null, args);
    }

    public void CallWithInstance(string[] functionNames, object instance, params object[] parameters)
    {
        foreach (string fn in functionNames)
        {
            if (fn == null)
            {
                UnityDebugger.Debugger.LogError("CSharp", "'" + fn + "' is not a CSharp function.");
                return;
            }

            DynValue result;
            object[] instanceAndParams = new object[parameters.Length + 1];
            instanceAndParams[0] = instance;
            parameters.CopyTo(instanceAndParams, 1);

            result = Call(fn, instanceAndParams);

            if (result != null && result.Type == DataType.String)
            {
                UnityDebugger.Debugger.LogError("CSharp", result.String);
            }
        }
    }

    public void RegisterType(Type type)
    {
        // nothing to do for C#
    }

    // This really doesn't need to exist, CallWithError is only for LUA
    public DynValue CallWithError(string functionName, params object[] args)
    {
        return Call(functionName, args);
    }

    private string GetConnectionPointClassDeclaration(string name)
    {
        return Environment.NewLine + " public struct MonoSharp_DynamicAssembly_" + name + " {}";
    }

    private string GetConnectionPointGetTypeExpression(string name)
    {
        return "typeof(MonoSharp_DynamicAssembly_" + name + ");";
    }

    private void CreateDelegates(Assembly assembly)
    {
        foreach (var type in GetAllTypesFromAssembly(assembly))
        {
            foreach (var method in GetAllMethodsFromType(type))
            {
                methods.Add(method.Name, method);
            }
        }
    }

    private MethodInfo[] GetAllMethodsFromType(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Static);
    }

    private Type[] GetAllTypesFromAssembly(Assembly assembly)
    {
        return assembly.GetTypes();
    }

    private Assembly GetCompiledAssembly(string name)
    {
        try
        {
            string className = GetConnectionPointGetTypeExpression(name);
            return ((Type)evaluator.Evaluate(className)).Assembly;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private Assembly GetCompiledAssemblyForScript(string className)
    {
        try
        {
            return ((Type)evaluator.Evaluate(className)).Assembly;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private class CompilingResult : ReportPrinter
    {
        /// <summary>
        /// The collection of compiling errors.
        /// </summary>
        public List<string> Errors = new List<string>();

        /// <summary>
        /// The collection of compiling warnings.
        /// </summary>
        public List<string> Warnings = new List<string>();

        /// <summary>
        /// Indicates if the last compilation yielded any errors.
        /// </summary>
        /// <value>If set to <c>true</c> indicates presence of compilation error(s).</value>
        public bool HasErrors
        {
            get
            {
                return Errors.Count > 0;
            }
        }

        /// <summary>
        /// Indicates if the last compilation yielded any warnings.
        /// </summary>
        /// <value>If set to <c>true</c> indicates presence of compilation warning(s).</value>
        public bool HasWarnings
        {
            get
            {
                return Warnings.Count > 0;
            }
        }

        /// <summary>
        /// Clears all errors and warnings.
        /// </summary>
        public new void Reset()
        {
            Errors.Clear();
            Warnings.Clear();
            base.Reset();
        }

        /// <summary>
        /// Handles compilation event message.
        /// </summary>
        /// <param name="msg">The compilation event message.</param>
        /// <param name="showFullPath">If set to <c>true</c> [show full path].</param>
        public override void Print(AbstractMessage msg, bool showFullPath)
        {
            string msgInfo = string.Format("{0} {1} CS{2:0000}: {3}", msg.Location, msg.MessageType, msg.Code, msg.Text);
            if (!msg.IsWarning)
            {
                Errors.Add(msgInfo);
            }
            else
            {
                Warnings.Add(msgInfo);
            }
        }

        public string GetErrorsLog()
        {
            return string.Join(Environment.NewLine, Errors.ToArray());
        }
    }
}