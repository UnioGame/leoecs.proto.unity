// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class Vector2IntInspector : ProtoComponentInspector<Vector2Int> {
        protected override bool OnRender (string label, ref Vector2Int value) {
            var newValue = EditorGUILayout.Vector2IntField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
