using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Svg.Skia;
using UnityEngine.UI;

namespace SkiaSharp.Unity {
  public class SVGLoader : MonoBehaviour {
    [SerializeField]
    private TextAsset svgFile;
        
    private SKBitmap bmp;
    private SKSurface surface;
    private Texture2D texture;
    private RawImage rawImage;


    void Start() {
      rawImage = GetComponent<RawImage>();
            
      using (var svg = new SKSvg())
      {
        if (svg.FromSvg(svgFile.text) is { }) { 
          bmp = svg.Picture.ToBitmap(SKColor.Empty, rawImage.GetComponent<RectTransform>().sizeDelta.x/svg.Picture.CullRect.Width, rawImage.GetComponent<RectTransform>().sizeDelta.y/ svg.Picture.CullRect.Height, SKColorType.Rgba8888, SKAlphaType.Opaque, null);
          surface = SKSurface.Create(bmp.Info); 
          SKCanvas canvas = new SKCanvas(bmp);
          TextureFormat format = (bmp.Info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
          texture = new Texture2D(bmp.Info.Width, bmp.Info.Height, format, false);
          texture.wrapMode = TextureWrapMode.Repeat;
          var pixmap = surface.PeekPixels();
          texture.LoadRawTextureData(bmp.GetPixels(), pixmap.RowBytes * pixmap.Height);
          texture.Apply();
          rawImage.texture = texture;
        }
      }
    }
  }
}