using System.Collections.Generic;
using UnityEngine;

namespace BlacHole.Gameplay.MapGeneration
{
    /// <summary>Interface for map generators.</summary>
    public interface IMapGenerator
    {
        void GenerateMap(int seed, int width, int height);
        void ClearMap();
        void Regenerate();

        IReadOnlyList<GameObject> GetSpawnedObjects();
        Bounds                    GetMapBounds();
    }
}
