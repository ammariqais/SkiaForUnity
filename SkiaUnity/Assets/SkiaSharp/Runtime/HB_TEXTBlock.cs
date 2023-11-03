using System;
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
		private int fontSize = 12, haloWidth, letterSpacing, haloBlur;
		[SerializeField]
		private Color fontColor = Color.black, haloColor = Color.black, backgroundColor = Color.clear;
		[SerializeField]
		private bool italic, bold, autoFitVertical = true, autoFitHorizontal, renderLinks;
		[SerializeField]
		private UnderlineStyle underlineStyle;
		[SerializeField]
		private StrikeThroughStyle strikeThroughStyle;
		[SerializeField]
		private float lineHeight = 1.0f, maxWidth = 264;
		[SerializeField]
		private HBColorFormat colorType = HBColorFormat.alpha8; 
		[SerializeField] 
		private TextAlignment textAlignment = TextAlignment.Left;
        
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
		private float currentWidth, currentHeight, currentPreferdWidth = 0;
		private static bool lowMemoryListen = false;

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
				return haloColor;
			}
			set {
				haloColor = value;
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
				return haloWidth;
			}
			set {
				haloWidth = value;
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
				return haloBlur;
			}
			set {
				haloBlur = value;
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
			rectTransform = transform as RectTransform;
			if (String.IsNullOrEmpty(Text)){
				return;
			}
			
			styleBoldItalic.FontFamily = "Segoe UI";
			styleBoldItalic.FontSize = fontSize;
			styleBoldItalic.TextColor = new SKColor(ColorToUint(fontColor));
			styleBoldItalic.HaloWidth = haloWidth;
			styleBoldItalic.HaloColor = haloWidth > 0 ? new SKColor(ColorToUint(haloColor)) : SKColor.Empty;
			styleBoldItalic.FontItalic = italic;
			styleBoldItalic.FontWeight = bold ? 700 : 400;
			styleBoldItalic.LetterSpacing = letterSpacing;
			styleBoldItalic.TextDirection = TextDirection.Auto;
			styleBoldItalic.HaloBlur = haloBlur;
			styleBoldItalic.BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty;
			styleBoldItalic.Underline = underlineStyle;
			styleBoldItalic.LineHeight = lineHeight;
			styleBoldItalic.StrikeThrough = strikeThroughStyle;
			if (lowMemoryListen == false) {
				Application.lowMemory += OnLoadMemory;
				lowMemoryListen = true;
			}

			
			if (rawImage) {
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
			Dispose();
			if (texture != null) {
				#if !UNITY_EDITOR
				Destroy(texture);
				#else
				DestroyImmediate(texture);
    #endif
				texture = null;
			}
			if (rs != null) {
				rs.Clear();
			}
			rs = new TextBlock();
			if (colorType == HBColorFormat.alpha8) {
				rawImage.color = fontColor;
			} else {
				rawImage.color = Color.white;
			}
			rs.Alignment = textAlignment;
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

			currentPreferdWidth = autoFitHorizontal ? rs.MeasuredWidth > maxWidth ? maxWidth : rs.MeasuredWidth + 20 : rectTransform.sizeDelta.x;
			rs.MaxWidth = currentPreferdWidth;
			rs.MaxHeight = autoFitVertical ? rs.MeasuredHeight : rectTransform.rect.height;

			if (autoFitVertical) {
				rectTransform.sizeDelta = autoFitHorizontal ? new Vector2(currentPreferdWidth, rs.MeasuredHeight ) : new Vector2(rectTransform.sizeDelta.x, rs.MeasuredHeight );
			}

			
			currentWidth = rectTransform.rect.width;
			currentHeight = rectTransform.rect.height;
			if (currentWidth == 0 || currentHeight == 0) {
				return;
			}

			if (info.IsEmpty) {
				info = new SKImageInfo((int)rectTransform.rect.width, (int)rectTransform.rect.height);
			} else {
				info.Width = (int)rectTransform.rect.width;
				info.Height = (int)rectTransform.rect.height;
			}
			
			surface = SKSurface.Create(info);
			canvas = surface.Canvas;
			TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
			texture = new Texture2D(info.Width, info.Height, format, false);
			rs.Paint(canvas);
			texture.hideFlags = HideFlags.HideAndDontSave;
			texture.name = "HB_Text";
			texture.wrapMode = TextureWrapMode.Repeat;
			pixmap = surface.PeekPixels();
			texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
			texture.Apply();
			rawImage.texture = texture;
			Dispose();
			textRendered = true;
		}

		private void RenderLinksCall() {
			Style styleLink = new Style() {
			FontFamily = "Arial",
			FontSize = fontSize,
			TextColor = SKColors.Blue,
			Underline = UnderlineStyle.Solid,
			HaloWidth = haloWidth,
			HaloColor = haloWidth > 0 ? new SKColor(ColorToUint(haloColor)) : SKColor.Empty,
			FontItalic = italic,
			FontWeight = bold ? 700 : 400,
			LetterSpacing = letterSpacing,
			TextDirection = TextDirection.Auto,
			HaloBlur = haloBlur,
			BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty,
			LineHeight = lineHeight,
			StrikeThrough = strikeThroughStyle,
		};
			
			string pattern = @"(https?://\S+|www\.\S+)";
			if (regex == null) {
				regex = new Regex(pattern);
			}
			
			MatchCollection matches = regex.Matches(Text);
				foreach (Match match in matches){
					var length = match.Index + match.Length;
					rs.ApplyStyle(match.Index,match.Length,styleLink);
					urls.Add(match.Index,new HBLinks() {
						IndexStart = match.Index,
						IndexEnd = length,
						Length = match.Length
					});
				}
		}
        
		private void FixedUpdate() {
			if (currentWidth != rectTransform.rect.width || rectTransform.rect.height != currentHeight) {
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
			styleBoldItalic.FontFamily = "Segoe UI";
			styleBoldItalic.FontSize = fontSize;
			styleBoldItalic.TextColor = new SKColor(ColorToUint(fontColor));
			styleBoldItalic.HaloWidth = haloWidth;
			styleBoldItalic.HaloColor = haloWidth > 0 ? new SKColor(ColorToUint(haloColor)) : SKColor.Empty;
			styleBoldItalic.FontItalic = italic;
			styleBoldItalic.FontWeight = bold ? 700 : 400;
			styleBoldItalic.LetterSpacing = letterSpacing;
			styleBoldItalic.TextDirection = TextDirection.Auto;
			styleBoldItalic.HaloBlur = haloBlur;
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
							return rs.Copy(url.Key, url.Value.Length).ToString();
						}
					}
				}

				return "";
		}

		private void OnDestroy() {
			Dispose();
			if (texture != null) {
				#if !UNITY_EDITOR
				Destroy(texture);
				#else
				DestroyImmediate(texture);
				#endif
			}

			if (skTypeface != null) {
				skTypeface.Dispose();
			}
			
			if (lowMemoryListen) {
				Application.lowMemory -= OnLoadMemory;
				lowMemoryListen = false;
			}
		}
		
		private void OnDisable() {
			Dispose();
			if (texture != null) {
				#if !UNITY_EDITOR
				Destroy(texture);
				#else
				DestroyImmediate(texture);
				#endif
			}

			if (skTypeface != null) {
				skTypeface.Dispose();
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

		static void OnLoadMemory() {
			Resources.UnloadUnusedAssets();
		}

		
		public void CalculateLayoutInputHorizontal() {}

		
		public void CalculateLayoutInputVertical() {}

		
		public float minWidth { get; }
		public float preferredWidth {
			get {
				if (rs != null) {
					return rs.MeasuredWidth;
				}
				return currentWidth;
			}
		}
		public float flexibleWidth { get; }
		public float minHeight { get; }
		private TextBlock temp = new TextBlock();
		public float preferredHeight {
			get {
				if (rs != null && textRendered) {
					return rs.MeasuredHeight;
				}
                
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
	}
}