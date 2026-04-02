using System.Collections.Generic;
using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Core.Utilities;

namespace BlacHole.Gameplay.Territory
{
    /// <summary>
    /// Grid-based territory capture system.
    /// ownerGrid[x,y] = ownerId (-1 = neutral).
    /// </summary>
    public class TerritoryService : ITerritoryService
    {
        private int[,]  _ownerGrid;
        private int     _width;
        private int     _height;
        private float   _cellSize;
        private Vector2 _origin;

        // cell count per owner
        private readonly Dictionary<int, int> _cellCounts = new Dictionary<int, int>();
        private int _totalCells;

        public int GridWidth  => _width;
        public int GridHeight => _height;

        // ── Initialisation ───────────────────────────────────────────────────

        public void InitialiseGrid(int width, int height, float cellSize, Vector2 origin)
        {
            _width    = width;
            _height   = height;
            _cellSize = cellSize;
            _origin   = origin;
            _ownerGrid = new int[width, height];
            _totalCells = width * height;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _ownerGrid[x, y] = -1;

            _cellCounts.Clear();
        }

        public void SetStartingTerritory(int ownerId, Vector2 startPos, float startRadius)
        {
            var centre = WorldToGrid(startPos);
            int rad    = Mathf.CeilToInt(startRadius / _cellSize);

            for (int dx = -rad; dx <= rad; dx++)
            {
                for (int dy = -rad; dy <= rad; dy++)
                {
                    int gx = centre.x + dx;
                    int gy = centre.y + dy;
                    if (gx < 0 || gx >= _width || gy < 0 || gy >= _height) continue;
                    if (dx * dx + dy * dy <= rad * rad)
                        SetCell(gx, gy, ownerId);
                }
            }
        }

        public int FillEnclosedArea(int ownerId, List<Vector2> tailPoints)
        {
            if (tailPoints == null || tailPoints.Count < 3) return 0;

            var filled = PolygonFill.Fill(tailPoints, _origin, _cellSize, _width, _height);
            int count  = 0;
            foreach (var cell in filled)
            {
                if (cell.x < 0 || cell.x >= _width || cell.y < 0 || cell.y >= _height) continue;
                if (_ownerGrid[cell.x, cell.y] != ownerId)
                {
                    SetCell(cell.x, cell.y, ownerId);
                    count++;
                }
            }
            return count;
        }

        public float GetTerritoryPercent(int ownerId)
        {
            if (_totalCells == 0) return 0f;
            _cellCounts.TryGetValue(ownerId, out int cnt);
            return (float)cnt / _totalCells;
        }

        public bool IsOnOwnTerritory(int ownerId, Vector2 pos)
            => GetOwnerAt(pos) == ownerId;

        public bool IsOnEnemyTerritory(int ownerId, Vector2 pos)
        {
            int owner = GetOwnerAt(pos);
            return owner != -1 && owner != ownerId;
        }

        public int GetOwnerAt(Vector2 pos)
        {
            var cell = WorldToGrid(pos);
            if (cell.x < 0 || cell.x >= _width || cell.y < 0 || cell.y >= _height)
                return -1;
            return _ownerGrid[cell.x, cell.y];
        }

        public Vector2Int WorldToGrid(Vector2 world)
            => new Vector2Int(
                Mathf.FloorToInt((world.x - _origin.x) / _cellSize),
                Mathf.FloorToInt((world.y - _origin.y) / _cellSize));

        public Vector2 GridToWorld(int gx, int gy)
            => _origin + new Vector2(gx * _cellSize + _cellSize * 0.5f,
                                     gy * _cellSize + _cellSize * 0.5f);

        // ── Private ──────────────────────────────────────────────────────────

        private void SetCell(int gx, int gy, int ownerId)
        {
            int prev = _ownerGrid[gx, gy];
            if (prev == ownerId) return;

            if (prev >= 0)
            {
                _cellCounts.TryGetValue(prev, out int prevCnt);
                _cellCounts[prev] = Mathf.Max(0, prevCnt - 1);
            }

            _ownerGrid[gx, gy] = ownerId;

            if (ownerId >= 0)
            {
                _cellCounts.TryGetValue(ownerId, out int cnt);
                _cellCounts[ownerId] = cnt + 1;
            }
        }
    }
}
