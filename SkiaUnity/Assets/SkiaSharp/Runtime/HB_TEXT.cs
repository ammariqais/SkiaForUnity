using System;
using UnityEngine;
using UnityEngine.UI;
using Topten.RichTextKit;
using TextAlignment = Topten.RichTextKit.TextAlignment;

namespace SkiaSharp.Unity {
	public class HB_TEXT : MonoBehaviour {
		[SerializeField]
		private string message;
		[SerializeField]
		private bool autoFitVertical = true;
		[SerializeField]
		RectTransform rectTransform;

		private SKCanvas canvas;
		private SKImageInfo info;
		private SKSurface surface;
		private RawImage rawImage;
		private Texture2D texture;
		private RichString rs;
		private CaretPosition cr = new CaretPosition();


		void Start() {
			rawImage = GetComponent<RawImage>();
			if (rawImage) {
				rs = new RichString()
						.Alignment(TextAlignment.Center)
						.FontFamily("Segoe UI")
						.MarginBottom(20)
						.MarginTop(200)
						.BackgroundColor(SKColors.PaleVioletRed)
						.Add("☞ ✥ Welcome To Skia4Unity ✥", fontSize: 80, fontWeight: 700, fontItalic: true)
						.BackgroundColor(SKColors.White)
						.Paragraph().Alignment(TextAlignment.Right)
						.Add(message, fontSize: 50, textColor: SKColors.Black)
						.Paragraph().Alignment(TextAlignment.Left)
						.Add(
							"𓀎 La célébration de Noël - 🌚 شكرا 🌕 Greek Lorem Ipsum is a transliteration of the (pseudo) Latin original 𓀀, eg b => β, L => Λ,𓀒 ",
							fontSize: 50, haloBlur: 20, textColor: SKColors.DarkRed)
						.Add("🧛🏻🎃👾👨‍👩‍👧‍👦👳🏻‍♂️🧕🏻 👮🏻‍️👨🏿‍🚒👩🏿‍✈️🌚🌕🃏", fontSize: 200, haloBlur: 200)
						.Alignment(TextAlignment.Center)
						.Add("♧ ♡ ♢ ♚ ♛ ♜ ♝ ♞ ♟", fontSize: 80);
				rs.MaxWidth = (int)GetComponent<RectTransform>().rect.width;

				Debug.LogError(rs.MeasuredHeight);
				if (autoFitVertical) {
					rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rs.MeasuredHeight );
				}

				rs.MaxHeight = autoFitVertical ? rs.MeasuredHeight : (int)GetComponent<RectTransform>().rect.height;
				Debug.LogError(rs.Revision);
				Debug.LogError(rs.MeasuredHeight);
				Debug.LogError(rs.MaxHeight);
				
				info = new SKImageInfo((int)GetComponent<RectTransform>().rect.width,
					(int)GetComponent<RectTransform>().rect.height);
				surface = SKSurface.Create(info);
				canvas = surface.Canvas;
				
				
				// Highlight code points 10 through 19...
				var options = new TextPaintOptions() {
					Selection = new TextRange(200, 300, true),
					SelectionColor = new SKColor(0xFFFF0000),
					SelectionHandleColor = SKColors.Aqua,
					//SelectionHandleScale = 333f
				};
				rs.Paint(canvas, options: options);

				TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
				texture = new Texture2D(info.Width, info.Height, format, false);
				texture.wrapMode = TextureWrapMode.Repeat;
				var pixmap = surface.PeekPixels();
				texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
				texture.Apply();
				rawImage.texture = texture;
			}

		}

		private void Update() {
			if (Input.GetMouseButtonUp(0)) {
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.y = Screen.height - mousePosition.y; // Convert from bottom-left to upper-left
				Debug.LogError(rs.HitTest(mousePosition.x,mousePosition.y).ClosestLine);
				rs.Paragraph();
				rs.Add("noodckdpockpodecjkopdejkopdkcopice", fontSize:200,textColor:SKColors.Bisque);

				var carret = rs.HitTest(mousePosition.x, mousePosition.y).CaretPosition;
				carret.AltPosition = true;
				rs.Paint(canvas);
				rs.MaxHeight = null;
				Debug.LogError(rs.MeasuredHeight);
				rs.MaxHeight = rs.MeasuredHeight;
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rs.MeasuredHeight );

				var pixmap = surface.PeekPixels();
				texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
				texture.Apply();
				rawImage.texture = texture;
			}
		}
	}
}