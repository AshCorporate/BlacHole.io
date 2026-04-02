using System;
using UnityEngine;

namespace BlacHole.Core.Utilities
{
    /// <summary>
    /// General-purpose math helpers used across gameplay systems.
    /// </summary>
    public static class MathUtils
    {
        // ── Segment intersection ─────────────────────────────────────────────

        /// <summary>
        /// Returns true when segment AB intersects segment CD (excluding shared endpoints).
        /// </summary>
        public static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float d1 = Cross(c, d, a);
            float d2 = Cross(c, d, b);
            float d3 = Cross(a, b, c);
            float d4 = Cross(a, b, d);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            if (Mathf.Approximately(d1, 0) && OnSegment(c, d, a)) return true;
            if (Mathf.Approximately(d2, 0) && OnSegment(c, d, b)) return true;
            if (Mathf.Approximately(d3, 0) && OnSegment(a, b, c)) return true;
            if (Mathf.Approximately(d4, 0) && OnSegment(a, b, d)) return true;

            return false;
        }

        private static float Cross(Vector2 o, Vector2 a, Vector2 b)
            => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

        private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
            => Mathf.Min(p.x, q.x) <= r.x && r.x <= Mathf.Max(p.x, q.x) &&
               Mathf.Min(p.y, q.y) <= r.y && r.y <= Mathf.Max(p.y, q.y);

        // ── Polygon area ─────────────────────────────────────────────────────

        /// <summary>Signed area of a polygon (positive = CCW).</summary>
        public static float PolygonSignedArea(Vector2[] polygon)
        {
            float area = 0f;
            int n = polygon.Length;
            for (int i = 0; i < n; i++)
            {
                var a = polygon[i];
                var b = polygon[(i + 1) % n];
                area += (a.x * b.y) - (b.x * a.y);
            }
            return area * 0.5f;
        }

        /// <summary>True if point P is inside convex or concave polygon.</summary>
        public static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            int n = polygon.Length;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];
                if ((pi.y > point.y) != (pj.y > point.y) &&
                    point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x)
                    inside = !inside;
            }
            return inside;
        }

        // ── Misc ─────────────────────────────────────────────────────────────

        public static float AngleDeltaDeg(float from, float to)
        {
            float delta = Mathf.Repeat(to - from + 180f, 360f) - 180f;
            return delta;
        }

        public static Vector2 Rotate(Vector2 v, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        public static float RemapClamped(float value, float inMin, float inMax, float outMin, float outMax)
        {
            float t = Mathf.Clamp01((value - inMin) / (inMax - inMin));
            return Mathf.LerpUnclamped(outMin, outMax, t);
        }
    }
}
