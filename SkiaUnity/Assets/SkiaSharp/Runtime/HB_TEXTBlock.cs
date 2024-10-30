using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Topten.RichTextKit;
using TextAlignment = Topten.RichTextKit.TextAlignment;

namespace SkiaSharp.Unity.HB {
	public enum HBColorFormat {
		alpha8,
		rgb32
	}
	
	public class HB_TEXTBlock : MonoBehaviour, ILayoutElement {
		[SerializeField]
		[TextArea]
		private string Text;
		[SerializeField]
		public TextAsset font;
		[SerializeField]
		private int fontSize = 12, letterSpacing, maxLines;
		[SerializeField]
		[Range (0,100)]
		private int outlineWidth, outlineBlur;
		[SerializeField]
		[Range (0,10)]
		private int shadowWidth;
		[SerializeField]
		[Range (0,1)]
		private int innerGlowWidth = 0;
		[SerializeField]
		[Range (-100,100)]
		float shadowOffsetX, shadowOffsetY = 1;
		[SerializeField]
		private Color fontColor = Color.black, outlineColor = Color.black, shadowColor = Color.black, innerGlowColor = Color.white, backgroundColor = Color.clear,linkColor = Color.blue;
		[SerializeField]
		private bool italic, bold, autoFitVertical = true, autoFitHorizontal, renderLinks, enableGradiant, enableEllipsis = true;
		[SerializeField]
		private UnderlineStyle underlineStyle;
		[SerializeField]
		private StrikeThroughStyle strikeThroughStyle;
		[SerializeField]
		private float lineHeight = 1.0f, maxWidth = 264, maxHeight = -1 , gradiantAngle = 90;
		[SerializeField]
		private HBColorFormat colorType = HBColorFormat.alpha8; 
		[SerializeField] 
		private TextAlignment textAlignment = TextAlignment.Left;
		[SerializeField]
		Color[] gradiantColors;
		[SerializeField]
		float[] gradiantPositions;

        
		private SKCanvas canvas;
		private SKImageInfo info = new SKImageInfo();
		private SKSurface surface;
		private SKPixmap pixmap;
		private RawImage rawImage;
		private Texture2D texture;
		private TextBlock rs;
		private Dictionary<int, HBLinks> urls = new Dictionary<int, HBLinks>();
		private Style styleBoldItalic = new Style();
		string pattern = @"(https?://\S+|www\.\S+)";
		private Regex regex;
		SKTypeface skTypeface;
		RectTransform rectTransform;
		private float currentWidth, currentHeight, currentPreferdWidth = 0, currentPreferdHeight;
		private TextGradient blockGradient;
		private bool widthPreferred, heightPreferred;

		public TextBlock Info => rs;

		public float MaxWidth {
			get => maxWidth;
			set {
				maxWidth = value;
				if (rawImage == null) {
					rawImage = GetComponent<RawImage>();
					rectTransform = transform as RectTransform;
				}
				if (rawImage) {
					urls.Clear();
					RenderText();
				}
			}
		}
		
		public float MaxHeight {
			get => maxHeight;
			set {
				maxHeight = value;
				if (rawImage == null) {
					rawImage = GetComponent<RawImage>();
					rectTransform = transform as RectTransform;
				}
				if (rawImage) {
					urls.Clear();
					RenderText();
				}
			}
		}
		
		public string text {
			get => Text;
			set {
				Text = value;
				ReUpdate();
			}
		}

		public Color FontColor {
			get {
				return fontColor;
			}
			set {
				if (value != fontColor) {
					fontColor = value;
					styleBoldItalic.TextColor = new SKColor(ColorToUint(fontColor));
					ReUpdate();
				}
			}
		}
		
		public Color HaloColor {
			get {
				return outlineColor;
			}
			set {
				outlineColor = value;
				ReUpdate();
			}
		}

		public Color ShadowColor {
			get {
				return shadowColor;
			}
			set {
				shadowColor = value;
				ReUpdate();
			}
		}

		public Color InnerGlowColor {
			get {
				return innerGlowColor;
			}
			set {
				innerGlowColor = value;
				ReUpdate();
			}
		}
		
		public Color BackgroundColor {
			get {
				return backgroundColor;
			}
			set {
				backgroundColor = value;
				ReUpdate();
			}
		}
		
		public HBColorFormat ColorType {
			get {
				return colorType;
			}
			set {
				colorType = value;
				ReUpdate();
			}
		}
		
