// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.Unity {
    public sealed class UnityModule : IProtoModule {
        Config _config;

        public struct Config {
            public bool DisableDebugSystems;
            public string SystemsName;
            public bool NotBakeComponentsInName;
            public string EntityNameFormat;
            public int UpdatePointWeight;
            public string[] DisableDebugWorlds;
            public const string DefaultEntityNameFormat = "D6";
        }

        public UnityModule (Config config = default) {
            _config = config;
            _config.EntityNameFormat ??= Config.DefaultEntityNameFormat;
            _config.DisableDebugWorlds ??= Array.Empty<string> ();
        }

        public void Init (IProtoSystems systems) {
            systems
                .AddSystem (new UnityWorldsSystem ())
                .AddSystem (new UnityLinkSystem ());
#if UNITY_EDITOR
            if (!_config.DisableDebugSystems) {
                systems.AddSystem (new ProtoSystemsDebugSystem (_config.SystemsName), _config.UpdatePointWeight);
                if (Array.IndexOf (_config.DisableDebugWorlds, null) == -1) {
                    systems.AddSystem (new ProtoWorldDebugSystem (default, !_config.NotBakeComponentsInName, _config.EntityNameFormat), _config.UpdatePointWeight);
                }
                foreach (var kv in systems.NamedWorlds ()) {
                    if (Array.IndexOf (_config.DisableDebugWorlds, kv.Key) == -1) {
                        systems.AddSystem (new ProtoWorldDebugSystem (kv.Key, !_config.NotBakeComponentsInName, _config.EntityNameFormat), _config.UpdatePointWeight);
                    }
                }
            }
#endif
        }

        public IProtoAspect[] Aspects () => null;
        public Type[] Dependencies () => null;
    }

    public interface IProtoUnityGizmoSystem {
        void DrawGizmos ();
    }
}

#if ENABLE_IL2CPP
// Unity IL2CPP performance optimization attribute.
namespace Unity.IL2CPP.CompilerServices {
    enum Option {
        NullChecks = 1,
        ArrayBoundsChecks = 2
    }

    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; private set; }
        public object Value { get; private set; }

        public Il2CppSetOptionAttribute (Option option, object value) { Option = option; Value = value; }
    }
}
#endif
