#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class PackagePathDebugger : EditorWindow {

    private string scriptName = "LinuxLoadResolver";
    private string resultMessage = "";

    [MenuItem("SKIA/Package Path Debugger")]
    public static void ShowWindow() {
        GetWindow<PackagePathDebugger>("Path Debugger");
    }

    private void OnGUI() {
        GUILayout.Label("Test Package Path Logic", EditorStyles.boldLabel);
        scriptName = EditorGUILayout.TextField("Script Name:", scriptName);

        if (GUILayout.Button("Find Package Path")) {
            resultMessage = TestGetPackagePath(scriptName);
        }

        GUILayout.Space(10);
        GUILayout.TextArea(resultMessage, GUILayout.Height(150));
    }

    private string TestGetPackagePath(string targetScriptName) {
        string[] guids = AssetDatabase.FindAssets($"{targetScriptName} t:Script");
        if (guids.Length == 0) return $"❌ Error: No script found named '{targetScriptName}'";

        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        
        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(assetPath);
        if (packageInfo != null) {
            return $"✅ Success (Installed Package)!\n\nPath: {packageInfo.resolvedPath}";
        }

        string absolutePath = Path.GetFullPath(assetPath);
        string currentDir = Path.GetDirectoryName(absolutePath);

        while (!string.IsNullOrEmpty(currentDir)) {
            if (File.Exists(Path.Combine(currentDir, "package.json"))) {
                 return $"✅ Success (Local Dev Mode)!\n\n" +
                        $"Script Location: {assetPath}\n" +
                        $"Package Root Found: {currentDir}\n" +
                        $"Contains package.json? Yes";
            }
            if (Directory.GetParent(currentDir) == null) break;
            currentDir = Directory.GetParent(currentDir).FullName;
        }

        return $"❌ Failed. Found script at '{assetPath}' but could not find a parent folder containing 'package.json'.";
    }
}
#endif