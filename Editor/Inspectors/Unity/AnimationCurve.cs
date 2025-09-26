// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class AnimationCurveInspector : ProtoComponentInspector<AnimationCurve> {
        protected override bool OnRender (string label, ref AnimationCurve value) {
            var newValue = EditorGUILayout.CurveField (label, value);
            if (newValue.Equals (value)) { return false; }
            value = newValue;
            return true;
        }
    }
}
