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
using Microsoft.IdentityModel.Tokens;

namespace Clingies.App.Windows;

public partial class ClingyWindow : Window
{
    private Clingy _clingy;
    public Guid ClingyId { get; private set; }
    private bool _updateScheduled = false;

    public event EventHandler<Guid> CloseRequested;
    public event EventHandler<PinRequestedEventArgs>? PinRequested;
    public event EventHandler<HeaderDragRequestedEventArgs>? HeaderDragRequested;
    public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;
    public event EventHandler<SizeChangeRequestedEventArgs>? SizeChangeRequested;
    public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
    public event EventHandler<KeyDownRequestedEventArgs>? KeyDownRequested;
    public event EventHandler<ResizeRightRequestedEventArgs>? ResizeRightRequested;
    public event EventHandler<ResizeLeftRequestedEventArgs>? ResizeLeftRequested;

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

    protected override void OnOpened(EventArgs e)
    {
        PositionChanged += OnPositionChanged;
        SizeChanged += OnSizeChanged;
        ContentBox.TextChanged += OnContentChanged;
        base.OnOpened(e);
    }

    private void OnHeaderDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        PositionChangeRequested?.Invoke(this, _clingy.Id);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        SizeChangeRequested?.Invoke(this, _clingy);
    }

    private void OnContentChanged(object? sender, EventArgs e)
    {
        ContentChangeRequested?.Invoke(this, _clingy);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Shift+Ctrl+P -> SET TITLE
        if (e.KeyModifiers == (KeyModifiers.Shift | KeyModifiers.Control) && e.Key == Key.P)
        {
            ShowTitleDialogAsync();
        }
    }

    private async void ShowTitleDialogAsync()
    {
        var previousTitle = TitleTextBlock.Text;
        var dialog = new TitleDialog(previousTitle.IsNullOrEmpty() ?
                            "Set Clingy Title" :
                            previousTitle!);

        var result = await dialog.ShowDialog<string>(this);
        if (!string.IsNullOrWhiteSpace(result))
        {
            _clingy.UpdateTitle(result);
            _clingyService.Update(_clingy);
            TitleTextBlock.Text = result;
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
                Height = finalHeight;
                _clingy.Resize(Width, finalHeight);
                _clingyService.Update(_clingy);
            }
        }, DispatcherPriority.Background);
    }

    private void OnResizeRight(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.East, e);

        _clingy.Resize(this.Width, this.Height);
        _clingyService.Update(_clingy);
    }

    private void OnResizeLeft(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.West, e);

        _clingy.Resize(this.Width, this.Height);
        _clingyService.Update(_clingy);
    }
}