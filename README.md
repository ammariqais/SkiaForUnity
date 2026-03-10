# SkiaSharp for Unity with Lottie Animation and HarfBuzz Text, SVG Supports

SkiaSharp for Unity is a powerful plugin that leverages the SkiaSharp 2D graphics library to enhance Unity game development. With the added advantage of HarfBuzz support, this plugin allows you to create high-quality, cross-platform 2D graphics and user interfaces for your Unity projects. HarfBuzz support enables better text rendering, including advanced features such as native fonts, emoji support, and right-to-left (RTL) language rendering.

SkiaSharp is a .NET binding to the Skia library, which is utilized by popular platforms like Google Chrome, Android, and Flutter. By integrating SkiaSharp into Unity, you gain access to a wide range of APIs for rendering vector graphics, text, and images. The inclusion of HarfBuzz further improves text rendering, making it ideal for internationalization and multilingual projects.

## Features

- **High-performance graphics:** Hardware-accelerated rendering with SKSurface reuse, SKTypeface caching with reference counting, dirty-flag rendering, and reduced per-render allocations.

- **Cross-platform support:** Windows, macOS, Linux, iOS, and Android.

- **Extensive API:** Rich set of APIs for drawing paths, shapes, text, and images.

- **Custom shaders:** Write custom shaders using the Skia graphics API for unique visual effects.

- **Lottie animations:** Import and play Lottie animations via the Skottie library.

- **SVG support:** Render scalable vector graphics using SVG.Skia — scale to any size without losing quality, cross-platform compatible.

- **HarfBuzz text rendering:** Native fonts, emoji, RTL/bidirectional text, and advanced text shaping.

- **SkiaGraphic:** Vector shape UI component for Canvas — rectangles, rounded rects, circles, ellipses with fills, strokes, shadows, and gradients.

- **Pre-Bake System:** Bake any SkiaGraphic or HB TextBlock to a Sprite PNG with auto SpriteAtlas for draw call batching and zero runtime cost.

- **Integration with Unity:** Seamlessly integrates with the Unity Editor and Canvas UI system.

https://github.com/user-attachments/assets/a28f7218-6a5a-46c6-9a2b-f21c516a9a1e

## SkiaGraphic

A Canvas UI component for rendering vector shapes with zero code.

- **Shapes** — Rectangle, RoundedRect (per-corner radii), Circle, Ellipse
- **Fill** — Solid color, LinearGradient, RadialGradient, SweepGradient, Image (Stretch/Fit/Fill/Tile)
- **Stroke** — Solid or gradient, dashed, configurable width
- **Drop Shadow** — Offset, blur, color
- **Inner Shadow** — Offset, blur, color
- **Resolution Scale** — Lower for mobile performance
- Right-click: `GameObject > Skia UI (Canvas) > Skia Graphic`

## HB TextBlock

Advanced text rendering component powered by HarfBuzz and SkiaSharp.

- **Vertical alignment** — Top, Middle, Bottom
- **Text direction** — LTR, RTL, Auto
- **Font weight** — 100 (Thin) to 900 (Black)
- **Font variant** — Normal, SuperScript, SubScript
- **Rich text** — `<b>` `<i>` `<u>` `<s>` `<sup>` `<sub>` `<size=N>` `<color=#HEX>` `<color=name>`
- **Fallback fonts** — multiple HBFontData assets for multi-language support
- **Paragraph spacing** — configurable space between paragraphs
- **Padding** — Left, Top, Right, Bottom text area padding
- **Link detection** — automatic URL rendering with click callbacks
- **Events** — onTextChanged, onLinkClicked
- **ILayoutElement** integration for Unity layout system
- **TMP-style Inspector** — section headers, B/I/U/S toggle strip, alignment buttons, collapsible sections, gradient editor, text info panel, live preview, tooltips on every field

## HB InputField

Full-featured input field built on HB TextBlock, similar to TMP_InputField.

- **Content types** — Standard, Integer, Decimal, Alphanumeric, Name, Email, Password
- **Line types** — SingleLine, MultiLine
- **Text settings** — font, font size, color, bold, italic, alignment, rich text
- **Placeholder** — configurable text and color
- **Auto-resize** — multiline fields grow height to fit content (min/max bounds)
- **Text scrolling** — horizontal for single-line, vertical for multiline
- **Caret** — configurable color, blink rate, width, or hide completely
- **Selection** — click, double-click (word), triple-click (all), Shift+arrows, Ctrl+A
- **Clipboard** — Ctrl+C, Ctrl+V, Ctrl+X
- **Navigation** — arrow keys, Ctrl+arrows (word jump), Home, End, Tab/Shift+Tab
- **Read-only mode** — custom background and font color
- **Focus highlight** — optional background color change
- **Mobile support** — hide native input overlay on iOS/Android
- **Character limit** and password masking
- **Events** — onValueChanged, onSubmit, onEndEdit, onFocus, onUnfocus

