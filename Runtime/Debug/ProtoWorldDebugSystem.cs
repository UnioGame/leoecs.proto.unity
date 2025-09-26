// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Leopotam.EcsProto.Unity {
    public sealed class ProtoEntityDebugView : MonoBehaviour {
        [NonSerialized]
        public ProtoWorld World;
        [NonSerialized]
        public ProtoEntity Entity;
        [NonSerialized]
        public ProtoWorldDebugSystem DebugSystem;
    }

    public sealed class ProtoWorldDebugSystem : IProtoInitSystem, IProtoRunSystem, IProtoDestroySystem, IProtoEventListener {
        readonly string _worldName;
        GameObject _rootGo;
        readonly Transform _entitiesRoot;
        readonly bool _bakeComponentsInName;
        readonly string _entityNameFormat;
        ProtoWorld _world;
        ProtoEntityDebugView[] _entities;
        Dictionary<int, ProtoEntity> _dirtyEntities;
        readonly Slice<Type> _typesCache;
        Dictionary<int, string> _intsCache;

        public ProtoWorldDebugSystem (string worldName = default, bool bakeComponentsInName = true, string entityNameFormat = "D6") {
            _bakeComponentsInName = bakeComponentsInName;
            _worldName = worldName;
            _entityNameFormat = entityNameFormat;
            _rootGo = new GameObject (_worldName != null ? $"[PROTO-WORLD {_worldName}]" : "[PROTO-WORLD]");
            Object.DontDestroyOnLoad (_rootGo);
            _rootGo.hideFlags = HideFlags.NotEditable;
            _entitiesRoot = new GameObject ("Entities").transform;
            _entitiesRoot.gameObject.hideFlags = HideFlags.NotEditable;
            _entitiesRoot.SetParent (_rootGo.transform, false);
            _typesCache = new Slice<Type> ();
        }

        public void Init (IProtoSystems systems) {
            _world = systems.World (_worldName);
            _entities = new ProtoEntityDebugView[_world.EntityGens ().Cap ()];
            _dirtyEntities = new (_entities.Length);
            _intsCache = new (_entities.Length);
            _world.AddEventListener (this);
            var gens = _world.EntityGens ();
            for (int i = 0, iMax = gens.Len (); i < iMax; i++) {
                if (gens.Get (i) > 0) {
                    OnEntityCreated ((ProtoEntity) i);
                }
            }
        }

        public void Run () {
            foreach (var pair in _dirtyEntities) {
                var entity = pair.Value;
                var entityName = Entity2Str (entity);
                if (_world.EntityGen (entity) > 0) {
                    GetComponentTypes (_world, entity, _typesCache);
                    for (int i = 0, iMax = _typesCache.Len (); i < iMax; i++) {
                        entityName = $"{entityName}:{EditorExtensions.CleanTypeNameCached (_typesCache.Get (i))}";
                    }
                }
                _entities[pair.Key].name = entityName;
            }
            _dirtyEntities.Clear ();
        }

        public void Destroy () {
            OnWorldDestroyed ();
        }

        public void OnEntityCreated (ProtoEntity entity) {
            var idx = entity.GetHashCode ();
            if (!_entities[idx]) {
                var go = new GameObject ();
                go.transform.SetParent (_entitiesRoot, false);
                var entityObserver = go.AddComponent<ProtoEntityDebugView> ();
                entityObserver.Entity = entity;
                entityObserver.World = _world;
                entityObserver.DebugSystem = this;
                _entities[idx] = entityObserver;
                if (_bakeComponentsInName) {
                    _dirtyEntities[idx] = entity;
                } else {
                    go.name = Entity2Str (entity);
                }
            }
            _entities[idx].gameObject.SetActive (true);
        }

        public void OnEntityDestroyed (ProtoEntity entity) {
            var idx = entity.GetHashCode ();
            if (_entities[idx]) {
                _entities[idx].gameObject.SetActive (false);
            }
        }

        public void OnEntityChanged (ProtoEntity entity, ushort poolId, bool added) {
            if (_bakeComponentsInName) {
                _dirtyEntities[entity.GetHashCode ()] = entity;
            }
        }

        public void OnWorldResized (int capacity) {
            Array.Resize (ref _entities, capacity);
        }

        public void OnWorldDestroyed () {
            if (Application.isPlaying) {
                Object.Destroy (_rootGo);
            } else {
                Object.DestroyImmediate (_rootGo);
            }
            _rootGo = null;
        }

        public ProtoEntityDebugView GetEntityView (ProtoEntity entity) {
            return _entities[entity.GetHashCode ()];
        }

        public GameObject GetGameObject () {
            return _rootGo;
        }

        static void GetComponentTypes (ProtoWorld world, ProtoEntity entity, Slice<Type> result) {
            result.Clear (false);
            if (world.EntityGen (entity) < 0) { return; }
            var pools = world.Pools ();
            var maskData = world.EntityMasks ().Data ();
            var maskLen = world.EntityMaskItemLen ();
            var maskOffset = world.EntityMaskOffset (entity);
            for (int i = 0, offset = 0; i < maskLen; i++, offset += 64, maskOffset++) {
                var v = maskData[maskOffset];
                for (var j = 0; v != 0 && j < 64; j++) {
                    if ((v & (1UL << j)) != 0) {
                        result.Add (pools.Get (offset + j).ItemType ());
                    }
                }
            }
        }

        string Entity2Str (ProtoEntity entity) {
            var id = entity.GetHashCode ();
            if (!_intsCache.TryGetValue (id, out var entityName)) {
                entityName = id.ToString (_entityNameFormat);
                _intsCache[id] = entityName;
            }
            return entityName;
        }
    }
}
#endif
