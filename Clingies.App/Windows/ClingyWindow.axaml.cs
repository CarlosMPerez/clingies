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
    ClingyService _clingyService;
    public ClingyWindow(ClingyService clingyService, Clingy clingy)
    {
        InitializeComponent();
        AttachDragEvents();
        _clingy = clingy;
        _clingyService = clingyService;
        ContentBox.Text = _clingy.Content;
        Title = _clingy.Title;
        Width = _clingy.Width;
        Height = _clingy.Height;
        Position = new PixelPoint((int)_clingy.PositionX, (int)_clingy.PositionY);
    }

    private void OnPinClick(object? sender, RoutedEventArgs e)
    {
        Topmost = !Topmost;
        if (Topmost)
        {
            PinButton.Background = new SolidColorBrush(Color.Parse("#444"));
            PinButton.Opacity = 1;
        }
        else
        {
            PinButton.Background = Brushes.Transparent;
            PinButton.Opacity = 0;
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        // TODO : SOFT DELETE FROM DB
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
}