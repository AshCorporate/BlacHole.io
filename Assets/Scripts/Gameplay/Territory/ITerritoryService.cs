using System.Collections.Generic;
using UnityEngine;

namespace BlacHole.Gameplay.Territory
{
    /// <summary>Interface for the territory capture grid.</summary>
    public interface ITerritoryService
    {
        void InitialiseGrid(int width, int height, float cellSize, Vector2 origin);

        /// <summary>Mark cells owned by this player within startRadius at startPos.</summary>
        void SetStartingTerritory(int ownerId, Vector2 startPos, float startRadius);

        /// <summary>Fill the area enclosed by tailPoints for ownerId. Returns number of cells captured.</summary>
        int FillEnclosedArea(int ownerId, List<Vector2> tailPoints);

        /// <summary>Fraction of map owned by this player (0..1).</summary>
        float GetTerritoryPercent(int ownerId);

        bool IsOnOwnTerritory(int ownerId, Vector2 pos);
        bool IsOnEnemyTerritory(int ownerId, Vector2 pos);

        /// <summary>Get owner id at world position (-1 = neutral).</summary>
        int GetOwnerAt(Vector2 pos);

        Vector2Int WorldToGrid(Vector2 world);
        Vector2    GridToWorld(int gx, int gy);

        int GridWidth  { get; }
        int GridHeight { get; }
    }
}
