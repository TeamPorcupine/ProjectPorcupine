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

public class DuplicateKeyComparer<TKey>
                :
             IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members

    private int equalReturn;

    public DuplicateKeyComparer(bool equalValueAtEnd = false)
    {
        this.equalReturn = equalValueAtEnd ? -1 : 1;
    }

    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
        {
            return equalReturn; // Handle equality as beeing greater
        }
        else
        {
            return result;
        }
    }

    #endregion
}