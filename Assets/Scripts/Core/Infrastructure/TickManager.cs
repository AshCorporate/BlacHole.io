using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlacHole.Core.Infrastructure
{
    /// <summary>
    /// Centralised update / fixed-update tick distribution.
    /// Systems register callbacks instead of each having their own MonoBehaviour.
    /// </summary>
    public class TickManager : MonoBehaviour
    {
        private readonly List<Action<float>> _updateListeners      = new List<Action<float>>();
        private readonly List<Action<float>> _fixedUpdateListeners = new List<Action<float>>();
        private readonly List<Action<float>> _lateUpdateListeners  = new List<Action<float>>();

        // ── Registration ─────────────────────────────────────────────────────

        public void RegisterUpdate(Action<float> cb)      => _updateListeners.Add(cb);
        public void RegisterFixedUpdate(Action<float> cb) => _fixedUpdateListeners.Add(cb);
        public void RegisterLateUpdate(Action<float> cb)  => _lateUpdateListeners.Add(cb);

        public void UnregisterUpdate(Action<float> cb)      => _updateListeners.Remove(cb);
        public void UnregisterFixedUpdate(Action<float> cb) => _fixedUpdateListeners.Remove(cb);
        public void UnregisterLateUpdate(Action<float> cb)  => _lateUpdateListeners.Remove(cb);

        // ── Unity callbacks ──────────────────────────────────────────────────

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _updateListeners.Count; i++)
                _updateListeners[i]?.Invoke(dt);
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            for (int i = 0; i < _fixedUpdateListeners.Count; i++)
                _fixedUpdateListeners[i]?.Invoke(dt);
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _lateUpdateListeners.Count; i++)
                _lateUpdateListeners[i]?.Invoke(dt);
        }

        public void Clear()
        {
            _updateListeners.Clear();
            _fixedUpdateListeners.Clear();
            _lateUpdateListeners.Clear();
        }
    }
}
