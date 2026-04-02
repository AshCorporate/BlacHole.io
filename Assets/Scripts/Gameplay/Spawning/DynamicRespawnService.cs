using System.Collections;
using UnityEngine;
using BlacHole.Data;
using BlacHole.Gameplay.Spawning;

namespace BlacHole.Gameplay.Spawning
{
    /// <summary>
    /// Respawns consumed Micro/Small objects over time to maintain map density.
    /// </summary>
    public class DynamicRespawnService : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SpawnCategoryConfig[] dynamicCategories;
        [SerializeField] private float respawnInterval = 3f;
        [SerializeField] private int   maxRespawnPerTick = 5;

        [Header("Map Bounds")]
        [SerializeField] private Vector2 mapMin = new Vector2(-50, -50);
        [SerializeField] private Vector2 mapMax = new Vector2( 50,  50);

        [Header("References")]
        [SerializeField] private ObjectSpawner spawner;

        private void Start()
        {
            StartCoroutine(RespawnLoop());
        }

        private IEnumerator RespawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(respawnInterval);
                RespawnObjects();
            }
        }

        private void RespawnObjects()
        {
            if (dynamicCategories == null || spawner == null) return;

            int count = 0;
            foreach (var cat in dynamicCategories)
            {
                if (cat == null) continue;
                if (cat.SpawnType != SpawnType.Dynamic) continue;
                if (count >= maxRespawnPerTick) break;

                Vector2 pos = new Vector2(
                    Random.Range(mapMin.x, mapMax.x),
                    Random.Range(mapMin.y, mapMax.y));

                spawner.SpawnByCategory(cat.CategoryName, pos);
                count++;
            }
        }
    }
}
