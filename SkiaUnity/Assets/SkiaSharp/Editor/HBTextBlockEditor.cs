using SkiaSharp.Unity.HB;
using UnityEngine;
using UnityEditor;
using TextAlignment = Topten.RichTextKit.TextAlignment;


[CustomEditor(typeof(HB_TEXTBlock))]
public class HBTextBlockEditor : Editor {
  private Texture2D autoAlignmentIcon;
  private Texture2D leftAlignmentIcon;
  private Texture2D centerAlignmentIcon;
  private Texture2D rightAlignmentIcon;
  private GUIStyle selectedStyle;
  private TextAlignment selectedAlignment;
  private SerializedProperty fontSizeProperty, fontColorProperty, fontProperty,
    italicProperty, boldProperty, haloColorProperty, haloWidthProperty, 
    letterSpacingProperty, autoFitVerticalProperty, renderLinksProperty, 
    haloBlurProperty, backgroundColorProperty, underlineStyleProperty, lineHeightProperty,
    strikeThroughStyleProperty ;
  bool showHaloSettings = false;
  bool showMoreSettings = false;

  private void OnEnable(){
    autoAlignmentIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SkiaSharp/Editor/Icons/btn_AlignMidLine.psd"); // Replace with the actual path to your left alignment icon
    leftAlignmentIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SkiaSharp/Editor/Icons/btn_AlignLeft.psd"); // Replace with the actual path to your left alignment icon
    centerAlignmentIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SkiaSharp/Editor/Icons/btn_AlignCenter.psd"); // Replace with the actual path to your center alignment icon
    rightAlignmentIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SkiaSharp/Editor/Icons/btn_AlignRight.psd"); // Replace with the actual path to your right alignment icon
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
    renderLinksProperty = serializedObject.FindProperty("renderLinks");
    haloBlurProperty =  serializedObject.FindProperty("haloBlur");
    backgroundColorProperty = serializedObject.FindProperty("backgroundColor");
    underlineStyleProperty = serializedObject.FindProperty("underlineStyle");
    lineHeightProperty = serializedObject.FindProperty("lineHeight");
    strikeThroughStyleProperty = serializedObject.FindProperty("strikeThroughStyle");

    HB_TEXTBlock script = (HB_TEXTBlock)target;
    selectedAlignment = script.textAlignment;
  }
  public override void OnInspectorGUI(){
    HB_TEXTBlock script = (HB_TEXTBlock)target;
    GUIStyle largeLabelStyle = new GUIStyle(GUI.skin.label);
    largeLabelStyle.fontSize = 12; // Adjust the font size as needed
    largeLabelStyle.fontStyle = FontStyle.Bold;
    script.text = EditorGUILayout.TextArea(script.text, GUILayout.Height(100));

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

    GUILayout.Label("Text Alignment:",largeLabelStyle);
    script.textAlignment = (TextAlignment)EditorGUILayout.EnumPopup(script.textAlignment);
    EditorGUILayout.PropertyField(letterSpacingProperty);
    EditorGUILayout.PropertyField(autoFitVerticalProperty);
    EditorGUILayout.PropertyField(renderLinksProperty);

    
    showHaloSettings = EditorGUILayout.Foldout(showHaloSettings, "Halo Settings", CreateTitleStyle());

    if (showHaloSettings)
    {
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.PropertyField(haloWidthProperty);
      EditorGUILayout.PropertyField(haloColorProperty);
      EditorGUILayout.PropertyField(haloBlurProperty);
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
    
  }
  
  private GUIStyle CreateTitleStyle()
  {
    GUIStyle style = new GUIStyle(EditorStyles.foldout);
    style.fontSize = 12;
    style.fontStyle = FontStyle.Bold;

    return style;
  }
}