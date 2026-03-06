#if UNITY_EDITOR

using SkiaSharp.Unity.HB;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TextAlignment = Topten.RichTextKit.TextAlignment;

[CanEditMultipleObjects]
[CustomEditor(typeof(HB_TEXTBlock))]
public class HBTextBlockEditor : Editor {

  // --- GUIContent labels with tooltips ---
  static readonly GUIContent k_TextInputHeader = new GUIContent("<b>Text Input</b>");
  static readonly GUIContent k_MainSettingsHeader = new GUIContent("<b>Main Settings</b>");
  static readonly GUIContent k_EffectsHeader = new GUIContent("<b>Effects</b>");
  static readonly GUIContent k_ExtraSettingsHeader = new GUIContent("<b>Extra Settings</b>");

  static readonly GUIContent k_FontAssetLabel = new GUIContent("Font", "HBFontData asset for text rendering. Drag a .ttf/.otf file or HBFontData asset.");
  static readonly GUIContent k_FontSizeLabel = new GUIContent("Font Size", "The size of the text in points.");
  static readonly GUIContent k_FontColorLabel = new GUIContent("Vertex Color", "The base color of the text.");
  static readonly GUIContent k_FontStyleLabel = new GUIContent("Font Style", "Style modifiers: Bold, Italic, Underline, Strikethrough.");
  static readonly GUIContent k_BoldLabel = new GUIContent("B", "Bold");
  static readonly GUIContent k_ItalicLabel = new GUIContent("I", "Italic");
  static readonly GUIContent k_UnderlineLabel = new GUIContent("U", "Underline");
  static readonly GUIContent k_StrikethroughLabel = new GUIContent("S", "Strikethrough");
  static readonly GUIContent k_ColorTypeLabel = new GUIContent("Color Format", "RGB32 for full color, Alpha8 for single-channel text.");
  static readonly GUIContent k_AlignmentLabel = new GUIContent("Alignment", "Horizontal text alignment.");
  static readonly GUIContent k_VAlignmentLabel = new GUIContent("V. Alignment", "Vertical text alignment within the container.");
  static readonly GUIContent k_SpacingLabel = new GUIContent("Spacing", "Character and line spacing options.");

  // Horizontal alignment buttons: Auto=0, Left=1, Center=2, Right=3
  static readonly GUIContent k_AlignLeft = new GUIContent("L", "Left");
  static readonly GUIContent k_AlignCenter = new GUIContent("C", "Center");
  static readonly GUIContent k_AlignRight = new GUIContent("R", "Right");
  static readonly GUIContent k_AlignAuto = new GUIContent("A", "Auto");

  // Vertical alignment buttons: Top=0, Middle=1, Bottom=2
  static readonly GUIContent k_VAlignTop = new GUIContent("T", "Top");
  static readonly GUIContent k_VAlignMiddle = new GUIContent("M", "Middle");
  static readonly GUIContent k_VAlignBottom = new GUIContent("B", "Bottom");
  // Text direction buttons: LTR=0, RTL=1, Auto=2
  static readonly GUIContent k_TextDirectionLabel = new GUIContent("Direction", "Text reading direction.");
  static readonly GUIContent k_DirLTR = new GUIContent("LTR", "Left to Right");
  static readonly GUIContent k_DirRTL = new GUIContent("RTL", "Right to Left");
  static readonly GUIContent k_DirAuto = new GUIContent("Auto", "Automatic detection");

  static readonly GUIContent k_FontWeightLabel = new GUIContent("Font Weight", "Font weight from 100 (thin) to 900 (black). Bold overrides to 700.");
  static readonly GUIContent k_FontVariantLabel = new GUIContent("Variant", "Font variant: Normal, SuperScript, or SubScript.");
  static readonly GUIContent k_PaddingLabel = new GUIContent("Padding", "Extra padding inside the text area (Left, Top, Right, Bottom).");

  static readonly GUIContent k_RichTextLabel = new GUIContent("Rich Text", "Enable rich text tags: <b>, <i>, <u>, <s>, <sup>, <sub>, <size=N>, <color=#HEX>.");

  static readonly GUIContent k_TextInfoHeader = new GUIContent("<b>Text Info</b>");

  static readonly GUIContent k_LetterSpacingLabel = new GUIContent("Letter", "Additional spacing between characters.");
  static readonly GUIContent k_LineHeightLabel = new GUIContent("Line", "Multiplier for line distance. 1.0 is default.");

  static readonly GUIContent k_OutlineLabel = new GUIContent("Outline");
  static readonly GUIContent k_OutlineWidthLabel = new GUIContent("Width", "Outline thickness in pixels.");
  static readonly GUIContent k_OutlineColorLabel = new GUIContent("Color", "Gradient color of the outline.");
  static readonly GUIContent k_OutlineBlurLabel = new GUIContent("Blur", "Softness of the outline edge.");
  static readonly GUIContent k_InnerGlowLabel = new GUIContent("Inner Glow");
  static readonly GUIContent k_InnerGlowWidthLabel = new GUIContent("Width", "Inner glow spread. 0 to disable.");
  static readonly GUIContent k_InnerGlowColorLabel = new GUIContent("Color", "Color of the inner glow.");
  static readonly GUIContent k_DropShadowLabel = new GUIContent("Drop Shadow");
  static readonly GUIContent k_ShadowWidthLabel = new GUIContent("Width", "Shadow spread. 0 to disable.");
  static readonly GUIContent k_ShadowColorLabel = new GUIContent("Color", "Gradient color of the shadow.");
  static readonly GUIContent k_ShadowOffsetLabel = new GUIContent("Offset", "Shadow offset.");
  static readonly GUIContent k_GradientLabel = new GUIContent("Color Gradient", "Apply a gradient overlay to the text.");
  static readonly GUIContent k_GradientAngleLabel = new GUIContent("Angle", "Gradient angle in degrees.");
  static readonly GUIContent k_GradientAddLabel = new GUIContent("+", "Add gradient stop.");
  static readonly GUIContent k_GradientRemoveLabel = new GUIContent("-", "Remove this stop.");

