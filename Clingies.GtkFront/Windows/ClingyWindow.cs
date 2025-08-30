// src/Clingies.Gtk/Windows/ClingyWindow.cs  (edited)
using System;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.Models;
using Clingies.GtkFront.Utils;
using Clingies.GtkFront.Windows.Parts;
using Gtk;

namespace Clingies.GtkFront.Windows
{
    public sealed class ClingyWindow : Window
    {
        private readonly ClingyDto dto;
        public int ClingyId => dto.Id;

        public event EventHandler<int>? CloseRequested;
        public event EventHandler<PinRequestedEventArgs>? PinRequested;
        public event EventHandler<TitleChangeRequestedEventArgs>? TitleChangeRequested;
        public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;
        public event EventHandler<RollRequestedEventArgs>? RollRequested;
        public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
        public event EventHandler<UpdateWindowSizeRequestedEventArgs>? UpdateWindowSizeRequested;
        public event EventHandler<LockRequestedEventArgs>? LockRequested;

        public IContextCommandProvider? CommandProvider { get; private set; }

        private readonly UtilsService _srvUtils;

        public ClingyWindow(ClingyDto clingyDto, UtilsService utils) : base(clingyDto.Title ?? string.Empty)
        {
            dto = clingyDto;
            _srvUtils = utils;

            Decorated = false;
            SkipTaskbarHint = true;
            SkipPagerHint = true;

            DefaultWidth  = Math.Max(200, (int)dto.Width);
            DefaultHeight = Math.Max(120, (int)dto.Height);
            Move((int)dto.PositionX, (int)dto.PositionY);

            // Build callbacks that *raise the same events* the Manager already listens to
            var cb = new ClingyWindowCallbacks(
                dto.Id,
                closeRequested: () => CloseRequested?.Invoke(this, dto.Id),
                positionChanged: (x, y) => PositionChangeRequested?.Invoke(this, new PositionChangeRequestedEventArgs(dto.Id, x, y)),
                sizeChanged: (w, h) => UpdateWindowSizeRequested?.Invoke(this, new UpdateWindowSizeRequestedEventArgs(dto.Id, w, h)),
                contentChanged: text => ContentChangeRequested?.Invoke(this, new ContentChangeRequestedEventArgs(dto.Id, text)),
                titleChanged: title => TitleChangeRequested?.Invoke(this, new TitleChangeRequestedEventArgs(dto.Id, title)),
                pinChanged: isPinned => PinRequested?.Invoke(this, new PinRequestedEventArgs(dto.Id, isPinned)),
                lockChanged: isLocked => LockRequested?.Invoke(this, new LockRequestedEventArgs(dto.Id, isLocked))
            );

            // Compose UI
            var root = new Box(Orientation.Vertical, 0) { Name = "clingy-window", BorderWidth = 0 };
            var title = ClingyTitleBarBuilder.Build(dto, this, _srvUtils, cb);
            var body  = ClingyBodyBuilder.Build(dto, this, _srvUtils, cb);

            root.PackStart(title, false, false, 0);
            root.PackStart(body,  true,  true,  0);
            Add(root);
            root.ShowAll();

            // Persist *size* continuously in a WM-agnostic way
            this.SizeAllocated += (_, a) =>
            {
                UpdateWindowSizeRequested?.Invoke(this, new UpdateWindowSizeRequestedEventArgs(dto.Id, a.Allocation.Width, a.Allocation.Height));
            };

            // Final save on close happens in Manager (listening to CloseRequested)
            this.DeleteEvent += (_, __) => CloseRequested?.Invoke(this, dto.Id);
        }

        public void SetContextCommandProvider(IContextCommandProvider provider) => CommandProvider = provider ?? throw new ArgumentNullException(nameof(provider));
    }
}
