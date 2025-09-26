// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Leopotam.EcsProto.Unity.Editor {
    sealed class TemplateGenerator : ScriptableObject {
        const string Title = "LeoECS Proto";

        const string StartupTemplate = "Startup.cs.txt";
        const string StartupModulesTemplate = "StartupModules.cs.txt";
        const string ModuleTemplate = "Module.cs.txt";
        const string CsIconName = "cs Script Icon";
        const string FolderIconName = "Folder Icon";

        [MenuItem ("Assets/Create/LeoECS Proto/Точка старта", false, -201)]
        static void CreateStartupTpl () {
            var assetPath = GetAssetPath ();
            CreateAndRenameAsset ($"{assetPath}/Startup.cs", GetIcon (CsIconName), (name) => {
                CreateTemplateInternal (GetTemplateContent (StartupTemplate), name);
            });
        }

        [MenuItem ("Assets/Create/LeoECS Proto/Точка старта (модули)", false, -200)]
        static void CreateStartupModulesTpl () {
            var assetPath = GetAssetPath ();
            CreateAndRenameAsset ($"{assetPath}/Startup.cs", GetIcon (CsIconName), (name) => {
                CreateTemplateInternal (GetTemplateContent (StartupModulesTemplate), name);
            });
        }

        [MenuItem ("Assets/Create/LeoECS Proto/Модуль", false, -199)]
        static void CreateModuleTpl () {
            var assetPath = GetAssetPath ();
            CreateAndRenameAsset ($"{assetPath}/Module", GetIcon (FolderIconName), (name) => {
                CreateEmptyFolder ($"{name}");
                var repl = new Dictionary<string, string> {
                    { "#SCRIPTNAME#", SanitizeClassName (Path.GetFileNameWithoutExtension (name)) }
                };
                var tpl = GetTemplateContent (ModuleTemplate);
                if (CreateTemplateInternal (tpl, $"{name}/Module.cs", repl) == null) {
                    if (EditorUtility.DisplayDialog (Title, "Создать структуру папок?", "Да", "Нет")) {
                        CreateEmptyFolder ($"{name}/Components");
                        CreateEmptyFolder ($"{name}/Systems");
                        CreateEmptyFolder ($"{name}/Views");
                        CreateEmptyFolder ($"{name}/Services");
                    }
                }
                AssetDatabase.Refresh ();
            });
        }

        static void CreateEmptyFolder (string folderPath) {
            if (!Directory.Exists (folderPath)) {
                try {
                    Directory.CreateDirectory (folderPath);
                    File.Create ($"{folderPath}/.gitkeep");
                } catch (Exception ex) {
                    Debug.LogError (ex.Message);
                }
            }
        }

        public static string CreateTemplate (string proto, string fileName, Dictionary<string, string> replacements = default) {
            if (string.IsNullOrEmpty (fileName)) {
                return "Ошибочное имя";
            }
            var ns = EditorSettings.projectGenerationRootNamespace.Trim ();
            if (string.IsNullOrEmpty (ns)) { ns = "Client"; }
            replacements ??= new Dictionary<string, string> ();
            replacements.TryAdd ("#NS#", ns);
            replacements.TryAdd ("#SCRIPTNAME#", SanitizeClassName (Path.GetFileNameWithoutExtension (fileName)));
            foreach (var kv in replacements) {
                proto = proto.Replace (kv.Key, kv.Value);
            }
            try {
                File.WriteAllText (AssetDatabase.GenerateUniqueAssetPath (fileName), proto);
            } catch (Exception ex) {
                Debug.LogError (ex.Message);
                return "Ошибка создания файла";
            }
            AssetDatabase.Refresh ();
            return null;
        }

        static string SanitizeClassName (string className) {
            var sb = new StringBuilder ();
            var needUp = true;
            foreach (var c in className) {
                if (char.IsLetterOrDigit (c)) {
                    sb.Append (needUp ? char.ToUpperInvariant (c) : c);
                    needUp = false;
                } else {
                    needUp = true;
                }
            }
            return sb.ToString ();
        }

        static string CreateTemplateInternal (string proto, string fileName, Dictionary<string, string> replacements = default) {
            var res = CreateTemplate (proto, fileName, replacements);
            if (res != null) {
                EditorUtility.DisplayDialog (Title, res, "Закрыть");
            }
            return res;
        }

        static string GetTemplateContent (string proto) {
            var pathHelper = CreateInstance<TemplateGenerator> ();
            var path = Path.GetDirectoryName (AssetDatabase.GetAssetPath (MonoScript.FromScriptableObject (pathHelper)));
            DestroyImmediate (pathHelper);
            try {
                return File.ReadAllText (Path.Combine (path ?? "", proto));
            } catch {
                return null;
            }
        }

        static string GetAssetPath () {
            var path = AssetDatabase.GetAssetPath (Selection.activeObject);
            if (!string.IsNullOrEmpty (path) && AssetDatabase.Contains (Selection.activeObject)) {
                if (!AssetDatabase.IsValidFolder (path)) {
                    path = Path.GetDirectoryName (path);
                }
            } else {
                path = "Assets";
            }
            return path;
        }

        static Texture2D GetIcon (string iconName) {
            return EditorGUIUtility.IconContent (iconName).image as Texture2D;
        }

        static Texture2D GetFolderIcon () {
            return EditorGUIUtility.IconContent ("Folder Icon").image as Texture2D;
        }

        static void CreateAndRenameAsset (string fileName, Texture2D icon, Action<string> onSuccess) {
            var action = CreateInstance<CustomEndNameAction> ();
            action.Callback = onSuccess;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists (0, action, fileName, icon, null);
        }

        sealed class CustomEndNameAction : EndNameEditAction {
            [NonSerialized] public Action<string> Callback;

            public override void Action (int instanceId, string pathName, string resourceFile) {
                Callback?.Invoke (pathName);
            }
        }
    }
}
