using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MVVMApplication.Models;

public struct FilterItem
{
    public string Text { get; }
    public Bitmap Image { get; }

    public FilterItem(string text, string imagePath)
    {
        Text = text;
        
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        Image = new Bitmap(assets.Open(new Uri("avares://MVVMApplication/Assets/" + imagePath)));
    }
}