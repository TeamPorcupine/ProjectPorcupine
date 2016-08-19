using System;
using System.Collections.Generic;

public class DuplicateKeyComparer<TKey>
                :
             IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members

    int equalReturn;

    public DuplicateKeyComparer(bool EqualValueAtEnd=false)
    {
        this.equalReturn=EqualValueAtEnd?-1:1;
    }

    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return equalReturn;   // Handle equality as beeing greater
        else
            return result;
    }

    #endregion
}