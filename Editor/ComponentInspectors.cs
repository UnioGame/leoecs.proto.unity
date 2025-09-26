// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Leopotam.EcsProto.QoL;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Leopotam.EcsProto.Unity.Editor {
    public struct EntityDebugInfo {
        public ProtoWorld World;
        public ProtoEntity Entity;
        public ProtoWorldDebugSystem System;
    }

    public static class ComponentInspectors {
        const int MaxFieldToStringLength = 128;

        static readonly Slice<object> _componentsCache = new ();
        static List<Type> _userComponentTypesCache = new ();
        static List<string> _userComponentNamesCache = new ();
        static readonly Type _uObjectType = typeof (Object);
        static readonly Type _flagsType = typeof (FlagsAttribute);
        static readonly Dictionary<Type, FieldInfo[]> _typesCache = new ();
        static readonly Dictionary<Type, IProtoComponentInspector> _userInspectors = new ();

        static ComponentInspectors () {
            var authAttrType = typeof (ProtoUnityAuthoringAttribute);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies ()) {
                foreach (var type in assembly.GetTypes ()) {
                    // сканируем инспекторы.
                    if (!type.IsInterface && !type.IsAbstract && typeof (IProtoComponentInspector).IsAssignableFrom (type)) {
                        var ins = (IProtoComponentInspector) Activator.CreateInstance (type);
                        var cType = ins.ItemType ();
                        if (!_userInspectors.TryGetValue (cType, out var prevIns) || ins.Priority () > prevIns.Priority ()) {
                            _userInspectors[cType] = ins;
                        }
                        continue;
                    }
                    // сканируем компоненты.
                    if (type.IsValueType && Attribute.IsDefined (type, authAttrType)) {
                        var name = ((ProtoUnityAuthoringAttribute) Attribute.GetCustomAttribute (type, authAttrType)).Name;
                        if (string.IsNullOrEmpty (name)) {
                            name = type.FullName.Replace ('.', '/').Replace ('+', '/');
                        }
                        var existIdx = _userComponentNamesCache.IndexOf (name);
                        if (existIdx != -1) {
                            var oldTypeName = DebugHelpers.CleanTypeName (_userComponentTypesCache[existIdx]);
                            var newTypeName = DebugHelpers.CleanTypeName (type);
                            Debug.LogError ($"[ProtoUnityAuthoring] авторинг с именем \"{name}\" уже зарегистрирован для типа \"{oldTypeName}\", авторинг под тем же именем для нового компонента \"{newTypeName}\" будет исключен из списка");
                            continue;
                        }
                        _userComponentTypesCache.Add (type);
                        _userComponentNamesCache.Add (name);
                    }
                }
            }
        }

        public static List<Type> UserComponentTypes () {
            return _userComponentTypesCache;
        }

        public static List<string> UserComponentNames () {
            return _userComponentNamesCache;
        }

        public static string RenderFilter (string filter) {
            GUILayout.BeginHorizontal ();
            EditorGUILayout.PrefixLabel ("Фильтр компонентов");
            filter = EditorGUILayout.TextField (filter).Trim ();
            GUI.enabled = filter.Length > 0;
            if (GUILayout.Button ("X", GUILayout.Width (EditorGUIUtility.singleLineHeight + 2))) {
                filter = "";
                GUI.FocusControl (null);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal ();
            return filter;
        }

        public static void RenderEntity (in EntityDebugInfo debugInfo, string filter = default) {
            debugInfo.World.EntityComponents (debugInfo.Entity, _componentsCache);
            RenderEntityComponents (_componentsCache, debugInfo, filter);
            _componentsCache.Clear ();
        }

        public static (bool changed, object newValue) RenderComponent (object component, in EntityDebugInfo debugInfo) {
            var type = component.GetType ();
            GUILayout.BeginVertical (GUI.skin.box, GUILayout.MaxWidth (float.MaxValue));
            var typeName = EditorExtensions.CleanTypeNameCached (type);
            var changed = false;
            object newValue = default;
            var (found, cChanged, cNewValue) = RenderCustom (typeName, type, component, debugInfo);
            if (found) {
                if (cChanged) {
                    changed = true;
                    newValue = cNewValue;
                }
            } else {
                EditorGUILayout.LabelField (typeName, EditorStyles.boldLabel);
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;
                foreach (var field in GetCachedType (type)) {
                    var (defChanged, defNewValue) = RenderField (component, field, debugInfo);
                    if (defChanged) {
                        changed = true;
                        field.SetValue (component, defNewValue);
                        newValue = component;
                    }
                }
                EditorGUI.indentLevel = indent;
            }
            GUILayout.EndVertical ();
            EditorGUILayout.Space ();
            return (changed, newValue);
        }

        static void RenderEntityComponents (Slice<object> components, in EntityDebugInfo debugInfo, string filter) {
            if (!string.IsNullOrEmpty (filter)) {
                filter = filter.ToLowerInvariant ();
            }
            for (int i = 0, iMax = components.Len (); i < iMax; i++) {
                var component = components.Get (i);
                var type = component.GetType ();
                var typeName = EditorExtensions.CleanTypeNameCached (type);
                if (!string.IsNullOrEmpty (filter) && EditorExtensions.CleanTypeNameCached (type, true).IndexOf (filter) == -1) {
                    continue;
                }
                GUILayout.BeginVertical (GUI.skin.box, GUILayout.MaxWidth (float.MaxValue));
                var pool = debugInfo.World.Pool (type);
                var (found, changed, newValue) = RenderCustom (typeName, type, component, debugInfo);
                if (found) {
                    if (changed) {
                        pool.SetRaw (debugInfo.Entity, newValue);
                    }
                } else {
                    EditorGUILayout.LabelField (typeName, EditorStyles.boldLabel);
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel++;
                    foreach (var field in GetCachedType (type)) {
                        var (defChanged, defNewValue) = RenderField (component, field, debugInfo);
                        if (defChanged) {
                            field.SetValue (component, defNewValue);
                            pool.SetRaw (debugInfo.Entity, component);
                        }
                    }
                    EditorGUI.indentLevel = indent;
                }
                GUILayout.EndVertical ();
                EditorGUILayout.Space ();
            }
        }

        static (bool changed, object newValue) RenderField (object component, FieldInfo field, in EntityDebugInfo debugInfo) {
            var fieldValue = field.GetValue (component);
            var fieldType = field.FieldType;
            var (found, changed, newValue) = RenderCustom (field.Name, fieldType, fieldValue, debugInfo);
            if (found) {
                return (changed, newValue);
            }
            if (fieldType == _uObjectType || fieldType.IsSubclassOf (_uObjectType)) {
                GUILayout.BeginHorizontal ();
                EditorGUILayout.PrefixLabel (field.Name);
                var newObjValue = EditorGUILayout.ObjectField ((Object) fieldValue, fieldType, true);
                GUILayout.EndHorizontal ();
                return (newObjValue != (Object) fieldValue, newObjValue);
            }
            if (fieldType.IsEnum) {
                var isFlags = Attribute.IsDefined (fieldType, _flagsType);
                var (eChanged, eNewValue) = RenderEnum (field.Name, fieldValue, isFlags);
                return (eChanged, eNewValue);
            }
            var strVal = fieldValue != null ? string.Format (CultureInfo.InvariantCulture, "{0}", fieldValue) : "null";
            if (strVal.Length > MaxFieldToStringLength) {
                strVal = strVal.Substring (0, MaxFieldToStringLength);
            }
            RenderSelectableText (field.Name, strVal);
            return (false, default);
        }

        public static void RenderSelectableText (string label, string text) {
            GUILayout.BeginHorizontal ();
            EditorGUILayout.PrefixLabel (label);
            EditorGUILayout.SelectableLabel (text, GUILayout.MaxHeight (EditorGUIUtility.singleLineHeight));
            GUILayout.EndHorizontal ();
        }

        static (bool found, bool changed, object newValue) RenderCustom (string label, Type type, object value, in EntityDebugInfo debugInfo) {
            if (_userInspectors.TryGetValue (type, out var inspector)) {
                var (changed, newValue) = inspector.Render (label, value, debugInfo);
                return (true, changed, newValue);
            }
            return (false, false, default);
        }

        static (bool changed, object newValue) RenderEnum (string label, object value, bool isFlags) {
            var enumValue = (Enum) value;
            Enum newValue;
            if (isFlags) {
                newValue = EditorGUILayout.EnumFlagsField (label, enumValue);
            } else {
                newValue = EditorGUILayout.EnumPopup (label, enumValue);
            }
            if (Equals (newValue, value)) {
                return (false, default);
            }
            return (true, newValue);
        }

        static FieldInfo[] GetCachedType (Type type) {
            if (!_typesCache.TryGetValue (type, out var fields)) {
                fields = type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _typesCache[type] = fields;
            }
            return fields;
        }
    }

    public interface IProtoComponentInspector {
        Type ItemType ();
        int Priority ();
        (bool changed, object newValue) Render (string label, object value, in EntityDebugInfo debugInfo);
    }

    public abstract class ProtoComponentInspector<T> : IProtoComponentInspector {
        public Type ItemType () => typeof (T);
        public virtual bool IsNullAllowed () => false;
        public virtual int Priority () => 0;

        public (bool changed, object newValue) Render (string label, object value, in EntityDebugInfo debugInfo) {
            if (value == null && !IsNullAllowed ()) {
                ComponentInspectors.RenderSelectableText (label, "null");
                return (false, default);
            }
            var typedValue = (T) value;
            if (OnRenderWithEntity (label, ref typedValue, debugInfo)) {
                return (true, typedValue);
            }
            return (false, default);
        }

        protected virtual bool OnRenderWithEntity (string label, ref T value, in EntityDebugInfo debugInfo) {
            return OnRender (label, ref value);
        }

        protected abstract bool OnRender (string label, ref T value);
    }
}
