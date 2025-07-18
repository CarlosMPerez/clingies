using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.Domain.Models;

namespace Clingies.Windows;

public partial class ClingyWindow : Window
{
    private readonly Clingy _clingy;
    public Guid ClingyId => _clingy.Id;
    public event EventHandler<Guid>? CloseRequested;
    public event EventHandler<PinRequestedEventArgs>? PinRequested;
    public event EventHandler<TitleChangeRequestedEventArgs>? TitleChangeRequested;
    public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;
    public event EventHandler<RollRequestedEventArgs>? RollRequested;
    public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
    public event EventHandler<UpdateWindowWidthRequestedEventArgs>? UpdateWindowWidthRequested;

    //public ClingyWindow() => InitializeWindow();

    public ClingyWindow(Clingy clingy) // : this()
    {
        InitializeComponent();
        _clingy = clingy;

        Position = new PixelPoint((int)clingy.PositionX, (int)clingy.PositionY);
        Width = clingy.Width;
        Topmost = clingy.IsPinned;
        // establish title props
        ClingyTitleBar.ClingyId = _clingy.Id;
        ClingyTitleBar.Title = _clingy.Title;
        ClingyTitleBar.IsRolled = _clingy.IsRolled;
        ClingyTitleBar.IsPinned = _clingy.IsPinned;
        // establish body props
        ClingyBody.ClingyId = _clingy.Id;
        ClingyBody.BodyContent = _clingy.Content;
        ClingyBody.IsRolled = _clingy.IsRolled;

        PositionChanged += OnPositionChanged;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
    }

    protected async override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Shift+Ctrl+T -> SET TITLE
        if (e.KeyModifiers == (KeyModifiers.Shift | KeyModifiers.Control) && e.Key == Key.T)
        {
            var dialog = new ClingyTitleDialog(string.IsNullOrEmpty(_clingy.Title) ? "" : _clingy.Title);
            var result = await dialog.ShowDialog<string>(this);
            if (!string.IsNullOrWhiteSpace(result))
            {
                this.ClingyTitleBar.Title = result;
                var args = new TitleChangeRequestedEventArgs(ClingyId, result);
                TitleChangeRequested?.Invoke(this, args);
            }
        }
    }

    public void ContentChangeRequest(string bodyContent, double height)
    {
        var args = new ContentChangeRequestedEventArgs(ClingyId, bodyContent, height);
        this.Height = this.ClingyTitleBar.Height + height;
        ContentChangeRequested?.Invoke(this, args);
    }

    public void WidthChangeRequest()
    {
        var args = new UpdateWindowWidthRequestedEventArgs(ClingyId, Width);
        UpdateWindowWidthRequested?.Invoke(this, args);
    }

    public void CloseRequest()
    {
        CloseRequested?.Invoke(this, ClingyId);
    }

    public void PinRequest(bool isPinned)
    {
        var args = new PinRequestedEventArgs(ClingyId, isPinned);
        PinRequested?.Invoke(this, args);
    }

    public void RollRequest(bool isRolled)
    {
        var args = new RollRequestedEventArgs(ClingyId, isRolled);
        RollRequested?.Invoke(this, args);
    }

    public void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        var args = new PositionChangeRequestedEventArgs(ClingyId, this.Position.X, this.Position.Y);
        PositionChangeRequested?.Invoke(this, args);
    }
}