  static readonly GUIContent k_OverflowLabel = new GUIContent("Overflow");
  static readonly GUIContent k_MaxLinesLabel = new GUIContent("Max Lines", "Maximum visible lines. 0 for unlimited.");
  static readonly GUIContent k_EllipsisLabel = new GUIContent("Ellipsis", "Append '...' when text is truncated.");
  static readonly GUIContent k_AutoSizeLabel = new GUIContent("Auto Size");
  static readonly GUIContent k_AutoFitVerticalLabel = new GUIContent("Height", "Automatically resize height to fit text.");
  static readonly GUIContent k_MaxHeightLabel = new GUIContent("Max", "Max height when auto-fitting. -1 for no limit.");
  static readonly GUIContent k_AutoFitHorizontalLabel = new GUIContent("Width", "Automatically resize width to fit text.");
  static readonly GUIContent k_MaxWidthLabel = new GUIContent("Max", "Max width when auto-fitting.");
  static readonly GUIContent k_AppearanceLabel = new GUIContent("Appearance");
  static readonly GUIContent k_BackgroundColorLabel = new GUIContent("Background", "Solid color drawn behind the text.");
  static readonly GUIContent k_RaycastTargetLabel = new GUIContent("Raycast Target", "Whether this text blocks raycasts for UI interaction.");
  static readonly GUIContent k_MaskableLabel = new GUIContent("Maskable", "Whether this graphic can be masked by a RectMask2D or Mask component.");
  static readonly GUIContent k_LinksLabel = new GUIContent("Links");
  static readonly GUIContent k_RenderLinksLabel = new GUIContent("Detect URLs", "Detect and render URL links in the text.");
  static readonly GUIContent k_LinkColorLabel = new GUIContent("Color", "Color for detected links.");

  static readonly GUIContent k_FallbackFontsLabel = new GUIContent("Fallback Fonts", "Additional HBFontData assets used when the primary font lacks glyphs.");
  static readonly GUIContent k_ParagraphSpacingLabel = new GUIContent("Para. Spacing", "Extra vertical space (in points) between paragraphs.");
  static readonly GUIContent k_EventsLabel = new GUIContent("Events");
  static readonly GUIContent k_OnTextChangedLabel = new GUIContent("On Text Changed", "Fired when the text content changes at runtime.");
  static readonly GUIContent k_OnLinkClickedLabel = new GUIContent("On Link Clicked", "Fired when a detected URL link is clicked. Passes the URL string.");

  static readonly string[] k_CollapseExpandHint = { "<i>(Click to collapse)</i>", "<i>(Click to expand)</i>" };

  // --- Cached GUIStyles ---
  private static GUIStyle s_SectionHeader;
  private static GUIStyle s_RightLabel;
  private static GUIStyle s_BoldFoldout;
  private static GUIStyle s_ToggleButtonLeft;
  private static GUIStyle s_ToggleButtonMid;
  private static GUIStyle s_ToggleButtonRight;
  private static bool s_StylesInitialized;

  // --- Static foldout state (persists across selection like TMP) ---
  private struct FoldoutState {
    public static bool effects = true;
    public static bool outline = true;
    public static bool innerGlow = true;
    public static bool dropShadow = true;
    public static bool extraSettings = false;
    public static bool overflow = true;
    public static bool autoSize = true;
    public static bool appearance = true;
    public static bool links = false;
    public static bool events = false;
  }

  // --- Serialized properties ---
  private SerializedProperty fontSizeProperty, fontColorProperty, fontProperty,
    italicProperty, boldProperty, haloColorProperty, shadowGradientColorProperty, haloWidthProperty,
    shadowWidthProperty, shadowOffsetXProperty, shadowOffsetYProperty, innerGlowColorProperty, innerGlowWidthProperty, letterSpacingProperty, autoFitVerticalProperty, renderLinksProperty,
    haloBlurProperty, backgroundColorProperty, underlineStyleProperty, lineHeightProperty,
    strikeThroughStyleProperty, textProperty, textAlignmentProperty, textVerticalAlignmentProperty, colorTypeProperty, autoFitHorizontalProperty, maxWidthProperty, maxHeightProperty, gradiantColorsProperty,
    gradiantPositionsProperty, enableGradiantProperty, gradiantAngleProperty, ellipsisProperty, maxLines, linkColorProperty,
    textDirectionProperty, fontWeightProperty, fontVariantProperty, paddingProperty, richTextProperty,
    fallbackFontsProperty, paragraphSpacingProperty, onTextChangedProperty, onLinkClickedProperty;

  private string currentFontFace = null;
  private bool _needsInitialRender = true;
  private float _lastTrackedWidth = -1;
  private float _lastTrackedHeight = -1;

  // ===================== Lifecycle =====================

  private void OnEnable() {
    _needsInitialRender = true;
    _lastTrackedWidth = -1;
    _lastTrackedHeight = -1;

    // Hide RawImage from inspector — HB_TEXTBlock manages it internally
    foreach (var t in targets) {
      var ri = ((Component)t).GetComponent<UnityEngine.UI.RawImage>();
      if (ri != null && (ri.hideFlags & HideFlags.HideInInspector) == 0) {
        ri.hideFlags |= HideFlags.HideInInspector;
      }
    }

    fontSizeProperty = serializedObject.FindProperty("fontSize");
    fontColorProperty = serializedObject.FindProperty("fontColor");
    fontProperty = serializedObject.FindProperty("font");
    italicProperty = serializedObject.FindProperty("italic");
    boldProperty = serializedObject.FindProperty("bold");
    haloColorProperty = serializedObject.FindProperty("outlineColor");
    shadowGradientColorProperty = serializedObject.FindProperty("shadowGradientColor");
    haloWidthProperty = serializedObject.FindProperty("outlineWidth");
    shadowWidthProperty = serializedObject.FindProperty("shadowWidth");
    shadowOffsetXProperty = serializedObject.FindProperty("shadowOffsetX");
    shadowOffsetYProperty = serializedObject.FindProperty("shadowOffsetY");
    innerGlowColorProperty = serializedObject.FindProperty("innerGlowColor");
    innerGlowWidthProperty = serializedObject.FindProperty("innerGlowWidth");
    letterSpacingProperty = serializedObject.FindProperty("letterSpacing");
    autoFitVerticalProperty = serializedObject.FindProperty("autoFitVertical");
    autoFitHorizontalProperty = serializedObject.FindProperty("autoFitHorizontal");
    maxWidthProperty = serializedObject.FindProperty("maxWidth");
    maxHeightProperty = serializedObject.FindProperty("maxHeight");
    renderLinksProperty = serializedObject.FindProperty("renderLinks");
    haloBlurProperty = serializedObject.FindProperty("outlineBlur");
    backgroundColorProperty = serializedObject.FindProperty("backgroundColor");
    underlineStyleProperty = serializedObject.FindProperty("underlineStyle");
    lineHeightProperty = serializedObject.FindProperty("lineHeight");
    strikeThroughStyleProperty = serializedObject.FindProperty("strikeThroughStyle");
    textProperty = serializedObject.FindProperty("Text");
    textAlignmentProperty = serializedObject.FindProperty("textAlignment");
    textVerticalAlignmentProperty = serializedObject.FindProperty("verticalAlignment");
    ellipsisProperty = serializedObject.FindProperty("enableEllipsis");
    colorTypeProperty = serializedObject.FindProperty("colorType");
    gradiantColorsProperty = serializedObject.FindProperty("gradiantColors");
    gradiantPositionsProperty = serializedObject.FindProperty("gradiantPositions");
    enableGradiantProperty = serializedObject.FindProperty("enableGradiant");
    gradiantAngleProperty = serializedObject.FindProperty("gradiantAngle");
    maxLines = serializedObject.FindProperty("maxLines");
    linkColorProperty = serializedObject.FindProperty("linkColor");
    textDirectionProperty = serializedObject.FindProperty("textDirection");
    fontWeightProperty = serializedObject.FindProperty("fontWeight");
    fontVariantProperty = serializedObject.FindProperty("fontVariant");
    paddingProperty = serializedObject.FindProperty("padding");
    richTextProperty = serializedObject.FindProperty("richText");
    fallbackFontsProperty = serializedObject.FindProperty("fallbackFonts");
    paragraphSpacingProperty = serializedObject.FindProperty("paragraphSpacing");
    onTextChangedProperty = serializedObject.FindProperty("onTextChanged");
    onLinkClickedProperty = serializedObject.FindProperty("onLinkClicked");

    if (fontProperty.propertyType == SerializedPropertyType.ObjectReference && fontProperty.objectReferenceValue != null) {
      currentFontFace = AssetDatabase.GetAssetPath(fontProperty.objectReferenceValue);
    }
  }

