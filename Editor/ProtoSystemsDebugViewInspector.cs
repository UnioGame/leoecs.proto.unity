// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor {
    [CustomEditor (typeof (ProtoSystemsDebugView))]
    sealed class ProtoSystemsDebugViewInspector : UnityEditor.Editor {
        static bool _initOpened;
        static bool _runOpened;
        static bool _destroyOpened;

        static string[] _labels = {
            "Init системы",
            "Run системы",
            "Destroy системы"
        };

        static Type[] _ifaces = {
            typeof(IProtoInitSystem),
            typeof(IProtoRunSystem),
            typeof(IProtoDestroySystem)
        };

        public override void OnInspectorGUI () {
            var view = (ProtoSystemsDebugView) target;
            var savedState = GUI.enabled;
            GUI.enabled = true;
            RenderCategory (0, view.Systems, ref _initOpened);
            RenderCategory (1, view.Systems, ref _runOpened);
            RenderCategory (2, view.Systems, ref _destroyOpened);
            GUI.enabled = savedState;
            EditorUtility.SetDirty (target);
        }

        void RenderCategory (int sysType, ProtoSystems systems, ref bool opened) {
            opened = EditorGUILayout.BeginFoldoutHeaderGroup (opened, _labels[sysType]);
            if (opened) {
                RenderLabeledList (sysType, systems);
            }
            EditorGUILayout.EndFoldoutHeaderGroup ();
            EditorGUILayout.Space ();
        }

        void RenderLabeledList (int sysType, IProtoNestedSystems systems) {
            EditorGUI.indentLevel++;
            var list = systems.Systems ();
            for (var i = 0; i < list.Len (); i++) {
                var item = list.Get (i);
                var itemType = item.GetType ();
                if (_ifaces[sysType].IsAssignableFrom (itemType)) {
                    var itemName = EditorExtensions.CleanTypeNameCached (item.GetType ());
                    EditorGUILayout.LabelField (itemName);
                }
                if (item is IProtoNestedSystems benchSystems) {
                    RenderLabeledList (sysType, benchSystems);
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
