using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;

/// <summary>
/// A class for connecting the UI to some test functions.
/// Tests can be added by name in the Start() function, and it will update
/// the UI with buttons for each test.
/// </summary>
public class TestController : MonoBehaviour
{
    /// <summary>
    /// The path to the log file. Calculated in start.
    /// </summary>
    protected string logPath;

    /// <summary>
    /// An object allowing us to append to the log file.
    /// </summary>
    protected TextWriter logFile;

    /// <summary>
    /// A Unity Text object that displays log messages.
    /// </summary>
    public Text logText;

    /// <summary>
    /// The prefab used to create buttons.
    /// </summary>
    public Button testButtonPrefab;

    /// <summary>
    /// The container that we put the buttons into.
    /// </summary>
    public RectTransform testButtonContainer;

    /// <summary>
    /// Sets up the log file, and adds a bunch of tests.
    /// </summary>
    public void Start()
    {
        logPath = Path.Combine(Application.dataPath, "log.txt");

        OpenLogFile();

        AddTest("Wipe Log File", WipeLogFile);
        AddTest("Clear Displayed Log", ClearDisplayLog);
        AddTest("Stop Coroutines", StopCoroutines);
        AddTest("All Pathfinding Speed Tests (x10)", () => { return PathfindingSpeedTest(10); });
        AddTest("All Pathfinding Speed Tests (x100)", () => { return PathfindingSpeedTest(100); });
        AddTest("Empty World Speed Tests (x10)", () => { return DoEmptyTests(10); });
        AddTest("Switchback World Speed Tests (x10)", () => { return DoSwitchbackTests(10); });
        AddTest("Diagonal World Speed Tests (x10)", () => { return DoDiagonalsTests(10); });
    }

    /// <summary>
    /// Opens the log file, and also tries to find information about the current
    /// git HEAD (so that it's easier to tell which tests belong to which branch,
    /// if you're trying out a lot of things in a lot of different branches).
    /// </summary>
    protected void OpenLogFile()
    {
        try {
            logFile = File.AppendText(logPath);
        }
        catch
        {
            Log("Could not open \"" + logPath + "\" for logging.");
        }

        try
        {
            string root = Path.GetDirectoryName(Application.dataPath);
            string git = Path.Combine(root, ".git");
            string head = File.ReadAllText(Path.Combine(git, "HEAD"));
            Log("CURRENT HEAD: " + head.Trim());
        }
        catch
        {
            Log("Could not find .git/HEAD. Omitting git branch information.");
        }
    }

    /// <summary>
    /// Create a new button for the given test.
    /// </summary>
    /// <param name="name">The text to be displayed on the button.</param>
    /// <param name="coroutine">A function that returns a coroutine that runs the test.</param>
    public void AddTest(string name, Func<IEnumerator> coroutine)
    {
        if (testButtonContainer == null || testButtonPrefab == null)
        {
            Logger.LogWarning("Can't set up tests in TestController without a container and a prefab.");
            return;
        }

        Button button = Instantiate<Button>(testButtonPrefab);
        button.GetComponentInChildren<Text>().text = name;
        button.onClick.AddListener(() =>
            {
                StartCoroutine(coroutine());
            });

        button.transform.SetParent(testButtonContainer);
    }

    /// <summary>
    /// Wipes the log file on disk, and also clears the display log.
    /// </summary>
    /// <returns>The coroutine for performing the wipe.</returns>
    public IEnumerator WipeLogFile()
    {
        if (logFile != null)
            logFile.Close();
        
        if (logText != null)
            logText.text = "";

        File.Delete(logPath);

        OpenLogFile();
        yield return null;
    }

    /// <summary>
    /// Clears the display log.
    /// </summary>
    /// <returns>The coroutine performing the clear.</returns>
    public IEnumerator ClearDisplayLog()
    {
        if (logText != null)
            logText.text = "";

        yield return null;
    }

    /// <summary>
    /// Stops all coroutines in this object.
    /// </summary>
    /// <returns>A coroutine for stopping all coroutines.</returns>
    public IEnumerator StopCoroutines()
    {
        StopAllCoroutines();
        yield return null;
    }

    /// <summary>
    /// Logs the given message to both the logfile and the displayed console.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    protected void Log(string message)
    {
        if (logText != null)
            logText.text += message + "\n";

        if (logFile != null)
        {
            logFile.WriteLine(message);
            logFile.Flush();
        }
    }

