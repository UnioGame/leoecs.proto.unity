// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class DoubleInspector : ProtoComponentInspector<double> {
        protected override bool OnRender (string label, ref double value) {
            var newValue = EditorGUILayout.DoubleField (label, value);
            if (System.Math.Abs (newValue - value) < double.Epsilon) { return false; }
            value = newValue;
            return true;
        }
    }
}
