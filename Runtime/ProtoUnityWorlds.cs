// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.Unity {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static class ProtoUnityWorlds {
        static ProtoWorld _defaultWorld;
        static Dictionary<string, ProtoWorld> _namedWorlds;

        public static bool Connected () {
            return _defaultWorld != null;
        }

        public static ProtoWorld Get (string worldName = default) {
#if DEBUG
            if (_defaultWorld == null) { throw new System.Exception ($"[ProtoUnityWorlds] нет подключенных миров, скорее всего не был сделан вызов \"systems.SetUnityWorlds()\""); }
#endif
            if (worldName == null) {
                return _defaultWorld;
            }
#if DEBUG
            if (!_namedWorlds.ContainsKey (worldName)) { throw new System.Exception ($"[ProtoUnityWorlds] не могу найти мир \"{worldName}\""); }
#endif
            return _namedWorlds[worldName];
        }

        public static void Set (IProtoSystems systems) {
            if (systems != null) {
                _defaultWorld = systems.World ();
                _namedWorlds = systems.NamedWorlds ();
            } else {
                _defaultWorld = null;
                _namedWorlds = null;
            }
        }
    }

    sealed class UnityWorldsSystem : IProtoInitSystem, IProtoDestroySystem {
        public void Init (IProtoSystems systems) {
            ProtoUnityWorlds.Set (systems);
        }

        public void Destroy () {
            ProtoUnityWorlds.Set (null);
        }
    }
}
