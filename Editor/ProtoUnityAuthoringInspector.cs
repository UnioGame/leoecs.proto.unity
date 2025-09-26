// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Leopotam.EcsProto.QoL;
using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor {
    [CustomEditor (typeof (ProtoUnityAuthoring), true)]
    sealed class ProtoUnityAuthoringInspector : UnityEditor.Editor {
        ProtoUnityAuthoring _authoring;
        readonly Slice<object> _componentsCache = new ();
        static string _filter = "";

        void OnEnable () {
            _authoring = (ProtoUnityAuthoring) target;
        }

        void OnAddDropdown () {
            var menu = new GenericMenu ();
            var types = ComponentInspectors.UserComponentTypes ();
            var names = ComponentInspectors.UserComponentNames ();
            for (var i = 0; i < types.Count; i++) {
                menu.AddItem (
                    new GUIContent (names[i]),
                    false,
                    (objType) => {
                        _authoring.Components ??= new List<object> ();
                        var objRaw = Activator.CreateInstance ((Type) objType, true);
                        var newType = objRaw.GetType ();
                        foreach (var c in _authoring.Components) {
                            if (c.GetType () == newType) {
                                EditorUtility.DisplayDialog ("ProtoUnityLink", "Такой компонент уже существует", "Закрыть");
                                return;
                            }
                        }
                        _authoring.Components.Add (objRaw);
                        EditorUtility.SetDirty (_authoring);
                    },
                    types[i]);
            }
            menu.ShowAsContext ();
        }

        public override void OnInspectorGUI () {
            if (Application.isPlaying) {
                RenderInPlay ();
            } else {
                RenderInStop ();
            }
        }

        void RenderInPlay () {
            if (!_authoring.Entity ().TryUnpack (out var world, out var entity)) {
                EditorGUILayout.HelpBox ("Ошибка сущности", MessageType.Warning, true);
                return;
            }
            world.EntityComponents (entity, _componentsCache);

            _filter = ComponentInspectors.RenderFilter (_filter);
            EditorGUILayout.Separator ();

            var view = new EntityDebugInfo {
                World = world,
                Entity = entity,
                System = default,
            };
            ComponentInspectors.RenderEntity (view, _filter);
            EditorUtility.SetDirty (target);

            _componentsCache.Clear ();
        }

        void RenderInStop () {
            DrawDefaultInspector ();
            EditorGUILayout.Space ();
            if (_authoring.Components != null && _authoring.Components.Count > 0) {
                for (var i = 0; i < _authoring.Components.Count; i++) {
                    GUILayout.BeginHorizontal ();
                    if (GUILayout.Button ("X", GUILayout.ExpandWidth (false))) {
                        _authoring.Components.RemoveAt (i);
                        EditorUtility.SetDirty (_authoring);
                        continue;
                    }
                    var itemProp = _authoring.Components[i];
                    if (itemProp == null) {
                        EditorGUILayout.HelpBox ("Сломанный компонент", MessageType.Warning, true);
                    } else {
                        var (changed, newValue) = ComponentInspectors.RenderComponent (itemProp, new EntityDebugInfo ());
                        if (changed) {
                            _authoring.Components[i] = newValue;
                            EditorUtility.SetDirty (_authoring);
                        }
                    }
                    GUILayout.EndHorizontal ();
                    EditorGUILayout.Space ();
                }
            } else {
                EditorGUILayout.HelpBox ("Должен присутствовать хотя бы один компонент", MessageType.Warning, true);
            }
            EditorGUILayout.Space ();
            if (GUILayout.Button ("Добавить Proto-компонент...", GUILayout.MaxHeight (EditorGUIUtility.singleLineHeight * 1.5f))) {
                OnAddDropdown ();
            }
        }
    }
}
