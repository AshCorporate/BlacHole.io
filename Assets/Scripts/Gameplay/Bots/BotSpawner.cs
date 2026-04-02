using UnityEngine;
using BlacHole.Data;

namespace BlacHole.Gameplay.Bots
{
    /// <summary>
    /// Spawns N bots at game start with randomised starting positions and colours.
    /// </summary>
    public class BotSpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private BotConfig      botConfig;
        [SerializeField] private PlayerConfig   playerConfig;
        [SerializeField] private MovementConfig movementConfig;
        [SerializeField] private TailConfig     tailConfig;

        [Header("Prefab")]
        [SerializeField] private GameObject botPrefab;

        [Header("Map")]
        [SerializeField] private Vector2 mapMin = new Vector2(-50, -50);
        [SerializeField] private Vector2 mapMax = new Vector2( 50,  50);

        private static readonly Color[] BotColors = new Color[]
        {
            new Color(1f, 0.27f, 0f),     // neon orange
            new Color(0.69f, 0.15f, 1f),  // neon purple
            new Color(1f, 0.03f, 0.23f),  // neon red
            new Color(0f, 1f, 0.08f),     // neon green
            new Color(1f, 0.84f, 0f),     // gold
            new Color(0f, 0.6f, 1f),      // blue
            new Color(1f, 0.5f, 0f),      // orange
            new Color(0.5f, 1f, 0f),      // lime
            new Color(1f, 0f, 0.8f),      // magenta
            new Color(0f, 1f, 0.9f),      // teal
        };

        private void Start()
        {
            int count = botConfig != null ? botConfig.BotCount : 5;
            SpawnBots(count);
        }

        public void SpawnBots(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = new Vector2(
                    Random.Range(mapMin.x + 5f, mapMax.x - 5f),
                    Random.Range(mapMin.y + 5f, mapMax.y - 5f));

                var go         = Instantiate(botPrefab, new Vector3(spawnPos.x, spawnPos.y, 0f), Quaternion.identity);
                var controller = go.GetComponent<BotController>();

                if (controller == null) continue;

                int    id    = i + 1;  // player is 0, bots start at 1
                string name  = $"Bot{id}";
                Color  color = BotColors[i % BotColors.Length];

                controller.Initialise(id, name, color);
                go.name = name;
            }
        }
    }
}
