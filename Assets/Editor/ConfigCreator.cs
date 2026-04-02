using UnityEditor;
using UnityEngine;

namespace BlacHole.Editor
{
    /// <summary>
    /// Creates default ScriptableObject config assets under Assets/Data/ScriptableObjects/.
    /// Menu: Tools → BlacHole.io → Create Default Configs
    /// </summary>
    public static class ConfigCreator
    {
        private const string OutputFolder = "Assets/Data/ScriptableObjects";

        [MenuItem("Tools/BlacHole.io/Create Default Configs")]
        public static void CreateDefaultConfigs()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(OutputFolder);

            CreateConfig<BlacHole.Data.MovementConfig>     ("MovementConfig");
            CreateConfig<BlacHole.Data.PlayerConfig>       ("PlayerConfig");
            CreateConfig<BlacHole.Data.MapGenerationConfig>("MapGenerationConfig");
            CreateConfig<BlacHole.Data.BotConfig>          ("BotConfig");
            CreateConfig<BlacHole.Data.TailConfig>         ("TailConfig");
            CreateConfig<BlacHole.Data.GravityConfig>      ("GravityConfig");
            CreateConfig<BlacHole.Data.TerritoryConfig>    ("TerritoryConfig");
            CreateConfig<BlacHole.Data.GameRulesConfig>    ("GameRulesConfig");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ Default configs created in Assets/Data/ScriptableObjects/");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void CreateConfig<T>(string assetName) where T : ScriptableObject
        {
            var assetPath = $"{OutputFolder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<T>(assetPath) != null)
            {
                Debug.Log($"[ConfigCreator] {assetName}.asset already exists — skipped.");
                return;
            }

            var instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, assetPath);
            Debug.Log($"[ConfigCreator] Created {assetPath}");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                var parts  = folderPath.Split('/');
                var parent = string.Join("/", parts, 0, parts.Length - 1);
                var child  = parts[parts.Length - 1];
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
