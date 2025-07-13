using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Clingies.App.Common.CustomEventArgs;
using System;

namespace Clingies.App.Windows.Controls;

public partial class TitleBarControl : UserControl
{
    private Guid _id;
    private bool _isRolled;
    private bool _isPinned;
    private string? _title;
    private ClingyNoteWindow? _parentWindow;

    public TitleBarControl()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;

        HeaderGrid.PointerPressed += OnHeaderDrag;
        HeaderGrid.DoubleTapped += OnDoubleTap;
        PinButton.Click += OnPinClick;
        CloseButton.Click += OnClose;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _parentWindow = (ClingyNoteWindow)this.GetVisualRoot()!;
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
        set { _isPinned = value; }
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
        LoadPinImage(_isPinned);
        _parentWindow?.PinRequest(_isPinned);
    }

    private void OnDoubleTap(object? sender, RoutedEventArgs e)
    {
        _isRolled = !_isRolled;
        _parentWindow?.RollRequest(_isRolled);
    }

    private void OnHeaderDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            _parentWindow?.BeginMoveDrag(e);
    }

    private void LoadPinImage(bool pinned)
    {
        var res = pinned ?
            "avares://Clingies.App/Assets/icon-pinned.png" : 
            "avares://Clingies.App/Assets/icon-unpinned.png";
        var uri = new Uri(res);
        using var stream = AssetLoader.Open(uri);
        PinButtonImage.Source = new Bitmap(stream);
    }
}
