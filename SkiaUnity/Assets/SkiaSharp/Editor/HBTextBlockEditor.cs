#if UNITY_EDITOR

using SkiaSharp.Unity.HB;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TextAlignment = Topten.RichTextKit.TextAlignment;


[CustomEditor(typeof(HB_TEXTBlock))]
public class HBTextBlockEditor : Editor {
  private GUIStyle selectedStyle;

  private SerializedProperty fontSizeProperty, fontColorProperty, fontProperty,
    italicProperty, boldProperty, haloColorProperty,shadowColorProperty, haloWidthProperty,
    shadowWidthProperty,shadowOffsetXProperty,shadowOffsetYProperty, letterSpacingProperty, autoFitVerticalProperty, renderLinksProperty,
    haloBlurProperty, backgroundColorProperty, underlineStyleProperty, lineHeightProperty,
    strikeThroughStyleProperty,textProperty, textAligmentProperty,colorTypeProperty, autoFitHorizontalProperty, maxWidthProperty, maxHeightProperty, gradiantColorsProperty
    ,gradiantPositionsProperty, enableGradiantProperty, gradiantAngleProperty, ellipsisProperty, maxLines, linkColorProperty;

  bool showHaloSettings = false;
  bool showMoreSettings = false;

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
    haloColorProperty = serializedObject.FindProperty("outlineColor");
    shadowColorProperty = serializedObject.FindProperty("shadowColor");
    haloWidthProperty = serializedObject.FindProperty("outlineWidth");
    shadowWidthProperty = serializedObject.FindProperty("shadowWidth");
    shadowOffsetXProperty = serializedObject.FindProperty("shadowOffsetX");
    shadowOffsetYProperty = serializedObject.FindProperty("shadowOffsetY");
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
  }
  public override void OnInspectorGUI(){
    HB_TEXTBlock script = (HB_TEXTBlock)target;
    GUIStyle largeLabelStyle = new GUIStyle(GUI.skin.label);
    largeLabelStyle.fontSize = 12; // Adjust the font size as needed
    largeLabelStyle.fontStyle = FontStyle.Bold;
    //script.text = EditorGUILayout.TextArea(script.text, GUILayout.Height(100));
    //textProperty.stringValue = script.text;
    EditorGUILayout.PropertyField(textProperty);
    EditorGUILayout.PropertyField(colorTypeProperty);

    // Display the fontSize field using the serialized property
    EditorGUILayout.PropertyField(fontProperty);
    EditorGUILayout.PropertyField(fontSizeProperty);
    EditorGUILayout.PropertyField(fontColorProperty);
    GUILayout.Label("Font Style:", largeLabelStyle);   
    EditorGUILayout.BeginHorizontal();
        
    Rect position = EditorGUILayout.GetControlRect();
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
    GUILayout.Label("Max Lines (0 for no limitation!):",largeLabelStyle);
    EditorGUILayout.PropertyField(maxLines);
    GUILayout.Label("Text Alignment:",largeLabelStyle);
    EditorGUILayout.PropertyField(textAligmentProperty);
    GUILayout.Label("Enable Ellipsis :",largeLabelStyle);
    EditorGUILayout.PropertyField(ellipsisProperty);

    EditorGUILayout.PropertyField(letterSpacingProperty);
    EditorGUILayout.PropertyField(autoFitVerticalProperty);
    if (script.AutoFitVertical) {
      GUILayout.Label("keep it -1 for no limitation!",largeLabelStyle);
      EditorGUILayout.PropertyField(maxHeightProperty);
    }
    EditorGUILayout.PropertyField(autoFitHorizontalProperty);
    if (script.AutoFitHorizontal) {
      EditorGUILayout.PropertyField(maxWidthProperty);
    }
    
    EditorGUILayout.PropertyField(enableGradiantProperty);
    if (script.IsGradiantEnabled) {
      EditorGUILayout.PropertyField(gradiantColorsProperty);
      EditorGUILayout.PropertyField(gradiantPositionsProperty);
      EditorGUILayout.PropertyField(gradiantAngleProperty);
    }
    
    EditorGUILayout.PropertyField(renderLinksProperty);

    if (script.RenderLinks) {
      EditorGUILayout.PropertyField(linkColorProperty);
    }
    
    
    showHaloSettings = EditorGUILayout.Foldout(showHaloSettings, "Halo Settings", CreateTitleStyle());

    if (showHaloSettings)
    {
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.PropertyField(haloWidthProperty);
      EditorGUILayout.PropertyField(haloColorProperty);
      EditorGUILayout.PropertyField(haloBlurProperty);
      EditorGUILayout.PropertyField(shadowWidthProperty);
      
      if(shadowWidthProperty.intValue > 0){
      EditorGUILayout.PropertyField(shadowOffsetXProperty);
      EditorGUILayout.PropertyField(shadowOffsetYProperty);
      EditorGUILayout.PropertyField(shadowColorProperty);
      }
      
      EditorGUILayout.EndVertical();
    }
    
    showMoreSettings = EditorGUILayout.Foldout(showMoreSettings, "More Settings", CreateTitleStyle());

    if (showMoreSettings)
    {
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.PropertyField(backgroundColorProperty);
      EditorGUILayout.PropertyField(underlineStyleProperty);
      EditorGUILayout.PropertyField(lineHeightProperty);
      EditorGUILayout.PropertyField(strikeThroughStyleProperty);
      EditorGUILayout.EndVertical();
    }

    serializedObject.ApplyModifiedProperties();
    if (!EditorApplication.isPlayingOrWillChangePlaymode) {
      script.ReUpdateEditMode();
    }
  }
  
  private GUIStyle CreateTitleStyle()
  {
    GUIStyle style = new GUIStyle(EditorStyles.foldout);
    style.fontSize = 12;
    style.fontStyle = FontStyle.Bold;

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
    Object[] objects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
    foreach (Object obj in objects) {
      GameObject gameObj = obj as GameObject;
      if (gameObj != null){
        HB_TEXTBlock myComponent = gameObj.GetComponent<HB_TEXTBlock>();
        if (myComponent != null) {
          myComponent.text = myComponent.text;
        }
      }
    }
  }

}
#endif
