using System.Collections.Generic;
using UnityEngine;
using BlacHole.Data;
using BlacHole.Gameplay.Objects;

namespace BlacHole.Gameplay.Spawning
{
    /// <summary>
    /// Pools and places map objects from prefab lists defined in SpawnCategoryConfig.
    /// Falls back to procedural default objects when no prefab is wired.
    /// </summary>
    public class ObjectSpawner : MonoBehaviour
    {
        [Header("Category Configs")]
        [SerializeField] private SpawnCategoryConfig[] categories;

        private readonly Dictionary<string, Queue<GameObject>> _pools
            = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (categories == null) return;
            foreach (var cat in categories)
            {
                if (cat == null) continue;
                _pools[cat.CategoryName] = new Queue<GameObject>();
            }
        }

        /// <summary>Spawn or retrieve pooled object for category at world position.</summary>
        public GameObject SpawnByCategory(string categoryName, Vector2 pos)
        {
            SpawnCategoryConfig cat = FindCategory(categoryName);

            // Try pool first
            if (_pools.TryGetValue(categoryName, out var pool) && pool.Count > 0)
            {
                var reused = pool.Dequeue();
                reused.transform.position = new Vector3(pos.x, pos.y, 0f);
                reused.SetActive(true);
                return reused;
            }

            // Instantiate from prefab list
            if (cat != null && cat.Prefabs != null && cat.Prefabs.Length > 0)
            {
                int idx  = Random.Range(0, cat.Prefabs.Length);
                var prefab = cat.Prefabs[idx];
                if (prefab != null)
                {
                    var go = Instantiate(prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
                    return go;
                }
            }

            // Fallback: procedural object
            return null;
        }

        /// <summary>Return an object to its category pool.</summary>
        public void Return(string categoryName, GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            if (_pools.TryGetValue(categoryName, out var pool))
                pool.Enqueue(go);
            else
                Destroy(go);
        }

        private SpawnCategoryConfig FindCategory(string name)
        {
            if (categories == null) return null;
            foreach (var c in categories)
                if (c != null && c.CategoryName == name) return c;
            return null;
        }
    }
}
