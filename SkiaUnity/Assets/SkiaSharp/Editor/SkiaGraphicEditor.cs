#if UNITY_EDITOR

using SkiaSharp.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;
using System.IO;

[CustomEditor(typeof(SkiaGraphic))]
[CanEditMultipleObjects]
public class SkiaGraphicEditor : Editor {
	// Shape
	private SerializedProperty shape;
	private SerializedProperty cornerRadii;

	// Fill
	private SerializedProperty fillType;
	private SerializedProperty fillColor;
	private SerializedProperty gradient;
	private SerializedProperty gradientAngle;
	private SerializedProperty fillImage;
	private SerializedProperty imageFit;

	// Stroke
	private SerializedProperty enableStroke;
	private SerializedProperty strokeColor;
	private SerializedProperty strokeWidth;
	private SerializedProperty enableDashedStroke;
	private SerializedProperty dashLength;
	private SerializedProperty dashGap;
	private SerializedProperty enableGradientStroke;
	private SerializedProperty strokeGradient;
	private SerializedProperty strokeGradientAngle;

	// Shadow
	private SerializedProperty enableShadow;
	private SerializedProperty shadowColor;
	private SerializedProperty shadowOffset;
	private SerializedProperty shadowBlur;

	// Inner Shadow
	private SerializedProperty enableInnerShadow;
	private SerializedProperty innerShadowColor;
	private SerializedProperty innerShadowOffset;
	private SerializedProperty innerShadowBlur;

	// Performance
	private SerializedProperty resolutionScale;

	// Bake
	private SerializedProperty bakedSprite;

	private GUIStyle headerStyle;

	private static readonly GUIContent k_RaycastTargetLabel = new GUIContent("Raycast Target");
	private static readonly GUIContent k_MaskableLabel = new GUIContent("Maskable");

	private void OnEnable() {
		shape = serializedObject.FindProperty("shape");
		cornerRadii = serializedObject.FindProperty("cornerRadii");
		fillType = serializedObject.FindProperty("fillType");
		fillColor = serializedObject.FindProperty("fillColor");
		gradient = serializedObject.FindProperty("gradient");
		gradientAngle = serializedObject.FindProperty("gradientAngle");
		fillImage = serializedObject.FindProperty("fillImage");
		imageFit = serializedObject.FindProperty("imageFit");
		enableStroke = serializedObject.FindProperty("enableStroke");
		strokeColor = serializedObject.FindProperty("strokeColor");
		strokeWidth = serializedObject.FindProperty("strokeWidth");
		enableDashedStroke = serializedObject.FindProperty("enableDashedStroke");
		dashLength = serializedObject.FindProperty("dashLength");
		dashGap = serializedObject.FindProperty("dashGap");
		enableGradientStroke = serializedObject.FindProperty("enableGradientStroke");
		strokeGradient = serializedObject.FindProperty("strokeGradient");
		strokeGradientAngle = serializedObject.FindProperty("strokeGradientAngle");
		enableShadow = serializedObject.FindProperty("enableShadow");
		shadowColor = serializedObject.FindProperty("shadowColor");
		shadowOffset = serializedObject.FindProperty("shadowOffset");
		shadowBlur = serializedObject.FindProperty("shadowBlur");
		enableInnerShadow = serializedObject.FindProperty("enableInnerShadow");
		innerShadowColor = serializedObject.FindProperty("innerShadowColor");
		innerShadowOffset = serializedObject.FindProperty("innerShadowOffset");
		innerShadowBlur = serializedObject.FindProperty("innerShadowBlur");
		resolutionScale = serializedObject.FindProperty("resolutionScale");
		bakedSprite = serializedObject.FindProperty("bakedSprite");

		// Hide RawImage and Image from inspector — SkiaGraphic manages them internally
		foreach (var t in targets) {
			var ri = ((Component)t).GetComponent<RawImage>();
			if (ri != null && (ri.hideFlags & HideFlags.HideInInspector) == 0)
				ri.hideFlags |= HideFlags.HideInInspector;
			var img = ((Component)t).GetComponent<Image>();
			if (img != null && (img.hideFlags & HideFlags.HideInInspector) == 0)
				img.hideFlags |= HideFlags.HideInInspector;
		}
	}

