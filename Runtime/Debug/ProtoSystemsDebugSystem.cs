// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Leopotam.EcsProto.Unity {
    public sealed class ProtoSystemsDebugView : MonoBehaviour {
        [NonSerialized] public ProtoSystems Systems;

        Slice<IProtoUnityGizmoSystem> _gizmoSystems;

        void OnDrawGizmos () {
            if (_gizmoSystems == null) {
                _gizmoSystems = new (64);
                CollectGizmoSystemList (Systems, _gizmoSystems);
            }
            for (var i = 0; i < _gizmoSystems.Len (); i++) {
                _gizmoSystems.Get (i).DrawGizmos ();
            }
        }

        static void CollectGizmoSystemList (IProtoNestedSystems systems, Slice<IProtoUnityGizmoSystem> gizmos) {
            var list = systems.Systems ();
            for (var i = 0; i < list.Len (); i++) {
                var item = list.Get (i);
                if (item is IProtoUnityGizmoSystem gizmoSystem) {
                    gizmos.Add (gizmoSystem);
                }
                if (item is IProtoNestedSystems nestedSystems) {
                    CollectGizmoSystemList (nestedSystems, gizmos);
                }
            }
        }
    }

    public sealed class ProtoSystemsDebugSystem : IProtoInitSystem, IProtoDestroySystem {
        readonly string _systemsName;
        GameObject _go;

        public ProtoSystemsDebugSystem (string systemsName = default) {
            _systemsName = systemsName;
        }

        public void Init (IProtoSystems systems) {
            _go = new GameObject (_systemsName != null ? $"[PROTO-SYSTEMS {_systemsName}]" : "[PROTO-SYSTEMS]");
            Object.DontDestroyOnLoad (_go);
            _go.hideFlags = HideFlags.NotEditable;
            var view = _go.AddComponent<ProtoSystemsDebugView> ();
            view.Systems = systems as ProtoSystems;
        }

        public void Destroy () {
            if (Application.isPlaying) {
                Object.Destroy (_go);
            } else {
                Object.DestroyImmediate (_go);
            }
            _go = null;
        }
    }
}
#endif
