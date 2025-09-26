// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using Leopotam.EcsProto.QoL;
using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class ProtoPackedEntity : ProtoComponentInspector<QoL.ProtoPackedEntity> {
        protected override bool OnRenderWithEntity (string label, ref QoL.ProtoPackedEntity value, in EntityDebugInfo debugInfo) {
            if (!Application.isPlaying || debugInfo.System == null) {
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.PrefixLabel (label);
                EditorGUILayout.SelectableLabel (debugInfo.System != null ? "<Нужен PlayMode>" : "<Не поддерживается>", GUILayout.MaxHeight (EditorGUIUtility.singleLineHeight));
                EditorGUILayout.EndHorizontal ();
                return false;
            }
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.PrefixLabel (label);
            if (value.TryUnpack (debugInfo.World, out var unpackedEntity)) {
                if (GUILayout.Button ("Показать сущность")) {
                    EditorGUIUtility.PingObject (debugInfo.System.GetEntityView (unpackedEntity));
                }
            } else {
                if (value == default) {
                    EditorGUILayout.SelectableLabel ("<Нет сущности>", GUILayout.MaxHeight (EditorGUIUtility.singleLineHeight));
                } else {
                    EditorGUILayout.SelectableLabel ("<Ошибка сущности>", GUILayout.MaxHeight (EditorGUIUtility.singleLineHeight));
                }
            }
            EditorGUILayout.EndHorizontal ();
            return false;
        }

        protected override bool OnRender (string label, ref QoL.ProtoPackedEntity value) => false;
    }
}