  // ===================== Main Inspector =====================

  public override void OnInspectorGUI() {
    InitStyles();
    serializedObject.Update();

    DrawTextInput();
    DrawMainSettings();
    DrawEffects();
    DrawExtraSettings();
    DrawTextInfo();

    ApplyAndRender();
  }

  // ===================== Sections =====================

  private void DrawTextInput() {
    DrawSectionHeader(k_TextInputHeader);
    EditorGUILayout.PropertyField(textProperty, GUIContent.none);
    // Rich text toggle + character count on one row
    string txt = textProperty.stringValue ?? "";
    Rect row = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
    Rect countRect = new Rect(row.xMax - 100f, row.y, 100f, row.height);
    GUI.Label(countRect, "Chars: " + txt.Length, s_RightLabel);
    Rect toggleRect = new Rect(row.x, row.y, row.width - 100f, row.height);
    float savedLabelWidth = EditorGUIUtility.labelWidth;
    EditorGUIUtility.labelWidth = 62f;
    EditorGUI.PropertyField(toggleRect, richTextProperty, k_RichTextLabel);
    EditorGUIUtility.labelWidth = savedLabelWidth;
  }

  private void DrawMainSettings() {
    DrawSectionHeader(k_MainSettingsHeader);

    DrawFontField();
    EditorGUILayout.PropertyField(fallbackFontsProperty, k_FallbackFontsLabel);
    EditorGUILayout.PropertyField(fontSizeProperty, k_FontSizeLabel);
    DrawFontStyleToggles();
    DrawFontWeightSlider();
    EditorGUILayout.PropertyField(fontVariantProperty, k_FontVariantLabel);
    EditorGUILayout.PropertyField(fontColorProperty, k_FontColorLabel);
    EditorGUILayout.PropertyField(colorTypeProperty, k_ColorTypeLabel);
    DrawHorizontalAlignmentButtons();
    DrawVerticalAlignmentButtons();
    DrawTextDirectionButtons();
    DrawSpacingRow();
    EditorGUILayout.PropertyField(paragraphSpacingProperty, k_ParagraphSpacingLabel);
    EditorGUILayout.PropertyField(paddingProperty, k_PaddingLabel);
    EditorGUILayout.Space();
  }

  private void DrawEffects() {
    FoldoutState.effects = DrawCollapsibleSectionHeader(k_EffectsHeader, FoldoutState.effects);
    if (!FoldoutState.effects) return;

    EditorGUI.indentLevel++;

    // Outline
    FoldoutState.outline = EditorGUILayout.Foldout(FoldoutState.outline, k_OutlineLabel, true, s_BoldFoldout);
    if (FoldoutState.outline) {
      EditorGUI.indentLevel++;
      EditorGUILayout.PropertyField(haloWidthProperty, k_OutlineWidthLabel);
      if (haloWidthProperty.intValue > 0) {
        EditorGUILayout.PropertyField(haloColorProperty, k_OutlineColorLabel);
        EditorGUILayout.PropertyField(haloBlurProperty, k_OutlineBlurLabel);
      }
      EditorGUI.indentLevel--;
    }

    // Inner Glow
    FoldoutState.innerGlow = EditorGUILayout.Foldout(FoldoutState.innerGlow, k_InnerGlowLabel, true, s_BoldFoldout);
    if (FoldoutState.innerGlow) {
      EditorGUI.indentLevel++;
      EditorGUILayout.PropertyField(innerGlowWidthProperty, k_InnerGlowWidthLabel);
      if (innerGlowWidthProperty.intValue > 0) {
        EditorGUILayout.PropertyField(innerGlowColorProperty, k_InnerGlowColorLabel);
      }
      EditorGUI.indentLevel--;
    }

    // Drop Shadow
    FoldoutState.dropShadow = EditorGUILayout.Foldout(FoldoutState.dropShadow, k_DropShadowLabel, true, s_BoldFoldout);
    if (FoldoutState.dropShadow) {
      EditorGUI.indentLevel++;
      EditorGUILayout.PropertyField(shadowWidthProperty, k_ShadowWidthLabel);
      if (shadowWidthProperty.intValue > 0) {
        EditorGUILayout.PropertyField(shadowGradientColorProperty, k_ShadowColorLabel);
        DrawShadowOffsetRow();
      }
      EditorGUI.indentLevel--;
    }

    // Gradient
    EditorGUILayout.PropertyField(enableGradiantProperty, k_GradientLabel);
    if (enableGradiantProperty.boolValue) {
      EditorGUI.indentLevel++;
      EditorGUILayout.PropertyField(gradiantAngleProperty, k_GradientAngleLabel);
      DrawGradientStops();
      EditorGUI.indentLevel--;
    }

    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
  }

