// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class BoundsIntInspector : ProtoComponentInspector<BoundsInt> {
        protected override bool OnRender (string label, ref BoundsInt value) {
            var newValue = EditorGUILayout.BoundsIntField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
