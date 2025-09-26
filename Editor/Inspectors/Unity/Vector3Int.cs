// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class Vector3IntInspector : ProtoComponentInspector<Vector3Int> {
        protected override bool OnRender (string label, ref Vector3Int value) {
            var newValue = EditorGUILayout.Vector3IntField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
