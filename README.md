# SkiaSharp for Unity with Lottie Animation and HarfBuzz Text, SVG Supports

SkiaSharp for Unity is a powerful plugin that leverages the SkiaSharp 2D graphics library to enhance Unity game development. With the added advantage of HarfBuzz support, this plugin allows you to create high-quality, cross-platform 2D graphics and user interfaces for your Unity projects. HarfBuzz support enables better text rendering, including advanced features such as native fonts, emoji support, and right-to-left (RTL) language rendering.

SkiaSharp is a .NET binding to the Skia library, which is utilized by popular platforms like Google Chrome, Android, and Flutter. By integrating SkiaSharp into Unity, you gain access to a wide range of APIs for rendering vector graphics, text, and images. The inclusion of HarfBuzz further improves text rendering, making it ideal for internationalization and multilingual projects.

## Added support for SVG.Skia:

SkiaForUnity now supports rendering SVG graphics using SVG.Skia. This allows you to leverage the power of SVG for creating scalable vector graphics within your Unity projects.

Benefits of SVG.Skia:

    Scalability: SVG graphics can be scaled to any size without losing quality.
    Cross-platform compatibility: SVG is a widely supported format that works across different platforms.

## Features

- **High-performance graphics:** SkiaSharp for Unity utilizes Skia's hardware-accelerated rendering capabilities to deliver fast and smooth graphics performance.

- **Cross-platform support:** SkiaSharp supports multiple platforms, including Windows, macOS, iOS, Android, allowing you to create graphics that work seamlessly across different devices.

- **Extensive API:** SkiaSharp provides a rich set of APIs for drawing paths, shapes, text, and images, enabling you to create visually stunning graphics and user interfaces.

- **Custom shaders:** With SkiaSharp for Unity, you can write custom shaders using the Skia graphics API, allowing you to create unique visual effects and stylized graphics.

- **Lottie animations:** The package includes the Skottie library, which enables you to import and play Lottie animations in your Unity projects.

- **HarfBuzz support:** The integration of HarfBuzz enhances text rendering in SkiaSharp, offering benefits such as native fonts, emoji rendering, and RTL language support. This makes SkiaSharp for Unity an excellent choice for projects that require advanced text rendering capabilities.
  

https://github.com/ammariqais/SkiaForUnity/assets/62248657/ac1a8c35-bb24-4b64-ac3e-85a5b06ed276



- **Integration with Unity:** SkiaSharp for Unity seamlessly integrates with the Unity Editor, providing a familiar development environment for working with Skia graphics and Lottie animations in your Unity projects.

## Watch the SkiaForUnity v1.0.0.Pre-2 Release Videos:
## Editor Mode
https://github.com/ammariqais/SkiaForUnity/assets/62248657/ccf71cd1-17ae-4442-b42f-4d3d067849f1

## Playing Animations

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
   
4. In Unity, go to Edit > Project Settings > Player and select the iOS platform.

5. Scroll down to the Other Settings section and locate the Scripting Define Symbols field.

6. Add __ UNIFIED _ _ and _ _IOS_ _ to the Scripting Define Symbols. This is necessary to enable the SkiaSharp functionality specifically for iOS.

7. Create a new script or modify an existing script to leverage the SkiaSharp API for drawing graphics and the Skottie API for playing Lottie animations.

8. Build and run your Unity project on an iOS device to see the Skia graphics and Lottie animations in action.

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

