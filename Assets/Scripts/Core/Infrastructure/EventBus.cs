using System;
using System.Collections.Generic;

namespace BlacHole.Core.Infrastructure
{
    /// <summary>
    /// Simple typed event bus for decoupled communication between systems.
    /// Subscribe with lambdas or method references; fire events by type.
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public void Fire<T>(T evt)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list) || list.Count == 0)
                return;

            // Iterate a snapshot to allow handlers to unsubscribe during dispatch
            var snapshot = new Delegate[list.Count];
            list.CopyTo(snapshot);
            foreach (var d in snapshot)
                (d as Action<T>)?.Invoke(evt);
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
