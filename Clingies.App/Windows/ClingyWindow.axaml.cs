using System;
using Avalonia.Controls;
using Clingies.App.Windows.Controls;
using Clingies.App.Common.CustomEventArgs;
using Clingies.Domain.Models;
using Avalonia;

namespace Clingies.App.Windows;
public partial class ClingyWindow : Window
{
    private Clingy _clingy;
    private bool _isRolled = false;
    private double _unrolledHeight;
    private bool _initiallyRolled;

    public Guid ClingyId => _clingy.Id;

    public event EventHandler<Guid>? CloseRequested;
    public event EventHandler<PinRequestedEventArgs>? PinRequested;
    public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
    public event EventHandler<RollRequestedEventArgs>? RollRequested;

    public ClingyWindow(Clingy clingy)
    {
        InitializeComponent();
        _clingy = clingy;
        _initiallyRolled = clingy.IsRolled;

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
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_initiallyRolled) ToggleRolled();
    }

    private void TogglePin()
    {
        bool newState = !_clingy.IsPinned;
        _clingy.SetPinState(newState);
        Topmost = newState;
        PinRequested?.Invoke(this, new PinRequestedEventArgs(_clingy.Id, newState));
    }

    private void ToggleRolled()
    {
        _isRolled = !_isRolled;
        _clingy.SetRolledState(_isRolled);

        Body.IsVisible = !_isRolled;
        Height = _isRolled ? 30 : _unrolledHeight;

        RollRequested?.Invoke(this, new RollRequestedEventArgs(_clingy.Id, _isRolled));
    }

    private void OnContentChanged(string text)
    {
        _clingy.UpdateContent(text);
        ContentChangeRequested?.Invoke(this, new ContentChangeRequestedEventArgs(_clingy.Id, text));

        if (!_isRolled)
        {
            // Measure height
            var tb = Body.FindControl<TextBox>("ContentBox");
            tb.Measure(new Size(tb.Bounds.Width, double.PositiveInfinity));

            var measuredHeight = tb.DesiredSize.Height;
            if (measuredHeight < 30) measuredHeight = 30;

            _unrolledHeight = measuredHeight + 40; // + title + margin
            Height = _unrolledHeight;
        }
    }
}
