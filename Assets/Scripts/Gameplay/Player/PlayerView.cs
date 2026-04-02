using UnityEngine;

namespace BlacHole.Gameplay.Player
{
    /// <summary>
    /// Handles the visual representation of a player:
    /// scales the sprite by radius, applies the player colour.
    /// </summary>
    public class PlayerView : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer bodySprite;
        [SerializeField] private SpriteRenderer ringSprite;
        [SerializeField] private float          pulseMagnitude = 0.08f;
        [SerializeField] private float          pulseSpeed     = 2.5f;

        private float _pulseTimer;

        private void Awake()
        {
            // Auto-find child sprites if not wired in inspector
            if (bodySprite == null)
            {
                var go = new GameObject("Body");
                go.transform.SetParent(transform, false);
                bodySprite = go.AddComponent<SpriteRenderer>();
                bodySprite.sprite = CreateCircleSprite(64);
                bodySprite.sortingOrder = 5;
            }

            if (ringSprite == null)
            {
                var go = new GameObject("Ring");
                go.transform.SetParent(transform, false);
                ringSprite = go.AddComponent<SpriteRenderer>();
                ringSprite.sprite = CreateRingSprite(64, 0.85f);
                ringSprite.sortingOrder = 6;
            }
        }

        public void Refresh(PlayerEntity entity)
        {
            _pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = entity.Radius * 2f * (1f + Mathf.Sin(_pulseTimer) * pulseMagnitude);

            transform.localScale = Vector3.one * entity.Radius * 2f;

            bodySprite.color = entity.PlayerColor;

            if (ringSprite != null)
            {
                float ringScale = 1f + Mathf.Sin(_pulseTimer) * pulseMagnitude;
                ringSprite.transform.localScale = Vector3.one * ringScale;
                Color rc = entity.PlayerColor;
                rc.a = 0.6f;
                ringSprite.color = rc;
            }
        }

        // ── Sprite creation helpers ──────────────────────────────────────────

        private static Sprite CreateCircleSprite(int resolution)
        {
            var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float r = resolution * 0.5f;
            for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                {
                    float dx = x - r + 0.5f, dy = y - r + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((r - dist) / 1.5f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution),
                                 new Vector2(0.5f, 0.5f), resolution);
        }

        private static Sprite CreateRingSprite(int resolution, float innerFrac)
        {
            var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float r = resolution * 0.5f;
            float inner = r * innerFrac;
            for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                {
                    float dx = x - r + 0.5f, dy = y - r + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = (dist >= inner && dist <= r) ? Mathf.Clamp01((r - dist) / 1.2f) : 0f;
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution),
                                 new Vector2(0.5f, 0.5f), resolution);
        }
    }
}
