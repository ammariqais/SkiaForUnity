using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Topten.RichTextKit;
using TextAlignment = Topten.RichTextKit.TextAlignment;

namespace SkiaSharp.Unity.HB {
	public enum HBColorFormat {
		alpha8,
		rgb32
	}

	[AddComponentMenu("Skia UI (Canvas)/HB TextBlock")]
	[RequireComponent(typeof(RawImage))]
	public class HB_TEXTBlock : MonoBehaviour, ILayoutElement {
		[SerializeField]
		[TextArea]
		protected string Text;
		[SerializeField]
		public UnityEngine.Object font;
		[SerializeField]
		protected int fontSize = 20, letterSpacing, maxLines;
		[SerializeField]
		[Range (0,100)]
		protected int outlineWidth, outlineBlur;
		[SerializeField]
		[Range (0,10)]
		protected int shadowWidth;
		[SerializeField]
		[Range (0,1)]
		protected int innerGlowWidth = 0;
		[SerializeField]
		[Range (-100,100)]
		protected float shadowOffsetX, shadowOffsetY = 1;
		[SerializeField]
		protected Color fontColor = Color.black, innerGlowColor = Color.white, backgroundColor = Color.clear,linkColor = Color.blue;
		[SerializeField]
		protected Gradient shadowGradientColor = new(), outlineColor = new ();
		[SerializeField]
		protected bool italic, bold, autoFitVertical = true, autoFitHorizontal, renderLinks, enableGradiant, enableEllipsis = true, richText;
		[SerializeField]
		protected UnderlineStyle underlineStyle;
		[SerializeField]
		protected StrikeThroughStyle strikeThroughStyle;
		[SerializeField]
		protected float lineHeight = 1.0f, maxWidth = 264, maxHeight = -1 , gradiantAngle = 90;
		[SerializeField]
		protected HBColorFormat colorType = HBColorFormat.rgb32;
		[SerializeField]
		protected TextAlignment textAlignment = TextAlignment.Center;
		[SerializeField]
		protected Color[] gradiantColors;
		[SerializeField]
		protected float[] gradiantPositions;
		[SerializeField]
		protected TextDirection textDirection = TextDirection.Auto;
		[SerializeField]
		[Range(100, 900)]
		protected int fontWeight = 400;
		[SerializeField]
		protected FontVariant fontVariant = FontVariant.Normal;
		[SerializeField]
		protected Vector4 padding; // left, top, right, bottom
		[SerializeField]
		protected HBFontData[] fallbackFonts;
		[SerializeField]
		[Range(0, 50)]
		protected float paragraphSpacing;

		public UnityEvent onTextChanged = new();
		public UnityEvent<string> onLinkClicked = new();

		protected SKCanvas canvas;
		protected SKImageInfo info = new();
		protected SKSurface surface;
		protected SKPixmap pixmap;
		protected RawImage rawImage;
		protected Texture2D texture;
		protected TextBlock rs;
		protected Dictionary<int, HBLinks> urls = new();
		protected Style styleBoldItalic = new();
		protected string pattern = @"(https?://\S+|www\.\S+)";
		protected Regex regex;
		protected SKTypeface skTypeface;
		private static readonly Dictionary<int, SKTypeface> _typefaceCache = new();
		private static readonly Dictionary<int, int> _typefaceRefCounts = new();
		private int _typefaceCacheKey;
		private bool _typefaceCacheRegistered;
		protected RectTransform rectTransform;
		protected float currentWidth, currentHeight, currentPreferdWidth = 0, currentPreferdHeight;
		protected TextGradient blockGradient;
		protected bool widthPreferred, heightPreferred;

		// Cached objects to avoid per-render allocations
		private FontMapper _cachedFontMapper;
		private int _cachedFontMapperKey;
		private Style _linkStyle;
		private static readonly Gradient _emptyGradient = new();

		public virtual TextBlock Info => rs;

