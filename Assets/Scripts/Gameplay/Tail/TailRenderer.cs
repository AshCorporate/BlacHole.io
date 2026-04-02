using UnityEngine;

namespace BlacHole.Gameplay.Tail
{
    /// <summary>
    /// Renders the tail using a LineRenderer, coloured by owner colour.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TailRenderer : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private float tailWidth     = 0.15f;
        [SerializeField] private Color coreColor     = Color.white;
        [SerializeField] private float glowAlpha     = 0.6f;

        private LineRenderer _lr;
        private ITailService _tail;
        private Color        _ownerColor;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _lr.useWorldSpace = true;
            _lr.startWidth    = tailWidth;
            _lr.endWidth      = tailWidth * 0.4f;
            _lr.numCapVertices = 4;
            _lr.sortingOrder   = 3;
        }

        public void Initialise(ITailService tail, Color ownerColor)
        {
            _tail       = tail;
            _ownerColor = ownerColor;

            // Build a simple gradient: core-white at head, owner-colour at tail
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(coreColor, 0f), new GradientColorKey(_ownerColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(glowAlpha, 1f) }
            );
            _lr.colorGradient = gradient;
        }

        private void LateUpdate()
        {
            if (_tail == null || !_tail.IsActive || _tail.TailPoints.Count < 2)
            {
                _lr.positionCount = 0;
                return;
            }

            var pts = _tail.TailPoints;
            _lr.positionCount = pts.Count;
            for (int i = 0; i < pts.Count; i++)
                _lr.SetPosition(i, new Vector3(pts[i].x, pts[i].y, 0f));
        }
    }
}
