using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Animation = SkiaSharp.Skottie.Animation;

namespace SkiaSharp.Unity {
  public sealed class SkottiePlayer : MonoBehaviour {
  [SerializeField]
  private TextAsset lottieFile;
  [SerializeField]
  int resWidth = 250, resHeight = 250;
  [SerializeField] 
  private string stateName;
  [SerializeField] 
  private bool resetAfterFinished = false;
  [SerializeField] 
  private bool autoPlay = false;
  [SerializeField] 
  private bool loop = false;

  public UnityAction<string> OnAnimationFinished;

  private Animation currentAnimation;
  private SKCanvas canvas;
  private SKRect rect;
  private double timer = 0, animationFps, animationDuration, animationStateDuration;
  private SKImageInfo info;
  private SKSurface surface;
  private RawImage rawImage;
  private Texture2D texture;
  private bool playAniamtion = false;
  private SkottieMarkers states;
  private SkottieMarkers.state currentState;
  
  private void Start() {
    if (lottieFile == null) {
      return;
    }

    playAniamtion = playAniamtion || autoPlay;
    LoadAnimation(lottieFile.text);
  }

  void LoadTexture() {
    if (texture != null) {
      DestroyImmediate(texture);
      texture = null;
    }
    
    states = JsonConvert.DeserializeObject<SkottieMarkers>(lottieFile.text);
    animationFps = currentAnimation.Fps;
    animationDuration = currentAnimation.Duration;
      
    if (stateName.Length > 0 && states != null && states.markers.Count > 0) {
      currentState = states.GetStateByName(stateName);
      if (currentState != null) {
        animationStateDuration = (currentState.tm + currentState.dr) / animationFps;
      }
    }
    rawImage = GetComponent<RawImage>();
    info = new SKImageInfo(resWidth, resWidth);
    surface = SKSurface.Create(info);
    rect = SKRect.Create(resWidth, resHeight);
    canvas = surface.Canvas;
    currentAnimation.SeekFrame(currentState?.tm ?? 0);
    currentAnimation.Render(canvas,rect);

    TextureFormat format = (info.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
    texture = new Texture2D(info.Width, info.Height, format, false);
    texture.wrapMode = TextureWrapMode.Repeat;
    var pixmap = surface.PeekPixels();
    texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
    texture.Apply();
    rawImage.texture = texture;
  }

  public void LoadAnimation(string json) {
    if (!Animation.TryParse(json, out currentAnimation)) {
      Debug.LogError("[SkottiePlayer] - wrong json file");
      return;
    }
    LoadTexture();
  }

  public void SetState(string name) {
      if (states != null && currentAnimation != null) {
        playAniamtion = false;
        currentState = states.GetStateByName(name);
        if (currentState != null) {
          animationStateDuration = (currentState.tm + currentState.dr) / animationFps;
          timer = currentState.tm/animationFps; 
          canvas.Clear();
          currentAnimation.SeekFrameTime(timer); 
          currentAnimation.Render(canvas,rect); 
          var pixmap = surface.PeekPixels(); 
          texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height); 
          texture.Apply(); 
          rawImage.texture = texture; 
        } else {
          Debug.LogError($"[SkottiePlayer] - SetState({name}), state not found!");
        }
      }
  }

    public string GetStateName() {
      if (currentState != null) {
        return currentState.cm;
      }
      return "";
    }

    public double GetFps() {
      return animationFps;
    }
    
    public double GetDurations() {
      return animationDuration;
    }

    public void PlayAnimation(bool? reset = null) {
      playAniamtion = true;
      resetAfterFinished = reset == null ? resetAfterFinished : reset.Value;
    }
  

    private void Update() {
        if (playAniamtion == false || currentAnimation == null || canvas == null) {
          return;
        }
        
        timer += Time.deltaTime;
        if (currentState != null && timer >= animationStateDuration) {
          timer = resetAfterFinished || loop
            ? currentState.tm / animationFps
            : animationStateDuration;
          playAniamtion = loop;
          OnAnimationFinished?.Invoke(currentState?.cm);
        } else if (timer >= animationDuration) {
          timer = resetAfterFinished || loop ? 0 : timer;
          playAniamtion = loop;
          OnAnimationFinished?.Invoke(currentState?.cm);
        }

        canvas.Clear();
        currentAnimation.SeekFrameTime(timer);
        currentAnimation.Render(canvas, rect);
        var pixmap = surface.PeekPixels();
        texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height);
        texture.Apply();
        rawImage.texture = texture;

    }

    private void OnDestroy() {
      if (texture != null) {
        DestroyImmediate(texture);
        texture = null;
      }

      if (currentAnimation != null) {
        currentAnimation.Dispose();
        currentAnimation = null;
      }

      if (canvas != null) {
        canvas.Dispose();
        canvas = null;
      }

    }
  }
}