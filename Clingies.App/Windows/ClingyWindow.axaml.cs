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

        TitleBar.Title = clingy.Title;
        Body.Content = clingy.Content;

        TitleBar.CloseRequested += (_, _) => CloseRequested?.Invoke(this, _clingy.Id);
        TitleBar.PinClicked += (_, _) => TogglePin();
        TitleBar.TitleDoubleClicked += (_, _) => ToggleRolled();
        Body.ContentChanged += (_, text) => OnContentChanged(text);

        if (!clingy.IsRolled)
        {
            Height = clingy.Height;
            _unrolledHeight = clingy.Height;
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Opacity = 0;

        Dispatcher.UIThread.Post(() =>
        {
            _unrolledHeight = Height;

            if (_initiallyRolled)
                ToggleRolled();

            Opacity = 1;
        }, DispatcherPriority.Render);
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
