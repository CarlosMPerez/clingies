using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Clingies.Application.Services;
using Clingies.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace Clingies.App.Windows;

public partial class ClingyWindow : Window
{
    private Clingy _clingy;
    ClingyNoteService _clingyService;

    public ClingyWindow(ClingyNoteService clingyService, Clingy clingy)
    {
        InitializeComponent();
        // Update height when content changes
        ContentBox.GetObservable(TextBox.TextProperty)
              .Subscribe(_ => UpdateWindowHeight());
        // Update height once on load
        this.Opened += (_, _) => UpdateWindowHeight();

        // Optional: if user resizes horizontally, we update vertical height again
        this.GetObservable(ClientSizeProperty)
            .Subscribe(size => UpdateWindowHeight());              
        AttachDragEvents();
        _clingy = clingy;
        _clingyService = clingyService;
        ContentBox.Text = _clingy.Content;
        TitleTextBlock.Text = _clingy.Title;
        Width = _clingy.Width;
        Height = _clingy.Height;
        Position = new PixelPoint((int)_clingy.PositionX, (int)_clingy.PositionY);
        Topmost = _clingy.IsPinned;
        PinButton.Background = _clingy.IsPinned ? new SolidColorBrush(Color.Parse("#444")) : Brushes.Transparent;
        PinButton.Opacity = _clingy.IsPinned ? 1 : 0;
    }

    private void OnPinClick(object? sender, RoutedEventArgs e)
    {
        Topmost = !Topmost;
        if (Topmost)
        {
            PinButton.Background = new SolidColorBrush(Color.Parse("#444"));
            PinButton.Opacity = 1;
            _clingy.SetPinState(true);
        }
        else
        {
            PinButton.Background = Brushes.Transparent;
            PinButton.Opacity = 0;
            _clingy.SetPinState(true);
        }

        _clingyService.Update(_clingy);
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        _clingyService.SoftDelete(_clingy.Id);
        this.Close();
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
        _clingy.Move(Position.X, Position.Y);
        _clingyService.Update(_clingy);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _clingy.Resize(Width, Height);
        _clingyService.Update(_clingy);
    }

    private void OnContentChanged(object? sender, EventArgs e)
    {
        string content = ContentBox.Text.IsNullOrEmpty() ? "" : ContentBox.Text!;
        _clingy.UpdateContent(content);
        _clingyService.Update(_clingy);
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
        ContentBox.Measure(new Size(Width, double.PositiveInfinity));
        var desired = ContentBox.DesiredSize.Height;

        // Add some padding for borders/margins
        Height = Math.Clamp(desired + 32, 80, 1000);
    }
}