		public bool AutoFitVertical {
			get {
				return autoFitVertical;
			}
			set {
				autoFitVertical = value;
				ReUpdate();
			}
		}
		
		public bool AutoFitHorizontal {
			get {
				return autoFitHorizontal;
			}
			set {
				autoFitHorizontal = value;
				ReUpdate();
			}
		}

		public bool IsGradiantEnabled {
			get {
				return enableGradiant;
			}
			set {
				enableGradiant = value;
				ReUpdate();
			}
		}
		
		public bool RenderLinks {
			get {
				return renderLinks;
			}
			set {
				renderLinks = value;
				ReUpdate();
			}
		}
		
		public TextAsset Font {
			get {
				return font;
			}
			set {
				font = value;
				ReUpdate();
			}
		}
		
		public bool Bold {
			get {
				return bold;
			}
			set {
				bold = value;
				ReUpdate();
			}
		}
		
		public bool Italic {
			get {
				return italic;
			}
			set {
				italic = value;
				ReUpdate();
			}
		}
		
		public int FontSize {
			get {
				return fontSize;
			}
			set {
				fontSize = value;
				ReUpdate();
			}
		}
		
		public int HaloWidth {
			get {
				return outlineWidth;
			}
			set {
				outlineWidth = value;
				ReUpdate();
			}
		}

		public int ShadowWidth {
			get {
				return shadowWidth;
			}
			set {
				shadowWidth = value;
				ReUpdate();
			}
		}

		public int InnerGlowWidth {
			get {
				return innerGlowWidth;
			}
			set {
				innerGlowWidth = value;
				ReUpdate();
			}
		}
		
		public int LetterSpacing {
			get {
				return letterSpacing;
			}
			set {
				letterSpacing = value;
				ReUpdate();
			}
		}
		
		public int HaloBlur {
			get {
				return outlineBlur;
			}
			set {
				outlineBlur = value;
				ReUpdate();
			}
		}
		
		public UnderlineStyle UnderLineStyle {
			get {
				return underlineStyle;
			}
			set {
				underlineStyle = value;
				ReUpdate();
			}
		}
		
		public StrikeThroughStyle  StrikeThroughStyle{
			get {
				return strikeThroughStyle;
			}
			set {
				strikeThroughStyle = value;
				ReUpdate();
			}
		}
		
		public float  LineHeight{
			get {
				return LineHeight;
			}
			set {
				LineHeight = value;
				ReUpdate();
			}
		}
		
		public TextAlignment  TextAlignment{
			get {
				return textAlignment;
			}
			set {
				if (textAlignment != value) {
					textAlignment = value;
					ReUpdate();
				}
			}
		}

		void Awake() {
			rawImage = GetComponent<RawImage>();
			if (rawImage) {
				rawImage.enabled = false;
			}
			rectTransform = transform as RectTransform;
			
			styleBoldItalic.FontSize = fontSize;
			styleBoldItalic.TextColor = new SKColor(ColorToUint(fontColor));
			styleBoldItalic.HaloWidth = outlineWidth;
			styleBoldItalic.ShadowWidth = shadowWidth;
			styleBoldItalic.HaloColor = outlineWidth > 0 ? new SKColor(ColorToUint(outlineColor)) : SKColor.Empty;
			styleBoldItalic.ShadowColor = shadowWidth > 0 ? new SKColor(ColorToUint(shadowColor)) : SKColor.Empty;
			styleBoldItalic.ShadowOffsetX = shadowOffsetX;
			styleBoldItalic.ShadowOffsetY = shadowOffsetY;
			styleBoldItalic.InnerGlowWidth = innerGlowWidth;
			styleBoldItalic.InnerGlowColor = innerGlowWidth > 0 ? new SKColor(ColorToUint(innerGlowColor)) : SKColor.Empty;
			
			styleBoldItalic.FontItalic = italic;
			styleBoldItalic.FontWeight = bold ? 700 : 400;
			styleBoldItalic.LetterSpacing = letterSpacing;
			styleBoldItalic.TextDirection = TextDirection.Auto;
			styleBoldItalic.HaloBlur = outlineBlur;
			styleBoldItalic.BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty;
			styleBoldItalic.Underline = underlineStyle;
			styleBoldItalic.LineHeight = lineHeight;
			styleBoldItalic.StrikeThrough = strikeThroughStyle;
		}

