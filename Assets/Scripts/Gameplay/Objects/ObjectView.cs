using UnityEngine;

namespace BlacHole.Gameplay.Objects
{
    /// <summary>
    /// Visual representation of an absorbable map object.
    /// Color-coded per cyberpunk palette.
    /// </summary>
    public class ObjectView : MonoBehaviour
    {
        private static readonly (string category, Color color, float baseSize)[] Palette = new[]
        {
            ("Micro",  new Color(0.224f, 1f,    0.078f),  0.4f),  // #39FF14 neon green
            ("Small",  new Color(1f,    0.420f, 0f),      0.8f),  // #FF6B00 neon orange
            ("Medium", new Color(0.690f, 0.149f, 1f),     2.0f),  // #B026FF neon purple
            ("Large",  new Color(1f,    0.027f, 0.227f),  4.0f),  // #FF073A neon red
            ("Mega",   new Color(1f,    0.843f, 0f),      8.0f),  // #FFD700 gold
        };

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null)
            {
                _sr = gameObject.AddComponent<SpriteRenderer>();
                _sr.sprite       = CreateSquareSprite();
                _sr.sortingOrder = 2;
            }
        }

        public void Initialise(string category, float radius)
        {
            if (_sr == null) Awake();

            Color  col  = Color.white;
            float  size = radius * 2f;

            foreach (var entry in Palette)
            {
                if (entry.category == category)
                {
                    col  = entry.color;
                    size = entry.baseSize;
                    break;
                }
            }

            _sr.color            = col;
            transform.localScale = Vector3.one * size;
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
