// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class QuaternionInspector : ProtoComponentInspector<Quaternion> {
        protected override bool OnRender (string label, ref Quaternion value) {
            var eulerAngles = value.eulerAngles;
            var newValue = EditorGUILayout.Vector3Field (label, eulerAngles);
            if (newValue == eulerAngles) { return false; }
            value = Quaternion.Euler (newValue);
            return true;
        }
    }
}
