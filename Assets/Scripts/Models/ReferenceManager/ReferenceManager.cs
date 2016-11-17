using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// This class it ment to replace all references between 
/// </summary>
public class ReferenceManager : IReferenceManager
{
    //all the references this manager is controling 
    HashSet<IReferenceable> _references;
    
    public ReferenceManager()
    {
        _references = new HashSet<IReferenceable>();
    }
    
    /*
     * currently callback system not implemented out of simplicity
     * 
    event Action<IReferenceable, IReferenceable> _referencesChanged;
    public void RegisterChange(Action<IReferenceable, IReferenceable> handler)
    {
        _referencesChanged += handler;
    }
    public void UnregisterChange(Action<IReferenceable, IReferenceable> handler)
    {
        _referencesChanged -= handler;
    }
    */

    //this method operates as if it was static (i.e it does not reference any memeber variables),
    //but interfaces dont enforce the use of static methods
    public static bool MakeConnection<T1, T2>(T1 reference1, T2 reference2)
        where T1 : IReferenceable
        where T2 : IReferenceable
    {
        if (reference1 == null || reference2 == null)
        {
            Debug.ULogErrorChannel("ReferenceManager", "Incorrect use of MakeConnection!");
            return false;
        }
        if (reference1.ReferenceManager.HasLinkInstance(reference2) && reference2.ReferenceManager.HasLinkInstance(reference1))
        {
            //connection already made
            Debug.ULogWarningChannel("ReferenceManager", "Reference already created between these class instances.");
            return false;
        }
        else if (reference1.ReferenceManager.HasLinkInstance(reference2) || reference2.ReferenceManager.HasLinkInstance(reference1))
        {
            //one knows of the connection the other is oblivious
            // this is a problem... improper use
            Debug.ULogErrorChannel("ReferenceManager", "References not being maintained correctly!");
            return false;
        }
        else
        {
            //make connection
            reference1.ReferenceManager.AddLinkInstance(reference2);
            reference2.ReferenceManager.AddLinkInstance(reference1);
            return true;
        }
    }

    //this method operates as if it was static (i.e it does not reference any memeber variables),
    //but interfaces dont enforce the use of static methods
    public static bool BreakConnection<T1, T2>(T1 reference1, T2 reference2)
        where T1 : IReferenceable
        where T2 : IReferenceable
    {
        if(reference1 == null || reference2 == null)
        {
            Debug.ULogErrorChannel("ReferenceManager", "Incorrect use of MakeConnection!");
            return false;
        }
        if (reference1.ReferenceManager.HasLinkInstance(reference2) && reference2.ReferenceManager.HasLinkInstance(reference1))
        {
            //remove connection
            reference1.ReferenceManager.RemoveLinkInstance(reference2);
            reference2.ReferenceManager.RemoveLinkInstance(reference1);
            return true;
        }
        else if (reference1.ReferenceManager.HasLinkInstance(reference2) || reference2.ReferenceManager.HasLinkInstance(reference1))
        {
            //one knows of the connection the other is oblivious
            // this is a problem... improper use
            Debug.ULogErrorChannel("ReferenceManager", "References not being maintained correctly!");
            return false;
        }
        else
        {
            //connection doesn't exsist
            Debug.ULogWarningChannel("ReferenceManager","Reference does not exsist between these class instances.");
            return false;
        }
    }
    
    //all Reference managers should have these methods but interfaces cant enforce the use of static methods
    //use this function to delete the referance manager used by reference1
    public static bool BreakAllConnections<T1>(T1 reference1)
        where T1 : IReferenceable
    {
        return reference1.ReferenceManager.BreakAllConnectionsOfInstance<T1>(reference1); 
    }

    private bool BreakAllConnectionsOfInstance<T1>(T1 reference1)
        where T1 : IReferenceable
    {
        bool result = true;
        while (_references.Count > 0)
        {
            //reference1 is the owner of the reference manager
            IReferenceable remove = FirstLinkInstance();
            if (BreakConnection(reference1, remove) == false)
            {
                result = false;
            }
        }
        return result;
    }

    //looks through the hash set and finds the number of items of type k (i.e Character, Tile, Furniture)
    public int GetCountOfType<K>()
        where K : class, IReferenceable
    {
        int count = 0;
        foreach (IReferenceable reference in _references)
        {
            K check = reference as K;
            if (check != null)
            {
                count++;
            }
        }
        return count;
    }

    //looks through the hash set and finds the items of type k (i.e Character, Tile, Furniture) then puts them in a list;
    public IEnumerable<K> GetEnumeratorOfType<K>()
        where K : class, IReferenceable
    {
        HashSet<K> result = new HashSet<K>();
        foreach (IReferenceable reference in _references)
        {
            K check = reference as K;
            if (check != null)
            {
                if (result.Contains(check))
                {
                    Debug.ULogErrorChannel("ReferenceManager", "References not being maintained correctly!");
                }
                else
                {
                    result.Add(check);
                }
            }
        }
        return result;
    }

    public K GetFirstOfType<K>()
        where K : class, IReferenceable
    {
        //return the first instance where the conversion of IReferenceable to K is not null
        return _references.FirstOrDefault((con) => (con as K != null)) as K;
    }

    private bool HasLinkInstance(IReferenceable link)
    {
        return _references.Contains(link);
    }

    private bool AddLinkInstance(IReferenceable link)
    {
        if (_references.Contains(link))
        {
            return false;
        }
        _references.Add(link);
        return true;
    }

    private bool RemoveLinkInstance(IReferenceable link)
    {
        if (_references.Contains(link))
        {   
            _references.Remove(link);
            return true;
        }
        return false;
    }
    
    private IReferenceable FirstLinkInstance()
    {
        return _references.FirstOrDefault();
    }
}
