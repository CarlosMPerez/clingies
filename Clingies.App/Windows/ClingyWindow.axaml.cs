using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Clingies.App.Windows;

public partial class ClingyWindow : Window
{
    public ClingyWindow()
    {
        InitializeComponent();
        AttachDragEvents();
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

    private void OnHeaderDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }    
}