    /// <summary>
    /// Run a function the specified number of times, and then compute the average time
    /// required. Yields between tests, so that the UI will be somewhat more responsive
    /// during the testing process.
    /// </summary>
    /// <param name="action">The action to be run.</param>
    /// <param name="count">The number of times to run it.</param>
    /// <param name="name">The name to print out the information for.</param>
    public IEnumerator DoTest(Action action, int count, string name)
    {
        float[] results = new float[count];
        float totalTime = 0.0f, startTime;

        for (int i = 0; i < count; ++i)
        {
            startTime = Time.realtimeSinceStartup;
            action();
            results[i] = Time.realtimeSinceStartup - startTime;
            totalTime += results[i];
            yield return null;
        }

        float average = (totalTime / count);

        // compute the standard sample deviation
        // formula based on Wikipedia
        // https://en.wikipedia.org/wiki/Standard_deviation#Corrected_sample_standard_deviation
        float sumOfSquaredError = 0.0f;

        for (int i = 0; i < count; ++i)
        {
            sumOfSquaredError += (results[i] - average) * (results[i] - average);
        }

        float correctedSampleStandardDeviation = Mathf.Sqrt(sumOfSquaredError / (count - 1));

        Log(name + ": " + (totalTime / count) + " seconds average, " +
            correctedSampleStandardDeviation + " standard deviation, " +
            count + " trials");
    }

    /// <summary>
    /// Runs a bunch of path tests on the given world.
    /// </summary>
    /// <returns>A coroutine that can be run with StartCoroutine.</returns>
    /// <param name="world">The world to run the pathfinding tests on.</param>
    /// <param name="prefix">The name of the world, to be printed out.</param>
    /// <param name="count">The number of trials to run per test.</param>
    public IEnumerator PathSpeedTests(World world, string prefix, int count)
    {
        // ensure that the tilegraph is set up
        world.tileGraph = new Path_TileGraph(world);

        Tile center = world.GetTileAt(world.Width / 2 + 1, world.Height / 2 + 1);
        Tile nearby = world.GetTileAt(world.Width / 2 + 1, world.Height / 2 + 11);
        Tile corner = world.GetTileAt(11, 11);

        // pathfinding without inventory
        yield return StartCoroutine(DoTest(() =>
                {
                    new Path_AStar(world, center, nearby);
                }, count, prefix + " Center to Nearby"));
 
        yield return null;

        yield return StartCoroutine(DoTest(() =>
                {
                    new Path_AStar(world, center, corner);
                }, count, prefix + " Center to Corner"));

        yield return null;

        // pathfinding in a world with different types of inventory
        Inventory invCorner = new Inventory("Steel", 10, 10);
        world.inventoryManager.PlaceInventory(corner, invCorner);
        Inventory invNearby = new Inventory("Iron", 10, 10);
        world.inventoryManager.PlaceInventory(nearby, invNearby);

        yield return StartCoroutine(DoTest(() =>
                {
                    new Path_AStar(world, center, null, invNearby.objectType, 10, true);
                }, count, prefix + " Center to Nearby Inventory"));

        yield return null;

        yield return StartCoroutine(DoTest(() =>
                {
                    new Path_AStar(world, center, null, invCorner.objectType, 10, true);
                }, count, prefix + " Center to Corner Inventory"));

        yield return null;

    }

    /// <summary>
    /// Runs all of the pathfinding tests.
    /// </summary>
    /// <returns>A coroutine that can be run with StartCoroutine.</returns>
    public IEnumerator PathfindingSpeedTest(int count)
    {
        yield return StartCoroutine(DoEmptyTests(count));
        yield return StartCoroutine(DoSwitchbackTests(count));
        yield return StartCoroutine(DoDiagonalsTests(count));
    }

    /// <summary>
    /// Performs pathfinding tests on the empty world.
    /// </summary>
    /// <returns>The coroutine that can be run with StartCoroutine.</returns>
    /// <param name="count">The number of tests to run.</param>
    public IEnumerator DoEmptyTests(int count)
    {
        yield return null;

        Log("Beginning " + count + " pathfinding speed tests on the empty world ...");

        World world = new World(100, 100, false);
        MakeWorldTraversible(world);
        yield return StartCoroutine(PathSpeedTests(world, "[Empty]", count));

        Log("Finished empty world speed tests.");
    }