  private void DrawExtraSettings() {
    FoldoutState.extraSettings = DrawCollapsibleSectionHeader(k_ExtraSettingsHeader, FoldoutState.extraSettings);
    if (!FoldoutState.extraSettings) return;

    EditorGUI.indentLevel++;

    // --- Overflow ---
    FoldoutState.overflow = EditorGUILayout.Foldout(FoldoutState.overflow, k_OverflowLabel, true, s_BoldFoldout);
    if (FoldoutState.overflow) {
      EditorGUI.indentLevel++;
      DrawOverflowRow();
      EditorGUI.indentLevel--;
    }

    // --- Auto Size ---
    FoldoutState.autoSize = EditorGUILayout.Foldout(FoldoutState.autoSize, k_AutoSizeLabel, true, s_BoldFoldout);
    if (FoldoutState.autoSize) {
      EditorGUI.indentLevel++;
      DrawAutoSizeRow(k_AutoFitVerticalLabel, autoFitVerticalProperty, maxHeightProperty, k_MaxHeightLabel);
      DrawAutoSizeRow(k_AutoFitHorizontalLabel, autoFitHorizontalProperty, maxWidthProperty, k_MaxWidthLabel);
      EditorGUI.indentLevel--;
    }

    // --- Appearance ---
    FoldoutState.appearance = EditorGUILayout.Foldout(FoldoutState.appearance, k_AppearanceLabel, true, s_BoldFoldout);
    if (FoldoutState.appearance) {
      EditorGUI.indentLevel++;
      EditorGUILayout.PropertyField(backgroundColorProperty, k_BackgroundColorLabel);
      DrawRawImageProperties();
      EditorGUI.indentLevel--;
    }

    // --- Links ---
    FoldoutState.links = EditorGUILayout.Foldout(FoldoutState.links, k_LinksLabel, true, s_BoldFoldout);
    if (FoldoutState.links) {
      EditorGUI.indentLevel++;
      EditorGUILayout.PropertyField(renderLinksProperty, k_RenderLinksLabel);
      if (renderLinksProperty.boolValue) {
        EditorGUILayout.PropertyField(linkColorProperty, k_LinkColorLabel);
      }
      EditorGUI.indentLevel--;
    }

    // --- Events ---
    FoldoutState.events = EditorGUILayout.Foldout(FoldoutState.events, k_EventsLabel, true, s_BoldFoldout);
    if (FoldoutState.events) {
      EditorGUI.indentLevel++;
      EditorGUILayout.PropertyField(onTextChangedProperty, k_OnTextChangedLabel);
      EditorGUILayout.PropertyField(onLinkClickedProperty, k_OnLinkClickedLabel);
      EditorGUI.indentLevel--;
    }

    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
  }

  // Max Lines + Ellipsis on one row
  private void DrawOverflowRow() {
    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
    float savedLabelWidth = EditorGUIUtility.labelWidth;
    int oldIndent = EditorGUI.indentLevel;

    float indentOffset = oldIndent * 15f;
    rect.x += indentOffset;
    rect.width -= indentOffset;
    EditorGUI.indentLevel = 0;

    float gap = 6f;
    float ellipsisWidth = 70f;
    float maxLinesWidth = rect.width - ellipsisWidth - gap;

    // Max Lines
    Rect mlRect = new Rect(rect.x, rect.y, maxLinesWidth, rect.height);
    EditorGUIUtility.labelWidth = 65f;
    EditorGUI.PropertyField(mlRect, maxLines, k_MaxLinesLabel);

    // Ellipsis toggle
    Rect elRect = new Rect(rect.x + maxLinesWidth + gap, rect.y, ellipsisWidth, rect.height);
    EditorGUIUtility.labelWidth = 48f;
    EditorGUI.PropertyField(elRect, ellipsisProperty, k_EllipsisLabel);

    EditorGUIUtility.labelWidth = savedLabelWidth;
    EditorGUI.indentLevel = oldIndent;
  }

  // Auto fit toggle + max constraint on one row: [Toggle Label] [checkbox] [Max] [value]
  private void DrawAutoSizeRow(GUIContent label, SerializedProperty toggleProp, SerializedProperty maxProp, GUIContent maxLabel) {
    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
    float savedLabelWidth = EditorGUIUtility.labelWidth;
    int oldIndent = EditorGUI.indentLevel;

    float indentOffset = oldIndent * 15f;
    rect.x += indentOffset;
    rect.width -= indentOffset;
    EditorGUI.indentLevel = 0;

    float gap = 6f;

    if (toggleProp.boolValue) {
      // Split: toggle takes half, max value takes other half
      float halfWidth = (rect.width - gap) / 2f;

      Rect toggleRect = new Rect(rect.x, rect.y, halfWidth, rect.height);
      EditorGUIUtility.labelWidth = 45f;
      EditorGUI.PropertyField(toggleRect, toggleProp, label);

      Rect maxRect = new Rect(rect.x + halfWidth + gap, rect.y, halfWidth, rect.height);
      EditorGUIUtility.labelWidth = 30f;
      EditorGUI.PropertyField(maxRect, maxProp, maxLabel);
    } else {
      EditorGUIUtility.labelWidth = 45f;
      EditorGUI.PropertyField(rect, toggleProp, label);
    }

    EditorGUIUtility.labelWidth = savedLabelWidth;
    EditorGUI.indentLevel = oldIndent;
  }

  // ===================== Custom Draws =====================

  private void DrawFontStyleToggles() {
    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 2f);
    EditorGUI.PrefixLabel(rect, k_FontStyleLabel);

    rect.x += EditorGUIUtility.labelWidth;
    rect.width -= EditorGUIUtility.labelWidth;
    float btnWidth = Mathf.Max(25f, rect.width / 4f);
    rect.width = btnWidth;

    // Bold
    EditorGUI.BeginChangeCheck();
    bool newBold = GUI.Toggle(rect, boldProperty.boolValue, k_BoldLabel, s_ToggleButtonLeft);
    if (EditorGUI.EndChangeCheck()) boldProperty.boolValue = newBold;
    rect.x += btnWidth;

    // Italic
    EditorGUI.BeginChangeCheck();
    bool newItalic = GUI.Toggle(rect, italicProperty.boolValue, k_ItalicLabel, s_ToggleButtonMid);
    if (EditorGUI.EndChangeCheck()) italicProperty.boolValue = newItalic;
    rect.x += btnWidth;

    // Underline: None(0) <-> Solid(2)
    EditorGUI.BeginChangeCheck();
    bool hasUnderline = underlineStyleProperty.enumValueIndex != 0;
    bool newUnderline = GUI.Toggle(rect, hasUnderline, k_UnderlineLabel, s_ToggleButtonMid);
    if (EditorGUI.EndChangeCheck()) {
      underlineStyleProperty.enumValueIndex = newUnderline ? 2 : 0;
    }
    rect.x += btnWidth;

