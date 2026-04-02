using UnityEngine;

namespace BlacHole.Gameplay.Territory
{
    /// <summary>
    /// Renders territory as a coloured semi-transparent tile overlay on the map.
    /// Each owner gets its own colour. Tiles are drawn via a grid of quads.
    /// </summary>
    public class TerritoryRenderer : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float tileAlpha    = 0.3f;
        [SerializeField] private int   maxOwners    = 10;
        [SerializeField] private Color[] ownerColors = new Color[]
        {
            new Color(0f, 1f, 1f),      // Player  — cyan
            new Color(1f, 0.27f, 0f),   // Bot 1   — neon orange
            new Color(0.69f, 0.15f, 1f),// Bot 2   — neon purple
            new Color(1f, 0.03f, 0.23f),// Bot 3   — neon red
            new Color(0f, 1f, 0.08f),   // Bot 4   — neon green
            new Color(1f, 0.84f, 0f),   // Bot 5   — gold
            new Color(0f, 0.6f, 1f),    // Bot 6   — blue
            new Color(1f, 0.5f, 0f),    // Bot 7   — orange
            new Color(0.5f, 1f, 0f),    // Bot 8   — lime
            new Color(1f, 0f, 0.8f),    // Bot 9   — magenta
        };

        private ITerritoryService _territory;
        private GameObject[,]     _tiles;
        private int                _gridW, _gridH;

        public void Initialise(ITerritoryService territory)
        {
            _territory = territory;
            _gridW     = territory.GridWidth;
            _gridH     = territory.GridHeight;
            _tiles     = new GameObject[_gridW, _gridH];

            // Pre-create tile GameObjects with SpriteRenderer
            for (int x = 0; x < _gridW; x++)
            {
                for (int y = 0; y < _gridH; y++)
                {
                    var go = new GameObject($"Tile_{x}_{y}");
                    go.transform.SetParent(transform, false);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite       = CreateSquareSprite();
                    sr.color        = Color.clear;
                    sr.sortingOrder = 1;

                    Vector2 world = territory.GridToWorld(x, y);
                    go.transform.position = new Vector3(world.x, world.y, 0f);

                    float cs = 1f; // default; will be scaled via parent or tile size
                    go.transform.localScale = Vector3.one * cs;

                    _tiles[x, y] = go;
                }
            }
        }

        private void LateUpdate()
        {
            if (_territory == null) return;
            RefreshTiles();
        }

        private void RefreshTiles()
        {
            for (int x = 0; x < _gridW; x++)
            {
                for (int y = 0; y < _gridH; y++)
                {
                    int owner = _territory.GetOwnerAt(_territory.GridToWorld(x, y));
                    var sr    = _tiles[x, y].GetComponent<SpriteRenderer>();

                    if (owner < 0)
                    {
                        sr.color = Color.clear;
                    }
                    else
                    {
                        Color c = owner < ownerColors.Length ? ownerColors[owner] : Color.white;
                        c.a     = tileAlpha;
                        sr.color = c;
                    }
                }
            }
        }

        private static Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(4, 4);
            for (int i = 0; i < 16; i++)
                tex.SetPixel(i % 4, i / 4, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
