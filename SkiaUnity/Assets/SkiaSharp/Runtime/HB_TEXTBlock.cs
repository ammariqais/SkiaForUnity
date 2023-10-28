using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Topten.RichTextKit;

namespace SkiaSharp.Unity.HB {
	public class HB_TEXTBlock : MonoBehaviour {
		[SerializeField]
		private string message;
		[SerializeField]
		private bool autoFitVertical = true;
		[SerializeField]
		RectTransform rectTransform;
		[SerializeField]
		private bool renderLinks;
		[SerializeField]
		private TextAsset font;

		private SKCanvas canvas;
		private SKImageInfo info;
		private SKSurface surface;
		private RawImage rawImage;
		private Texture2D texture;
		private TextBlock rs;
		private Dictionary<int, HBLinks> urls = new Dictionary<int, HBLinks>();
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
			if (String.IsNullOrEmpty(message)){
				return;
			}
			
			if (rawImage) {
				RenderText();
			}
		}

		private void RenderText() {
			currentWidth = GetComponent<RectTransform>().rect.width;
			currentHeight = GetComponent<RectTransform>().rect.height;
			if (currentWidth == 0 || currentHeight == 0) {
				return;
			}
			rs = new TextBlock();
			rs.AddText(message, styleBoldItalic);
			
			if (renderLinks) {
				RenderLinks();
			}

			if (font != null) {
				var bytes = font.bytes;
				var datadata = SKData.CreateCopy(bytes);
				SKTypeface skTypeface = SKTypeface.FromData(datadata);
				rs.FontMapper = new FontMapper(skTypeface);
			}
            
			rs.MaxWidth = (int)GetComponent<RectTransform>().rect.width;
			rs.MaxHeight = autoFitVertical ? rs.MeasuredHeight : (int)GetComponent<RectTransform>().rect.height;
			
			if (autoFitVertical) {
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rs.MeasuredHeight );
			}
				
			info = new SKImageInfo((int)GetComponent<RectTransform>().rect.width,
				(int)GetComponent<RectTransform>().rect.height);
			surface = SKSurface.Create(info);
			canvas = surface.Canvas;
				
                
			rs.Paint(canvas);

			TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
			texture = new Texture2D(info.Width, info.Height, format, false);
			texture.wrapMode = TextureWrapMode.Repeat;
			var pixmap = surface.PeekPixels();
			texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
			texture.Apply();
			rawImage.texture = texture;
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

		private void Update() {
			if (currentWidth != GetComponent<RectTransform>().rect.width ||
			    GetComponent<RectTransform>().rect.height != currentHeight) {
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
	}
}