    // Strikethrough: None(0) <-> Solid(1)
    EditorGUI.BeginChangeCheck();
    bool hasStrike = strikeThroughStyleProperty.enumValueIndex != 0;
    bool newStrike = GUI.Toggle(rect, hasStrike, k_StrikethroughLabel, s_ToggleButtonRight);
    if (EditorGUI.EndChangeCheck()) {
      strikeThroughStyleProperty.enumValueIndex = newStrike ? 1 : 0;
    }
  }

  private void DrawSpacingRow() {
    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
    EditorGUI.PrefixLabel(rect, k_SpacingLabel);

    int oldIndent = EditorGUI.indentLevel;
    EditorGUI.indentLevel = 0;
    float savedLabelWidth = EditorGUIUtility.labelWidth;

    rect.x += savedLabelWidth;
    rect.width = (rect.width - savedLabelWidth - 3f) / 2f;
    EditorGUIUtility.labelWidth = Mathf.Min(rect.width * 0.55f, 50f);

    EditorGUI.PropertyField(rect, letterSpacingProperty, k_LetterSpacingLabel);
    rect.x += rect.width + 3f;
    EditorGUI.PropertyField(rect, lineHeightProperty, k_LineHeightLabel);

    EditorGUIUtility.labelWidth = savedLabelWidth;
    EditorGUI.indentLevel = oldIndent;
  }

  private void DrawTextDirectionButtons() {
    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 2f);
    EditorGUI.PrefixLabel(rect, k_TextDirectionLabel);

    rect.x += EditorGUIUtility.labelWidth;
    rect.width -= EditorGUIUtility.labelWidth;
    float btnWidth = Mathf.Max(25f, rect.width / 3f);
    rect.width = btnWidth;

    // LTR=0, RTL=1, Auto=2
    int current = textDirectionProperty.enumValueIndex;

    EditorGUI.BeginChangeCheck();
    if (GUI.Toggle(rect, current == 0, k_DirLTR, s_ToggleButtonLeft) && current != 0) current = 0;
    rect.x += btnWidth;
    if (GUI.Toggle(rect, current == 1, k_DirRTL, s_ToggleButtonMid) && current != 1) current = 1;
    rect.x += btnWidth;
    if (GUI.Toggle(rect, current == 2, k_DirAuto, s_ToggleButtonRight) && current != 2) current = 2;
    if (EditorGUI.EndChangeCheck()) {
      textDirectionProperty.enumValueIndex = current;
    }
  }

  private void DrawFontWeightSlider() {
    // If bold is on, show disabled slider at 700
    bool isBold = boldProperty.boolValue;
    EditorGUI.BeginDisabledGroup(isBold);
    if (isBold) {
      Rect rect = EditorGUILayout.GetControlRect();
      EditorGUI.IntSlider(rect, k_FontWeightLabel, 700, 100, 900);
    } else {
      EditorGUILayout.IntSlider(fontWeightProperty, 100, 900, k_FontWeightLabel);
    }
    EditorGUI.EndDisabledGroup();
  }

  private void DrawTextInfo() {
    DrawSectionHeader(k_TextInfoHeader);
    var hb = (HB_TEXTBlock)target;
    var info = hb.Info;
    if (info == null) return;

    EditorGUI.BeginDisabledGroup(true);
    string txt = textProperty.stringValue ?? "";
    int wordCount = string.IsNullOrWhiteSpace(txt) ? 0 : txt.Split(new[] { ' ', '\n', '\r', '\t' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
    int lineCount = info.Lines?.Count ?? 0;

    Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
    float thirdWidth = rect.width / 3f;

    GUI.Label(new Rect(rect.x, rect.y, thirdWidth, rect.height), "Words: " + wordCount, EditorStyles.miniLabel);
    GUI.Label(new Rect(rect.x + thirdWidth, rect.y, thirdWidth, rect.height), "Lines: " + lineCount, EditorStyles.miniLabel);
    GUI.Label(new Rect(rect.x + thirdWidth * 2f, rect.y, thirdWidth, rect.height), "Chars: " + txt.Length, EditorStyles.miniLabel);

    rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
    float halfWidth = rect.width / 2f;
    GUI.Label(new Rect(rect.x, rect.y, halfWidth, rect.height),
      $"Size: {info.MeasuredWidth:F1} x {info.MeasuredHeight:F1}", EditorStyles.miniLabel);
    var rt = hb.GetComponent<RectTransform>();
    if (rt != null) {
      GUI.Label(new Rect(rect.x + halfWidth, rect.y, halfWidth, rect.height),
        $"Rect: {rt.rect.width:F1} x {rt.rect.height:F1}", EditorStyles.miniLabel);
    }
    EditorGUI.EndDisabledGroup();
  }

  private void DrawShadowOffsetRow() {
    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
    EditorGUI.PrefixLabel(rect, k_ShadowOffsetLabel);

    int oldIndent = EditorGUI.indentLevel;
    EditorGUI.indentLevel = 0;
    float savedLabelWidth = EditorGUIUtility.labelWidth;

    rect.x += savedLabelWidth;
    rect.width = (rect.width - savedLabelWidth - 3f) / 2f;
    EditorGUIUtility.labelWidth = 14f;

    EditorGUI.PropertyField(rect, shadowOffsetXProperty, new GUIContent("X"));
    rect.x += rect.width + 3f;
    EditorGUI.PropertyField(rect, shadowOffsetYProperty, new GUIContent("Y"));

    EditorGUIUtility.labelWidth = savedLabelWidth;
    EditorGUI.indentLevel = oldIndent;
  }

  private void DrawHorizontalAlignmentButtons() {
    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 2f);
    EditorGUI.PrefixLabel(rect, k_AlignmentLabel);

    rect.x += EditorGUIUtility.labelWidth;
    rect.width -= EditorGUIUtility.labelWidth;
    float btnWidth = Mathf.Max(25f, rect.width / 4f);
    rect.width = btnWidth;

    // Auto=0, Left=1, Center=2, Right=3
    int current = textAlignmentProperty.enumValueIndex;

    EditorGUI.BeginChangeCheck();
    if (GUI.Toggle(rect, current == 1, k_AlignLeft, s_ToggleButtonLeft) && current != 1) current = 1;
    rect.x += btnWidth;
    if (GUI.Toggle(rect, current == 2, k_AlignCenter, s_ToggleButtonMid) && current != 2) current = 2;
    rect.x += btnWidth;
    if (GUI.Toggle(rect, current == 3, k_AlignRight, s_ToggleButtonMid) && current != 3) current = 3;
    rect.x += btnWidth;
    if (GUI.Toggle(rect, current == 0, k_AlignAuto, s_ToggleButtonRight) && current != 0) current = 0;
    if (EditorGUI.EndChangeCheck()) {
      textAlignmentProperty.enumValueIndex = current;
    }
  }

  private void DrawVerticalAlignmentButtons() {
    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 2f);
    EditorGUI.PrefixLabel(rect, k_VAlignmentLabel);

    rect.x += EditorGUIUtility.labelWidth;
    rect.width -= EditorGUIUtility.labelWidth;
    float btnWidth = Mathf.Max(25f, rect.width / 3f);
    rect.width = btnWidth;

    // Top=0, Middle=1, Bottom=2
    int current = textVerticalAlignmentProperty.enumValueIndex;

    EditorGUI.BeginChangeCheck();
    if (GUI.Toggle(rect, current == 0, k_VAlignTop, s_ToggleButtonLeft) && current != 0) current = 0;
    rect.x += btnWidth;
    if (GUI.Toggle(rect, current == 1, k_VAlignMiddle, s_ToggleButtonMid) && current != 1) current = 1;
    rect.x += btnWidth;
    if (GUI.Toggle(rect, current == 2, k_VAlignBottom, s_ToggleButtonRight) && current != 2) current = 2;
    if (EditorGUI.EndChangeCheck()) {
      textVerticalAlignmentProperty.enumValueIndex = current;
    }
  }

  private void DrawGradientStops() {
    int colorCount = gradiantColorsProperty.arraySize;
    int posCount = gradiantPositionsProperty.arraySize;

    // Draw each stop: [Color] [Position 0-1] [-]
    for (int i = 0; i < colorCount; i++) {
      Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
      float savedLabelWidth = EditorGUIUtility.labelWidth;
      int oldIndent = EditorGUI.indentLevel;
      EditorGUI.indentLevel = 0;

      float indent = savedLabelWidth;
      rect.x += indent;
      float available = rect.width - indent;
      float removeWidth = 22f;
      float gap = 3f;
      float fieldWidth = (available - removeWidth - gap * 2f) / 2f;

      // Color field
      Rect colorRect = new Rect(rect.x, rect.y, fieldWidth, rect.height);
      var colorProp = gradiantColorsProperty.GetArrayElementAtIndex(i);
      EditorGUI.PropertyField(colorRect, colorProp, GUIContent.none);

      // Position field
      Rect posRect = new Rect(rect.x + fieldWidth + gap, rect.y, fieldWidth, rect.height);
      if (i < posCount) {
        var posProp = gradiantPositionsProperty.GetArrayElementAtIndex(i);
        EditorGUIUtility.labelWidth = 28f;
        EditorGUI.PropertyField(posRect, posProp, new GUIContent("Pos"));
      }

      // Remove button
      Rect removeRect = new Rect(rect.x + fieldWidth * 2f + gap * 2f, rect.y, removeWidth, rect.height);
      if (GUI.Button(removeRect, k_GradientRemoveLabel) && colorCount > 1) {
        gradiantColorsProperty.DeleteArrayElementAtIndex(i);
        if (i < posCount) gradiantPositionsProperty.DeleteArrayElementAtIndex(i);
        break;
      }

      EditorGUIUtility.labelWidth = savedLabelWidth;
      EditorGUI.indentLevel = oldIndent;
    }

    // Add button
    Rect addRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
    addRect.x += EditorGUIUtility.labelWidth;
    addRect.width = 60f;
    if (GUI.Button(addRect, k_GradientAddLabel)) {
      gradiantColorsProperty.arraySize++;
      gradiantPositionsProperty.arraySize = gradiantColorsProperty.arraySize;
      // Default new color to white (alpha 1)
      var newColor = gradiantColorsProperty.GetArrayElementAtIndex(gradiantColorsProperty.arraySize - 1);
      newColor.colorValue = Color.white;
      // Default new position to 1.0
      var newPos = gradiantPositionsProperty.GetArrayElementAtIndex(gradiantPositionsProperty.arraySize - 1);
      newPos.floatValue = 1f;
    }
  }

  private void DrawRawImageProperties() {
    var hb = (HB_TEXTBlock)target;
    var rawImage = hb.GetComponent<UnityEngine.UI.RawImage>();
    if (rawImage == null) return;

    // Use a shared SerializedObject across all target RawImages
    var rawTargets = new UnityEngine.Object[targets.Length];
    for (int i = 0; i < targets.Length; i++) {
      rawTargets[i] = ((Component)targets[i]).GetComponent<UnityEngine.UI.RawImage>();
    }
    var rawSo = new SerializedObject(rawTargets);
    rawSo.Update();

    var raycastProp = rawSo.FindProperty("m_RaycastTarget");
    var maskableProp = rawSo.FindProperty("m_Maskable");

    EditorGUILayout.PropertyField(raycastProp, k_RaycastTargetLabel);
    EditorGUILayout.PropertyField(maskableProp, k_MaskableLabel);

    rawSo.ApplyModifiedProperties();
  }

  private void DrawFontField() {
    var rect = EditorGUILayout.GetControlRect();
    var label = EditorGUI.BeginProperty(rect, k_FontAssetLabel, fontProperty);

    // Handle drag-and-drop of raw font files onto this field
    if (rect.Contains(Event.current.mousePosition)) {
      if (Event.current.type == EventType.DragUpdated) {
        foreach (var obj in DragAndDrop.objectReferences) {
          if (obj is HBFontData || obj is TextAsset || HBFontDataCreator.IsFontFile(obj)) {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            Event.current.Use();
            break;
          }
        }
      } else if (Event.current.type == EventType.DragPerform) {
        var dropped = DragAndDrop.objectReferences[0];
        if (dropped is HBFontData || dropped is TextAsset) {
          fontProperty.objectReferenceValue = dropped;
          DragAndDrop.AcceptDrag();
          Event.current.Use();
        } else if (HBFontDataCreator.IsFontFile(dropped)) {
          var fontPath = AssetDatabase.GetAssetPath(dropped);
          var targetObjects = targets;
          DragAndDrop.AcceptDrag();
          Event.current.Use();
          EditorApplication.delayCall += () => {
            var hbFont = HBFontDataCreator.CreateFromPath(fontPath);
            if (hbFont == null) return;
            foreach (var t in targetObjects) {
              if (t == null) continue;
              var so = new SerializedObject(t);
              var fp = so.FindProperty("font");
              fp.objectReferenceValue = hbFont;
              so.ApplyModifiedProperties();
            }
          };
        }
      }
    }

    EditorGUI.BeginChangeCheck();
    EditorGUI.showMixedValue = fontProperty.hasMultipleDifferentValues;
    var current = fontProperty.objectReferenceValue;
    // Accept both HBFontData (new) and TextAsset (legacy .txt/.json) for backwards compatibility
    var newObj = EditorGUI.ObjectField(rect, label, current, typeof(UnityEngine.Object), false);
    EditorGUI.showMixedValue = false;

    if (EditorGUI.EndChangeCheck() && newObj != current) {
      // Only accept HBFontData, TextAsset, or null
      if (newObj == null || newObj is HBFontData || newObj is TextAsset) {
        fontProperty.objectReferenceValue = newObj;
      }
    }

    EditorGUI.EndProperty();
  }

  // ===================== Change Detection & Rendering =====================

  private void ApplyAndRender() {
    bool propertiesChanged = serializedObject.ApplyModifiedProperties();

    // Check if font asset changed
    bool fontFaceChanged = false;
    if (fontProperty.propertyType == SerializedPropertyType.ObjectReference && fontProperty.objectReferenceValue != null) {
      var assetPath = AssetDatabase.GetAssetPath(fontProperty.objectReferenceValue);
      if (assetPath != currentFontFace) {
        currentFontFace = assetPath;
        fontFaceChanged = true;
        propertiesChanged = true;
      }
    }

    // Detect RectTransform size changes in edit mode
    bool sizeChanged = false;
    if (!EditorApplication.isPlayingOrWillChangePlaymode && targets.Length == 1) {
      var rt = ((HB_TEXTBlock)target).GetComponent<RectTransform>();
      if (rt != null) {
        float w = rt.rect.width;
        float h = rt.rect.height;
        if (w > 0 && h > 0 && (w != _lastTrackedWidth || h != _lastTrackedHeight)) {
          _lastTrackedWidth = w;
          _lastTrackedHeight = h;
          sizeChanged = true;
        }
      }
    }

    bool shouldRender;
    if (EditorApplication.isPlayingOrWillChangePlaymode) {
      shouldRender = propertiesChanged;
    } else {
      shouldRender = propertiesChanged || sizeChanged || _needsInitialRender;
    }

    if (shouldRender) {
      _needsInitialRender = false;
      foreach (var t in targets) {
        var hbText = (HB_TEXTBlock)t;
        if (hbText == null) continue;
        if (fontFaceChanged) {
          hbText.RefreshFontFamily();
        }
        hbText.ReUpdateEditMode();
      }
    }
  }

  // ===================== Preview =====================

  public override bool HasPreviewGUI() => true;

  public override GUIContent GetPreviewTitle() => new GUIContent("HB Text Preview");

  public override void OnPreviewGUI(Rect r, GUIStyle background) {
    var hb = (HB_TEXTBlock)target;
    var rawImage = hb.GetComponent<UnityEngine.UI.RawImage>();
    if (rawImage != null && rawImage.texture != null) {
      EditorGUI.DrawPreviewTexture(r, rawImage.texture, null, ScaleMode.ScaleToFit);
    } else {
      EditorGUI.DropShadowLabel(r, "No preview available");
    }
  }

  // ===================== UI Helpers =====================

  private static void InitStyles() {
    if (s_StylesInitialized) return;
    s_StylesInitialized = true;

    // Section header: 22px tall, bold, theme-aware background
    s_SectionHeader = new GUIStyle(EditorStyles.label) {
      fixedHeight = 22,
      richText = true,
      border = new RectOffset(9, 9, 0, 0),
      overflow = new RectOffset(9, 0, 0, 0),
      padding = new RectOffset(0, 0, 4, 0),
      fontStyle = FontStyle.Bold
    };
    var headerTex = new Texture2D(1, 1);
    headerTex.SetPixel(0, 0, EditorGUIUtility.isProSkin
      ? new Color(0.22f, 0.22f, 0.22f, 1f)
      : new Color(0.76f, 0.76f, 0.76f, 1f));
    headerTex.Apply();
    headerTex.hideFlags = HideFlags.DontSave;
    s_SectionHeader.normal.background = headerTex;

    s_RightLabel = new GUIStyle(EditorStyles.label) {
      alignment = TextAnchor.MiddleRight,
      richText = true,
      fontSize = 10
    };

    s_BoldFoldout = new GUIStyle(EditorStyles.foldout) {
      fontStyle = FontStyle.Bold
    };

    s_ToggleButtonLeft = new GUIStyle(EditorStyles.miniButtonLeft) {
      padding = new RectOffset(4, 4, 2, 2),
      fontStyle = FontStyle.Bold,
      fixedHeight = 20
    };
    s_ToggleButtonMid = new GUIStyle(EditorStyles.miniButtonMid) {
      padding = new RectOffset(4, 4, 2, 2),
      fontStyle = FontStyle.Bold,
      fixedHeight = 20
    };
    s_ToggleButtonRight = new GUIStyle(EditorStyles.miniButtonRight) {
      padding = new RectOffset(4, 4, 2, 2),
      fontStyle = FontStyle.Bold,
      fixedHeight = 20
    };
  }

  private static void DrawSectionHeader(GUIContent label) {
    EditorGUILayout.Space();
    Rect rect = EditorGUILayout.GetControlRect(false, 22);
    GUI.Label(rect, label, s_SectionHeader);
  }

  private static bool DrawCollapsibleSectionHeader(GUIContent label, bool isExpanded) {
    EditorGUILayout.Space();
    Rect rect = EditorGUILayout.GetControlRect(false, 24);
    if (GUI.Button(rect, label, s_SectionHeader))
      isExpanded = !isExpanded;
    GUI.Label(rect, isExpanded ? k_CollapseExpandHint[0] : k_CollapseExpandHint[1], s_RightLabel);
    return isExpanded;
  }
}




