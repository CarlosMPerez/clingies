using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Clingies.Services;
using Clingies.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Clingies.UserControls;

public partial class ClingyTitle : UserControl
{
    private Guid _id;
    private bool _isRolled;
    private bool _isPinned;
    private bool _isLocked;
    private string? _title;
    private ClingyWindow? _parentWindow;
    UtilsService _utils;

    public ClingyTitle()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;

        _utils = App.Services!.GetRequiredService<UtilsService>();
        CloseButtonImage.Source = _utils.LoadBitmap("clingy_close");
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _parentWindow = (ClingyWindow)this.GetVisualRoot()!;
    }

    public bool IsRolled
    {
        get { return _isRolled; }
        set { _isRolled = value; }
    }

    public Guid ClingyId
    {
        get { return _id; }
        set { _id = value; }
    }

    public bool IsPinned
    {
        get { return _isPinned; }
        set
        {
            _isPinned = value;
            PinButtonImage.Source = _isPinned ?
                _utils.LoadBitmap("clingy_pinned") :
                _utils.LoadBitmap("clingy_unpinned");
        }
    }

    public bool IsLocked
    {
        get { return _isLocked; }
        set
        {
            _isLocked = value;
            LockedImage.Source = _isLocked ?
                _utils.LoadBitmap("clingy_locked") :
                null;

        }
    }

    public string? Title
    {
        get { return _title; }
        set
        {
            _title = value;
            TitleText.Text = value;
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        _parentWindow?.CloseRequest();
    }

    private void OnPinClick(object? sender, RoutedEventArgs e)
    {
        _isPinned = !_isPinned;
        IsPinned = _isPinned;
        _parentWindow?.PinRequest(_isPinned);
    }

    private void OnDoubleTap(object? sender, RoutedEventArgs e)
    {
        _isRolled = !_isRolled;
        _parentWindow?.RollRequest(_isRolled);
    }

    private void OnDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _parentWindow?.BeginMoveDrag(e);
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
        {
            _parentWindow?.ShowContextMenu(e);
        }
    }
}