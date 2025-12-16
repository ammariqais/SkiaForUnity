#if UNITY_EDITOR_LINUX
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

public class LinuxLoadResolverWindow : EditorWindow {
  private static string libraryStatus = "Not Checked";

  [MenuItem("Jawaker/[HarfBuzz] Linux Load Resolver Status")]
  public static void ShowWindow() {
    GetWindow<LinuxLoadResolverWindow>("Linux Load Resolver Status");
  }

  private void OnGUI() {
    GUILayout.Label("HarfBuzz Library Load Status", EditorStyles.boldLabel);
    GUILayout.Label($"Status: {libraryStatus}", EditorStyles.wordWrappedLabel);

    if (GUILayout.Button("Reload Library")) {
      CheckLibraryStatus();
    }
  }

  [InitializeOnLoadMethod]
  private static void Initialize() {
    CheckLibraryStatus();
  }

  private static void CheckLibraryStatus() {
    IntPtr result = LoadHarfBuzzLibrary();
    libraryStatus = result == IntPtr.Zero ? "Failed" : "Success";
    Debug.Log($"Library Load Status: {libraryStatus} {result}");
  }

  private static IntPtr LoadHarfBuzzLibrary() {
    string packagePath = GetPackagePath("com.jawaker.skiaunity"); // Replace with your package name

    if (string.IsNullOrEmpty(packagePath)) {
      Debug.LogError("SkiaForUnity package not found in the Package Manager.");
      return IntPtr.Zero;
    }

    string libraryPath = Path.Combine(packagePath, "Library", "linux", "linux-x64", "native", "libHarfBuzzSharp.so");

    if (File.Exists(libraryPath)) {
      return HarfBuzzSharp.LibraryLoader.LoadLibrary(libraryPath);
    } else {
      Debug.LogError($"Library not found at path: {libraryPath}");
      return IntPtr.Zero;
    }
  }

  private static string GetPackagePath(string packageName) {
    string packagePath = null;

    var request = UnityEditor.PackageManager.Client.List(true);

    while (!request.IsCompleted) {
      System.Threading.Thread.Sleep(10);
    }

    if (request.Status == UnityEditor.PackageManager.StatusCode.Success) {
      foreach (var package in request.Result) {
        if (package.name == packageName) {
          packagePath = package.resolvedPath;
          break;
        }
      }
    } else if (request.Status >= UnityEditor.PackageManager.StatusCode.Failure) {
      Debug.LogError($"Failed to list packages: {request.Error.message}");
    }

    return packagePath;
  }
}
#endif