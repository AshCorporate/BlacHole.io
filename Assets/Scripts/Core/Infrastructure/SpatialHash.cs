using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlacHole.Core.Infrastructure
{
    /// <summary>
    /// 2D spatial hash grid for fast proximity queries.
    /// Used by spawn validation, gravity system, and collision.
    /// </summary>
    public class SpatialHash<T>
    {
        private readonly float _cellSize;
        private readonly Dictionary<long, List<T>> _buckets;

        public SpatialHash(float cellSize = 5f)
        {
            _cellSize = Mathf.Max(0.01f, cellSize);
            _buckets  = new Dictionary<long, List<T>>();
        }

        // ── Hashing ──────────────────────────────────────────────────────────

        private int GridX(float x) => Mathf.FloorToInt(x / _cellSize);
        private int GridY(float y) => Mathf.FloorToInt(y / _cellSize);

        private long Key(int gx, int gy)
        {
            // Pack two ints into a long for use as dictionary key
            return ((long)(uint)gx << 32) | (uint)gy;
        }

        private long Key(Vector2 pos) => Key(GridX(pos.x), GridY(pos.y));

        // ── Insert / Remove ──────────────────────────────────────────────────

        public void Insert(Vector2 pos, T item)
        {
            var key = Key(pos);
            if (!_buckets.TryGetValue(key, out var list))
            {
                list = new List<T>();
                _buckets[key] = list;
            }
            list.Add(item);
        }

        public bool Remove(Vector2 pos, T item)
        {
            var key = Key(pos);
            if (_buckets.TryGetValue(key, out var list))
                return list.Remove(item);
            return false;
        }

        public void Clear() => _buckets.Clear();

        // ── Queries ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all items within a radius of the query point.
        /// </summary>
        public void Query(Vector2 center, float radius, List<T> results)
        {
            results.Clear();
            int minGx = GridX(center.x - radius);
            int maxGx = GridX(center.x + radius);
            int minGy = GridY(center.y - radius);
            int maxGy = GridY(center.y + radius);

            float radiusSq = radius * radius;

            for (int gx = minGx; gx <= maxGx; gx++)
            {
                for (int gy = minGy; gy <= maxGy; gy++)
                {
                    if (_buckets.TryGetValue(Key(gx, gy), out var list))
                    {
                        foreach (var item in list)
                            results.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether any item exists within radius (fast early-out).
        /// </summary>
        public bool HasAny(Vector2 center, float radius)
        {
            int minGx = GridX(center.x - radius);
            int maxGx = GridX(center.x + radius);
            int minGy = GridY(center.y - radius);
            int maxGy = GridY(center.y + radius);

            for (int gx = minGx; gx <= maxGx; gx++)
            {
                for (int gy = minGy; gy <= maxGy; gy++)
                {
                    if (_buckets.TryGetValue(Key(gx, gy), out var list) && list.Count > 0)
                        return true;
                }
            }
            return false;
        }

        public int TotalCount
        {
            get
            {
                int count = 0;
                foreach (var list in _buckets.Values)
                    count += list.Count;
                return count;
            }
        }
    }
}