[InitializeOnLoad]
public class HBTextBlockOpenCallback {
  private static bool _renderScheduled;

  static HBTextBlockOpenCallback() {
    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    EditorSceneManager.sceneOpened += OnSceneOpened;
    EditorApplication.hierarchyChanged += OnHierarchyChanged;

    if (!EditorApplication.isPlayingOrWillChangePlaymode) {
      ScheduleRender();
    }
  }

  private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode) {
    ScheduleRender();
  }

  private static void OnPlayModeStateChanged(PlayModeStateChange state) {
    if (state == PlayModeStateChange.EnteredEditMode) {
      ScheduleRender();
    }
  }

  private static void OnHierarchyChanged() {
    if (!EditorApplication.isPlayingOrWillChangePlaymode) {
      ScheduleRender();
    }
  }

  private static void ScheduleRender() {
    if (_renderScheduled) return;
    _renderScheduled = true;
    // Double delay: first delay lets Unity finish deserialization,
    // second delay ensures Canvas layout has computed RectTransform sizes.
    EditorApplication.delayCall += () => {
      EditorApplication.delayCall += () => {
        _renderScheduled = false;
        RenderAllTexts();
      };
    };
  }

  private static void RenderAllTexts() {
    if (EditorApplication.isPlayingOrWillChangePlaymode) return;

    Canvas.ForceUpdateCanvases();

    var allTextBlocks = Resources.FindObjectsOfTypeAll<HB_TEXTBlock>();
    foreach (var textBlock in allTextBlocks) {
      if (textBlock == null || textBlock.gameObject == null) continue;
      // Skip prefab assets — only render objects in loaded scenes
      if (EditorUtility.IsPersistent(textBlock)) continue;
      if (!textBlock.gameObject.scene.isLoaded) continue;
      if (string.IsNullOrEmpty(textBlock.text)) continue;
      textBlock.ReUpdateEditMode();
    }
  }
}

