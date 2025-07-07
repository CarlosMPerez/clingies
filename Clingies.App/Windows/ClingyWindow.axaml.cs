using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Clingies.App.Windows.CustomEventArgs;
using Clingies.Domain.Models;

namespace Clingies.App.Windows;

public partial class ClingyWindow : Window
{
    private Clingy _clingy;
    private bool _isRolled;
    private bool _isInitiallyRolled = false;
    public Guid ClingyId { get; private set; }
    private bool _updateScheduled = false;

    public event EventHandler<Guid>? CloseRequested;
    public event EventHandler<PinRequestedEventArgs>? PinRequested;
    public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;
    public event EventHandler<SizeChangeRequestedEventArgs>? SizeChangeRequested;
    public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
    public event EventHandler<TitleChangeRequestedEventArgs>? TitleChangeRequested;
    public event EventHandler<UpdateWindowHeightRequestedEventArgs>? UpdateWindowHeightRequested;
    public event EventHandler<UpdateWindowWidthRequestedEventArgs>? UpdateWindowWidthRequested;
    public event EventHandler<RollRequestedEventArgs>? RollRequested;

    public ClingyWindow(Clingy clingy)
    {
        InitializeComponent();

        AttachDragEvents();
        _clingy = clingy;
        ClingyId = clingy.Id;
        ContentBox.Text = _clingy.Content;
        TitleTextBlock.Text = _clingy.Title;
        // Width = _clingy.Width;
        // Height = _clingy.Height;
        Position = new PixelPoint((int)_clingy.PositionX, (int)_clingy.PositionY);
        Topmost = _clingy.IsPinned;
        LoadPinImage(_clingy.IsPinned);
        _isInitiallyRolled = _clingy.IsRolled;
    }

    private void ToggleRolled(bool isRolled)
    {
        _isRolled = isRolled;

        if (isRolled)
        {
            BodyBorder.IsVisible = false;
            WindowGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Pixel);
            SizeToContent = SizeToContent.Height;
        }
        else
        {
            BodyBorder.IsVisible = true;
            // Restore body row to auto-height
            WindowGrid.RowDefinitions[1].Height = GridLength.Auto;

            // Force full layout refresh if this clingy started as rolled
            if (_isInitiallyRolled)
            {
                InvalidateMeasure();
                InvalidateArrange();
                SizeToContent = SizeToContent.Manual;
                SizeToContent = SizeToContent.Height;
                _isInitiallyRolled = false; // only needed once
            }
            else
            {
                SizeToContent = SizeToContent.Height;
            }
        }
    }

    private void OnPinClick(object? sender, RoutedEventArgs e)
    {
        LoadPinImage(!_clingy.IsPinned);
        var args = new PinRequestedEventArgs(_clingy.Id, !_clingy.IsPinned);
        PinRequested?.Invoke(this, args);
    }

    private void LoadPinImage(bool pinned)
    {
        string imageRes = pinned ?
            "avares://Clingies.App/Assets/icon-pinned.png" :
            "avares://Clingies.App/Assets/icon-unpinned.png";
        var uri = new Uri(imageRes);
        using var stream = AssetLoader.Open(uri);
        PinButtonImage.Source = new Bitmap(stream);

    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, _clingy.Id);
    }

    private void AttachDragEvents()
    {
        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };
    }

    private void OnHeaderDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        PositionChanged += OnPositionChanged;
        SizeChanged += OnSizeChanged;
        ContentBox.TextChanged += OnContentChanged;

        Dispatcher.UIThread.Post(() =>
        {
            ToggleRolled(_clingy.IsRolled);
        }, DispatcherPriority.Loaded);
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        var args = new PositionChangeRequestedEventArgs(ClingyId,
                                this.Position.X, this.Position.Y);
        PositionChangeRequested?.Invoke(this, args);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (double.IsNaN(this.Width) || double.IsNaN(this.Height))
            return; // skip update until layout resolves        

        var args = new SizeChangeRequestedEventArgs(ClingyId, this.Width, this.Height);
        SizeChangeRequested?.Invoke(this, args);
    }

    private void OnContentChanged(object? sender, EventArgs e)
    {
        if (_isRolled) return; // don't resize if rolled

        if (sender is TextBox tb)
        {
            var newText = tb.Text ?? string.Empty;
            var args = new ContentChangeRequestedEventArgs(ClingyId, newText);
            ContentChangeRequested?.Invoke(this, args);

            // Force size recalculation
            SizeToContent = SizeToContent.Manual;
            InvalidateMeasure();
            InvalidateArrange();
            SizeToContent = SizeToContent.Height;          
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Shift+Ctrl+T -> SET TITLE
        if (e.KeyModifiers == (KeyModifiers.Shift | KeyModifiers.Control) && e.Key == Key.T)
        {
            string prevTitle = string.IsNullOrEmpty(TitleTextBlock.Text) ? "" : TitleTextBlock.Text;
            var args = new TitleChangeRequestedEventArgs(ClingyId, prevTitle);
            TitleChangeRequested?.Invoke(this, args);
        }
    }

    private void OnTitleBarDoubleTapped(object? sender, RoutedEventArgs e)
    {
        bool newState = !_isRolled;
        _isRolled = newState;
        _clingy.SetRolledState(newState);
        ToggleRolled(newState);
        var args = new RollRequestedEventArgs(_clingy.Id, _clingy.IsRolled);
        RollRequested?.Invoke(this, args);
    }

    private void OnResizeRight(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.East, e);
        var args = new UpdateWindowWidthRequestedEventArgs(ClingyId, this.Width);
        UpdateWindowWidthRequested?.Invoke(this, args);
    }

    private void OnResizeLeft(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.West, e);

        var args = new UpdateWindowWidthRequestedEventArgs(ClingyId, this.Width);
        UpdateWindowWidthRequested?.Invoke(this, args);
    }
}