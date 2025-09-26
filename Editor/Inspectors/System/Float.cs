// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class FloatInspector : ProtoComponentInspector<float> {
        protected override bool OnRender (string label, ref float value) {
            var newValue = EditorGUILayout.FloatField (label, value);
            if (System.Math.Abs (newValue - value) < float.Epsilon) { return false; }
            value = newValue;
            return true;
        }
    }
}