static class HBTextBlockMenuItems {
  [MenuItem("GameObject/Skia UI (Canvas)/HB TextBlock", false, 10)]
  static void CreateHBTextBlock(MenuCommand menuCommand) {
    // Find or create Canvas
    GameObject parent = menuCommand.context as GameObject;
    Canvas canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;

    if (canvas == null) {
      // Look for existing Canvas in scene
      canvas = Object.FindObjectOfType<Canvas>();
    }

    if (canvas == null) {
      // Create new Canvas + EventSystem (like Unity's built-in UI creation)
      var canvasGO = new GameObject("Canvas");
      canvas = canvasGO.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
      canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
      Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");

      // Create EventSystem if none exists
      if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) {
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
      }
    }

    // Create HB TextBlock
    var go = new GameObject("HB TextBlock");
    go.AddComponent<UnityEngine.UI.RawImage>();
    var hb = go.AddComponent<HB_TEXTBlock>();
    Undo.RegisterCreatedObjectUndo(go, "Create HB TextBlock");

    // Parent to canvas or selected object
    Transform parentTransform = parent != null && parent.GetComponentInParent<Canvas>() != null
      ? parent.transform
      : canvas.transform;
    GameObjectUtility.SetParentAndAlign(go, parentTransform.gameObject);

    // Set default RectTransform size
    var rt = go.GetComponent<RectTransform>();
    rt.sizeDelta = new Vector2(200, 50);

