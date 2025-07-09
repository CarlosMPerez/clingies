using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
    private double _unrolledHeight;
    private bool _isInitiallyRolled = false;
    public Guid ClingyId { get; private set; }
    public event EventHandler<Guid>? CloseRequested;
    public event EventHandler<PinRequestedEventArgs>? PinRequested;
    public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;
    public event EventHandler<SizeChangeRequestedEventArgs>? SizeChangeRequested;
    public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
    public event EventHandler<TitleChangeRequestedEventArgs>? TitleChangeRequested;
    public event EventHandler<UpdateWindowWidthRequestedEventArgs>? UpdateWindowWidthRequested;
    public event EventHandler<RollRequestedEventArgs>? RollRequested;

    public ClingyWindow()
    {
        InitializeComponent();
        _clingy = null!;
    }

    public ClingyWindow(Clingy clingy)
    {
        InitializeComponent();

        AttachDragEvents();
        _clingy = clingy;
        ClingyId = clingy.Id;
        ContentBox.Text = _clingy.Content;
        TitleTextBlock.Text = _clingy.Title;
        Position = new PixelPoint((int)_clingy.PositionX, (int)_clingy.PositionY);
        Width = clingy.Width;
        Topmost = _clingy.IsPinned;
        LoadPinImage(_clingy.IsPinned);
        _isInitiallyRolled = _clingy.IsRolled;
        if (!clingy.IsRolled)
        {
            Height = clingy.Height;
            _unrolledHeight = clingy.Height;
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.Opacity = 0;

        PositionChanged += OnPositionChanged;
        SizeChanged += OnSizeChanged;
        ContentBox.TextChanged += OnContentChanged;

        Dispatcher.UIThread.Post(() =>
        {
            // Let layout fully build unrolled
            _unrolledHeight = Height; // capture true measured height

            if (_isInitiallyRolled)
            {
                ToggleRolled(true);
            }

            Opacity = 1;
        }, DispatcherPriority.Render);
    }

    private void ToggleRolled(bool isRolled)
    {
        _isRolled = isRolled;

        if (isRolled)
        {
            // Save current height for restoring later
            _unrolledHeight = Height;
            BodyBorder.IsVisible = false;
            WindowGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Pixel);
            SizeToContent = SizeToContent.Manual;
            // Shrink to just title height (approx)
            var titleHeight = TitleBorder.Bounds.Height;
            if (titleHeight <= 0)
                titleHeight = 28; // use a reasonable default if not yet measured

            Height = titleHeight + 2;
        }
        else
        {
            BodyBorder.IsVisible = true;
            WindowGrid.RowDefinitions[1].Height = GridLength.Auto;
            SizeToContent = SizeToContent.Manual;
            Height = _unrolledHeight;
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

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        var args = new PositionChangeRequestedEventArgs(ClingyId,
                                this.Position.X, this.Position.Y);
        PositionChangeRequested?.Invoke(this, args);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (double.IsNaN(this.Width) || double.IsNaN(this.Height) || _isInitiallyRolled)
            return;

        if (!_isRolled) _unrolledHeight = Height;

        var args = new SizeChangeRequestedEventArgs(ClingyId, this.Height);
        SizeChangeRequested?.Invoke(this, args);
    }

    private void OnContentChanged(object? sender, EventArgs e)
    {
        if (_isRolled) return;

        if (sender is TextBox tb)
        {
            var newText = tb.Text ?? string.Empty;
            var args = new ContentChangeRequestedEventArgs(ClingyId, newText);
            ContentChangeRequested?.Invoke(this, args);

            // Estimate width of content area
            double availableWidth = tb.Bounds.Width > 0 ? tb.Bounds.Width : Width - 20;

            // Create a throwaway TextBlock to measure
            var measurementBlock = new TextBlock
            {
                Text = newText,
                FontFamily = tb.FontFamily,
                FontSize = tb.FontSize,
                FontWeight = tb.FontWeight,
                FontStyle = tb.FontStyle,
                TextWrapping = TextWrapping.Wrap,
                Width = availableWidth
            };

            // Measure it
            measurementBlock.Measure(new Size(availableWidth, double.PositiveInfinity));
            double contentHeight = measurementBlock.DesiredSize.Height;

            // Fallback minimum
            if (contentHeight < 30)
                contentHeight = 30;

            // Estimate title height
            double titleHeight = TitleBorder.Bounds.Height;
            if (titleHeight < 1)
                titleHeight = 28;

            double totalHeight = titleHeight + contentHeight + 12;

            Height = totalHeight;
            _unrolledHeight = totalHeight;
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