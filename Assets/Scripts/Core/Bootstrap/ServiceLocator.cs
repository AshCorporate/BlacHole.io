using System;
using System.Collections.Generic;

namespace BlacHole.Core
{
    /// <summary>
    /// Simple static service locator / IoC container.
    /// Register services at startup; retrieve them anywhere via Get&lt;T&gt;.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
                _services[type] = service;
            else
                _services.Add(type, service);
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
                return service as T;
            throw new InvalidOperationException($"[ServiceLocator] Service of type {type.Name} is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return service != null;
            }
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}
