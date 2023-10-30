using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Topten.RichTextKit;
using TextAlignment = Topten.RichTextKit.TextAlignment;

namespace SkiaSharp.Unity.HB {
	public class HB_TEXTBlock : MonoBehaviour, ILayoutElement {
		[SerializeField]
		[TextArea]
		private string Text;
		[SerializeField]
		public TextAsset font;

		private SKCanvas canvas;
		private SKImageInfo info;
		private SKSurface surface;
		private SKPixmap pixmap;
		private RawImage rawImage;
		private Texture2D texture;
		private TextBlock rs;
		private Dictionary<int, HBLinks> urls = new Dictionary<int, HBLinks>();
		SKTypeface skTypeface;
		RectTransform rectTransform;
		public TextAlignment textAlignment = TextAlignment.Left;

		private float currentWidth, currentHeight;

		public string text {
			get => Text;
			set {
				Text = value;
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
		public int fontSize = 12;
		public Color fontColor = Color.black;
		public bool italic = false;
		public bool bold = false;
		public Color haloColor = Color.black;
		public int haloWidth, letterSpacing;
		public bool autoFitVertical = true;
		public bool renderLinks;
		public int haloBlur;
		public Color backgroundColor = Color.clear;
		public UnderlineStyle underlineStyle;
		public StrikeThroughStyle strikeThroughStyle;
		public float lineHeight = 1.0f;

		void Awake() {
			rawImage = GetComponent<RawImage>();
			rectTransform = transform as RectTransform;
			if (String.IsNullOrEmpty(Text)){
				return;
			}
			
			if (rawImage) {
				RenderText();
			}
		}
		
		// Convert a Color to a uint
		public uint ColorToUint(Color color)
		{
			uint alpha = (uint)(color.a * 255);
			uint red = (uint)(color.r * 255);
			uint green = (uint)(color.g * 255);
			uint blue = (uint)(color.b * 255);

			return (alpha << 24) | (red << 16) | (green << 8) | blue;
		}

		private void RenderText() {
			Style styleBoldItalic = new Style() {
				FontFamily = "Segoe UI",
				FontSize = fontSize,
				TextColor = new SKColor(ColorToUint(fontColor)),
				HaloWidth = haloWidth,
				HaloColor = haloWidth > 0 ? new SKColor(ColorToUint(haloColor)) : SKColor.Empty,
				FontItalic = italic,
				FontWeight = bold ? 700 : 400,
				LetterSpacing = letterSpacing,
				TextDirection = TextDirection.Auto,
				HaloBlur = haloBlur,
				BackgroundColor = backgroundColor.a > 0 ? new SKColor(ColorToUint(backgroundColor)) : SKColors.Empty,
				Underline = underlineStyle,
				LineHeight = lineHeight,
				StrikeThrough = strikeThroughStyle,
			};
			
			
			Dispose();
			if (texture != null) {
				DestroyImmediate(texture);
				texture = null;
			}
			if (rs != null) {
				rs.Clear();
			}
			rs = new TextBlock();
			rs.Alignment = textAlignment;
			rs.AddText(Text, styleBoldItalic);
			
			if (renderLinks) {
				RenderLinks();
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
			rs.MaxWidth = rectTransform.rect.width;
			rs.MaxHeight = autoFitVertical ? rs.MeasuredHeight : rectTransform.rect.height;
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

			if (autoFitVertical) {
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rs.MeasuredHeight );
			}

			
			currentWidth = rectTransform.rect.width;
			currentHeight = rectTransform.rect.height;
			
			if (currentWidth == 0 || currentHeight == 0) {
				return;
			}
            
			info = new SKImageInfo((int)rectTransform.rect.width,
				(int)rectTransform.rect.height);
			surface = SKSurface.Create(info);
			canvas = surface.Canvas;
			rs.Paint(canvas);
			TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
			texture = new Texture2D(info.Width, info.Height, format, false);
			texture.wrapMode = TextureWrapMode.Repeat;
			pixmap = surface.PeekPixels();
			texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
			texture.Apply();
			rawImage.texture = texture;
			Dispose();
		}

		private void RenderLinks() {
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
			Regex regex = new Regex(pattern);
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


		public void LinkPressed() {
				RectTransform rawImageRect = GetComponent<RectTransform>();
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, Input.mousePosition, null, out var localMousePosition)) {
					float normalizedX = Mathf.InverseLerp(-rawImageRect.rect.width / 2, rawImageRect.rect.width / 2, localMousePosition.x);
					float normalizedY = Mathf.InverseLerp(-rawImageRect.rect.height / 2, rawImageRect.rect.height / 2, localMousePosition.y);
					var caretPos = rs.HitTest(rawImageRect.sizeDelta.x * normalizedX, rawImageRect.sizeDelta.y * normalizedY);
					foreach (var url in urls) {
						if (caretPos.ClosestCodePointIndex >= url.Value.IndexStart && caretPos.ClosestCodePointIndex <=  url.Value.IndexEnd) {
							Application.OpenURL(rs.Copy(url.Key,url.Value.Length).ToString());
							break;
						}
					}
				}
		}

		private void OnDestroy() {
			Dispose();
			if (texture != null) {
				DestroyImmediate(texture);
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

		
		public void CalculateLayoutInputHorizontal() {
			if (rs != null) {
				preferredWidth = rs.MeasuredWidth;
			}
		}

		
		public void CalculateLayoutInputVertical() {
			if (rs != null) {
				preferredHeight = rs.MeasuredHeight;
			}
		}

		
		public float minWidth { get; }
		public float preferredWidth { get; set; }
		public float flexibleWidth { get; }
		public float minHeight { get; }
		public float preferredHeight { get; set; }
		public float flexibleHeight { get; }
		public int layoutPriority { get; }
	}
}