		private void OnEnable() {
			if (String.IsNullOrEmpty(Text)){
				return;
			}
			
			if (rawImage) {
				urls.Clear();
				RenderText();
			}
		}

		private bool textRendered;
        
		
		// Convert a Color to a uint
		public uint ColorToUint(Color color){
			uint alpha = (uint)(color.a * 255);
			uint red = (uint)(color.r * 255);
			uint green = (uint)(color.g * 255);
			uint blue = (uint)(color.b * 255);
			return (alpha << 24) | (red << 16) | (green << 8) | blue;
		}
		
		

		private void RenderText() {
			if (rawImage && String.IsNullOrEmpty(text)) {
				rawImage.enabled = false;
			}
			Dispose();
			#if !UNITY_EDITOR
				DestroyImmediate(rawImage.texture);
			#else
			DestroyImmediate(rawImage.texture);
			#endif
			
			if (texture != null) {
				#if !UNITY_EDITOR
				DestroyImmediate(texture);
				#else
				DestroyImmediate(texture);
				#endif
			}
			
			if (rs == null) {
				rs = new TextBlock();
			}
			rs.Clear();
			rs.MaxHeight = null;
			rs.MaxWidth = null;
			rs.MaxLines = maxLines == 0 ? null : maxLines;
			if (colorType == HBColorFormat.alpha8) {
				rawImage.color = fontColor;
			} else {
				rawImage.color = Color.white;
			}
			rs.Alignment = textAlignment;
			rs.EllipsisEnabled = enableEllipsis;            
			rs.AddText(Text, styleBoldItalic);
			
			if (renderLinks) {
				RenderLinksCall();
			}

			if (font != null) {
				if (skTypeface == null) {
					var bytes = font.bytes;
					SKData copy = SKData.CreateCopy(bytes);
					skTypeface = SKTypeface.FromData(copy);
					copy.Dispose();
				}
				rs.FontMapper = new FontMapper(skTypeface);
			}

			if ( (!autoFitHorizontal && rectTransform.rect.width == 0)) {
				return;
			}
			
			currentPreferdWidth = autoFitHorizontal ? rs.MeasuredWidth > maxWidth ? maxWidth : rs.MeasuredWidth : rectTransform.rect.width;
			rs.MaxWidth = currentPreferdWidth;
		//	rs.MaxWidth = currentPreferdWidth = rs.MeasuredWidth;
			currentPreferdHeight = autoFitVertical ? maxHeight > -1  && rs.MeasuredHeight > maxHeight ? maxHeight : rs.MeasuredHeight : rectTransform.rect.height;

			rs.MaxHeight = currentPreferdHeight;
			//rs.MaxHeight = currentPreferdHeight = rs.MeasuredHeight;


			if (autoFitVertical) {
				var size= autoFitHorizontal ? new Vector2(currentPreferdWidth, heightPreferred == true ? rectTransform.sizeDelta.y : rs.MeasuredHeight ) : new Vector2(rectTransform.rect.width, heightPreferred == true ? rs.MeasuredHeight : rs.MeasuredHeight );
				if (size.x == 0) {
					return;
				}

				rectTransform.sizeDelta = size;
			} else {
				var size= autoFitHorizontal ? new Vector2(currentPreferdWidth, !heightPreferred ? currentPreferdHeight : rectTransform.rect.height) : new Vector2(rectTransform.rect.width, rectTransform.rect.height );
				rectTransform.sizeDelta = size;
			}
			
			currentWidth = rectTransform.rect.width;
			currentHeight = rectTransform.rect.height;
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

			if (currentWidth == 0 || currentHeight == 0) {
				return;
			}

			int roundedWidth = Mathf.CeilToInt(rectTransform.rect.width / 4) * 4;
			int roundedHeight = Mathf.CeilToInt(rectTransform.rect.height / 4) * 4;
			
			if (info.IsEmpty) {
				info = new SKImageInfo(roundedWidth, roundedHeight);
			} else {
				info.Width = roundedWidth;
				info.Height = roundedHeight;
			}

			info.ColorType = colorType == HBColorFormat.alpha8 ? SKColorType.Alpha8 : info.ColorType ;

			surface = SKSurface.Create(info);
			canvas = surface.Canvas;
			canvas.Scale(1, -1);
            canvas.Translate(0, -info.Height);
			TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : info.ColorType == SKColorType.Alpha8 ? TextureFormat.Alpha8 : TextureFormat.RGBA32;
			if (texture == null) {
				texture = new Texture2D(roundedWidth, roundedHeight, format, false);
			} else {
				texture.Reinitialize(roundedWidth, roundedHeight, format, false);
			}
			
			if (enableGradiant && gradiantColors != null) {
				SKColor[] colors = new SKColor[gradiantColors.Length];
				for (int i = 0; i < gradiantColors.Length; i++) {
					colors[i] = new SKColor(ColorToUint(gradiantColors[i]));
				}
				blockGradient = TextGradient.Linear(colors, gradiantPositions, gradiantAngle);
				var options = new TextPaintOptions() {
					TextGradient = blockGradient
				};
				rs.Paint(canvas,options);
			} else {
				rs.Paint(canvas);
			}
			
			texture.name = "HB_Text";
			texture.wrapMode = TextureWrapMode.Repeat;
			pixmap = surface.PeekPixels();
			texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
			texture.Compress(false);
			texture.Apply();
			rawImage.texture = texture;
			if (!rawImage.enabled) {
				rawImage.enabled = true;
			}
			Dispose();
			textRendered = true;
		}