## HB Text Animator

Sequential multi-step animations on any HB TextBlock.

- **Effects** — Typewriter, Fade, Color Lerp, Scale, Slide, Shake, Gradient Angle, Outline Pulse, Shadow Animate, Font Size, Letter Spacing
- Per-step duration, delay, and ease curve
- Loop support, editor preview, onStepComplete/onComplete events

## Pre-Bake System

Render any SkiaGraphic or HB TextBlock in the editor, save as a Sprite PNG, and batch via a shared SpriteAtlas — **zero SkiaSharp cost at runtime**.

- One-click **Bake to PNG** button in the inspector
- Auto-creates and manages `SpriteAtlas` for draw call batching (20 baked elements → 1-2 draw calls)
- Swaps RawImage → Image component automatically
- **Nine-slice** support for SkiaGraphic solid fills (resize freely without re-bake)
- **Unbake** restores live rendering instantly
- Inspector properties greyed out when baked to prevent accidental edits

## Performance

- **Dirty flag rendering** — HB TextBlock property changes coalesce into a single render per frame
- **Gradient caching** — Hash-based dirty check avoids recreating gradient objects when nothing changed
- **Layout rebuild throttling** — `LayoutRebuilder.MarkLayoutForRebuild` only called when size actually changes
- **Typewriter cache** — HB Text Animator only allocates a new Substring when visible character count changes
- **SKTypeface caching** — Reference-counted typeface cache shared across all text components
- **SKSurface reuse** — Surfaces resized in-place instead of recreated each frame

https://github.com/ammariqais/SkiaForUnity/assets/62248657/ac1a8c35-bb24-4b64-ac3e-85a5b06ed276

## Videos

### Editor Mode
https://github.com/ammariqais/SkiaForUnity/assets/62248657/ccf71cd1-17ae-4442-b42f-4d3d067849f1

### Playing Animations
https://github.com/ammariqais/SkiaForUnity/assets/62248657/cc7a5d56-48e7-4e28-8e18-b8e8ec776eab

## Getting Started

To get started with SkiaSharp for Unity, follow these steps:

1. Clone or download the SkiaSharp for Unity repository from [GitHub](git@github.com:ammariqais/SkiaForUnity.git). or you can use

**Package Manager**
1. Open your Unity project and navigate to `Windows`> `Package Manager`.
2. Click the `+` button in the top-left corner and select Add package from git URL
3. Enter the following URL and click `Add`

`https://github.com/ammariqais/SkiaForUnity.git?path=SkiaUnity/Assets/SkiaSharp`

3. Import the SkiaSharp package into your Unity project.

4. if you're building your game for iOS. **This step is necessary to have the library work on iOS**

   4a. In Unity, go to Edit > Project Settings > Player and select the iOS platform.

   4b. Scroll down to the Other Settings section and locate the Scripting Define Symbols field.

   4c. Add  `__UNIFIED__` and `__IOS__` to the Scripting Define Symbols.

   ![image](https://github.com/user-attachments/assets/f85ad50d-71e7-4276-9c26-f7044581ff0c)


6. Create a new script or modify an existing script to leverage the SkiaSharp API for drawing graphics and the Skottie API for playing Lottie animations.

## Tutorials
[Document](https://ammariqais.github.io/SkiaForUnity/html/)

## Contributing

Contributions to SkiaSharp for Unity are welcome! If you encounter any issues, have suggestions for improvements, or would like to contribute code, please feel free to open an issue or submit a pull request on the [GitHub repository](https://github.com/ammariqais/SkiaForUnity).

## License

SkiaSharp for Unity is licensed under the [MIT License](https://github.com/ammariqais/SkiaForUnity/blob/main/LICENSE). Please refer to the LICENSE file for more information.

## Support

If you have any questions, need assistance, or want to join a community of developers using SkiaSharp for Unity, you can:

- Visit the [SkiaSharp for Unity GitHub Discussions](https://github.com/ammariqais/SkiaForUnity/discussions) page to engage in discussions and ask questions.

- Open an issue on the [GitHub repository](https://github.com/ammariqais/SkiaForUnity/issues) if you encounter any bugs or problems.

## Acknowledgements

SkiaSharp for Unity is built on top of the amazing SkiaSharp library and includes the Skottie library for Lottie animations. We would like to express our gratitude to the SkiaSharp and Skottie communities for their hard work and dedication in developing and maintaining these powerful libraries.

## Authors

SkiaSharp for Unity is developed and maintained by [Qais Ammari](https://github.com/ammariqais).