    Selection.activeGameObject = go;
  }

  [MenuItem("GameObject/Skia UI (Canvas)/HB InputField", false, 11)]
  static void CreateHBInputField(MenuCommand menuCommand) {
    GameObject parent = menuCommand.context as GameObject;
    Canvas canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;

    if (canvas == null)
      canvas = Object.FindObjectOfType<Canvas>();

    if (canvas == null) {
      var canvasGO = new GameObject("Canvas");
      canvas = canvasGO.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
      canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
      Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");

      if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) {
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
      }
    }

    Transform parentTransform = parent != null && parent.GetComponentInParent<Canvas>() != null
      ? parent.transform
      : canvas.transform;

    // Root: HB InputField (Image background + HBInputField)
    var root = new GameObject("HB InputField");
    var rootRT = root.AddComponent<RectTransform>();
    var bgImage = root.AddComponent<UnityEngine.UI.Image>();
    bgImage.color = new Color(1f, 1f, 1f, 1f);
    bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
    bgImage.type = UnityEngine.UI.Image.Type.Sliced;
    var inputField = root.AddComponent<HBInputField>();
    GameObjectUtility.SetParentAndAlign(root, parentTransform.gameObject);
    rootRT.sizeDelta = new Vector2(300, 40);

    // Text Viewport (RectMask2D for clipping)
    var viewport = new GameObject("Text Area");
    var viewportRT = viewport.AddComponent<RectTransform>();
    viewport.AddComponent<UnityEngine.UI.RectMask2D>();
    viewport.transform.SetParent(root.transform, false);
    viewportRT.anchorMin = Vector2.zero;
    viewportRT.anchorMax = Vector2.one;
    viewportRT.offsetMin = new Vector2(10, 6);
    viewportRT.offsetMax = new Vector2(-10, -6);

    // Placeholder
    var placeholderGO = new GameObject("Placeholder");
    placeholderGO.AddComponent<UnityEngine.UI.RawImage>();
    var placeholderText = placeholderGO.AddComponent<HB_TEXTBlock>();
    placeholderGO.transform.SetParent(viewport.transform, false);
    var placeholderRT = placeholderGO.GetComponent<RectTransform>();
    placeholderRT.anchorMin = Vector2.zero;
    placeholderRT.anchorMax = Vector2.one;
    placeholderRT.offsetMin = Vector2.zero;
    placeholderRT.offsetMax = Vector2.zero;

    // Text
    var textGO = new GameObject("Text");
    textGO.AddComponent<UnityEngine.UI.RawImage>();
    var textComp = textGO.AddComponent<HB_TEXTBlock>();
    textGO.transform.SetParent(viewport.transform, false);
    var textRT = textGO.GetComponent<RectTransform>();
    textRT.anchorMin = Vector2.zero;
    textRT.anchorMax = Vector2.one;
    textRT.offsetMin = Vector2.zero;
    textRT.offsetMax = Vector2.zero;

    // Caret
    var caretGO = new GameObject("Caret");
    var caretRT = caretGO.AddComponent<RectTransform>();
    var caretImg = caretGO.AddComponent<UnityEngine.UI.Image>();
    caretImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
    caretImg.raycastTarget = false;
    caretGO.transform.SetParent(textGO.transform, false);
    caretRT.sizeDelta = new Vector2(2, 20);
    caretGO.SetActive(false);

    // Wire references via SerializedObject
    Undo.RegisterCreatedObjectUndo(root, "Create HB InputField");
    var so = new SerializedObject(inputField);
    so.FindProperty("textComponent").objectReferenceValue = textComp;
    so.FindProperty("placeholder").objectReferenceValue = placeholderText;
    so.FindProperty("textViewport").objectReferenceValue = viewportRT;
    so.FindProperty("caretImage").objectReferenceValue = caretImg;
    so.ApplyModifiedPropertiesWithoutUndo();

    // Configure text components for input field layout
    float textWidth = 300f - 20f; // root width minus viewport padding
    var textSO = new SerializedObject(textComp);
    textSO.FindProperty("maxWidth").floatValue = textWidth;
    textSO.FindProperty("textAlignment").enumValueIndex = 0; // Left
    textSO.FindProperty("autoFitVertical").boolValue = true;
    textSO.ApplyModifiedPropertiesWithoutUndo();

    var placeholderSO = new SerializedObject(placeholderText);
    placeholderSO.FindProperty("Text").stringValue = "Enter text...";
    placeholderSO.FindProperty("fontColor").colorValue = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    placeholderSO.FindProperty("maxWidth").floatValue = textWidth;
    placeholderSO.FindProperty("textAlignment").enumValueIndex = 0; // Left
    placeholderSO.FindProperty("autoFitVertical").boolValue = true;
    placeholderSO.ApplyModifiedPropertiesWithoutUndo();

    Selection.activeGameObject = root;
  }
}
#endif
