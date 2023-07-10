#if UNITY_EDITOR
using System.Reflection;
using SkiaSharp.Unity;
using UnityEditor;
using UnityEngine;
namespace SkiaSharp.UnityEditor {

  [CustomEditor(typeof(SkottiePlayer))]
  public class SlottiePlayerEditor : Editor
{
  private System.Reflection.MethodInfo UpdateAnimation;
  private System.Reflection.MethodInfo PlayAnimation;
  private System.Reflection.LocalVariableInfo xx;
  private static bool isEditorUpdateActive = false;
  private FieldInfo timerField;
  private FieldInfo playAniamtionField;
  private GUIStyle labelStyle;
  private void OnEnable() {
    UpdateAnimation = target.GetType().GetMethod("Start", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    PlayAnimation = target.GetType().GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    timerField = target.GetType().GetField("timer", BindingFlags.Instance | BindingFlags.NonPublic);
    playAniamtionField = target.GetType().GetField("playAniamtion", BindingFlags.Instance | BindingFlags.NonPublic);
    // Create a GUIStyle with desired color and size
    labelStyle = new GUIStyle(EditorStyles.label) {
      normal = {
        textColor = Color.red // Set the color
      },
      fontSize = 12 // Set the font size
    };
  }
  
  private void UpdateEditor() {
    SkottiePlayer myScript = (SkottiePlayer)target;
    if (!(bool)playAniamtionField.GetValue(myScript)) {
      isEditorUpdateActive = false;
      EditorApplication.update -= UpdateEditor;
      CallUpdateAnimation();
    }
    if (!isEditorUpdateActive)
      return;
    

    CallPlayAnimation();
    
  }

  public override void OnInspectorGUI() {

    base.OnInspectorGUI();
    if (EditorApplication.isPlaying) {
      isEditorUpdateActive = false;
      EditorApplication.update -= UpdateEditor;

      return;
    }

    SkottiePlayer myScript = (SkottiePlayer)target;

    
    GUILayout.Space(20);
    EditorGUILayout.LabelField("SkottiePlayer Editor",labelStyle);

    if (isEditorUpdateActive) {
      if (GUILayout.Button("Stop Animation!")) {
        isEditorUpdateActive = false;
        EditorApplication.update -= UpdateEditor;
        CallUpdateAnimation();
      }
      EditorGUILayout.LabelField($"current timer : {(double)timerField.GetValue(myScript)}");

    } else {
      CallUpdateAnimation();
      if (GUILayout.Button("Play Animation!")) {
        timerField.SetValue(myScript,0);
        playAniamtionField.SetValue(myScript,true);
        isEditorUpdateActive = true;
        EditorApplication.update += UpdateEditor;
      }
    }
  }

  private void CallUpdateAnimation() {
    // Get the target script
    SkottiePlayer myScript = (SkottiePlayer)target;
    if (target == null) {
      return;
    }

    // Use reflection to call the private method
    UpdateAnimation?.Invoke(myScript, null);
  }
  
  private void CallPlayAnimation() {
    // Get the target script
    SkottiePlayer myScript = (SkottiePlayer)target;

    // Use reflection to call the private method
    PlayAnimation?.Invoke(myScript, null);
  }
  
  private void OnDisable() {
    isEditorUpdateActive = false;
    EditorApplication.update -= UpdateEditor;
    CallUpdateAnimation();
  }
}
}
#endif