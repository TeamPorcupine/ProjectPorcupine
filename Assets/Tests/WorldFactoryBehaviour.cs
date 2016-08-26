using System.Collections;
using UnityEngine;

/// <summary>
/// Creates a world, waits a while, and then destroys it.
/// </summary>
public class WorldFactoryBehaviour : MonoBehaviour
{
    void Awake()
    {
        StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        Debug.Log("Creating world.");

        WorldFactory factory = new WorldFactory();
        factory.Create();

        yield return new WaitForSeconds(3f);

        Debug.Log("Destroying world.");

        // TODO Not implemented.
        //// factory.Destroy();
    }
}
