// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor.Inspectors {
    sealed class LayerMaskInspector : ProtoComponentInspector<LayerMask> {
        static string[] _layerNames;

        static string[] GetLayerNames () {
            if (_layerNames == null) {
                var size = 0;
                var names = new string[32];
                for (var i = 0; i < names.Length; i++) {
                    names[i] = LayerMask.LayerToName (i);
                    if (names[i].Length > 0) { size = i + 1; }
                }
                if (size != names.Length) { System.Array.Resize (ref names, size); }
                _layerNames = names;
            }
            return _layerNames;
        }

        protected override bool OnRender (string label, ref LayerMask value) {
            var newValue = EditorGUILayout.MaskField (label, value, GetLayerNames ());
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}
