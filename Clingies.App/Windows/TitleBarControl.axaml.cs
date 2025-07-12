// ==========================
// TitleBarControl.axaml.cs
// ==========================
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace Clingies.App.Controls;

public partial class TitleBarControl : UserControl
{
    public event EventHandler<Guid>? CloseRequested;
    public event EventHandler<PinRequestedEventArgs>? PinRequested;
    public event EventHandler<TitleChangeRequestedEventArgs>? TitleChangeRequested;
    public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;

    public string Title
    {
        get => TitleTextBlock.Text ?? "";
        set => TitleTextBlock.Text = value;
    }

    public TitleBarControl()
    {
        InitializeComponent();
    }

    private void OnClose(object? sender, RoutedEventArgs e)
        => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void OnPin(object? sender, RoutedEventArgs e)
        => PinRequested?.Invoke(this, EventArgs.Empty);

    private void OnDoubleTap(object? sender, RoutedEventArgs e)
        => TitleChangeRequested?.Invoke(this, EventArgs.Empty);

    private void OnDrag(object? sender, PointerPressedEventArgs e)
        => PositionChangeRequested?.Invoke(this, e);

    public string Title
    {
        get => TitleText.Text;
        set => TitleText.Text = value;
    }

    public bool IsPinned
    {
        set => PinImage.Source = LoadPinImage(value);
    }

    private IImage LoadPinImage(bool pinned)
    {
        var res = pinned ?
            "avares://Clingies.App/Assets/icon-pinned.png" : 
            "avares://Clingies.App/Assets/icon-unpinned.png";
        var uri = new Uri(res);
        using var stream = Avalonia.Platform.AssetLoader.Open(uri);
        return new Avalonia.Media.Imaging.Bitmap(stream);
    }
}
