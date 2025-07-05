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

    public ClingyWindow(Clingy clingy)
    {
        InitializeComponent();

        Opened += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                UpdateWindowHeight(); // once, after first layout
            }, DispatcherPriority.Background);
        };

        AttachDragEvents();
        _clingy = clingy;
        ClingyId = clingy.Id;
        ContentBox.Text = _clingy.Content;
        TitleTextBlock.Text = _clingy.Title;
        Width = _clingy.Width;
        Height = _clingy.Height;
        Position = new PixelPoint((int)_clingy.PositionX, (int)_clingy.PositionY);
        Topmost = _clingy.IsPinned;
        LoadPinImage(_clingy.IsPinned);

        // Update height when content changes
        ContentBox.GetObservable(TextBox.TextProperty)
              .Subscribe(_ => UpdateWindowHeight());
        // Update height once on load
        ContentBox.TextChanged += (_, _) => UpdateWindowHeight();
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
        PositionChanged += OnPositionChanged;
        SizeChanged += OnSizeChanged;
        ContentBox.TextChanged += OnContentChanged;
        base.OnOpened(e);
    }
    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        var args = new PositionChangeRequestedEventArgs(ClingyId,
                                this.Position.X, this.Position.Y);
        PositionChangeRequested?.Invoke(this, args);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var args = new SizeChangeRequestedEventArgs(ClingyId, this.Width, this.Height);
        SizeChangeRequested?.Invoke(this, args);
    }

    private void OnContentChanged(object? sender, EventArgs e)
    {
        if (sender is TextBox tb)
        {
            var newText = tb.Text ?? string.Empty;
            var args = new ContentChangeRequestedEventArgs(ClingyId, newText);
            ContentChangeRequested?.Invoke(this, args);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Shift+Ctrl+P -> SET TITLE
        if (e.KeyModifiers == (KeyModifiers.Shift | KeyModifiers.Control) && e.Key == Key.P)
        {
            string prevTitle = string.IsNullOrEmpty(TitleTextBlock.Text) ? "" : TitleTextBlock.Text;
            var args = new TitleChangeRequestedEventArgs(ClingyId, prevTitle);
            TitleChangeRequested?.Invoke(this, args);
        }
    }


    private void UpdateWindowHeight()
    {
        if (_updateScheduled) return;
        _updateScheduled = true;

        Dispatcher.UIThread.Post(() =>
        {
            _updateScheduled = false;
            LayoutRoot.Measure(new Size(Bounds.Width, double.PositiveInfinity));
            LayoutRoot.Arrange(new Rect(LayoutRoot.DesiredSize));

            var contentHeight = LayoutRoot.DesiredSize.Height;

            const double headerHeight = 30;
            const double padding = 16;
            const double minTotalHeight = headerHeight + padding + 40;

            double finalHeight = Math.Max(minTotalHeight, contentHeight + headerHeight);
            finalHeight = Math.Clamp(finalHeight, 100, 1000);

            if (Math.Abs(Height - finalHeight) > 1)
            {
                var args = new UpdateWindowHeightRequestedEventArgs(ClingyId, finalHeight);
                UpdateWindowHeightRequested?.Invoke(this, args);
            }
        }, DispatcherPriority.Background);
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