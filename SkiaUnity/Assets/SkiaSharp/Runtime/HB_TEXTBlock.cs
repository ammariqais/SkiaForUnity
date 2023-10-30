using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Topten.RichTextKit;

namespace SkiaSharp.Unity.HB {
	public class HB_TEXTBlock : MonoBehaviour, ILayoutElement {
		[SerializeField]
		private string message;
		[SerializeField]
		private bool autoFitVertical = true;
		[SerializeField]
		private bool renderLinks;
		[SerializeField]
		private TextAsset font;

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

		private float currentWidth, currentHeight;

		public string text {
			get => message;
			set {
				message = value;
				if (rawImage) {
					urls.Clear();
					RenderText();
				}
			}
		}

		void Awake() {
			rawImage = GetComponent<RawImage>();
			rectTransform = transform as RectTransform;
			if (String.IsNullOrEmpty(message)){
				return;
			}
			
			if (rawImage) {
				RenderText();
			}
		}

		private void RenderText() {
			Dispose();
			if (texture != null) {
				DestroyImmediate(texture);
				texture = null;
			}
			rs = new TextBlock();
			rs.AddText(message, styleBoldItalic);
			
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
			string pattern = @"(https?://\S+|www\.\S+)";
			Regex regex = new Regex(pattern);
			MatchCollection matches = regex.Matches(message);
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
        


		
		private Style styleLink = new Style() {
			FontFamily = "Arial",
			FontSize = 32,
			TextColor = SKColors.Blue,
			Underline = UnderlineStyle.Solid
		};
		
		private Style styleBoldItalic = new Style() {
			FontFamily = "Segoe UI",
			FontSize = 32,
		};

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
			if (rs != null) {
				rs.Clear();
				rs = null;
			}

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