using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MVVMApplication.Utils;

public static class Helpers
{
    private static IAssetLoader AssetsLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
    private const string UriPath = "avares://MVVMApplication/Assets/";

    public static Bitmap ImageNameToBitmap(string imageNameNoExtension) =>
        new Bitmap(AssetsLoader.Open(new Uri($"{UriPath}{imageNameNoExtension}.png")));
}