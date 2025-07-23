using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Clingies.Windows;

namespace Clingies.UserControls;

public partial class ClingyBody : UserControl
{
    private Guid _id;
    private string? _bodyContent;
    private bool _isRolled;
    private bool _isLocked;
    private ClingyWindow? _parentWindow;

    public string? BodyContent
    {
        get { return _bodyContent; }
        set
        {
            _bodyContent = string.IsNullOrWhiteSpace(value) ? "" : value;
            ContentBox.Text = _bodyContent;
        }
    }

    public bool IsRolled
    {
        get { return _isRolled; }
        set
        {
            _isRolled = value;
            this.IsVisible = !value;
        }
    }

    public bool IsLocked
    {
        get { return _isLocked; }
        set
        {
            _isLocked = value;
            this.ContentBox.IsReadOnly = _isLocked;
        }
    }
    public ClingyBody()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;
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
}