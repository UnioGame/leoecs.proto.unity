// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class BoundsInspector : ProtoComponentInspector<Bounds> {
        protected override bool OnRender (string label, ref Bounds value) {
            var newValue = EditorGUILayout.BoundsField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
