// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class Vector4Inspector : ProtoComponentInspector<Vector4> {
        protected override bool OnRender (string label, ref Vector4 value) {
            var newValue = EditorGUILayout.Vector4Field (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
