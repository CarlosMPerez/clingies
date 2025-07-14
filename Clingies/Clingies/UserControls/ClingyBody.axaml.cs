using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Clingies.Common.CustomEventArgs;
using Clingies.Windows;

namespace Clingies.UserControls;

public partial class ClingyBody : UserControl
{
    public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
    private Guid _id;
    private string? _bodyContent;
    private ClingyWindow? _parentWindow;

    public ClingyBody()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;
        ContentBox.TextChanged += OnContentChanged;
        BorderLeft.PointerPressed += OnResizeLeft;
        BorderRight.PointerPressed += OnResizeRight;
    }
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _parentWindow = (ClingyWindow)this.GetVisualRoot()!;
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
            _bodyContent = string.IsNullOrWhiteSpace(value) ? "" : value;
            ContentBox.Text = _bodyContent;
        }
    }

    private void OnContentChanged(object? sender, TextChangedEventArgs e)
    {
        if (IsVisible)
        {
            // Measure height
            ContentBox.Measure(new Size(ContentBox.Bounds.Width, double.PositiveInfinity));

            var measuredHeight = ContentBox.DesiredSize.Height;
            if (measuredHeight < 30) measuredHeight = 30;

            Height = measuredHeight + 40; // + title + margin

            // call parentwindow with new height and content
            _parentWindow!.ContentChangeRequest(_bodyContent, Height);
        }
    }

    private void OnResizeRight(object? sender, PointerPressedEventArgs e)
    {
        _parentWindow?.BeginResizeDrag(WindowEdge.East, e);
        _parentWindow!.WidthChangeRequest();
    }

    private void OnResizeLeft(object? sender, PointerPressedEventArgs e)
    {
        _parentWindow?.BeginResizeDrag(WindowEdge.West, e);
        _parentWindow!.WidthChangeRequest();
    }    
}