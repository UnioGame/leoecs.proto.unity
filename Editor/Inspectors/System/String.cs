// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class StringInspector : ProtoComponentInspector<string> {
        public override bool IsNullAllowed () {
            return true;
        }

        protected override bool OnRender (string label, ref string value) {
            var newValue = EditorGUILayout.TextField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
