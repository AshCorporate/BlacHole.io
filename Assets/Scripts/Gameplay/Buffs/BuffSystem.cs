using System.Collections.Generic;
using UnityEngine;

namespace BlacHole.Gameplay.Buffs
{
    /// <summary>
    /// Active buff / debuff modifier.
    /// </summary>
    public class BuffModifier
    {
        public string Id            { get; }
        public float  SpeedModifier { get; }
        public float  Duration      { get; set; }  // -1 = permanent until removed
        public bool   IsPermanent   => Duration < 0f;

        public BuffModifier(string id, float speedMod, float duration)
        {
            Id            = id;
            SpeedModifier = speedMod;
            Duration      = duration;
        }
    }

    /// <summary>
    /// Manages a list of active modifiers; computes their combined effect each tick.
    /// </summary>
    public class BuffSystem
    {
        private readonly List<BuffModifier> _active = new List<BuffModifier>();

        public float TotalSpeedModifier { get; private set; } = 1f;

        public void Add(BuffModifier buff)
        {
            // Remove any existing buff with same ID first
            Remove(buff.Id);
            _active.Add(buff);
            Recalculate();
        }

        public void Remove(string id)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                if (_active[i].Id == id) { _active.RemoveAt(i); break; }
            Recalculate();
        }

        public void Tick(float dt)
        {
            bool changed = false;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var b = _active[i];
                if (b.IsPermanent) continue;
                b.Duration -= dt;
                if (b.Duration <= 0f) { _active.RemoveAt(i); changed = true; }
            }
            if (changed) Recalculate();
        }

        private void Recalculate()
        {
            float total = 1f;
            foreach (var b in _active)
                total += b.SpeedModifier;
            TotalSpeedModifier = Mathf.Max(0f, total);
        }

        public bool HasBuff(string id)
        {
            foreach (var b in _active)
                if (b.Id == id) return true;
            return false;
        }

        public void Clear() => _active.Clear();
    }
}
