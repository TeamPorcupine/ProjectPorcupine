#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;

namespace ProjectPorcupine.Pathfinding
{
    public class Cache
    {
        private const int MaxCacheSize = 20;

        private List<CacheKey> pathAge;
        private Dictionary<CacheKey, List<Tile>> pathLookup;

        public Cache()
        {
            pathAge = new List<CacheKey>();
            pathLookup = new Dictionary<CacheKey, List<Tile>>(new CacheKeyEqualityComparer());
        }

        public void Insert(List<Tile> path)
        {
            CacheKey key = new CacheKey(path);

            if (pathLookup.ContainsKey(key))
            {
                // Move path to last used
                pathAge.Remove(key);
                pathAge.Insert(0, key);
            }
            else
            {
                pathAge.Insert(0, key);
                pathLookup[key] = path;

                EnforceMaxCount();
            }
        }

        public bool Contains(CacheKey key)
        {
            return pathLookup.ContainsKey(key);
        }

        public bool Contains(Tile start, Tile end)
        {
            return pathLookup.ContainsKey(new CacheKey(start, end));
        }

        public List<Tile> GetPath(Tile start, Tile end)
        {
            return GetPath(new CacheKey(start, end));
        }

        public List<Tile> GetPath(CacheKey key)
        {
            return pathLookup[key];
        }

        private void EnforceMaxCount()
        {
            while (pathLookup.Count > MaxCacheSize)
            {
                int index = pathAge.Count - 1;
                CacheKey key = pathAge[index];
                pathAge.RemoveAt(index);
                pathLookup.Remove(key);
            }
        }

        public class CacheKey
        {
            private Tile start;
            private Tile end;

            public CacheKey(Tile startTile, Tile endTile)
            {
                start = startTile;
                end = endTile;
            }

            public CacheKey(List<Tile> path)
            {
                start = path.First();
                end = path.Last();
            }

            public override int GetHashCode()
            {
                return start.GetHashCode() ^ end.GetHashCode();
            }

            public bool Equals(CacheKey key)
            {
                return start.Equals(key.start) && end.Equals(key.end);
            }
        }

        public class CacheKeyEqualityComparer : IEqualityComparer<CacheKey>
        {
            public bool Equals(CacheKey b1, CacheKey b2)
            {
                if (b2 == null && b1 == null)
                {
                    return true;
                }
                else if (b1 == null | b2 == null)
                {
                    return false;
                }
                else if (b1.Equals(b2))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode(CacheKey bx)
            {
                return bx.GetHashCode();
            }
        }
    }
}
