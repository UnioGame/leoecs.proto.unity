// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;

namespace Leopotam.EcsProto.Unity.Editor {
    [CustomEditor (typeof (ProtoEntityDebugView))]
    sealed class ProtoEntityDebugViewInspector : UnityEditor.Editor {
        static string _filter = "";
        public override void OnInspectorGUI () {
            _filter = ComponentInspectors.RenderFilter (_filter);
            EditorGUILayout.Separator ();

            var observer = (ProtoEntityDebugView) target;
            var view = new EntityDebugInfo {
                World = observer.World,
                Entity = observer.Entity,
                System = observer.DebugSystem,
            };
            ComponentInspectors.RenderEntity (view, _filter);
            EditorUtility.SetDirty (target);
        }
    }
}
