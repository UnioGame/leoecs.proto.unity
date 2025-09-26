// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Leopotam.EcsProto.QoL;
using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.Unity {
    public interface IProtoUnityAuthoring {
        void Authoring (in ProtoPackedEntityWithWorld entity, GameObject go);
    }

    public sealed class ProtoUnityAuthoringAttribute : Attribute {
        public readonly string Name;

        public ProtoUnityAuthoringAttribute () : this (default) { }

        public ProtoUnityAuthoringAttribute (string name) {
            Name = name;
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    [DefaultExecutionOrder (10000)]
    [DisallowMultipleComponent]
    public class ProtoUnityAuthoring : MonoBehaviour {
        [SerializeField] string _worldName;
        [SerializeField] AuthoringType _authoringType = AuthoringType.OnStart;
        [SerializeField] DestroyType _destroyAfterAuthoring = DestroyType.GameObject;
        [SerializeReference, HideInInspector] public List<object> Components;

        ProtoPackedEntityWithWorld _packed;

        protected virtual void Start () {
            if (_authoringType == AuthoringType.OnStart) {
                ProcessAuthoring ();
            }
        }

        public virtual void ProcessAuthoring (bool callAfterAuthoring = true) {
#if UNITY_EDITOR
            if (Components == null || Components.Count == 0) { throw new Exception ($"[ProtoUnityAuthoring] Пустой список компонентов"); }
#endif
            _worldName = !string.IsNullOrEmpty (_worldName) ? _worldName : default;
            var world = ProtoUnityWorlds.Get (_worldName);
            var entityCreated = false;
            ProtoEntity entity = default;
            ProtoPackedEntityWithWorld packedEntity = default;
            var go = gameObject;
            foreach (var c in Components) {
#if UNITY_EDITOR
                if (c == null) { throw new Exception ($"[ProtoUnityAuthoring] Обнаружен сломанный компонент"); }
#endif
                var pool = world.Pool (c.GetType ());
                if (!entityCreated) {
                    pool.NewEntityRaw (out entity);
                    packedEntity = world.PackEntityWithWorld (entity);
                    entityCreated = true;
                } else {
                    pool.AddRaw (entity);
                }
                if (c is IProtoUnityAuthoring linkE) {
                    linkE.Authoring (packedEntity, go);
                }
                pool.SetRaw (entity, c);
            }

            _packed = packedEntity;

            if (callAfterAuthoring) {
                ProcessAfterAuthoring ();
            }
        }

        public virtual void ProcessAuthoringForEntity (ProtoWorld world, ProtoEntity entity, bool callAfterAuthoring = true) {
#if UNITY_EDITOR
            if (Components == null || Components.Count == 0) { throw new Exception ($"[ProtoUnityAuthoring] Пустой список компонентов"); }
#endif
            var packed = world.PackEntityWithWorld (entity);
            var go = gameObject;
            foreach (var c in Components) {
#if UNITY_EDITOR
                if (c == null) { throw new Exception ($"[ProtoUnityAuthoring] Обнаружен сломанный компонент"); }
#endif
                var pool = world.Pool (c.GetType ());
                pool.AddRaw (entity);
                if (c is IProtoUnityAuthoring linkE) {
                    linkE.Authoring (packed, go);
                }
                pool.SetRaw (entity, c);
            }

            _packed = world.PackEntityWithWorld (entity);

            if (callAfterAuthoring) {
                ProcessAfterAuthoring ();
            }
        }

        public virtual void ProcessAfterAuthoring () {
            switch (_destroyAfterAuthoring) {
                case DestroyType.Component:
                    Destroy (this);
                    return;
                case DestroyType.GameObject:
                    Destroy (gameObject);
                    return;
            }
        }

        public ProtoPackedEntityWithWorld Entity () => _packed;

        public enum AuthoringType {
            OnStart,
            Manual,
        }

        public enum DestroyType {
            None,
            Component,
            GameObject,
        }
    }
}