		protected enum VerticalAlignment { Top, Middle, Bottom }
		[SerializeField]
		protected VerticalAlignment verticalAlignment = VerticalAlignment.Top; // Default alignment


		public virtual float MaxWidth {
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

		public virtual float MaxHeight {
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

		public virtual string text {
			get => Text;
			set {
				Text = value;
				ReUpdate();
				onTextChanged?.Invoke();
			}
		}

		public virtual void SetRichText(string value) {
			richText = true;
			Text = value;
			ReUpdate();
			onTextChanged?.Invoke();
		}

		public virtual Color FontColor {
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

		public float alpha {
			get {
				if (rawImage == null) rawImage = GetComponent<RawImage>();
				return rawImage != null ? rawImage.color.a : 1f;
			}
			set {
				if (rawImage == null) rawImage = GetComponent<RawImage>();
				if (rawImage != null) {
					Color c = rawImage.color;
					c.a = value;
					rawImage.color = c;
				}
			}
		}

		public virtual Gradient HaloColor {
			get {
				return outlineColor;
			}
			set {
				outlineColor = value;
				ReUpdate();
			}
		}

		public virtual Gradient ShadowGradientColor {
			get => shadowGradientColor;
			set {
				shadowGradientColor = value;
				ReUpdate();
			}
		}

		public virtual Color InnerGlowColor {
			get {
				return innerGlowColor;
			}
			set {
				innerGlowColor = value;
				ReUpdate();
			}
		}

		public virtual Color BackgroundColor {
			get {
				return backgroundColor;
			}
			set {
				backgroundColor = value;
				ReUpdate();
			}
		}

		public virtual HBColorFormat ColorType {
			get {
				return colorType;
			}
			set {
				colorType = value;
				ReUpdate();
			}
		}

		public virtual bool AutoFitVertical {
			get {
				return autoFitVertical;
			}
			set {
				autoFitVertical = value;
				ReUpdate();
			}
		}

		public virtual bool AutoFitHorizontal {
			get {
				return autoFitHorizontal;
			}
			set {
				autoFitHorizontal = value;
				ReUpdate();
			}
		}

		public virtual bool IsGradiantEnabled {
			get {
				return enableGradiant;
			}
			set {
				enableGradiant = value;
				ReUpdate();
			}
		}
		public virtual bool RenderLinks {
			get {
				return renderLinks;
			}
			set {
				renderLinks = value;
				ReUpdate();
			}
		}

		public virtual UnityEngine.Object Font {
			get {
				return font;
			}
			set {
				font = value;
				ReUpdate();
			}
		}

		protected byte[] GetFontBytes() {
			if (font is HBFontData hb) return hb.fontBytes;
			if (font is TextAsset ta) return ta.bytes;
			return null;
		}

		public virtual bool Bold {
			get {
				return bold;
			}
			set {
				bold = value;
				ReUpdate();
			}
		}

		public virtual bool Italic {
			get {
				return italic;
			}
			set {
				italic = value;
				ReUpdate();
			}
		}

		public virtual int FontSize {
			get {
				return fontSize;
			}
			set {
				fontSize = value;
				ReUpdate();
			}
		}

		public virtual int HaloWidth {
			get {
				return outlineWidth;
			}
			set {
				outlineWidth = value;
				ReUpdate();
			}
		}

		public virtual int ShadowWidth {
			get {
				return shadowWidth;
			}
			set {
				shadowWidth = value;
				ReUpdate();
			}
		}

		public virtual int InnerGlowWidth {
			get {
				return innerGlowWidth;
			}
			set {
				innerGlowWidth = value;
				ReUpdate();
			}
		}

		public virtual int LetterSpacing {
			get {
				return letterSpacing;
			}
			set {
				letterSpacing = value;
				ReUpdate();
			}
		}

		public virtual int HaloBlur {
			get {
				return outlineBlur;
			}
			set {
				outlineBlur = value;
				ReUpdate();
			}
		}

		public virtual UnderlineStyle UnderLineStyle {
			get {
				return underlineStyle;
			}
			set {
				underlineStyle = value;
				ReUpdate();
			}
		}

		public virtual StrikeThroughStyle  StrikeThroughStyle{
			get {
				return strikeThroughStyle;
			}
			set {
				strikeThroughStyle = value;
				ReUpdate();
			}
		}

		public virtual float LineHeight{
			get {
				return lineHeight;
			}
			set {
				lineHeight = value;
				ReUpdate();
			}
		}

		public virtual TextAlignment TextAlignment{
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

		protected virtual void Awake() {
			rawImage = GetComponent<RawImage>();
			if (rawImage) {
				rawImage.enabled = false;
				#if UNITY_EDITOR
				rawImage.hideFlags |= HideFlags.HideInInspector;
				#endif
			}
			rectTransform = transform as RectTransform;

			styleBoldItalic.FontSize = fontSize;
			styleBoldItalic.TextColor = new SKColor(ColorToUint(fontColor));
			styleBoldItalic.HaloWidth = outlineWidth;
			styleBoldItalic.ShadowWidth = shadowWidth;
			styleBoldItalic.HaloColor = outlineWidth > 0 ? outlineColor : _emptyGradient;
			styleBoldItalic.ShadowGradientColor = shadowGradientColor != null && shadowGradientColor.colorKeys.Length > 0
				? shadowGradientColor
				: _emptyGradient;
			styleBoldItalic.ShadowOffsetX = shadowOffsetX;
			styleBoldItalic.ShadowOffsetY = shadowOffsetY;
			styleBoldItalic.InnerGlowWidth = innerGlowWidth;
			styleBoldItalic.InnerGlowColor = innerGlowWidth > 0 ? new SKColor(ColorToUint(innerGlowColor)) : SKColor.Empty;

			styleBoldItalic.FontItalic = italic;
			styleBoldItalic.FontWeight = bold ? 700 : fontWeight;
			styleBoldItalic.LetterSpacing = letterSpacing;
			styleBoldItalic.TextDirection = textDirection;
			styleBoldItalic.HaloBlur = outlineBlur;
			styleBoldItalic.BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty;
			styleBoldItalic.Underline = underlineStyle;
			styleBoldItalic.LineHeight = lineHeight;
			styleBoldItalic.StrikeThrough = strikeThroughStyle;
			styleBoldItalic.FontVariant = fontVariant;
		}

		protected virtual void OnEnable() {
			if (String.IsNullOrEmpty(Text)){
				return;
			}

			if (rawImage) {
				urls.Clear();
				RenderText();
			}
		}

		protected bool textRendered;


		// Convert a Color to a uint
		public virtual uint ColorToUint(Color color){
			uint alpha = (uint)(color.a * 255);
			uint red = (uint)(color.r * 255);
			uint green = (uint)(color.g * 255);
			uint blue = (uint)(color.b * 255);
			return (alpha << 24) | (red << 16) | (green << 8) | blue;
		}

		protected virtual void RenderText() {
			if (rawImage && String.IsNullOrEmpty(text)) {
				rawImage.enabled = false;
			}

			// Dispose only the pixmap from previous render; keep surface for reuse
			if (pixmap != null) {
				pixmap.Dispose();
				pixmap = null;
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
			rs.BaseDirection = textDirection;
			rs.EllipsisEnabled = enableEllipsis;
			if (paragraphSpacing > 0 && Text.Contains("\n")) {
				string[] paragraphs = Text.Split('\n');
				var spacerStyle = styleBoldItalic.Modify(fontSize: paragraphSpacing, lineHeight: 1f);
				for (int i = 0; i < paragraphs.Length; i++) {
					if (i > 0) {
						rs.AddText("\n", styleBoldItalic);
						rs.AddText("\n", spacerStyle);
					}
					if (string.IsNullOrEmpty(paragraphs[i])) continue;
					if (richText) {
						HBRichTextParser.Parse(rs, paragraphs[i], styleBoldItalic);
					} else {
						rs.AddText(paragraphs[i], styleBoldItalic);
					}
				}
			} else if (richText) {
				HBRichTextParser.Parse(rs, Text, styleBoldItalic);
			} else {
				rs.AddText(Text, styleBoldItalic);
			}

			if (renderLinks) {
				RenderLinksCall();
			}

			if (font != null) {
				var fontBytes = GetFontBytes();
				if (fontBytes == null) return;
				if (skTypeface == null) {
					int key = font.GetInstanceID();
					if (!_typefaceCache.ContainsKey(key)) {
						SKData copy = SKData.CreateCopy(fontBytes);
						_typefaceCache[key] = SKTypeface.FromData(copy);
						copy.Dispose();
						_typefaceRefCounts[key] = 0;
					}
					skTypeface = _typefaceCache[key];
					_typefaceCacheKey = key;
					_typefaceRefCounts[key]++;
					_typefaceCacheRegistered = true;
				}
				// Reuse FontMapper if typeface hasn't changed
				int typefaceKey = font.GetInstanceID();
				if (_cachedFontMapper == null || _cachedFontMapperKey != typefaceKey) {
					_cachedFontMapper = new FontMapper(skTypeface);
					_cachedFontMapperKey = typefaceKey;
				}
				rs.FontMapper = _cachedFontMapper;
			} else {
				// Reset to default FontMapper so system fonts respect weight/italic
				rs.FontMapper = null;
			}

			if ( (!autoFitHorizontal && rectTransform.rect.width == 0)) {
				return;
			}

			// Calculate extra padding needed for effects (outline, blur, shadow) + user padding
			float effectPadX = 0f;
			float effectPadY = 0f;
			if (outlineWidth > 0) {
				float outlineExtra = outlineWidth + outlineBlur;
				effectPadX = Mathf.Max(effectPadX, outlineExtra);
				effectPadY = Mathf.Max(effectPadY, outlineExtra);
			}
			if (shadowWidth > 0) {
				effectPadX = Mathf.Max(effectPadX, shadowWidth + Mathf.Abs(shadowOffsetX));
				effectPadY = Mathf.Max(effectPadY, shadowWidth + Mathf.Abs(shadowOffsetY));
			}
			// Add user-defined padding (left, top, right, bottom)
			float userPadL = padding.x;
			float userPadT = padding.y;
			float userPadR = padding.z;
			float userPadB = padding.w;
			effectPadX += (userPadL + userPadR) / 2f;
			effectPadY += (userPadT + userPadB) / 2f;
			float padW = effectPadX * 2f;
			float padH = effectPadY * 2f;

			// --- Width ---
			if (autoFitHorizontal) {
				// Measure unconstrained (MaxWidth is null from line 483)
				float naturalW = rs.MeasuredWidth;
				float neededW = naturalW + padW;
				if (maxWidth > -1 && neededW > maxWidth) {
					// Text exceeds user's max limit — constrain
					currentPreferdWidth = maxWidth;
					rs.MaxWidth = Mathf.Max(0, maxWidth - padW);
				} else {
					// Text fits — size rect to content, add small buffer to MaxWidth
					// to prevent floating-point precision issues from truncating
					// single-word text (no break points) to 0 lines.
					currentPreferdWidth = neededW;
					rs.MaxWidth = Mathf.Ceil(naturalW) + 2;
				}
			} else {
				currentPreferdWidth = rectTransform.rect.width;
				rs.MaxWidth = Mathf.Max(0, currentPreferdWidth - padW);
			}

			// --- Height ---
			// Capture measured height BEFORE setting MaxHeight (which can trigger truncation)
			float measuredH = rs.MeasuredHeight;
			if (autoFitVertical) {
				float neededH = measuredH + padH;
				if (maxHeight > -1 && neededH > maxHeight) {
					currentPreferdHeight = maxHeight;
				} else {
					currentPreferdHeight = neededH;
				}
			} else {
				currentPreferdHeight = rectTransform.rect.height;
			}

			if (autoFitVertical && maxHeight <= -1) {
				rs.MaxHeight = null;
			} else if (autoFitVertical) {
				if (maxHeight > -1 && measuredH + padH > maxHeight) {
					rs.MaxHeight = Mathf.Max(0, maxHeight - padH);
				} else {
					// Add buffer to prevent exact-match truncation
					rs.MaxHeight = Mathf.Ceil(measuredH) + 2;
				}
			} else {
				rs.MaxHeight = Mathf.Max(0, currentPreferdHeight - padH);
			}

			if (autoFitVertical) {
				float fitHeight = measuredH + padH;
				var size = autoFitHorizontal
					? new Vector2(currentPreferdWidth, fitHeight)
					: new Vector2(rectTransform.rect.width, fitHeight);

				if (size.x == 0) return;

				// Only set sizeDelta if the RectTransform is not in stretch mode for that axis
				if (rectTransform.anchorMin.x == rectTransform.anchorMax.x) {
					rectTransform.sizeDelta = new Vector2(size.x, rectTransform.sizeDelta.y);
				}
				if (rectTransform.anchorMin.y == rectTransform.anchorMax.y) {
					rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, size.y);
				}
			} else {
				var size = autoFitHorizontal
					? new Vector2(currentPreferdWidth, rectTransform.rect.height)
					: new Vector2(rectTransform.rect.width, rectTransform.rect.height);

				if (rectTransform.anchorMin.x == rectTransform.anchorMax.x) {
					rectTransform.sizeDelta = new Vector2(size.x, rectTransform.sizeDelta.y);
				}
				if (rectTransform.anchorMin.y == rectTransform.anchorMax.y) {
					rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, size.y);
				}
			}


			currentWidth = rectTransform.rect.width;
			currentHeight = rectTransform.rect.height;
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

			if (currentWidth == 0 || currentHeight == 0) {
				return;
			}

			int roundedWidth = Mathf.CeilToInt(rectTransform.rect.width / 4) * 4;
			int roundedHeight = Mathf.CeilToInt(rectTransform.rect.height / 4) * 4;

			SKColorType targetColorType = colorType == HBColorFormat.alpha8 ? SKColorType.Alpha8 : SKColorType.Rgba8888;

			// Reuse surface if size and format match, otherwise recreate
			if (surface != null && (info.Width != roundedWidth || info.Height != roundedHeight || info.ColorType != targetColorType)) {
				surface.Dispose();
				surface = null;
				canvas = null;
			}

			if (info.IsEmpty) {
				info = new SKImageInfo(roundedWidth, roundedHeight);
			} else {
				info.Width = roundedWidth;
				info.Height = roundedHeight;
			}
			info.ColorType = targetColorType;

			if (surface == null) {
				surface = SKSurface.Create(info);
			}
			canvas = surface.Canvas;
			canvas.Clear();
			canvas.ResetMatrix();
			canvas.Scale(1, -1);

			// Calculate vertical offset based on alignment
			float baseEffectPadY = effectPadY - (userPadT + userPadB) / 2f;
			float baseEffectPadX = effectPadX - (userPadL + userPadR) / 2f;
			float translateX = baseEffectPadX + userPadL;
			float verticalOffset;
			if (verticalAlignment == VerticalAlignment.Middle) {
				verticalOffset = (-info.Height + (currentHeight - rs.MeasuredHeight) / 2);
			} else if (verticalAlignment == VerticalAlignment.Bottom) {
				verticalOffset = -info.Height + (currentHeight - rs.MeasuredHeight) - userPadB;
			} else {
				verticalOffset = -info.Height + baseEffectPadY + userPadT;
			}
			canvas.Translate(translateX, verticalOffset);


			TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : info.ColorType == SKColorType.Alpha8 ? TextureFormat.Alpha8 : TextureFormat.RGBA32;
			if (texture == null) {
				texture = new Texture2D(roundedWidth, roundedHeight, format, false);
				texture.name = "HB_Text";
				texture.wrapMode = TextureWrapMode.Repeat;
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

			pixmap = surface.PeekPixels();
			texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
			texture.Apply();
			rawImage.texture = texture;
			if (!rawImage.enabled) {
				rawImage.enabled = true;
			}

			// Dispose pixmap (it just references the surface's memory)
			pixmap.Dispose();
			pixmap = null;

			textRendered = true;
		}

		protected virtual void RenderLinksCall() {
			if (_linkStyle == null) {
				_linkStyle = new Style();
			}
			_linkStyle.FontSize = fontSize;
			_linkStyle.TextColor = new SKColor(ColorToUint(linkColor));
			_linkStyle.Underline = UnderlineStyle.Solid;
			_linkStyle.HaloWidth = outlineWidth;
			_linkStyle.ShadowWidth = shadowWidth;
			_linkStyle.HaloColor = outlineWidth > 0 ? outlineColor : _emptyGradient;
			_linkStyle.ShadowGradientColor = shadowGradientColor != null && shadowGradientColor.colorKeys.Length > 0
				? shadowGradientColor
				: _emptyGradient;
			_linkStyle.InnerGlowColor = innerGlowWidth > 0 ? new SKColor(ColorToUint(innerGlowColor)) : SKColor.Empty;
			_linkStyle.InnerGlowWidth = innerGlowWidth;
			_linkStyle.FontItalic = italic;
			_linkStyle.FontWeight = bold ? 700 : 400;
			_linkStyle.LetterSpacing = letterSpacing;
			_linkStyle.TextDirection = TextDirection.Auto;
			_linkStyle.HaloBlur = outlineBlur;
			_linkStyle.BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty;
			_linkStyle.LineHeight = lineHeight;
			_linkStyle.StrikeThrough = strikeThroughStyle;

			if (regex == null) {
				regex = new Regex(pattern, RegexOptions.Compiled);
			}

			MatchCollection matches = regex.Matches(Text);
				foreach (Match match in matches) {
					var length = match.Index + match.Length;
					int differnce = 0;
					if (length > rs.Length) {
						differnce = length - rs.Length;
						length = length - differnce;
					}

					rs.ApplyStyle(Mathf.Clamp(match.Index,0,rs.Length), Mathf.Clamp(match.Length - differnce,0,match.Length),_linkStyle);
					urls.Add(match.Index,new HBLinks() {
						IndexStart = match.Index,
						IndexEnd = length,
						Length = match.Length,
						link = match.Value
					});
				}
		}

		protected virtual void FixedUpdate() {
			if (currentWidth != rectTransform.rect.width || currentHeight != currentPreferdHeight) {
				urls.Clear();
				RenderText();
			}
		}

		public virtual void ReUpdate() {
			if (rawImage == null) {
				rawImage = GetComponent<RawImage>();
				rectTransform = transform as RectTransform;
			}
			SyncStyleFromFields();
			urls.Clear();
			RenderText();
		}

		public virtual float GradientAngle {
			get => gradiantAngle;
			set {
				if (gradiantAngle != value) {
					gradiantAngle = value;
					ReUpdate();
				}
			}
		}

		/// <summary>
		/// Set multiple properties at once and render a single frame.
		/// Used by HBTextAnimator to avoid multiple re-renders per frame.
		/// </summary>
		public virtual void ApplyAnimatedValues(
			string displayText = null,
			Color? fontColor = null,
			int? haloWidth = null,
			float? shadowOffX = null,
			float? shadowOffY = null,
			int? fontSize = null,
			int? letterSpacing = null,
			float? gradientAngle = null,
			float? maxWidth = null,
			float? maxHeight = null) {
			if (displayText != null) Text = displayText;
			if (fontColor.HasValue) this.fontColor = fontColor.Value;
			if (haloWidth.HasValue) outlineWidth = haloWidth.Value;
			if (shadowOffX.HasValue) shadowOffsetX = shadowOffX.Value;
			if (shadowOffY.HasValue) shadowOffsetY = shadowOffY.Value;
			if (fontSize.HasValue) this.fontSize = fontSize.Value;
			if (letterSpacing.HasValue) this.letterSpacing = letterSpacing.Value;
			if (gradientAngle.HasValue) gradiantAngle = gradientAngle.Value;
			if (maxWidth.HasValue) this.maxWidth = maxWidth.Value;
			if (maxHeight.HasValue) this.maxHeight = maxHeight.Value;

			if (rawImage == null) {
				rawImage = GetComponent<RawImage>();
				rectTransform = transform as RectTransform;
			}
			SyncStyleFromFields();
			urls.Clear();
			RenderText();
		}

		protected void SyncStyleFromFields() {
			styleBoldItalic.FontSize = fontSize;
			styleBoldItalic.TextColor = new SKColor(ColorToUint(fontColor));
			styleBoldItalic.HaloWidth = outlineWidth;
			styleBoldItalic.ShadowWidth = shadowWidth;
			styleBoldItalic.ShadowOffsetX = shadowOffsetX;
			styleBoldItalic.ShadowOffsetY = shadowOffsetY;
			styleBoldItalic.HaloColor = outlineWidth > 0 ? outlineColor : _emptyGradient;
			styleBoldItalic.ShadowGradientColor = shadowGradientColor != null && shadowGradientColor.colorKeys.Length > 0
				? shadowGradientColor
				: _emptyGradient;
			styleBoldItalic.InnerGlowColor = innerGlowWidth > 0 ? new SKColor(ColorToUint(innerGlowColor)) : SKColor.Empty;
			styleBoldItalic.InnerGlowWidth = innerGlowWidth;
			styleBoldItalic.FontItalic = italic;
			styleBoldItalic.FontWeight = bold ? 700 : fontWeight;
			styleBoldItalic.LetterSpacing = letterSpacing;
			styleBoldItalic.TextDirection = textDirection;
			styleBoldItalic.HaloBlur = outlineBlur;
			styleBoldItalic.BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty;
			styleBoldItalic.Underline = underlineStyle;
			styleBoldItalic.LineHeight = lineHeight;
			styleBoldItalic.StrikeThrough = strikeThroughStyle;
			styleBoldItalic.FontVariant = fontVariant;
		}

		// --- Reset ---
		[ContextMenu("Reset to Defaults")]
		private void ResetToDefaults() {
			#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(this, "Reset HB_TEXTBlock");
			#endif
			Text = "";
			fontSize = 20;
			letterSpacing = 0;
			maxLines = 0;
			outlineWidth = 0;
			outlineBlur = 0;
			shadowWidth = 0;
			innerGlowWidth = 0;
			shadowOffsetX = 0;
			shadowOffsetY = 1;
			fontColor = Color.black;
			innerGlowColor = Color.white;
			backgroundColor = Color.clear;
			linkColor = Color.blue;
			italic = false;
			bold = false;
			autoFitVertical = true;
			autoFitHorizontal = false;
			renderLinks = false;
			enableGradiant = false;
			enableEllipsis = true;
			richText = false;
			underlineStyle = default;
			strikeThroughStyle = default;
			lineHeight = 1.0f;
			maxHeight = -1;
			gradiantAngle = 90;
			textAlignment = TextAlignment.Center;
			verticalAlignment = VerticalAlignment.Top;
			textDirection = TextDirection.Auto;
			fontWeight = 400;
			fontVariant = FontVariant.Normal;
			padding = Vector4.zero;
			paragraphSpacing = 0;
			fallbackFonts = null;
			gradiantColors = null;
			gradiantPositions = null;
			ReUpdate();
		}

		#if UNITY_EDITOR
		public virtual void ReUpdateEditMode() {
			if (rawImage == null) {
				rawImage = GetComponent<RawImage>();
				rectTransform = transform as RectTransform;
			}
			SyncStyleFromFields();
			urls.Clear();
			RenderText();
		}
		#endif

		public virtual string LinkPressed() {
				RectTransform rawImageRect = GetComponent<RectTransform>();
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, Input.mousePosition, Camera.main, out var localMousePosition)) {
					float normalizedX = Mathf.InverseLerp(-rawImageRect.rect.width / 2, rawImageRect.rect.width / 2, localMousePosition.x);
					float normalizedY = Mathf.InverseLerp(-rawImageRect.rect.height / 2, rawImageRect.rect.height / 2, localMousePosition.y);
					var caretPos = rs.HitTest(rawImageRect.sizeDelta.x * normalizedX, rawImageRect.sizeDelta.y * normalizedY);
					foreach (var url in urls) {
						if (caretPos.ClosestCodePointIndex >= url.Value.IndexStart && caretPos.ClosestCodePointIndex <=  url.Value.IndexEnd) {
							onLinkClicked?.Invoke(url.Value.link);
							return url.Value.link;
						}
					}
				}

				return "";
		}

		protected virtual void OnDestroy() {
			Dispose();
			if (texture != null) {
				DestroyImmediate(texture);
				texture = null;
			}

			if (rs != null) {
				rs.Clear();
				rs = null;
			}

			if (_typefaceCacheRegistered && _typefaceRefCounts.ContainsKey(_typefaceCacheKey)) {
				_typefaceRefCounts[_typefaceCacheKey]--;
				if (_typefaceRefCounts[_typefaceCacheKey] <= 0) {
					if (_typefaceCache.TryGetValue(_typefaceCacheKey, out var cachedTypeface)) {
						cachedTypeface?.Dispose();
						_typefaceCache.Remove(_typefaceCacheKey);
					}
					_typefaceRefCounts.Remove(_typefaceCacheKey);
				}
			}
			skTypeface = null;
			_cachedFontMapper = null;
		}

		protected virtual void OnDisable() {
			Dispose();
			if (rawImage != null) {
				DestroyImmediate(rawImage.texture);
			}

			if (texture != null) {
				DestroyImmediate(texture);
				texture = null;
			}
		}

		protected virtual void Dispose() {
			if (pixmap != null) {
				pixmap.Dispose();
				pixmap = null;
			}

			if (surface != null) {
				surface.Dispose();
				surface = null;
			}
			// canvas is owned by surface — already disposed above
			canvas = null;
		}

		public virtual void CalculateLayoutInputHorizontal() {}


		public virtual void CalculateLayoutInputVertical() {}


		public virtual float minWidth { get; }
		public virtual float preferredWidth {
			get {
				widthPreferred = true;
				if (rs != null) {
					return currentPreferdWidth;
				}
				return currentWidth;
			}
		}
		public virtual float flexibleWidth { get; }
		public virtual float minHeight { get; }
		protected TextBlock temp = new TextBlock();

		public virtual float preferredHeight {
			get {
				heightPreferred = true;
				if (rs != null && textRendered) {
					return currentPreferdHeight;
				}
				temp.Clear();
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
		public virtual float flexibleHeight { get; }
		public virtual int layoutPriority { get; }

		public virtual void RefreshFontFamily() {
			if (font != null) {
				int key = font.GetInstanceID();
				if (!_typefaceCache.ContainsKey(key)) {
					var fontBytes = GetFontBytes();
					if (fontBytes == null) return;
					SKData copy = SKData.CreateCopy(fontBytes);
					_typefaceCache[key] = SKTypeface.FromData(copy);
					copy.Dispose();
					_typefaceRefCounts[key] = 0;
				}
				skTypeface = _typefaceCache[key];
				_cachedFontMapper = new FontMapper(skTypeface);
				_cachedFontMapperKey = key;
				if (rs != null) {
					rs.FontMapper = _cachedFontMapper;
				}
			}
		}
	}
}
