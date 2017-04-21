using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IReferenceManager
{
    /*
    bool MakeConnection<T1, T2>(T1 reference1, T2 reference2)
        where T1 : IReferenceable
        where T2 : IReferenceable;
    bool BreakConnection<T1, T2>(T1 reference1, T2 reference2)
        where T1 : IReferenceable
        where T2 : IReferenceable;
    bool BreakAllConnections<T1>(T1 reference1)
        where T1 : IReferenceable;
    */
    int GetCountOfType<K>()
        where K : class, IReferenceable;
    IEnumerable<K> GetEnumeratorOfType<K>()
        where K : class, IReferenceable;
    K GetFirstOfType<K>()
        where K : class, IReferenceable;
}

