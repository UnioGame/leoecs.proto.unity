// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Leopotam.EcsProto.Unity {
    public static class EditorExtensions {
        static readonly Dictionary<Type, (string, string)> _namesCache = new ();

        public static string CleanTypeNameCached (Type type, bool lowCased = false) {
            if (!_namesCache.TryGetValue (type, out var names)) {
                var name = DebugHelpers.CleanTypeName (type);
                names = (name, name.ToLowerInvariant ());
                _namesCache[type] = names;
            }
            return lowCased ? names.Item2 : names.Item1;
        }
    }
}
#endif
