//SkiaSharp For Unity
//version 2.0.0 alpha

using UnityEngine;
using UnityEngine.UI;
using SkiaSharp;
using Topten.RichTextKit;
using TextAlignment = Topten.RichTextKit.TextAlignment;

public class TextDrawer : MonoBehaviour {
  [SerializeField]
  private string message;
  
  private SKCanvas canvas;
  private SKImageInfo info;
  private SKSurface surface;
  private RawImage rawImage;
  private Texture2D texture;
  RichString _richString;


  void Start() {
	  rawImage = GetComponent<RawImage>();
	  info = new SKImageInfo((int)GetComponent<RectTransform>().rect.width, (int)GetComponent<RectTransform>().rect.height);
	  surface = SKSurface.Create(info);
	  canvas = surface.Canvas;

      
	  if (rawImage) {
		  var rs = new RichString()
			  .Alignment(TextAlignment.Center)
			  .FontFamily("Segoe UI")
			  .MarginBottom(20)
			  .MarginTop(20)
			  .Add("Welcome To Skia4Unity", fontSize: 64, fontWeight: 700, fontItalic: true)
			  .Paragraph().Alignment(TextAlignment.Left)
			  .MarginRight(40).MarginLeft(40)
			  .FontSize(20)
			  .Add(
				  "😀😀This is a test string This is a test 😀😀string This is a test string This is a 🙆‍♂ test string This is a test string This is a test string",textColor:SKColors.Tomato)
			  .Paragraph().Alignment(TextAlignment.Right)
			  .Add(message, fontSize: 32, textColor: SKColors.White)
			  .Paragraph().Alignment(TextAlignment.Center)
			  .Add("🧛🏻",fontSize:128)
			  ;
		  
		  
		  rs.MaxWidth = (int)GetComponent<RectTransform>().rect.width;
		  rs.MaxHeight = (int)GetComponent<RectTransform>().rect.height;
		  // Highlight code points 10 through 19...
		  var options = new TextPaintOptions() {
			  Selection = new TextRange(200,300,true),
			  SelectionColor = new SKColor(0xFFFF0000),
		  };
		  rs.Paint(canvas,options:options);
          
		  TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
		  texture = new Texture2D(info.Width, info.Height, format, false);
		  texture.wrapMode = TextureWrapMode.Repeat;
		  var pixmap = surface.PeekPixels();
		  texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
		  texture.Apply();
		  rawImage.texture = texture;
	  }
	  
  }
}