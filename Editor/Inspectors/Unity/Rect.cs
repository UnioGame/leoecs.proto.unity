// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class RectInspector : ProtoComponentInspector<Rect> {
        protected override bool OnRender (string label, ref Rect value) {
            var newValue = EditorGUILayout.RectField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
