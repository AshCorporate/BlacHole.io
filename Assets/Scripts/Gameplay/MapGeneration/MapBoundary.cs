using UnityEngine;

namespace BlacHole.Gameplay.MapGeneration
{
    /// <summary>
    /// Renders the map edge boundary as a neon-cyan LineRenderer rectangle.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class MapBoundary : MonoBehaviour
    {
        [Header("Map Size")]
        [SerializeField] private float mapWidth  = 100f;
        [SerializeField] private float mapHeight = 100f;

        [Header("Visual")]
        [SerializeField] private Color  boundaryColor = new Color(0f, 1f, 1f, 1f); // neon cyan
        [SerializeField] private float  lineWidth     = 0.5f;

        private LineRenderer _lr;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _lr.useWorldSpace  = true;
            _lr.loop           = true;
            _lr.positionCount  = 4;
            _lr.startWidth     = lineWidth;
            _lr.endWidth       = lineWidth;
            _lr.numCapVertices = 4;
            _lr.sortingOrder   = 10;

            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = boundaryColor;
            _lr.material    = mat;
            _lr.startColor  = boundaryColor;
            _lr.endColor    = boundaryColor;

            DrawBoundary();
        }

        public void SetSize(float width, float height)
        {
            mapWidth  = width;
            mapHeight = height;
            DrawBoundary();
        }

        private void DrawBoundary()
        {
            if (_lr == null) return;

            float hw = mapWidth  * 0.5f;
            float hh = mapHeight * 0.5f;

            _lr.SetPosition(0, new Vector3(-hw, -hh, 0f));
            _lr.SetPosition(1, new Vector3( hw, -hh, 0f));
            _lr.SetPosition(2, new Vector3( hw,  hh, 0f));
            _lr.SetPosition(3, new Vector3(-hw,  hh, 0f));
        }
    }
}