	public override void OnInspectorGUI() {
		if (headerStyle == null) {
			headerStyle = new GUIStyle(EditorStyles.boldLabel) {
				fontSize = 12
			};
		}

		serializedObject.Update();

		bool isBaked = bakedSprite.objectReferenceValue != null;

		// Grey out all rendering options when baked
		EditorGUI.BeginDisabledGroup(isBaked);

		// Shape
		EditorGUILayout.LabelField("Shape", headerStyle);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(shape);
		if ((SkiaShapeType)shape.enumValueIndex == SkiaShapeType.RoundedRect) {
			EditorGUILayout.PropertyField(cornerRadii, new GUIContent("Corner Radii", "X=TopLeft, Y=TopRight, Z=BottomRight, W=BottomLeft"));
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.Space(4);

		// Fill
		EditorGUILayout.LabelField("Fill", headerStyle);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(fillType);
		var ft = (SkiaFillType)fillType.enumValueIndex;
		if (ft == SkiaFillType.Solid) {
			EditorGUILayout.PropertyField(fillColor);
		} else if (ft == SkiaFillType.Image) {
			EditorGUILayout.PropertyField(fillImage, new GUIContent("Image", "Texture must have Read/Write enabled in import settings."));
			EditorGUILayout.PropertyField(imageFit, new GUIContent("Fit Mode"));
		} else if (ft != SkiaFillType.None) {
			EditorGUILayout.PropertyField(gradient);
			if (ft == SkiaFillType.LinearGradient) {
				EditorGUILayout.PropertyField(gradientAngle, new GUIContent("Angle"));
			}
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.Space(4);

		// Stroke
		EditorGUILayout.LabelField("Stroke", headerStyle);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(enableStroke, new GUIContent("Enable"));
		if (enableStroke.boolValue) {
			EditorGUILayout.PropertyField(enableGradientStroke, new GUIContent("Gradient Stroke"));
			if (enableGradientStroke.boolValue) {
				EditorGUILayout.PropertyField(strokeGradient, new GUIContent("Gradient"));
				EditorGUILayout.PropertyField(strokeGradientAngle, new GUIContent("Angle"));
			} else {
				EditorGUILayout.PropertyField(strokeColor, new GUIContent("Color"));
			}
			EditorGUILayout.PropertyField(strokeWidth, new GUIContent("Width"));
			EditorGUILayout.PropertyField(enableDashedStroke, new GUIContent("Dashed"));
			if (enableDashedStroke.boolValue) {
				EditorGUILayout.PropertyField(dashLength, new GUIContent("Dash Length"));
				EditorGUILayout.PropertyField(dashGap, new GUIContent("Dash Gap"));
			}
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.Space(4);

		// Shadow
		EditorGUILayout.LabelField("Shadow", headerStyle);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(enableShadow, new GUIContent("Enable"));
		if (enableShadow.boolValue) {
			EditorGUILayout.PropertyField(shadowColor, new GUIContent("Color"));
			EditorGUILayout.PropertyField(shadowOffset, new GUIContent("Offset"));
			EditorGUILayout.PropertyField(shadowBlur, new GUIContent("Blur"));
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.Space(4);

		// Inner Shadow
		EditorGUILayout.LabelField("Inner Shadow", headerStyle);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(enableInnerShadow, new GUIContent("Enable"));
		if (enableInnerShadow.boolValue) {
			EditorGUILayout.PropertyField(innerShadowColor, new GUIContent("Color"));
			EditorGUILayout.PropertyField(innerShadowOffset, new GUIContent("Offset"));
			EditorGUILayout.PropertyField(innerShadowBlur, new GUIContent("Blur"));
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.Space(4);

		// Performance
		EditorGUILayout.LabelField("Performance", headerStyle);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(resolutionScale, new GUIContent("Resolution Scale", "Lower for better mobile performance. 0.5 = half resolution."));
		EditorGUI.indentLevel--;

		EditorGUI.EndDisabledGroup();

		EditorGUILayout.Space(4);

		// Bake
		EditorGUILayout.LabelField("Bake", headerStyle);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(bakedSprite, new GUIContent("Baked Sprite", "When assigned, uses this sprite via Image component + SpriteAtlas for batching. Zero SkiaSharp cost at runtime."));
		if (bakedSprite.objectReferenceValue != null) {
			EditorGUILayout.HelpBox("Baked. Using Image + SpriteAtlas for draw call batching. Zero SkiaSharp cost at runtime.", MessageType.Info);
			if (GUILayout.Button("Unbake (Return to Live Rendering)")) {
				serializedObject.ApplyModifiedProperties();
				UnbakeSelectedGraphics();
				GUIUtility.ExitGUI();
			}
		} else {
			if (GUILayout.Button("Bake to PNG (Auto Atlas)")) {
				serializedObject.ApplyModifiedProperties();
				BakeSelectedGraphics();
				GUIUtility.ExitGUI();
			}
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.Space(4);

		// UI Settings (RawImage proxy properties)
		EditorGUILayout.LabelField("UI Settings", headerStyle);
		EditorGUI.indentLevel++;
		DrawRawImageProperties();
		EditorGUI.indentLevel--;

		serializedObject.ApplyModifiedProperties();
	}

	private void BakeSelectedGraphics() {
		string dir = "Assets/BakedSkiaGraphics";
		if (!AssetDatabase.IsValidFolder(dir))
			AssetDatabase.CreateFolder("Assets", "BakedSkiaGraphics");

		// Load or create SpriteAtlas
		string atlasPath = $"{dir}/SkiaGraphicAtlas.spriteatlas";
		SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
		if (atlas == null) {
			atlas = new SpriteAtlas();
			atlas.SetPackingSettings(new SpriteAtlasPackingSettings {
				enableRotation = false,
				enableTightPacking = false,
				padding = 4
			});
			atlas.SetTextureSettings(new SpriteAtlasTextureSettings {
				readable = false,
				generateMipMaps = false,
				sRGB = true,
				filterMode = FilterMode.Bilinear
			});
			AssetDatabase.CreateAsset(atlas, atlasPath);
		}

		foreach (var t in targets) {
			var graphic = (SkiaGraphic)t;
			byte[] png = graphic.BakeToTexture();
			if (png == null || png.Length == 0) {
				Debug.LogWarning($"SkiaGraphic: Bake failed for {graphic.name}");
				continue;
			}

			string assetPath = GetUniqueBakePath(dir, graphic.gameObject.name, "_baked");

			File.WriteAllBytes(assetPath, png);
			AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

			// Import as Sprite with compression + nine-slice border
			Vector4 border = graphic.GetNineSliceBorder();
			TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
			if (importer != null) {
				importer.textureType = TextureImporterType.Sprite;
				importer.spriteImportMode = SpriteImportMode.Single;
				importer.spriteBorder = border;
				importer.textureCompression = TextureImporterCompression.Compressed;
				importer.alphaIsTransparency = true;
				importer.npotScale = TextureImporterNPOTScale.None;
				importer.mipmapEnabled = false;
				importer.sRGBTexture = true;
				importer.SaveAndReimport();
			}

			Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
			if (spriteAsset == null) {
				Debug.LogWarning($"SkiaGraphic: Could not load baked sprite at {assetPath}");
				continue;
			}

			// Add sprite to atlas
			atlas.Add(new Object[] { AssetDatabase.LoadAssetAtPath<Object>(assetPath) });

			// Swap RawImage → Image (destroy texture first to prevent leak)
			var rawImg = graphic.GetComponent<RawImage>();
			bool raycast = rawImg != null && rawImg.raycastTarget;
			bool maskable = rawImg != null && rawImg.maskable;
			if (rawImg != null) {
				if (rawImg.texture != null) {
					DestroyImmediate(rawImg.texture);
					rawImg.texture = null;
				}
				Undo.DestroyObjectImmediate(rawImg);
			}

			var img = graphic.GetComponent<Image>();
			if (img == null) {
				img = graphic.gameObject.AddComponent<Image>();
				Undo.RegisterCreatedObjectUndo(img, "Bake SkiaGraphic");
			}
			img.sprite = spriteAsset;
			if (border != Vector4.zero) {
				img.type = Image.Type.Sliced;
				img.fillCenter = true;
			} else {
				img.type = Image.Type.Simple;
			}
			img.preserveAspect = false;
			img.color = Color.white;
			img.raycastTarget = raycast;
			img.maskable = maskable;
			img.hideFlags |= HideFlags.HideInInspector;

			// Assign bakedSprite
			var so = new SerializedObject(graphic);
			so.FindProperty("bakedSprite").objectReferenceValue = spriteAsset;
			so.ApplyModifiedProperties();
			EditorUtility.SetDirty(graphic);
		}

		EditorUtility.SetDirty(atlas);
		AssetDatabase.SaveAssets();
	}

	private void UnbakeSelectedGraphics() {
		foreach (var t in targets) {
			var graphic = (SkiaGraphic)t;

			// Remove Image, re-add RawImage (only one Graphic per GO)
			var img = graphic.GetComponent<Image>();
			if (img != null) Undo.DestroyObjectImmediate(img);

			var rawImg = graphic.GetComponent<RawImage>();
			if (rawImg == null) {
				rawImg = graphic.gameObject.AddComponent<RawImage>();
				Undo.RegisterCreatedObjectUndo(rawImg, "Unbake SkiaGraphic");
			}
			rawImg.enabled = true;
			rawImg.hideFlags |= HideFlags.HideInInspector;

			// Clear bakedSprite
			var so = new SerializedObject(graphic);
			so.FindProperty("bakedSprite").objectReferenceValue = null;
			so.ApplyModifiedProperties();

			// Re-trigger OnDisable → OnEnable to reinitialize rawImage + paint
			graphic.enabled = false;
			graphic.enabled = true;

			EditorUtility.SetDirty(graphic);
		}
	}

	private static string GetUniqueBakePath(string dir, string objectName, string suffix) {
		// Sanitize object name for filesystem
		foreach (char c in Path.GetInvalidFileNameChars())
			objectName = objectName.Replace(c, '_');

		string baseName = $"{objectName}{suffix}";
		string path = $"{dir}/{baseName}.png";
		if (!File.Exists(path)) return path;

		int counter = 1;
		while (File.Exists($"{dir}/{baseName}_{counter}.png"))
			counter++;
		return $"{dir}/{baseName}_{counter}.png";
	}

	[DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
	static void DrawSkiaGizmo(SkiaGraphic graphic, GizmoType gizmoType) {
		RectTransform rt = graphic.GetComponent<RectTransform>();
		if (rt == null) return;

		// Cyan when selected, semi-transparent when not
		bool selected = (gizmoType & GizmoType.Selected) != 0;
		Gizmos.color = selected ? new Color(0f, 0.8f, 1f, 1f) : new Color(0f, 0.8f, 1f, 0.3f);

		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);

		// Draw wireframe rect outline
		Gizmos.DrawLine(corners[0], corners[1]);
		Gizmos.DrawLine(corners[1], corners[2]);
		Gizmos.DrawLine(corners[2], corners[3]);
		Gizmos.DrawLine(corners[3], corners[0]);

		// Draw bake indicator (filled center dot) when baked
		if (graphic.IsBaked && selected) {
			Vector3 center = (corners[0] + corners[2]) * 0.5f;
			Handles.color = new Color(0.2f, 1f, 0.3f, 0.8f);
			float size = Vector3.Distance(corners[0], corners[2]) * 0.02f;
			Handles.DrawSolidDisc(center, rt.forward, Mathf.Max(size, 2f));
		}
	}

	private void DrawRawImageProperties() {
		bool isBaked = bakedSprite.objectReferenceValue != null;
		var uiTargets = new Object[targets.Length];
		for (int i = 0; i < targets.Length; i++) {
			if (isBaked)
				uiTargets[i] = ((Component)targets[i]).GetComponent<Image>();
			else
				uiTargets[i] = ((Component)targets[i]).GetComponent<RawImage>();
		}

		if (uiTargets[0] == null) return;

		var uiSo = new SerializedObject(uiTargets);
		uiSo.Update();

		var raycastProp = uiSo.FindProperty("m_RaycastTarget");
		var maskableProp = uiSo.FindProperty("m_Maskable");

		EditorGUILayout.PropertyField(raycastProp, k_RaycastTargetLabel);
		EditorGUILayout.PropertyField(maskableProp, k_MaskableLabel);

		uiSo.ApplyModifiedProperties();
	}
}

static class SkiaGraphicMenuItems {
	[MenuItem("GameObject/Skia UI (Canvas)/Skia Graphic", false, 9)]
	static void CreateSkiaGraphic(MenuCommand menuCommand) {
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

		var go = new GameObject("Skia Graphic");
		go.AddComponent<UnityEngine.UI.RawImage>();
		go.AddComponent<SkiaGraphic>();
		Undo.RegisterCreatedObjectUndo(go, "Create Skia Graphic");

		Transform parentTransform = parent != null && parent.GetComponentInParent<Canvas>() != null
			? parent.transform
			: canvas.transform;
		GameObjectUtility.SetParentAndAlign(go, parentTransform.gameObject);

		var rt = go.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(200, 200);

		Selection.activeGameObject = go;
	}
}
#endif