    /// <summary>
    /// Performs pathfinding tests on the switchback world.
    /// </summary>
    /// <returns>The coroutine that can be run with StartCoroutine.</returns>
    /// <param name="count">The number of tests to run.</param>
    public IEnumerator DoSwitchbackTests(int count)
    {
        yield return null;

        Log("Beginning " + count + " pathfinding speed tests on the switchback world ...");

        World world = new World(100, 100, false);
        MakeWorldTraversible(world);
        MakeSwitchback(world);
        yield return StartCoroutine(PathSpeedTests(world, "[Switchback]", count));

        Log("Finished switchback world speed tests.");
    }

    /// <summary>
    /// Performs pathfinding tests on the diagonals world.
    /// </summary>
    /// <returns>The coroutine that can be run with StartCoroutine.</returns>
    /// <param name="count">The number of tests to run.</param>
    public IEnumerator DoDiagonalsTests(int count)
    {
        yield return null;

        Log("Beginning " + count + " pathfinding speed tests on the diagonal world ...");

        World world = new World(100, 100, false);
        MakeWorldTraversible(world);
        MakeDiagonals(world);
        yield return StartCoroutine(PathSpeedTests(world, "[Diagonals]", count));

        Log("Finished diagonal world speed tests.");
    }

    /// <summary>
    /// Makes the entire world traversible, because there's no better way to test
    /// the pathfinding system.
    /// </summary>
    /// <param name="world">The world to change.</param>
    protected static void MakeWorldTraversible(World world)
    {
        for (int x = 0; x < world.Width; ++x)
        {
            for (int y = 0; y < world.Height; ++y)
            {
                world.GetTileAt(x, y).Type = TileType.Floor;
            }
        }
    }

    /// <summary>
    /// Changes the tile at the given location to TileType.Floor, then places a wall.
    /// </summary>
    /// <param name="world">The world in which to place the wall.</param>
    /// <param name="x">The tile x coordinate.</param>
    /// <param name="y">The tile y coordinate.</param>
    protected static void PlaceWall(World world, int x, int y)
    {
        Tile tile = world.GetTileAt(x, y);
        tile.Type = TileType.Floor;
        world.PlaceFurniture("furn_SteelWall", tile, false);
    }

    /// <summary>
    /// Sets up a world with a bunch of corridors that snake back and forth.
    /// </summary>
    /// <param name="world">The world that we're modifying.</param>
    public static void MakeSwitchback(World world)
    {
        // need a boundary because apparently we can't build next to the wall?
        int boundarySize = 10;

        // the size of the corridors to build
        int corridorSize = 5;

        bool xIsOffset = false;

        // first set up the border box
        for (int x = boundarySize; x < world.Width - boundarySize; ++x)
        {
            PlaceWall(world, x, boundarySize);
            PlaceWall(world, x, world.Height - boundarySize - 1);
        }

        for (int y = boundarySize; y < world.Height - boundarySize; ++y)
        {
            PlaceWall(world, boundarySize, y);
            PlaceWall(world, world.Width - boundarySize - 1, y);
        }

        // then set up the rows dividing us up
        for (int y = boundarySize; y < world.Height - boundarySize; y += corridorSize)
        {
            xIsOffset = !(xIsOffset);

            int xStart = boundarySize + (xIsOffset ? corridorSize : 1);
            int count = world.Width - 2 * boundarySize - corridorSize - 1;
            for (int x = xStart; x < xStart + count; ++x)
            {
                PlaceWall(world, x, y);
            }
        }
    }

    /// <summary>
    /// Creates a world with a long diagonal box.
    /// The side of the box closest to (0, 0) is closed.
    /// The side of the box closest to (Width, Height) is open.
    /// This funnels all paths from the center into the upper-right corner.
    /// </summary>
    /// <param name="world">World.</param>
    public static void MakeDiagonals(World world)
    {
        int boundarySize = 10;
        int thickness = 8;

        int xMin = boundarySize, xMax = world.Width - boundarySize - 1;
        int yMin = boundarySize, yMax = world.Height - boundarySize - 1;

        for (int i = 0; i <= thickness; ++i)
        {
            PlaceWall(world, xMin + i, yMin + thickness - i);
        }

        int xSize = (xMax - thickness) - (xMin + thickness);
        int ySize = (yMax - thickness) - (yMin + thickness);
        int size = Math.Min(xSize, ySize);

        for (int i = 1; i <= size; ++i)
        {
            PlaceWall(world, xMin + i, yMin + thickness + i);
            PlaceWall(world, xMin + thickness + i, yMin + i);
        }
    }

    /// <summary>
    /// Sets up a maze, in order to really test the pathfinding.
    /// </summary>
    /// <param name="world">The world that we're modifying.</param>
    public static void MakeMaze(World world)
    {
    }

}