		private void RenderLinksCall() {
			Style styleLink = new Style() {
			FontSize = fontSize,
			TextColor = new SKColor(ColorToUint(linkColor)),
			Underline = UnderlineStyle.Solid,
			HaloWidth = outlineWidth,
			ShadowWidth = shadowWidth,
			HaloColor = outlineWidth > 0 ? new SKColor(ColorToUint(outlineColor)) : SKColor.Empty,
			ShadowColor = shadowWidth > 0 ? new SKColor(ColorToUint(shadowColor)) : SKColor.Empty,
			InnerGlowColor = innerGlowWidth > 0 ? new SKColor(ColorToUint(innerGlowColor)) : SKColor.Empty,
			InnerGlowWidth = innerGlowWidth,
			FontItalic = italic,
			FontWeight = bold ? 700 : 400,
			LetterSpacing = letterSpacing,
			TextDirection = TextDirection.Auto,
			HaloBlur = outlineBlur,
			BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty,
			LineHeight = lineHeight,
			StrikeThrough = strikeThroughStyle,
		};
			
			string pattern = @"(https?://\S+|www\.\S+)";
			if (regex == null) {
				regex = new Regex(pattern);
			}
			
			MatchCollection matches = regex.Matches(Text);
				foreach (Match match in matches) {
					var length = match.Index + match.Length;
					int differnce = 0;
					if (length > rs.Length) {
						differnce = length - rs.Length;
						length = length - differnce;
					}

					rs.ApplyStyle(Mathf.Clamp(match.Index,0,rs.Length), Mathf.Clamp(match.Length - differnce,0,match.Length),styleLink);
					urls.Add(match.Index,new HBLinks() {
						IndexStart = match.Index,
						IndexEnd = length,
						Length = match.Length,
						link = match.Value
					});
				}
		}
        
		private void FixedUpdate() {
			if (currentWidth != rectTransform.rect.width || currentHeight != currentPreferdHeight) {
				urls.Clear();
				RenderText();
			}
		}

		public void ReUpdate() {
			if (rawImage == null) {
				rawImage = GetComponent<RawImage>();
				rectTransform = transform as RectTransform;
			}
			urls.Clear();
			RenderText();
		}
		
		#if UNITY_EDITOR
		public void ReUpdateEditMode() {
			if (rawImage == null) {
				rawImage = GetComponent<RawImage>();
				rectTransform = transform as RectTransform;
			}

			styleBoldItalic.FontSize = fontSize;
			styleBoldItalic.TextColor = new SKColor(ColorToUint(fontColor));
			styleBoldItalic.HaloWidth = outlineWidth;
			styleBoldItalic.ShadowWidth = shadowWidth;
			styleBoldItalic.ShadowOffsetX = shadowOffsetX;
			styleBoldItalic.ShadowOffsetY = shadowOffsetY;
			styleBoldItalic.HaloColor = outlineWidth > 0 ? new SKColor(ColorToUint(outlineColor)) : SKColor.Empty;
			styleBoldItalic.ShadowColor = shadowWidth > 0 ? new SKColor(ColorToUint(shadowColor)) : SKColor.Empty;
			styleBoldItalic.InnerGlowColor = innerGlowWidth > 0 ? new SKColor(ColorToUint(innerGlowColor)) : SKColor.Empty;
			styleBoldItalic.InnerGlowWidth = innerGlowWidth;
			styleBoldItalic.FontItalic = italic;
			styleBoldItalic.FontWeight = bold ? 700 : 400;
			styleBoldItalic.LetterSpacing = letterSpacing;
			styleBoldItalic.TextDirection = TextDirection.Auto;
			styleBoldItalic.HaloBlur = outlineBlur;
			styleBoldItalic.BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty;
			styleBoldItalic.Underline = underlineStyle;
			styleBoldItalic.LineHeight = lineHeight;
			styleBoldItalic.StrikeThrough = strikeThroughStyle;
			urls.Clear();
			RenderText();
		}
		#endif

