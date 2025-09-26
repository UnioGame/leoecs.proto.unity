// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.Unity {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static class ProtoUnityLinks {
        static readonly Dictionary<string, LinkItem> _objects = new ();
        static readonly Slice<string> _clearKeysCache = new ();

        public static (GameObject, bool) Get (string name) {
            return _objects.TryGetValue (name, out var item) ? (item.Obj, true) : (default, false);
        }

        public static void Set (string name, GameObject obj, bool inScene = true) {
#if DEBUG
            if (_objects.ContainsKey (name)) { throw new Exception ($"[ProtoUnityLink] Объект с именем \"{name}\" уже существует"); }
            if (!obj) { throw new Exception ($"[ProtoUnityLink] Попытка зарегистрировать ошибочный объект с именем \"{name}\""); }
#endif
            _objects[name] = new LinkItem { Obj = obj, InScene = inScene };
        }

        public static void Del (string name) {
            _objects.Remove (name);
        }

        public static void Clear (bool onlyScene) {
            if (onlyScene) {
                foreach (var kv in _objects) {
                    if (kv.Value.InScene) {
                        _clearKeysCache.Add (kv.Key);
                    }
                }
                for (int i = 0, iMax = _clearKeysCache.Len (); i < iMax; i++) {
                    _objects.Remove (_clearKeysCache.Get (i));
                }
                _clearKeysCache.Clear (false);
            } else {
                _objects.Clear ();
            }
        }

        struct LinkItem {
            public GameObject Obj;
            public bool InScene;
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    [DefaultExecutionOrder (-10000)]
    public sealed class ProtoUnityLink : MonoBehaviour {
        [SerializeField] RegistrationType _registrationType = RegistrationType.OnAwake;
        [SerializeField] string _linkName;
        [SerializeField] bool _inScene = true;

        public string LinkName () {
            return _linkName;
        }

        public bool InScene () {
            return _inScene;
        }

        void Awake () {
            if (_registrationType == RegistrationType.OnAwake) {
                ProtoUnityLinks.Set (_linkName, gameObject, _inScene);
            }
        }

        void OnEnable () {
            if (_registrationType == RegistrationType.OnEnable) {
                ProtoUnityLinks.Set (_linkName, gameObject, _inScene);
            }
        }

        void OnDisable () {
            if (_registrationType == RegistrationType.OnEnable) {
                ProtoUnityLinks.Del (_linkName);
            }
        }

        void OnDestroy () {
            if (_registrationType == RegistrationType.OnAwake) {
                ProtoUnityLinks.Del (_linkName);
            }
        }

        enum RegistrationType {
            OnAwake,
            OnEnable
        }
    }

    public class DIUnityAttribute : Attribute {
        public readonly string LinkName;

        public DIUnityAttribute (string linkName) {
            LinkName = linkName;
        }
    }

    sealed class UnityLinkSystem : IProtoInitSystem {
        readonly Type _attrType = typeof (DIUnityAttribute);
        readonly Type _goType = typeof (GameObject);
        readonly Type _compType = typeof (Component);

        public void Init (IProtoSystems systems) {
            var allSystems = systems.Systems ();
            for (int i = 0, iMax = allSystems.Len (); i < iMax; i++) {
                InjectToSystem (allSystems.Get (i));
            }
        }

        void InjectToSystem (IProtoSystem system) {
            foreach (var fi in system.GetType ().GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (fi.IsStatic) { continue; }
                if (!Attribute.IsDefined (fi, _attrType)) { continue; }
                var linkName = ((DIUnityAttribute) Attribute.GetCustomAttribute (fi, _attrType)).LinkName;
#if DEBUG
                if (string.IsNullOrEmpty (linkName)) { throw new Exception ($"[DIUnity] Ошибка инъекции в поле \"{fi.Name}\" системы \"{system.GetType ()}\" - имя объекта не может быть пустым"); }
                if (!(fi.FieldType == _goType || _compType.IsAssignableFrom (fi.FieldType))) {
                    throw new Exception ($"[DIUnity] Ошибка инъекции в поле \"{fi.Name}\" системы \"{system.GetType ()}\" - поддерживаются только GameObject и Component");
                }
#endif
                var (go, ok) = ProtoUnityLinks.Get (linkName);
#if DEBUG
                if (!ok) { throw new Exception ($"[DIUnity] Ошибка инъекции в поле \"{fi.Name}\" системы \"{system.GetType ()}\" - объект с именем \"{linkName}\" не найден"); }
#endif
                // GameObject.
                if (fi.FieldType == _goType) {
                    fi.SetValue (system, go);
                    continue;
                }
                // Component.
                if (_compType.IsAssignableFrom (fi.FieldType)) {
#if DEBUG
                    if (!go.TryGetComponent (fi.FieldType, out _)) { throw new Exception ($"[DIUnity] Ошибка инъекции в поле \"{fi.Name}\" системы \"{system.GetType ()}\" - объект с именем \"{linkName}\" не содержит компонента с типом \"{fi.FieldType}\""); }
#endif
                    fi.SetValue (system, go.GetComponent (fi.FieldType));
                }
            }
        }
    }
}
