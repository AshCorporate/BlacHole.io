using System.Collections.Generic;
using UnityEngine;

namespace BlacHole.Core.Utilities
{
    /// <summary>
    /// Scanline polygon fill on a 2D integer grid.
    /// Used by TerritoryService to determine which cells fall inside a closed tail loop.
    /// </summary>
    public static class PolygonFill
    {
        /// <summary>
        /// Given a list of world-space polygon vertices and grid parameters,
        /// fills all grid cells whose centres fall inside the polygon.
        /// Returns the list of filled (gridX, gridY) cell coordinates.
        /// </summary>
        public static List<Vector2Int> Fill(
            List<Vector2> polygon,
            Vector2 gridOrigin,
            float cellSize,
            int gridWidth,
            int gridHeight)
        {
            var result = new List<Vector2Int>();
            if (polygon == null || polygon.Count < 3) return result;

            // Convert polygon vertices to grid space
            var gridPoly = new Vector2[polygon.Count];
            for (int i = 0; i < polygon.Count; i++)
                gridPoly[i] = WorldToGrid(polygon[i], gridOrigin, cellSize);

            // Scanline fill
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var p in gridPoly)
            {
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            int scanMin = Mathf.Max(0, Mathf.FloorToInt(minY));
            int scanMax = Mathf.Min(gridHeight - 1, Mathf.CeilToInt(maxY));

            for (int gy = scanMin; gy <= scanMax; gy++)
            {
                float scanY = gy + 0.5f;  // cell centre
                var intersects = new List<float>();

                int n = gridPoly.Length;
                for (int i = 0, j = n - 1; i < n; j = i++)
                {
                    float y0 = gridPoly[j].y;
                    float y1 = gridPoly[i].y;
                    float x0 = gridPoly[j].x;
                    float x1 = gridPoly[i].x;

                    if ((y0 <= scanY && y1 > scanY) || (y1 <= scanY && y0 > scanY))
                    {
                        float t = (scanY - y0) / (y1 - y0);
                        intersects.Add(x0 + t * (x1 - x0));
                    }
                }

                intersects.Sort();

                for (int k = 0; k + 1 < intersects.Count; k += 2)
                {
                    int xStart = Mathf.Max(0, Mathf.CeilToInt(intersects[k]));
                    int xEnd   = Mathf.Min(gridWidth - 1, Mathf.FloorToInt(intersects[k + 1]));
                    for (int gx = xStart; gx <= xEnd; gx++)
                        result.Add(new Vector2Int(gx, gy));
                }
            }

            return result;
        }

        private static Vector2 WorldToGrid(Vector2 world, Vector2 origin, float cellSize)
            => new Vector2((world.x - origin.x) / cellSize, (world.y - origin.y) / cellSize);
    }
}