		public string LinkPressed() {
				RectTransform rawImageRect = GetComponent<RectTransform>();
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, Input.mousePosition, Camera.main, out var localMousePosition)) {
					float normalizedX = Mathf.InverseLerp(-rawImageRect.rect.width / 2, rawImageRect.rect.width / 2, localMousePosition.x);
					float normalizedY = Mathf.InverseLerp(-rawImageRect.rect.height / 2, rawImageRect.rect.height / 2, localMousePosition.y);
					var caretPos = rs.HitTest(rawImageRect.sizeDelta.x * normalizedX, rawImageRect.sizeDelta.y * normalizedY);
					foreach (var url in urls) {
						if (caretPos.ClosestCodePointIndex >= url.Value.IndexStart && caretPos.ClosestCodePointIndex <=  url.Value.IndexEnd) {
							return url.Value.link;
						}
					}
				}

				return "";
		}

		private void OnDestroy() {
			Dispose();
			if (texture != null) {
				#if !UNITY_EDITOR
				DestroyImmediate(texture);
				#else
				DestroyImmediate(texture);
				#endif
				texture = null;
			}


			if (skTypeface != null) {
				skTypeface.Dispose();
			}
		}
		
		private void OnDisable() {
			Dispose();
			#if !UNITY_EDITOR
				DestroyImmediate(rawImage.texture);
			#else
			DestroyImmediate(rawImage.texture);
			#endif
			
			if (texture != null) {
				#if !UNITY_EDITOR
				DestroyImmediate(texture);
				#else
				DestroyImmediate(texture);
				#endif
			}
		}

		private void Dispose() {
			if (pixmap != null) {
				pixmap.Dispose();
				pixmap = null;
			}
			
			if (surface != null) {
				surface.Dispose();
			}
			
			if (canvas != null) {
				canvas.Dispose();
				canvas = null;
			}
		}
		
		public void CalculateLayoutInputHorizontal() {}

		
		public void CalculateLayoutInputVertical() {}

		
		public float minWidth { get; }
		public float preferredWidth {
			get {
				widthPreferred = true;
				if (rs != null) {
					return currentPreferdWidth;
				}
				return currentWidth;
			}
		}
		public float flexibleWidth { get; }
		public float minHeight { get; }
		private TextBlock temp = new TextBlock();
		public float preferredHeight {
			get {
				heightPreferred = true;
				if (rs != null && textRendered) {
					return currentPreferdHeight;
				}
				styleBoldItalic.FontSize = fontSize;
				styleBoldItalic.FontWeight = bold ? 700 : 400;

				temp.AddText(text,styleBoldItalic);
				if (rectTransform == null) {
					rectTransform = transform as RectTransform;
				}
				var currentPreferdWidth2 = autoFitHorizontal ? temp.MeasuredWidth > maxWidth ? maxWidth : temp.MeasuredWidth + 20 : rectTransform.sizeDelta.x;
				temp.MaxWidth = currentPreferdWidth2;
				temp.MaxHeight = autoFitVertical ? temp.MeasuredHeight : rectTransform.rect.height;

				return temp.MeasuredHeight;
			}
		}
		public float flexibleHeight { get; }
		public int layoutPriority { get; }

		public void RefreshFontFamily() {
			if (font != null) {
				var bytes = font.bytes;
				SKData copy = SKData.CreateCopy(bytes);
				skTypeface = SKTypeface.FromData(copy);
				copy.Dispose();
				if (skTypeface != null) {
					rs.FontMapper = new FontMapper(skTypeface);
				}
			}
		}
	}
}