// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class Vector2Inspector : ProtoComponentInspector<Vector2> {
        protected override bool OnRender (string label, ref Vector2 value) {
            var newValue = EditorGUILayout.Vector2Field (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
