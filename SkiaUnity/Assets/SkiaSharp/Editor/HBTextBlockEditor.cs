#if UNITY_EDITOR
using SkiaSharp.Unity.HB;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


[CustomEditor(typeof(HB_TEXTBlock))]
public class HBTextBlockEditor : Editor {
  private GUIStyle selectedStyle;
  private SerializedProperty fontSizeProperty, fontColorProperty, fontProperty,
    italicProperty, boldProperty, haloColorProperty, haloWidthProperty, 
    letterSpacingProperty, autoFitVerticalProperty, renderLinksProperty,
    haloBlurProperty, backgroundColorProperty, underlineStyleProperty, lineHeightProperty,
    strikeThroughStyleProperty,textProperty, textAligmentProperty,colorTypeProperty, autoFitHorizontalProperty, maxWidthProperty, maxHeightProperty, gradiantColorsProperty
    ,gradiantPositionsProperty, enableGradiantProperty, gradiantAngleProperty, ellipsisProperty, maxLines, linkColorProperty;
  
  private bool showAdvanceStyleSettings;
  private bool showExtraSettings;
  private bool showBasicFontSettings = true;
  private bool showAdvanceFontSettings;
  private string currentFontFace = null;
  
  private void OnEnable(){
    selectedStyle = new GUIStyle();
    selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/pre background@2x.png") as Texture2D;
    selectedStyle.normal.textColor = Color.white;
    selectedStyle.alignment = TextAnchor.MiddleCenter;
    selectedStyle.fontSize = 14;
    
    // Find the serialized property for fontSize
    fontSizeProperty = serializedObject.FindProperty("fontSize");
    fontColorProperty = serializedObject.FindProperty("fontColor");
    fontProperty = serializedObject.FindProperty("font");
    italicProperty = serializedObject.FindProperty("italic");
    boldProperty = serializedObject.FindProperty("bold");
    haloColorProperty = serializedObject.FindProperty("haloColor");
    haloWidthProperty = serializedObject.FindProperty("haloWidth");
    letterSpacingProperty = serializedObject.FindProperty("letterSpacing");
    autoFitVerticalProperty = serializedObject.FindProperty("autoFitVertical");
    autoFitHorizontalProperty = serializedObject.FindProperty("autoFitHorizontal");
    maxWidthProperty = serializedObject.FindProperty("maxWidth");
    maxHeightProperty = serializedObject.FindProperty("maxHeight");
    renderLinksProperty = serializedObject.FindProperty("renderLinks");
    haloBlurProperty =  serializedObject.FindProperty("haloBlur");
    backgroundColorProperty = serializedObject.FindProperty("backgroundColor");
    underlineStyleProperty = serializedObject.FindProperty("underlineStyle");
    lineHeightProperty = serializedObject.FindProperty("lineHeight");
    strikeThroughStyleProperty = serializedObject.FindProperty("strikeThroughStyle");
    textProperty = serializedObject.FindProperty("Text");
    textAligmentProperty = serializedObject.FindProperty("textAlignment");
    ellipsisProperty = serializedObject.FindProperty("enableEllipsis");
    colorTypeProperty = serializedObject.FindProperty("colorType");
    gradiantColorsProperty = serializedObject.FindProperty("gradiantColors");
    gradiantPositionsProperty = serializedObject.FindProperty("gradiantPositions");
    enableGradiantProperty = serializedObject.FindProperty("enableGradiant");
    gradiantAngleProperty = serializedObject.FindProperty("gradiantAngle");
    maxLines = serializedObject.FindProperty("maxLines");
    linkColorProperty = serializedObject.FindProperty("linkColor");
    if (fontProperty.propertyType == SerializedPropertyType.ObjectReference && fontProperty.objectReferenceValue != null) {
      currentFontFace = AssetDatabase.GetAssetPath(fontProperty.objectReferenceValue);
    }
  }
  public override void OnInspectorGUI() {
    var script = (HB_TEXTBlock)target;
    var largeLabelStyle = new GUIStyle(GUI.skin.label) {
      fontSize = 12, // Adjust the font size as needed
      fontStyle = FontStyle.Bold
    };

    EditorGUILayout.PropertyField(textProperty);
    showBasicFontSettings = EditorGUILayout.Foldout(showBasicFontSettings, "Font Settings", CreateTitleStyle());
    
    if (showBasicFontSettings) {
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.PropertyField(fontColorProperty);
      EditorGUILayout.PropertyField(fontProperty);
      EditorGUILayout.PropertyField(fontSizeProperty);
      EditorGUILayout.PropertyField(textAligmentProperty);
      
      GUILayout.Label("Font Style:", largeLabelStyle);   
      EditorGUILayout.BeginHorizontal();
      var position = EditorGUILayout.GetControlRect();
      EditorGUI.LabelField(position, "Italic");
      position.x += EditorGUIUtility.labelWidth;
      position.width = 100f;
      italicProperty.boolValue = EditorGUI.Toggle(position, italicProperty.boolValue);

      position = EditorGUILayout.GetControlRect();
      EditorGUI.LabelField(position, "Bold");
      position.x += EditorGUIUtility.labelWidth;
      position.width = 100f;
      boldProperty.boolValue = EditorGUI.Toggle(position, boldProperty.boolValue);
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.EndVertical();
    }
    
    showAdvanceStyleSettings = EditorGUILayout.Foldout(showAdvanceStyleSettings, "Advance Font Style", CreateTitleStyle());
    if (showAdvanceStyleSettings) {
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.PropertyField(enableGradiantProperty);
      if (script.IsGradiantEnabled) {
        EditorGUILayout.PropertyField(gradiantColorsProperty);
        EditorGUILayout.PropertyField(gradiantPositionsProperty);
        EditorGUILayout.PropertyField(gradiantAngleProperty);
      }
      EditorGUILayout.PropertyField(haloWidthProperty);
      EditorGUILayout.PropertyField(haloColorProperty);
      EditorGUILayout.PropertyField(haloBlurProperty);
      EditorGUILayout.EndVertical();
    }

    showExtraSettings = EditorGUILayout.Foldout(showExtraSettings, "Extra Settings", CreateTitleStyle());
    if (showExtraSettings) {
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.PropertyField(backgroundColorProperty);
      EditorGUILayout.PropertyField(underlineStyleProperty);
      EditorGUILayout.PropertyField(lineHeightProperty);
      EditorGUILayout.PropertyField(strikeThroughStyleProperty);
      EditorGUILayout.EndVertical();
    }

    showAdvanceFontSettings = EditorGUILayout.Foldout(showAdvanceFontSettings, "Advanced Font Settings", CreateTitleStyle());
    if (showAdvanceFontSettings) {
      EditorGUILayout.BeginVertical("box");
      GUILayout.Label("Max Lines (0 for no limitation!):", largeLabelStyle);
      EditorGUILayout.PropertyField(maxLines);
      EditorGUILayout.PropertyField(colorTypeProperty);
      GUILayout.Label("Enable Ellipsis :", largeLabelStyle);
      EditorGUILayout.PropertyField(ellipsisProperty);
      EditorGUILayout.PropertyField(letterSpacingProperty);
      EditorGUILayout.PropertyField(autoFitVerticalProperty);
      
      if (script.AutoFitVertical) {
        GUILayout.Label("keep it -1 for no limitation!", largeLabelStyle);
        EditorGUILayout.PropertyField(maxHeightProperty);
      }

      EditorGUILayout.PropertyField(autoFitHorizontalProperty);
      if (script.AutoFitHorizontal) {
        EditorGUILayout.PropertyField(maxWidthProperty);
      }
      
      EditorGUILayout.PropertyField(renderLinksProperty);
      if (script.RenderLinks) {
        EditorGUILayout.PropertyField(linkColorProperty);
      }
      EditorGUILayout.EndVertical();
    }

    serializedObject.ApplyModifiedProperties();
    if (fontProperty.propertyType == SerializedPropertyType.ObjectReference && fontProperty.objectReferenceValue != null) {
      var assetPath = AssetDatabase.GetAssetPath(fontProperty.objectReferenceValue);
      if (assetPath != currentFontFace) {
        currentFontFace = assetPath;
        script.RefreshFontFamily();
      }
    }
    
    if (!EditorApplication.isPlayingOrWillChangePlaymode) {
      script.ReUpdateEditMode();
    }
  }
  
  private GUIStyle CreateTitleStyle()
  {
    var style = new GUIStyle(EditorStyles.foldout) {
      fontSize = 12,
      fontStyle = FontStyle.Bold
    };
    return style;
  }
}


[InitializeOnLoad]
public class HBTextBlockOpenCallback {
  static HBTextBlockOpenCallback() {
    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    EditorSceneManager.sceneOpened += OnSceneOpened;

    if (!EditorApplication.isPlayingOrWillChangePlaymode) {
      RenderTextures();
    }
  }

  private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode) {
    RenderTextures();
  }

  private static void OnPlayModeStateChanged(PlayModeStateChange state) {
    if (state == PlayModeStateChange.EnteredEditMode) {
      RenderTextures();
    }
  }

  private static void RenderTextures() {
    var objects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
    foreach (var obj in objects) {
      var gameObj = obj as GameObject;
      if (gameObj == null) {continue;}
      var myComponent = gameObj.GetComponent<HB_TEXTBlock>();
      if (myComponent != null) {
        myComponent.text = myComponent.text;
      }
    }
  }

}
#endif
