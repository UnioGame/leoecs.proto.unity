// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class IntInspector : ProtoComponentInspector<int> {
        protected override bool OnRender (string label, ref int value) {
            var newValue = EditorGUILayout.IntField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
