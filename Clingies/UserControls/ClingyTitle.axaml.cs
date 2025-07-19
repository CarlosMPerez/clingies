using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Clingies.Domain.Interfaces;
using Clingies.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Clingies.UserControls;

public partial class ClingyTitle : UserControl
{
    private Guid _id;
    private bool _isRolled;
    private bool _isPinned;
    private string? _title;
    private ClingyWindow? _parentWindow;

    IIconPathRepository _iconRepo;

    public ClingyTitle()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;

        _iconRepo = App.Services.GetRequiredService<IIconPathRepository>();
        CloseButtonImage.Source = LoadPinImage(_iconRepo.GetDarkPath("clingy-close")!);
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _parentWindow = (ClingyWindow)this.GetVisualRoot()!;
    }
    
    public bool IsRolled
    {
        get { return _isRolled; }
        set { _isRolled = value; }
    }

    public Guid ClingyId
    {
        get { return _id; }
        set { _id = value; }
    }

    public bool IsPinned
    {
        get { return _isPinned; }
        set
        {
            _isPinned = value;
            PinButtonImage.Source = _isPinned ?
                LoadPinImage(_iconRepo.GetDarkPath("clingy-pinned")!) :
                LoadPinImage(_iconRepo.GetDarkPath("clingy-unpinned")!);

        }
    }

    public string? Title
    {
        get { return _title; }
        set
        {
            _title = value;
            TitleText.Text = value;
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        _parentWindow?.CloseRequest();
    }

    private void OnPinClick(object? sender, RoutedEventArgs e)
    {
        _isPinned = !_isPinned;
        IsPinned = _isPinned;
        _parentWindow?.PinRequest(_isPinned);
    }

    private void OnDoubleTap(object? sender, RoutedEventArgs e)
    {
        _isRolled = !_isRolled;
        _parentWindow?.RollRequest(_isRolled);
    }

    private void OnDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _parentWindow?.BeginMoveDrag(e);
        }
    }

    private Bitmap LoadPinImage(string iconPath)
    {
        var uri = new Uri(iconPath);
        using var stream = AssetLoader.Open(uri);
        return new Bitmap(stream);
    }
}