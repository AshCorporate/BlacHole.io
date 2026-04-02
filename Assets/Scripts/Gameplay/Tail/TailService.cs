using System.Collections.Generic;
using UnityEngine;
using BlacHole.Core.Utilities;
using BlacHole.Gameplay.Player;
using BlacHole.Data;

namespace BlacHole.Gameplay.Tail
{
    /// <summary>
    /// Pure C# tail logic.
    /// Records points at fixed intervals, tracks timeout, detects self-intersection.
    /// </summary>
    public class TailService : ITailService
    {
        private readonly PlayerEntity _entity;
        private readonly TailConfig   _cfg;

        private readonly List<Vector2> _points = new List<Vector2>();
        private float _distAccum;
        private float _timer;

        public IReadOnlyList<Vector2> TailPoints => _points;
        public float TimeRemaining               => _timer;
        public bool  IsActive                    { get; private set; }

        // Events
        public event System.Action OnTailTimeout;
        public event System.Action OnSelfIntersection;

        public TailService(PlayerEntity entity, TailConfig cfg)
        {
            _entity = entity;
            _cfg    = cfg;
        }

        public void StartTail(Vector2 startPos)
        {
            Clear();
            IsActive = true;
            _timer   = _cfg != null ? _cfg.MaxTailDuration : 10f;
            AddPoint(startPos);
        }

        public void StopTail()
        {
            IsActive = false;
        }

        public void Clear()
        {
            _points.Clear();
            _distAccum = 0f;
            _timer     = 0f;
            IsActive   = false;
        }

        public void Tick(float dt)
        {
            if (!IsActive || !_entity.IsAlive) return;

            // Countdown timeout
            _timer -= dt;
            if (_timer <= 0f)
            {
                _timer = 0f;
                OnTailTimeout?.Invoke();
                return;
            }

            // Record new points by distance
            float interval = _cfg != null ? _cfg.RecordInterval : 0.08f;
            int maxPts      = _cfg != null ? _cfg.MaxPoints      : 500;

            if (_points.Count > 0)
            {
                float moved = Vector2.Distance(_entity.Position, _points[_points.Count - 1]);
                _distAccum += moved;
            }

            while (_distAccum >= interval)
            {
                _distAccum -= interval;
                AddPoint(_entity.Position);

                if (_points.Count > maxPts)
                    _points.RemoveAt(0);
            }

            // Self-intersection check (skip last 3 segments to allow normal movement)
            if (_points.Count >= 5)
                CheckSelfIntersection();
        }

        private void AddPoint(Vector2 pos)
        {
            _points.Add(pos);
        }

        private void CheckSelfIntersection()
        {
            int n = _points.Count;
            if (n < 4) return;

            // New segment is between last two points
            Vector2 a = _points[n - 2];
            Vector2 b = _points[n - 1];

            // Compare against all earlier segments except the 3 closest
            for (int i = 0; i < n - 3; i++)
            {
                Vector2 c = _points[i];
                Vector2 d = _points[i + 1];
                if (MathUtils.SegmentsIntersect(a, b, c, d))
                {
                    OnSelfIntersection?.Invoke();
                    return;
                }
            }
        }
    }
}
