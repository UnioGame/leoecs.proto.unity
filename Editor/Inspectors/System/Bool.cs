// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class BoolInspector : ProtoComponentInspector<bool> {
        protected override bool OnRender (string label, ref bool value) {
            var newValue = EditorGUILayout.Toggle (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
