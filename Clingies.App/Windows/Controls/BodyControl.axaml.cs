using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Clingies.App.Common.CustomEventArgs;

namespace Clingies.App.Windows.Controls;

public partial class BodyControl : UserControl
{

    public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
    public event EventHandler<UpdateWindowWidthRequestedEventArgs>? UpdateWindowWidthRequested;
    private Guid _id;
    private string? _bodyContent;
    private Window? _parentWindow;

    public BodyControl()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;
    }
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _parentWindow = (Window)this.GetVisualRoot()!;
    }


    public Guid ClingyId
    {
        get { return _id; }
        set { _id = value; }
    }

    public string? BodyContent
    {
        get { return _bodyContent; }
        set
        {
            _bodyContent = value;
            ContentBox.Text = value;
        }
    }

    private void OnResizeRight(object? sender, PointerPressedEventArgs e)
    {
        _parentWindow?.BeginResizeDrag(WindowEdge.East, e);
        CallUpdateWindow();
    }

    private void OnResizeLeft(object? sender, PointerPressedEventArgs e)
    {
        _parentWindow?.BeginResizeDrag(WindowEdge.West, e);
        CallUpdateWindow();
    }

    private void CallUpdateWindow()
    {
        var args = new UpdateWindowWidthRequestedEventArgs(_id, _parentWindow != null ? _parentWindow.Width : 0);
        UpdateWindowWidthRequested?.Invoke(this, args);
    }
}