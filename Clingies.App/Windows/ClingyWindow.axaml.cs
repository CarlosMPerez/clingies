using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Clingies.App.Windows;

public partial class ClingyWindow : Window
{
    public ClingyWindow()
    {
        InitializeComponent();
        AttachDragEvents();
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
}