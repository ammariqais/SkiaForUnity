//SkiaSharp For Unity
//version 0.0.1 alpha

using System.Text;
using HarfBuzzSharp;
using UnityEngine;
using UnityEngine.UI;
using SkiaSharp;
using SkiaSharp.HarfBuzz;

using Animation = SkiaSharp.Skottie.Animation;
using Font = HarfBuzzSharp.Font;

public class SkottieImageDraw : MonoBehaviour {
  [SerializeField]
  private TextAsset lottieFile;
  [SerializeField]
  int resWidth = 250, resHeight = 250;
  
  private Animation currentAnimation;
  private SKCanvas canvas;
  private SKRect rect;
  private float timer = 0;
  private SKImageInfo info;
  private SKSurface surface;
  private RawImage rawImage;
  private Texture2D texture;
  
    void Start() {
      if (!Animation.TryParse(lottieFile.text, out currentAnimation)) {
        Debug.LogError("[Skia4Unity] wrong json file");
        return;
      }
      
      rawImage = GetComponent<RawImage>();
      info = new SKImageInfo(resWidth, resHeight);
      surface = SKSurface.Create(info);
      rect = SKRect.Create(resWidth, resHeight);
      canvas = surface.Canvas;
      currentAnimation.SeekFrameTime(0);
      Debug.LogError(currentAnimation.OutPoint);
      currentAnimation.Render(canvas,rect);
      TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
      texture = new Texture2D(info.Width, info.Height, format, false);
      texture.wrapMode = TextureWrapMode.Repeat;
      var pixmap = surface.PeekPixels();
      texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
      texture.Apply();
      rawImage.texture = texture;
      
    }
  

    private void Update() {
      return;
      if (currentAnimation != null) {
        if (timer > currentAnimation.Duration) {
          timer = 0;
        }

        timer += Time.deltaTime;
        canvas.Clear();
        currentAnimation.SeekFrameTime(timer);
        currentAnimation.Render(canvas,rect);
        var pixmap = surface.PeekPixels();
        texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
        texture.Apply();
        rawImage.texture = texture;
      }
    }

    private void OnDisable() {
      canvas.Dispose();
      currentAnimation.Dispose();
    }
}